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
        #region public properties

        public SampleDto Sample { get; set; }
        public SampleResultDto SampleResult { get; set; }

        #endregion
    }

    internal class PdfGenerator
    {
        #region fields

        private readonly Document _pdfDocument;
        private readonly Page _pdfPage;
        private readonly ReportPackageDto _reportPackage;
        private readonly TextState _sectionTextSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false);
        private readonly TextState _sectionTitleBoldSize12 = new TextState(fontFamily:"Arial", bold:true, italic:false);

        #endregion

        #region constructors and destructor

        public PdfGenerator(ReportPackageDto reportPackage)
        {
            var pdflicense = new License();
            pdflicense.SetLicense(licenseName:@"Aspose.Pdf.lic");
            pdflicense.Embedded = true;

            _sectionTitleBoldSize12.FontSize = 12;
            _sectionTitleBoldSize12.FontStyle = FontStyles.Bold;
            _sectionTextSize10.FontSize = 10;

            _reportPackage = reportPackage;

            _pdfDocument = new Document();
            _pdfPage = _pdfDocument.Pages.Add();
        }

        #endregion

        public byte[] CreateCopyOfRecordPdf(bool draftMode = false)
        {
            if (_reportPackage == null)
            {
                throw new NullReferenceException(message:"_reportPackage");
            }

            _pdfPage.SetPageSize(width:PageSize.PageLetter.Width, height:PageSize.PageLetter.Height);
            _pdfPage.PageInfo.IsLandscape = true;
            _pdfPage.PageInfo.Margin.Top = 20;
            _pdfPage.PageInfo.Margin.Left = 10;
            _pdfPage.PageInfo.Margin.Bottom = 20;
            _pdfPage.PageInfo.Margin.Right = 10;

            // header and footer
            var reportName = _reportPackage.Name;
            var authorityName = _reportPackage.RecipientOrganizationName;
            var industryName = _reportPackage.OrganizationName;
            var submissionDateTimeString = _reportPackage.SubmissionDateTimeLocal?.ToString(format:"MMMM dd yyyy hh:mm tt").ToUpper() ?? "";

            HeaderFooterTable(pdfPage:_pdfPage, reportName:reportName, submittedDateTimeString:submissionDateTimeString, authorityName:authorityName, industryName:industryName,
                              draftMode:draftMode);

            // report info part  
            ReportInfoTable(pdfPage:_pdfPage);

            if(!_reportPackage.ReportPackageElementCategories.Contains(item:ReportElementCategoryName.SamplesAndResults))
            {
                CommentTable();
            }

            //TO determine the order of the following 3 sections 
            foreach (var elementCategory in _reportPackage.ReportPackageElementCategories)
            {
                PrintPdfSections(reportEkeeCategoryName:elementCategory);
            }

            //Add draft stamp to pdf 
            if (draftMode)
            {
                AddWatermark();
            }

            var mStream = new MemoryStream();
            _pdfDocument.Save(outputStream:mStream, format:SaveFormat.Pdf);
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
            var attachmentsTable = new Table
                                   {
                                       Broken = TableBroken.VerticalInSamePage,
                                       DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2)
                                   };
            _pdfPage.Paragraphs.Add(paragraph:attachmentsTable);

            attachmentsTable.Margin.Top = 20;
            attachmentsTable.ColumnWidths = "33% 34% 33%";
            var row = attachmentsTable.Rows.Add();

            var cell = row.Cells.Add(text:"Attachments", ts:_sectionTitleBoldSize12);
            cell.ColSpan = 3;

            var attachments = _reportPackage.AttachmentTypes.SelectMany(i => i.FileStores).ToList();
            if (attachments.Any())
            {
                row = attachmentsTable.Rows.Add();
                cell = row.Cells.Add(text:"These files are also part of the Copy Of Record.", ts:_sectionTextSize10);
                cell.ColSpan = 3;

                //Attachment files list table
                var tableOrder = new BorderInfo(borderSide:BorderSide.All, borderWidth:0.1F);
                var attachmentFilesTable = new Table
                                           {
                                               Border = tableOrder,
                                               DefaultCellBorder = tableOrder,
                                               DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2),
                                               Margin = {Top = 10}
                                           };

                attachmentFilesTable.DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2);
                _pdfPage.Paragraphs.Add(paragraph:attachmentFilesTable);

                var titleBoldSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                     {
                                         FontSize = 8,
                                         FontStyle = FontStyles.Bold
                                     };

                var titleSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                 {
                                     FontSize = 8
                                 };

                attachmentFilesTable.ColumnWidths = "33% 34% 33%";
                row = attachmentFilesTable.Rows.Add();
                row.DefaultCellTextState = titleBoldSize8;
                row.BackgroundColor = Color.LightGray;

                row.Cells.Add(text:"Original File Name");
                row.Cells.Add(text:"System Generated Unique File Name");
                row.Cells.Add(text:"Attachment Type");

                foreach (var attachedFile in attachments)
                {
                    row = attachmentFilesTable.Rows.Add();
                    row.DefaultCellTextState = titleSize8;
                    row.Cells.Add(text:attachedFile.OriginalFileName);
                    row.Cells.Add(text:attachedFile.Name);
                    row.Cells.Add(text:attachedFile.ReportElementTypeName);
                }
            }
            else
            {
                // show "No attachments included
                row = attachmentsTable.Rows.Add();
                cell = row.Cells.Add(text:"No attachments included.", ts:_sectionTextSize10);
                cell.ColSpan = 3;
            }
        }

        // Comment section
        private void CommentTable()
        {
            var commentTable = new Table {DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2)};
            _pdfPage.Paragraphs.Add(paragraph:commentTable);

            commentTable.Margin.Top = 20;
            commentTable.ColumnWidths = "100%";
            var row = commentTable.Rows.Add();
            row.Cells.Add(text:"Comments", ts:_sectionTitleBoldSize12);

            row = commentTable.Rows.Add();
            row.Cells.Add(text:_reportPackage.Comments.GetValueOrEmptyString(), ts:_sectionTextSize10);
        }

        // This include TTO Certification, and Signature Certification
        private void CertificationsTable()
        {
            foreach (var certification in _reportPackage.CertificationTypes.Where(cert => cert.IsIncluded))
            {
                var certificateTable = new Table
                                       {
                                           DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2),
                                           Broken = TableBroken.VerticalInSamePage
                                       };
                _pdfPage.Paragraphs.Add(paragraph:certificateTable);

                certificateTable.Margin.Top = 20;
                certificateTable.ColumnWidths = "100%";
                var row = certificateTable.Rows.Add();
                row.Cells.Add(text:certification.ReportElementTypeName, ts:_sectionTitleBoldSize12);

                row = certificateTable.Rows.Add();
                row.Cells.Add(text:certification.ReportElementTypeContent.GetValueOrEmptyString(), ts:_sectionTextSize10);
            }
        }

        private void SampleAndResult()
        {
            // According to Chris, we can assume there is only one sampleAndResults in one report; 
            if (_reportPackage.SamplesAndResultsTypes != null && _reportPackage.SamplesAndResultsTypes.Count > 0)
            {
                var sampleResultsTextTable = new Table
                                             {
                                                 IsKeptWithNext = true
                                             };

                _pdfPage.Paragraphs.Add(paragraph:sampleResultsTextTable);

                sampleResultsTextTable.Margin.Top = 20;

                sampleResultsTextTable.ColumnWidths = "100%";
                var row = sampleResultsTextTable.Rows.Add();

                var sectionTitle = _reportPackage.SamplesAndResultsTypes[index:0].ReportElementTypeName;
                row.Cells.Add(text:sectionTitle, ts:_sectionTitleBoldSize12);

                var allSamples = _reportPackage.SamplesAndResultsTypes.SelectMany(i => i.Samples).ToList();
                if (!allSamples.Any())
                {
                    // Show "No samples reported. 
                    row = sampleResultsTextTable.Rows.Add();
                    row.Cells.Add(text:"No samples reported.", ts:_sectionTextSize10);
                }
                else
                {
                    var tableOrder = new BorderInfo(borderSide:BorderSide.All, borderWidth:0.1F);

                    //Samples and Results table  
                    var sampleResultsTable = new Table
                                             {
                                                 Border = tableOrder,
                                                 DefaultCellBorder = tableOrder,
                                                 DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2)
                                             };

                    _pdfPage.Paragraphs.Add(paragraph:sampleResultsTable);

                    // Month, Parameter, result, MDL, Sample Start, Sample End, Collection Method, Lab SampleId, Analysis Method, EPA method, Analysis Data, Flow
                    sampleResultsTable.ColumnWidths = "6.48% 19.48% 9.38% 9.38% 7.58% 7.58% 7.78% 8.28% 7.98% 7.58% 8.48%"; //needs to equal 100%

                    var sampleMonitoringPointerGroups = allSamples.GroupBy(i => i.MonitoringPointId);
                    foreach (var sampleMonitoringPointerGroup in sampleMonitoringPointerGroups)
                    {
                        var samples = sampleMonitoringPointerGroup.Select(i => i).ToList();
                        if (samples.Any())
                        {
                            var monitoringPointName = samples[index:0].MonitoringPointName;
                            DrawMonitoringPointSamplesAndResultsTable(sampleResultsTable:sampleResultsTable, samples:samples, monitoringPointName:monitoringPointName,
                                                                      tableOrder:tableOrder);
                        }
                    }
                }
            }
        }

        private void DrawMonitoringPointSamplesAndResultsTable(Table sampleResultsTable, List<SampleDto> samples, string monitoringPointName, BorderInfo tableOrder)
        {
            // table header text font
            var centerTextBoldSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                       {
                                           FontSize = 10,
                                           FontStyle = FontStyles.Bold,
                                           HorizontalAlignment = HorizontalAlignment.Center
                                       };

            var leftTextBoldSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                     {
                                         FontSize = 10,
                                         FontStyle = FontStyles.Bold,
                                         HorizontalAlignment = HorizontalAlignment.Left
                                     };

            var rightTextBoldSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                      {
                                          FontSize = 10,
                                          FontStyle = FontStyles.Bold,
                                          HorizontalAlignment = HorizontalAlignment.Right
                                      };

            // cell text font
            var centerTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                  {
                                      FontSize = 8,
                                      HorizontalAlignment = HorizontalAlignment.Center
                                  };

            var leftTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                {
                                    FontSize = 8,
                                    HorizontalAlignment = HorizontalAlignment.Left
                                };

            var rightTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                 {
                                     FontSize = 8,
                                     HorizontalAlignment = HorizontalAlignment.Right
                                 };

            // Monitoring Point row 
            var row = sampleResultsTable.Rows.Add();
            row.DefaultCellTextState = leftTextBoldSize10;
            row.BackgroundColor = Color.LightGray;
            row.Border = tableOrder;
            var cell = row.Cells.Add(text:$"Monitoring Point:{monitoringPointName}");
            cell.ColSpan = 11;

            row = sampleResultsTable.Rows.Add();
            row.DefaultCellTextState = centerTextBoldSize10;
            row.BackgroundColor = Color.LightGray;

            row.Cells.Add(text:"Month");
            row.Cells.Add(text:"Parameter");
            row.Cells.Add(text:"Result");
            row.Cells.Add(text:"MDL");
            row.Cells.Add(text:"Sample Start");
            row.Cells.Add(text:"Sample End");
            row.Cells.Add(text:"Collection Method");
            row.Cells.Add(text:"Lab Sample ID");
            row.Cells.Add(text:"Analysis Method");
            row.Cells.Add(text:"Analysis Date");
            row.Cells.Add(text:"Flow");

            // samples in the same monitoring pointer are grouped by month plus year
            // in the same group,  samples are sorted by  start date ASC, end date ASC, parameter name ASC, collection method ASC. 
            var monthYearGroups = samples
                .GroupBy(a => new {a.StartDateTimeLocal.Month, a.StartDateTimeLocal.Year}, (key, group) => new
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
                    sample.StartDateTimeLocal = sample.StartDateTimeLocal.AddSeconds(value:-sample.StartDateTimeLocal.Second)
                                                      .AddMilliseconds(value:-sample.StartDateTimeLocal.Millisecond);
                    sample.EndDateTimeLocal = sample.EndDateTimeLocal.AddSeconds(value:-sample.EndDateTimeLocal.Second).AddMilliseconds(value:-sample.EndDateTimeLocal.Millisecond);

                    sampleResultExtensions.AddRange(collection:sample.SampleResults.Select(sampleResult => new SampleResultExtension
                                                                                                           {
                                                                                                               Sample = sample,
                                                                                                               SampleResult = sampleResult
                                                                                                           }));
                }

                // sort sampleResultExtensions by  start date ASC, end date ASC, parameter name ASC, limit basis ASC, collection method ASC   
                sampleResultExtensions = sampleResultExtensions.OrderBy(a => a.Sample.StartDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower())
                                                               .ThenBy(b => b.Sample.EndDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower())
                                                               .ThenBy(c => c.SampleResult.ParameterName)
                                                               .ThenBy(d => d.Sample.CollectionMethodName)
                                                               .ToList();

                // Fill data; 
                for (var i = 0; i < sampleResultExtensions.Count; i++)
                {
                    var sampleResultExtension = sampleResultExtensions[index:i];
                    row = sampleResultsTable.Rows.Add();
                    if (i == 0)
                    {
                        row.Cells.Add(text:sampleResultExtension.Sample.StartDateTimeLocal.ToString(format:"MMMM"), ts:centerTextSize8);
                    }
                    else
                    {
                        row.Cells.Add(text:"");
                    }

                    row.Cells.Add(text:sampleResultExtension.SampleResult.ParameterName, ts:leftTextSize8);
                    row.Cells.Add(text:GetSampleResultValue(sampleResultDto:sampleResultExtension.SampleResult), ts:rightTextSize8);
                    var mdl = sampleResultExtension.SampleResult.EnteredMethodDetectionLimit.GetValueOrEmptyString();
                    if (!mdl.Equals(value:string.Empty))
                    {
                        mdl = $"{mdl} {sampleResultExtension.SampleResult.UnitName}";
                    }

                    row.Cells.Add(text:$"{mdl}", ts:rightTextSize8);

                    row.Cells.Add(text:sampleResultExtension.Sample.StartDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue:" 12:00 am", newValue:""),
                                  ts:centerTextSize8);
                    row.Cells.Add(text:sampleResultExtension.Sample.EndDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue:" 12:00 am", newValue:""),
                                  ts:centerTextSize8);

                    row.Cells.Add(text:sampleResultExtension.Sample.CollectionMethodName, ts:centerTextSize8);
                    row.Cells.Add(text:sampleResultExtension.Sample.LabSampleIdentifier.GetValueOrEmptyString(), ts:centerTextSize8);
                    row.Cells.Add(text:sampleResultExtension.SampleResult.AnalysisMethod.GetValueOrEmptyString(), ts:centerTextSize8);
                    var analysisDateTimeString = sampleResultExtension.SampleResult.AnalysisDateTimeLocal.HasValue
                                                     ? sampleResultExtension
                                                         .SampleResult.AnalysisDateTimeLocal.Value.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower()
                                                         .Replace(oldValue:" 12:00 am", newValue:"")
                                                     : "";

                    row.Cells.Add(text:analysisDateTimeString, ts:centerTextSize8);
                    row.Cells.Add(text:$"{sampleResultExtension.Sample.FlowEnteredValue.GetValueOrEmptyString()} {sampleResultExtension.Sample.FlowUnitName.GetValueOrEmptyString()}",
                                  ts:rightTextSize8);

                    if (!string.IsNullOrWhiteSpace(value:sampleResultExtension.SampleResult.MassLoadingValue))
                    {
                        row = sampleResultsTable.Rows.Add();
                        row.Cells.Add(text:"");
                        row.Cells.Add(text:sampleResultExtension.SampleResult.ParameterName, ts:leftTextSize8);
                        row.Cells.Add(text:$"{sampleResultExtension.SampleResult.MassLoadingValue} {sampleResultExtension.SampleResult.MassLoadingUnitName}", ts:rightTextSize8);
                        row.Cells.Add(text:$"{mdl}", ts:rightTextSize8);
                        row.Cells.Add(text:sampleResultExtension.Sample.StartDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue:" 12:00 am", newValue:""),
                                      ts:centerTextSize8);
                        row.Cells.Add(text:sampleResultExtension.Sample.EndDateTimeLocal.ToString(format:"MM/dd/yyyy hh:mm tt").ToLower().Replace(oldValue:" 12:00 am", newValue:""),
                                      ts:centerTextSize8);
                        row.Cells.Add(text:sampleResultExtension.Sample.CollectionMethodName, ts:centerTextSize8);
                        row.Cells.Add(text:sampleResultExtension.Sample.LabSampleIdentifier.GetValueOrEmptyString(), ts:centerTextSize8);
                        row.Cells.Add(text:sampleResultExtension.SampleResult.AnalysisMethod.GetValueOrEmptyString(), ts:centerTextSize8);
                        row.Cells.Add(text:analysisDateTimeString, ts:centerTextSize8);
                        row.Cells.Add(text:$"{sampleResultExtension.Sample.FlowEnteredValue.GetValueOrEmptyString()} {sampleResultExtension.Sample.FlowUnitName.GetValueOrEmptyString()}",
                                      ts:rightTextSize8);
                    }
                }
            }
        }

        private string GetSampleResultValue(SampleResultDto sampleResultDto)
        {
            if (string.IsNullOrWhiteSpace(value:sampleResultDto.EnteredValue))
            {
                return sampleResultDto.Qualifier;
            }

            return $"{sampleResultDto.Qualifier}{sampleResultDto.EnteredValue} {sampleResultDto.UnitName}";
        }

        private void ReportInfoTable(Page pdfPage)
        {
            var textStateSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false);
            var boldTextStateSize10 = new TextState(fontFamily:"Arial", bold:true, italic:false);

            textStateSize10.FontSize = 10;
            boldTextStateSize10.FontSize = 10;

            var reportInfoTable = new Table
                                  {
                                      DefaultCellPadding = new MarginInfo(left:3, bottom:3, right:3, top:3),
                                      Margin = {Top = 20f}
                                  };

            pdfPage.Paragraphs.Add(paragraph:reportInfoTable);
            reportInfoTable.ColumnWidths = "15% 30% 10% 15% 30%";

            reportInfoTable.SetColumnTextState(colNumber:0, textState:boldTextStateSize10);
            reportInfoTable.SetColumnTextState(colNumber:1, textState:textStateSize10);
            reportInfoTable.SetColumnTextState(colNumber:2, textState:textStateSize10);
            reportInfoTable.SetColumnTextState(colNumber:3, textState:boldTextStateSize10);
            reportInfoTable.SetColumnTextState(colNumber:4, textState:textStateSize10);

            //--------------------------------Row 1 
            var row = reportInfoTable.Rows.Add();
            row.Cells.Add(text:"Report:", ts:boldTextStateSize10);
            row.Cells.Add(text:_reportPackage.Name, ts:textStateSize10);

            row.Cells.Add(text:"");
            row.Cells.Add(text:"Period:", ts:boldTextStateSize10);
            row.Cells.Add(text:$"{_reportPackage.PeriodStartDateTimeLocal:MMMM dd, yyyy} - {_reportPackage.PeriodEndDateTimeLocal:MMMM dd, yyyy}", ts:textStateSize10);

            //--------------------------------Row 2
            // empty row
            row = reportInfoTable.Rows.Add();
            row.MinRowHeight = 10;

            //--------------------------------Row 3
            row = reportInfoTable.Rows.Add();
            row.Cells.Add(text:"Industry Name:", ts:boldTextStateSize10);
            row.Cells.Add(text:_reportPackage.OrganizationName, ts:textStateSize10);

            row.Cells.Add(text:"");

            row.Cells.Add(text:"Submitted Date:", ts:boldTextStateSize10);

            row.Cells.Add(text:_reportPackage.SubmissionDateTimeLocal?.ToString(format:"MMMM dd, yyyy hh:mm tt ") ?? "", ts:textStateSize10);

            //--------------------------------Row 4
            row = reportInfoTable.Rows.Add();
            row.Cells.Add(text:"Industry Number:", ts:boldTextStateSize10);
            row.Cells.Add(text:_reportPackage.OrganizationReferenceNumber.GetValueOrEmptyString(), ts:textStateSize10);

            row.Cells.Add(text:"");

            row.Cells.Add(text:"Submitted By:", ts:boldTextStateSize10);
            var submitter = $"{_reportPackage.SubmitterFirstName.GetValueOrEmptyString()} {_reportPackage.SubmitterLastName.GetValueOrEmptyString()}";
            row.Cells.Add(text:submitter, ts:textStateSize10);

            //--------------------------------Row 5
            row = reportInfoTable.Rows.Add();
            row.Cells.Add(text:"Address:", ts:boldTextStateSize10);
            if (string.IsNullOrWhiteSpace(value:_reportPackage.OrganizationAddressLine2))
            {
                _reportPackage.OrganizationAddressLine2 = "";
            }

            var addressLine1 = _reportPackage.OrganizationAddressLine1.GetValueOrEmptyString();
            var addressLine2 = _reportPackage.OrganizationAddressLine2.GetValueOrEmptyString();
            var cityName = _reportPackage.OrganizationCityName.GetValueOrEmptyString();
            var jursdicationName = _reportPackage.OrganizationJurisdictionName.GetValueOrEmptyString();
            var zipCode = _reportPackage.OrganizationZipCode.GetValueOrEmptyString();

            var address1 = string.IsNullOrWhiteSpace(value:addressLine2) ? $"{addressLine1}," : $"{addressLine1} {addressLine2},";

            row.Cells.Add(text:address1, ts:textStateSize10);

            row.Cells.Add(text:"");

            row.Cells.Add(text:"Title:", ts:boldTextStateSize10);
            row.Cells.Add(text:_reportPackage.SubmitterTitleRole.GetValueOrEmptyString(), ts:textStateSize10);

            // Add another row for city, jurisdiction and zip code 
            var address2 = $"{cityName}, {jursdicationName} {zipCode}";
            row = reportInfoTable.Rows.Add();
            row.Cells.Add(text:"");
            row.Cells.Add(text:address2);

            //  Add empty for the rest cells
            row.Cells.Add(text:"");
            row.Cells.Add(text:"");
            row.Cells.Add(text:"");
        }

        private static void HeaderFooterTable(Page pdfPage, string reportName, string submittedDateTimeString, string authorityName, string industryName, bool draftMode)
        {
            var headerTable = new Table();

            pdfPage.Header = new HeaderFooter();
            var headerMargin = new MarginInfo(left:10, bottom:10, right:10, top:5);
            pdfPage.Header.Margin = headerMargin;

            pdfPage.Header.Paragraphs.Add(paragraph:headerTable);

            var leftHeaderFooterTextState = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                            {
                                                FontSize = 6,
                                                FontStyle = FontStyles.Bold,
                                                ForegroundColor = Color.Gray,
                                                HorizontalAlignment = HorizontalAlignment.Left
                                            };

            var centerHeaderFooterTextState = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                              {
                                                  FontSize = 6,
                                                  FontStyle = FontStyles.Bold,
                                                  ForegroundColor = Color.Gray,
                                                  HorizontalAlignment = HorizontalAlignment.Center
                                              };

            var rightHeaderFooterTextState = new TextState(fontFamily:"Arial", bold:false, italic:false)
                                             {
                                                 FontSize = 6,
                                                 FontStyle = FontStyles.Bold,
                                                 ForegroundColor = Color.Gray,
                                                 HorizontalAlignment = HorizontalAlignment.Right
                                             };

            headerTable.ColumnWidths = "33% 34% 33%";
            headerTable.DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2);
            headerTable.Broken = TableBroken.None;
            headerTable.Margin.Top = 5f;

            headerTable.SetColumnTextState(colNumber:0, textState:leftHeaderFooterTextState);
            headerTable.SetColumnTextState(colNumber:1, textState:centerHeaderFooterTextState);
            headerTable.SetColumnTextState(colNumber:2, textState:rightHeaderFooterTextState);

            var row = headerTable.Rows.Add();
            row.Cells.Add(text:authorityName);
            row.Cells.Add(text:draftMode ? "" : "Copy Of Record");
            row.Cells.Add(text:industryName);

            // Footer 
            var footerTable = new Table();
            pdfPage.Footer = new HeaderFooter
                             {
                                 Margin =
                                 {
                                     Left = headerMargin.Left,
                                     Right = headerMargin.Right
                                 }
                             };
            pdfPage.Footer.Paragraphs.Add(paragraph:footerTable);

            footerTable.ColumnWidths = "33% 34% 33%";
            footerTable.DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2);
            footerTable.Broken = TableBroken.None;
            footerTable.Margin.Bottom = -5f;
            footerTable.SetColumnTextState(colNumber:0, textState:leftHeaderFooterTextState);
            footerTable.SetColumnTextState(colNumber:1, textState:centerHeaderFooterTextState);
            footerTable.SetColumnTextState(colNumber:2, textState:rightHeaderFooterTextState);

            var text = new TextFragment(text:"Page $p of $P");
            text.TextState.FontSize = 8;

            row = footerTable.Rows.Add();
            row.Cells.Add(text:reportName);
            var cell = row.Cells.Add(text:"");
            cell.Paragraphs.Add(paragraph:text);
            row.Cells.Add(text:submittedDateTimeString);
        }

        private void AddWatermark()
        {
            var annotationText = "DRAFT";
            var textStamp = new TextStamp(value:annotationText)
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
                pdfPage?.AddStamp(stamp:textStamp);
            }
        }
    }
}