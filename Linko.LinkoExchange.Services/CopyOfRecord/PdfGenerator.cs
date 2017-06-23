using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    internal class SampleResultExtension
    {
        public SampleDto Sample { get; set; }
        public SampleResultDto SampleResult { get; set; }
    }

    internal class PdfGenerator
    {
        private readonly ReportPackageDto _reportPackage;
        readonly TextState _sectionTitleBoldSize12 = new TextState("Arial", true, false);
        readonly TextState _sectionTextSize10 = new TextState("Arial", false, false);

        private readonly Document _pdfDocument;
        private readonly Page _pdfPage;

        public PdfGenerator(ReportPackageDto reportPackage)
        {
            License pdflicense = new License();
            pdflicense.SetLicense(@"Aspose.Pdf.lic");
            pdflicense.Embedded = true;

            _sectionTitleBoldSize12.FontSize = 12;
            _sectionTitleBoldSize12.FontStyle = FontStyles.Bold;
            _sectionTextSize10.FontSize = 10;

            _reportPackage = reportPackage;

            _pdfDocument = new Document();
            _pdfPage = _pdfDocument.Pages.Add();
        }

        public byte[] CreateCopyOfRecordPdf(bool draftMode = false)
        {
            if (_reportPackage == null)
            {
                throw new NullReferenceException("_reportPackage");
            }

            _pdfPage.SetPageSize(PageSize.PageLetter.Width, PageSize.PageLetter.Height);
            _pdfPage.PageInfo.IsLandscape = true;
            _pdfPage.PageInfo.Margin.Top = 20;
            _pdfPage.PageInfo.Margin.Left = 10;
            _pdfPage.PageInfo.Margin.Bottom = 20;
            _pdfPage.PageInfo.Margin.Right = 10;

            // header and footer
            var reportName = _reportPackage.Name;
            var authorityName = _reportPackage.RecipientOrganizationName;
            var industryName = _reportPackage.OrganizationName;
            var submissionDateTimeString = _reportPackage.SubmissionDateTimeLocal?.ToString("MMMM dd yyyy hh:mm tt").ToUpper() ?? "";

            HeaderFooterTable(_pdfPage, reportName, submissionDateTimeString, authorityName, industryName, draftMode);

            // report info part  
            ReportInfoTable(_pdfPage);

            //TO determine the order of the following 3 sections 
            foreach (var elementCategory in _reportPackage.ReportPackageElementCategories)
            {
                PrintPdfSections(elementCategory);
            }

            //Add draft stamp to pdf 
            if (draftMode)
            {
                AddWatermark();
            }

            var mStream = new MemoryStream();
            _pdfDocument.Save(mStream, SaveFormat.Pdf);
            return mStream.ToArray();
        }

        private void PrintPdfSections(ReportElementCategoryName reportEkeeCategoryName)
        {
            switch (reportEkeeCategoryName)
            {
                case ReportElementCategoryName.Attachments:
                    AttachmentsTable();
                    break;
                case ReportElementCategoryName.Certifications:
                    CertificationsTable();
                    break;
                case ReportElementCategoryName.SamplesAndResults:
                    SampleAndResult();
                    //Comments section follows samples and results 
                    CommentTable();
                    break;
            }
        }

        private void AttachmentsTable()
        {
            var attachmentsTable = new Table();
            attachmentsTable.Broken = TableBroken.VerticalInSamePage;
            attachmentsTable.DefaultCellPadding = new MarginInfo(2, 2, 2, 2);
            _pdfPage.Paragraphs.Add(attachmentsTable);

            attachmentsTable.Margin.Top = 20;
            attachmentsTable.ColumnWidths = "33% 34% 33%";
            var row = attachmentsTable.Rows.Add();

            var cell = row.Cells.Add("Attachments", _sectionTitleBoldSize12);
            cell.ColSpan = 3;

            var attachments = _reportPackage.AttachmentTypes.SelectMany(i => i.FileStores).ToList();
            if (attachments.Any())
            {
                row = attachmentsTable.Rows.Add();
                cell = row.Cells.Add("These files are also part of the Copy Of Record.", _sectionTextSize10);
                cell.ColSpan = 3;

                //Attachment files list table
                var tableOrder = new BorderInfo(BorderSide.All, 0.1F);
                var attachmentFilesTable = new Table
                {
                    Border = tableOrder,
                    DefaultCellBorder = tableOrder,
                    DefaultCellPadding = new MarginInfo(2, 2, 2, 2),
                    Margin = { Top = 10 }
                };

                attachmentFilesTable.DefaultCellPadding = new MarginInfo(2, 2, 2, 2);
                _pdfPage.Paragraphs.Add(attachmentFilesTable);

                var titleBoldSize8 = new TextState("Arial", false, false);
                titleBoldSize8.FontSize = 8;
                titleBoldSize8.FontStyle = FontStyles.Bold;

                var titleSize8 = new TextState("Arial", false, false);
                titleSize8.FontSize = 8;

                attachmentFilesTable.ColumnWidths = "33% 34% 33%";
                row = attachmentFilesTable.Rows.Add();
                row.DefaultCellTextState = titleBoldSize8;
                row.BackgroundColor = Color.LightGray;

                row.Cells.Add("Original File Name");
                row.Cells.Add("System Generated Unique File Name");
                row.Cells.Add("Attachment Type");

                foreach (var attachedFile in attachments)
                {
                    row = attachmentFilesTable.Rows.Add();
                    row.DefaultCellTextState = titleSize8;
                    row.Cells.Add(attachedFile.OriginalFileName);
                    row.Cells.Add(attachedFile.Name);
                    row.Cells.Add(attachedFile.ReportElementTypeName);
                }
            }
            else
            {
                // show "No attachments included
                row = attachmentsTable.Rows.Add();
                cell = row.Cells.Add("No attachments included.", _sectionTextSize10);
                cell.ColSpan = 3;
            }
        }

        // Comment section
        private void CommentTable()
        {
            var commentTable = new Table { DefaultCellPadding = new MarginInfo(2, 2, 2, 2) };
            _pdfPage.Paragraphs.Add(commentTable);

            commentTable.Margin.Top = 20;
            commentTable.ColumnWidths = "100%";
            var row = commentTable.Rows.Add();
            row.Cells.Add("Comments", _sectionTitleBoldSize12);

            row = commentTable.Rows.Add();
            row.Cells.Add(_reportPackage.Comments.GetValueOrEmptyString(), _sectionTextSize10);
        }

        // This include TTO Certification, and Signature Certification
        private void CertificationsTable()
        {
            foreach (var certification in _reportPackage.CertificationTypes)
            {
                var certificateTable = new Table { DefaultCellPadding = new MarginInfo(2, 2, 2, 2) };
                certificateTable.Broken = TableBroken.VerticalInSamePage;
                _pdfPage.Paragraphs.Add(certificateTable);

                certificateTable.Margin.Top = 20;
                certificateTable.ColumnWidths = "100%";
                var row = certificateTable.Rows.Add();
                row.Cells.Add(certification.ReportElementTypeName, _sectionTitleBoldSize12);

                row = certificateTable.Rows.Add();
                row.Cells.Add(certification.ReportElementTypeContent.GetValueOrEmptyString(), _sectionTextSize10);
            }
        }

        private void SampleAndResult()
        {
            // According to Chris, we can assume there is only one sampleAndResults in one report; 
            if (_reportPackage.SamplesAndResultsTypes != null && _reportPackage.SamplesAndResultsTypes.Count > 0)
            {
                var sampleResultsTextTable = new Table();
                sampleResultsTextTable.IsKeptWithNext = true;

                _pdfPage.Paragraphs.Add(sampleResultsTextTable);

                sampleResultsTextTable.Margin.Top = 20;

                sampleResultsTextTable.ColumnWidths = "100%";
                var row = sampleResultsTextTable.Rows.Add();

                var sectionTitle = _reportPackage.SamplesAndResultsTypes[0].ReportElementTypeName;
                row.Cells.Add(sectionTitle, _sectionTitleBoldSize12);

                var allSamples = _reportPackage.SamplesAndResultsTypes.SelectMany(i => i.Samples).ToList();
                if (!allSamples.Any())
                {
                    // Show "No samples reported. 
                    row = sampleResultsTextTable.Rows.Add();
                    row.Cells.Add("No samples reported.", _sectionTextSize10);
                }
                else
                {
                    var tableOrder = new BorderInfo(BorderSide.All, 0.1F);

                    //Samples and Results table  
                    var sampleResultsTable = new Table
                    {
                        Border = tableOrder,
                        DefaultCellBorder = tableOrder,
                        DefaultCellPadding = new MarginInfo(2, 2, 2, 2)
                    };

                    _pdfPage.Paragraphs.Add(sampleResultsTable);
                    sampleResultsTable.ColumnWidths = "6% 19% 11.6% 5.2% 7.1% 7.1% 7.3% 7.8% 8% 5.3% 7.1% 8.5%";


                    var sampleMonitoringPointerGroups = allSamples.GroupBy(i => i.MonitoringPointId);
                    foreach (var sampleMonitoringPointerGroup in sampleMonitoringPointerGroups)
                    {
                        var samples = sampleMonitoringPointerGroup.Select(i => i).ToList();
                        if (samples.Any())
                        {
                            var monitoringPointName = samples[0].MonitoringPointName;
                            DrawMonitoringPointSamplesAndResultsTable(sampleResultsTable, samples, monitoringPointName, tableOrder);
                        }
                    }
                }
            }
        }

        private void DrawMonitoringPointSamplesAndResultsTable(Table sampleResultsTable, List<SampleDto> samples, string monitoringPointName, BorderInfo tableOrder)
        {
            // table header text font
            var centerTextBoldSize10 = new TextState("Arial", false, false);
            centerTextBoldSize10.FontSize = 10;
            centerTextBoldSize10.FontStyle = FontStyles.Bold;
            centerTextBoldSize10.HorizontalAlignment = HorizontalAlignment.Center;

            var leftTextBoldSize10 = new TextState("Arial", false, false);
            leftTextBoldSize10.FontSize = 10;
            leftTextBoldSize10.FontStyle = FontStyles.Bold;
            leftTextBoldSize10.HorizontalAlignment = HorizontalAlignment.Left;

            var rightTextBoldSize10 = new TextState("Arial", false, false);
            rightTextBoldSize10.FontSize = 10;
            rightTextBoldSize10.FontStyle = FontStyles.Bold;
            rightTextBoldSize10.HorizontalAlignment = HorizontalAlignment.Right;

            // cell text font
            var centerTextSize8 = new TextState("Arial", false, false);
            centerTextSize8.FontSize = 8;
            centerTextSize8.HorizontalAlignment = HorizontalAlignment.Center;

            var leftTextSize8 = new TextState("Arial", false, false);
            leftTextSize8.FontSize = 8;
            leftTextSize8.HorizontalAlignment = HorizontalAlignment.Left;

            var rightTextSize8 = new TextState("Arial", false, false);
            rightTextSize8.FontSize = 8;
            rightTextSize8.HorizontalAlignment = HorizontalAlignment.Right;

            // Monitoring Point row 
            var row = sampleResultsTable.Rows.Add();
            row.DefaultCellTextState = leftTextBoldSize10;
            row.BackgroundColor = Color.LightGray;
            row.Border = tableOrder;
            var cell = row.Cells.Add($"Monitoring Point:{monitoringPointName}");
            cell.ColSpan = 12;

            row = sampleResultsTable.Rows.Add();
            row.DefaultCellTextState = centerTextBoldSize10;
            row.BackgroundColor = Color.LightGray;

            row.Cells.Add("Month");
            row.Cells.Add("Parameter");
            row.Cells.Add("Result");
            row.Cells.Add("MDL");
            row.Cells.Add("Sample Start");
            row.Cells.Add("Sample End");
            row.Cells.Add("Collection Method");
            row.Cells.Add("Lab Sample ID");
            row.Cells.Add("Analysis Method");
            row.Cells.Add("EPA Method");
            row.Cells.Add("Analysis Date");
            row.Cells.Add("Flow");

            // samples in the same monitoring pointer are grouped by month plus year
            // in the same group,  samples are sorted by  start date ASC, end date ASC, parameter name ASC, collection method ASC. 
            var monthYearGroups = samples
                .GroupBy(a => new { a.StartDateTimeLocal.Month, a.StartDateTimeLocal.Year }, (key, group) => new
                {
                    key.Month,
                    key.Year,
                    SamplesGroupByMonthAndYear = group.ToList()
                })
                .OrderBy(i => i.Year)
                .ThenBy(j => j.Month);

            foreach (var monthYearGroup in monthYearGroups)
            {
                var sampleResultExtensions = new List<SampleResultExtension>();

                foreach (var sample in monthYearGroup.SamplesGroupByMonthAndYear)
                {
                    // strip off the seconds and milliseconds part for time 
                    sample.StartDateTimeLocal = sample.StartDateTimeLocal.AddSeconds(-sample.StartDateTimeLocal.Second).AddMilliseconds(-sample.StartDateTimeLocal.Millisecond);
                    sample.EndDateTimeLocal = sample.EndDateTimeLocal.AddSeconds(-sample.EndDateTimeLocal.Second).AddMilliseconds(-sample.EndDateTimeLocal.Millisecond);

                    sampleResultExtensions.AddRange(sample.SampleResults.Select(sampleResult => new SampleResultExtension
                    {
                        Sample = sample,
                        SampleResult = sampleResult
                    }));
                }

                // sort sampleResultExtensions by  start date ASC, end date ASC, parameter name ASC, limitbasis ASC, collection method ASC   
                sampleResultExtensions = sampleResultExtensions.OrderBy(a => a.Sample.StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower())
                                      .ThenBy(b => b.Sample.EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower())
                                      .ThenBy(c => c.SampleResult.ParameterName)
                                      .ThenBy(d => d.Sample.CollectionMethodName)
                                      .ToList();

                // Fill data; 
                for (var i = 0; i < sampleResultExtensions.Count; i++)
                {
                    var sampleResultExtension = sampleResultExtensions[i];
                    row = sampleResultsTable.Rows.Add();
                    if (i == 0)
                    {
                        row.Cells.Add(sampleResultExtension.Sample.StartDateTimeLocal.ToString("MMMM"), centerTextSize8);
                    }
                    else
                    {
                        row.Cells.Add("");
                    }

                    row.Cells.Add(sampleResultExtension.SampleResult.ParameterName, leftTextSize8);
                    row.Cells.Add(GetSampleResultValue(sampleResultExtension.SampleResult), rightTextSize8);
                    var mdl = sampleResultExtension.SampleResult.EnteredMethodDetectionLimit.GetValueOrEmptyString(); 
                    
                    row.Cells.Add($"{mdl}", rightTextSize8);

                    row.Cells.Add(text: sampleResultExtension.Sample.StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue: " 12:00 am",newValue: ""), centerTextSize8);
                    row.Cells.Add(sampleResultExtension.Sample.EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue: " 12:00 am",newValue: ""), centerTextSize8);

                    row.Cells.Add(sampleResultExtension.Sample.CollectionMethodName, centerTextSize8);
                    row.Cells.Add(sampleResultExtension.Sample.LabSampleIdentifier.GetValueOrEmptyString(), centerTextSize8);
                    row.Cells.Add(sampleResultExtension.SampleResult.AnalysisMethod.GetValueOrEmptyString(), centerTextSize8);
                    row.Cells.Add(sampleResultExtension.SampleResult.IsApprovedEPAMethod.ToString(), centerTextSize8);
                    var analysisDateTimeString = sampleResultExtension.SampleResult.AnalysisDateTimeLocal.HasValue
                                                     ? sampleResultExtension.SampleResult.AnalysisDateTimeLocal.Value.ToString("MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue: " 12:00 am",newValue: "")
                                                     : "";

                    row.Cells.Add(analysisDateTimeString, centerTextSize8);
                    row.Cells.Add($"{sampleResultExtension.Sample.FlowValue.GetValueOrEmptyString()} {sampleResultExtension.Sample.FlowUnitName.GetValueOrEmptyString()}", rightTextSize8);

                    if (!string.IsNullOrWhiteSpace(sampleResultExtension.SampleResult.MassLoadingValue))
                    {
                        row = sampleResultsTable.Rows.Add();
                        row.Cells.Add("");
                        row.Cells.Add(sampleResultExtension.SampleResult.ParameterName, leftTextSize8);
                        row.Cells.Add($"{sampleResultExtension.SampleResult.MassLoadingValue} {sampleResultExtension.SampleResult.MassLoadingUnitName}", rightTextSize8);
                        row.Cells.Add($"{sampleResultExtension.SampleResult.EnteredMethodDetectionLimit.GetValueOrEmptyString()}", rightTextSize8);
                        row.Cells.Add(sampleResultExtension.Sample.StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue: " 12:00 am",newValue: ""), centerTextSize8);
                        row.Cells.Add(sampleResultExtension.Sample.EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue: " 12:00 am",newValue: ""), centerTextSize8);
                        row.Cells.Add(sampleResultExtension.Sample.CollectionMethodName, centerTextSize8);
                        row.Cells.Add(sampleResultExtension.Sample.LabSampleIdentifier.GetValueOrEmptyString(), centerTextSize8);
                        row.Cells.Add(sampleResultExtension.SampleResult.AnalysisMethod.GetValueOrEmptyString(), centerTextSize8);
                        row.Cells.Add(sampleResultExtension.SampleResult.IsApprovedEPAMethod.ToString(), centerTextSize8);
                        row.Cells.Add(analysisDateTimeString, centerTextSize8);
                        row.Cells.Add($"{sampleResultExtension.Sample.FlowValue.GetValueOrEmptyString()} {sampleResultExtension.Sample.FlowUnitName.GetValueOrEmptyString()}", rightTextSize8);
                    }
                }
            }
        }

        private string GetSampleResultValue(SampleResultDto sampleResultDto)
        {
            if (string.IsNullOrWhiteSpace(sampleResultDto.Value))
            {
                return sampleResultDto.Qualifier;
            }

            return $"{sampleResultDto.Qualifier}{sampleResultDto.Value} {sampleResultDto.UnitName}";
        }

        private void ReportInfoTable(Page pdfPage)
        {
            var textStateSize10 = new TextState("Arial", false, false);
            var boldTextStateSize10 = new TextState("Arial", true, false);

            textStateSize10.FontSize = 10;
            boldTextStateSize10.FontSize = 10;

            var reportInfoTable = new Table
            {
                DefaultCellPadding = new MarginInfo(3, 3, 3, 3),
                Margin = { Top = 20f }
            };

            pdfPage.Paragraphs.Add(reportInfoTable);
            reportInfoTable.ColumnWidths = "15% 30% 10% 15% 30%";

            reportInfoTable.SetColumnTextState(0, boldTextStateSize10);
            reportInfoTable.SetColumnTextState(1, textStateSize10);
            reportInfoTable.SetColumnTextState(2, textStateSize10);
            reportInfoTable.SetColumnTextState(3, boldTextStateSize10);
            reportInfoTable.SetColumnTextState(4, textStateSize10);

            //--------------------------------Row 1 
            var row = reportInfoTable.Rows.Add();
            row.Cells.Add("Report:", boldTextStateSize10);
            row.Cells.Add(_reportPackage.Name, textStateSize10);

            row.Cells.Add("");
            row.Cells.Add("Period:", boldTextStateSize10);
            row.Cells.Add($"{_reportPackage.PeriodStartDateTimeLocal:MMMM dd, yyyy} - {_reportPackage.PeriodEndDateTimeLocal:MMMM dd, yyyy}", textStateSize10);

            //--------------------------------Row 2
            // empty row
            row = reportInfoTable.Rows.Add();
            row.MinRowHeight = 10;

            //--------------------------------Row 3
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Industry Name:", boldTextStateSize10);
            row.Cells.Add(_reportPackage.OrganizationName, textStateSize10);

            row.Cells.Add("");

            row.Cells.Add("Submitted Date:", boldTextStateSize10);

            row.Cells.Add(_reportPackage.SubmissionDateTimeLocal?.ToString("MMMM dd, yyyy hh:mm tt ") ?? "", textStateSize10);

            //--------------------------------Row 4
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Industry Number:", boldTextStateSize10);
            row.Cells.Add(_reportPackage.OrganizationReferenceNumber.GetValueOrEmptyString(), textStateSize10);

            row.Cells.Add("");

            row.Cells.Add("Submitted By:", boldTextStateSize10); 
            var submitter = $"{_reportPackage.SubmitterFirstName.GetValueOrEmptyString()} {_reportPackage.SubmitterLastName.GetValueOrEmptyString()}";
            row.Cells.Add(submitter, textStateSize10);

            //--------------------------------Row 5
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Address:", boldTextStateSize10);
            if (string.IsNullOrWhiteSpace(_reportPackage.OrganizationAddressLine2))
            {
                _reportPackage.OrganizationAddressLine2 = "";
            }

            var addressLine1 = _reportPackage.OrganizationAddressLine1.GetValueOrEmptyString();
            var addressLine2 = _reportPackage.OrganizationAddressLine2.GetValueOrEmptyString();
            var cityName = _reportPackage.OrganizationCityName.GetValueOrEmptyString();
            var jursdicationName = _reportPackage.OrganizationJurisdictionName.GetValueOrEmptyString();
            var zipCode = _reportPackage.OrganizationZipCode.GetValueOrEmptyString();
            
            var address1 =""; 
            if(string.IsNullOrWhiteSpace(addressLine2))
            {
                address1 = $"{addressLine1},";
            }
            else
            {
                address1 = $"{addressLine1} {addressLine2},";
            }
            
            row.Cells.Add(address1, textStateSize10);

            row.Cells.Add("");

            row.Cells.Add("Title:", boldTextStateSize10);
            row.Cells.Add(_reportPackage.SubmitterTitleRole.GetValueOrEmptyString(), textStateSize10);
            
            // Add another row for city, jurisdiction and zip code 
            var address2 = $"{cityName}, {jursdicationName} {zipCode}";
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("");
            row.Cells.Add(address2);   

            //  Add empty for the rest cells
            row.Cells.Add("");
            row.Cells.Add("");
            row.Cells.Add("");
        }

        private static void HeaderFooterTable(Page pdfPage, string reportName, string submittedDateTimeString, string authorityName, string industryName, bool draftMode)
        {
            Table headerTable = new Table();

            pdfPage.Header = new HeaderFooter();
            var headerMargin = new MarginInfo(10, 10, 10, 5);
            pdfPage.Header.Margin = headerMargin;

            pdfPage.Header.Paragraphs.Add(headerTable);

            TextState leftHeaderFooterTextState = new TextState("Arial", false, false);
            leftHeaderFooterTextState.FontSize = 6;
            leftHeaderFooterTextState.FontStyle = FontStyles.Bold;
            leftHeaderFooterTextState.ForegroundColor = Color.Gray;
            leftHeaderFooterTextState.HorizontalAlignment = HorizontalAlignment.Left;

            TextState centerHeaderFooterTextState = new TextState("Arial", false, false);
            centerHeaderFooterTextState.FontSize = 6;
            centerHeaderFooterTextState.FontStyle = FontStyles.Bold;
            centerHeaderFooterTextState.ForegroundColor = Color.Gray;
            centerHeaderFooterTextState.HorizontalAlignment = HorizontalAlignment.Center;

            TextState rightHeaderFooterTextState = new TextState("Arial", false, false);
            rightHeaderFooterTextState.FontSize = 6;
            rightHeaderFooterTextState.FontStyle = FontStyles.Bold;
            rightHeaderFooterTextState.ForegroundColor = Color.Gray;
            rightHeaderFooterTextState.HorizontalAlignment = HorizontalAlignment.Right;

            headerTable.ColumnWidths = "33% 34% 33%";
            headerTable.DefaultCellPadding = new MarginInfo(2, 2, 2, 2);
            headerTable.Broken = TableBroken.None;
            headerTable.Margin.Top = 5f;

            headerTable.SetColumnTextState(0, leftHeaderFooterTextState);
            headerTable.SetColumnTextState(1, centerHeaderFooterTextState);
            headerTable.SetColumnTextState(2, rightHeaderFooterTextState);

            Row row = headerTable.Rows.Add();
            row.Cells.Add(authorityName);
            if(draftMode)
            {
                row.Cells.Add("");
            }
            else
            {
                row.Cells.Add("Copy Of Record");
            }
            row.Cells.Add(industryName);

            // Footer 
            Table footerTable = new Table();
            pdfPage.Footer = new HeaderFooter();
            pdfPage.Footer.Margin.Left = headerMargin.Left;
            pdfPage.Footer.Margin.Right = headerMargin.Right;
            pdfPage.Footer.Paragraphs.Add(footerTable);

            footerTable.ColumnWidths = "33% 34% 33%";
            footerTable.DefaultCellPadding = new MarginInfo(2, 2, 2, 2);
            footerTable.Broken = TableBroken.None;
            footerTable.Margin.Bottom = -5f;
            footerTable.SetColumnTextState(0, leftHeaderFooterTextState);
            footerTable.SetColumnTextState(1, centerHeaderFooterTextState);
            footerTable.SetColumnTextState(2, rightHeaderFooterTextState);

            TextFragment text = new TextFragment("Page $p of $P");
            text.TextState.FontSize = 8;

            row = footerTable.Rows.Add();
            row.Cells.Add(reportName);
            var cell = row.Cells.Add("");
            cell.Paragraphs.Add(text);
            row.Cells.Add(submittedDateTimeString);
        }

        private void AddWatermark()
        {
            string annotationText = "DRAFT";
            var textStamp = new TextStamp(annotationText)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            textStamp.TextState.ForegroundColor = Color.IndianRed;
            textStamp.TextState.FontSize = 160;
            textStamp.Opacity = 0.3;
            textStamp.RotateAngle = 45;

            _pdfDocument.ProcessParagraphs();
            foreach (var page in _pdfDocument.Pages)
            {
                var pdfPage = page as Page;
                pdfPage?.AddStamp(textStamp);
            }
        }
    }
}
