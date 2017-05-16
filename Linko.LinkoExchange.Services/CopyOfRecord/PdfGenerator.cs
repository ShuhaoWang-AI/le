using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Linko.LinkoExchange.Core.Enum;
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

        private readonly TextState _boldTextState = new TextState("Arial", true, false);
        private readonly TextState _normalTextState = new TextState("Arial", false, false);

        private readonly Document _pdfDocument;
        private readonly Page _pdfPage = null;

        public PdfGenerator(ReportPackageDto reportPackage)
        {
            PdfInit();
            this._reportPackage = reportPackage;
            _boldTextState.FontSize = 12;
            _normalTextState.FontSize = 10;
            _pdfDocument = new Document();
            _pdfPage = _pdfDocument.Pages.Add();
        }

        private void PdfInit()
        {
            License pdflicense = new License();
            pdflicense.SetLicense(@"Aspose.Pdf.lic");
            pdflicense.Embedded = true;
        }

        public void CreateCopyOfRecordPdf()
        {


            _pdfPage.SetPageSize(PageSize.PageLetter.Width, PageSize.PageLetter.Height);
            _pdfPage.PageInfo.IsLandscape = true;
            _pdfPage.PageInfo.Margin.Top = 30;
            _pdfPage.PageInfo.Margin.Left = 10;
            _pdfPage.PageInfo.Margin.Bottom = 20;
            _pdfPage.PageInfo.Margin.Right = 10;

            // header and footer
            var reportName = _reportPackage.Name;
            var authorityName = _reportPackage.RecipientOrganizationName;
            var industryName = _reportPackage.OrganizationName;
            var submissionDateTimeString = _reportPackage.SubmissionDateTimeLocal.Value.ToString("MMMM dd yyyy hh:mm tt").ToUpper();

            HeaderFooterTable(_pdfPage, reportName, submissionDateTimeString, authorityName, industryName);

            // report info part  
            ReportInfoTable(_pdfPage, _normalTextState, _boldTextState);

            //TO determine the order of the following 3 sections 
            foreach (var elementCategory in _reportPackage.ReportPackageTemplateElementCategories)
            {
                PrintPdfSections(elementCategory);
            }

            // save to PDF here 
            var tick = DateTime.Now.Ticks;
            _pdfDocument.Save($"C:\\work\\temp\\reprot_LE_{tick}.pdf");

            //TODO convert the pdf into binary data
            //return new byte[] { };
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
            attachmentsTable.DefaultCellPadding = new MarginInfo(2, 3, 2, 3);
            _pdfPage.Paragraphs.Add(attachmentsTable);

            attachmentsTable.Margin.Top = 20;
            attachmentsTable.ColumnWidths = "33% 33% 33%";
            var row = attachmentsTable.Rows.Add();

            var cell = row.Cells.Add("Attachments", _boldTextState);
            cell.ColSpan = 3;

            row = attachmentsTable.Rows.Add();
            cell = row.Cells.Add("These files are also part of the Copy Of Records.", _normalTextState);
            cell.ColSpan = 3;

            //Attachment files list table
            var tableOrder = new BorderInfo(BorderSide.All, 0.1F);
            var attachmentFilesTable = new Table
            {
                Border = tableOrder,
                DefaultCellBorder = tableOrder,
                DefaultCellPadding = new MarginInfo(3, 3, 3, 3),
                Margin = { Top = 10 }
            };

            attachmentFilesTable.DefaultCellPadding = new MarginInfo(3, 3, 3, 3);
            _pdfPage.Paragraphs.Add(attachmentFilesTable);

            attachmentFilesTable.ColumnWidths = "33% 33% 33%";
            row = attachmentFilesTable.Rows.Add();
            row.BackgroundColor = Color.LightGray;

            row.Cells.Add("Original File Name");
            row.Cells.Add("System Generated Unqiue File Name");
            row.Cells.Add("Attachment Type");

            var attachments = _reportPackage.AttachmentTypes.SelectMany(i => i.ReportFiles).Select(i => i.FileStore);
            foreach (var attachedFile in attachments)
            {
                row = attachmentFilesTable.Rows.Add();
                row.Cells.Add(attachedFile.Name);
                row.Cells.Add(attachedFile.OriginalFileName);
                row.Cells.Add(attachedFile.ReportElementTypeName);
            }
        }

        // Comment section
        private void CommentTable()
        {
            var commentTable = new Table { DefaultCellPadding = new MarginInfo(2, 3, 2, 3) };
            _pdfPage.Paragraphs.Add(commentTable);

            commentTable.Margin.Top = 20;
            commentTable.ColumnWidths = "96%";
            var row = commentTable.Rows.Add();
            row.Cells.Add("Comments", _boldTextState);

            row = commentTable.Rows.Add();
            row.Cells.Add(_reportPackage.Comments, _normalTextState);
        }

        // This include TTO Certification, and Singature Certification
        private void CertificationsTable()
        {
            foreach (var certification in _reportPackage.CertificationTypes)
            {
                var certificateTable = new Table { DefaultCellPadding = new MarginInfo(2, 3, 2, 3) };
                _pdfPage.Paragraphs.Add(certificateTable);

                certificateTable.Margin.Top = 20;
                certificateTable.ColumnWidths = "96%";
                var row = certificateTable.Rows.Add();
                row.Cells.Add(certification.ReportElementTypeName, _boldTextState);

                row = certificateTable.Rows.Add();
                row.Cells.Add(certification.ReportElementTypeContent, _normalTextState);
            }
        }

        private void SampleAndResult()
        {
            var sampleResultsTextTable = new Table();
            sampleResultsTextTable.IsKeptWithNext = true;

            _pdfPage.Paragraphs.Add(sampleResultsTextTable);

            sampleResultsTextTable.Margin.Top = 20;

            sampleResultsTextTable.ColumnWidths = "100%";
            var row = sampleResultsTextTable.Rows.Add();

            row.Cells.Add("Samples and Results", _boldTextState);

            var tableOrder = new BorderInfo(BorderSide.All, 0.1F);

            //Samples and Results table  
            var sampleResultsTable = new Table
            {
                Border = tableOrder,
                DefaultCellBorder = tableOrder,
                DefaultCellPadding = new MarginInfo(3, 3, 3, 3)
            };

            _pdfPage.Paragraphs.Add(sampleResultsTable);
            sampleResultsTable.ColumnWidths = "6% 16.2% 8.3% 5.8% 8.9% 8.9% 7.3% 7.8% 8% 5.3% 8.9% 7.4%";

            // According to Chris, we can assume there is only one sampleAndResults in one report; 
            if (_reportPackage.SamplesAndResultsTypes != null && _reportPackage.SamplesAndResultsTypes.Count > 0)
            {
                var allSamples = _reportPackage.SamplesAndResultsTypes.SelectMany(i => i.ReportSamples).Select(i => i.Sample);
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

        private void DrawMonitoringPointSamplesAndResultsTable(Table sampleResultsTable, List<SampleDto> samples, string monitoringPointName, BorderInfo tableOrder)
        {
            // Monitoring Point row 
            var row = sampleResultsTable.Rows.Add();
            row.BackgroundColor = Color.LightGray;
            row.Border = tableOrder;
            var cell = row.Cells.Add($"Monitoring Point:{monitoringPointName}");
            cell.ColSpan = 12;

            row = sampleResultsTable.Rows.Add();
            row.BackgroundColor = Color.LightGray;

            row.Cells.Add("Month");
            row.Cells.Add("Parameter");
            row.Cells.Add("Result");
            row.Cells.Add("MDL");
            row.Cells.Add("Sample Start");
            row.Cells.Add("Sample End");
            row.Cells.Add("Collection Method");
            row.Cells.Add("Lab Sample ID");
            row.Cells.Add("Analys Method");
            row.Cells.Add("EPA Method");
            row.Cells.Add("Analys Date");
            row.Cells.Add("Flow");

            // samples in the same monitoring pointer are grouped by month plus year
            // in the same group,  samples are sorted by  start date asc, end date asc, param name asc, collection method asc. 
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
                    sampleResultExtensions.AddRange(sample.SampleResults.Select(sampleResult => new SampleResultExtension
                    {
                        Sample = sample,
                        SampleResult = sampleResult
                    }));
                }

                // sort sampleResultExtensions by  start date asc, end date asc, param name asc, limitbasis asc, collection method asc   
                sampleResultExtensions = sampleResultExtensions.OrderBy(a => a.Sample.StartDateTimeLocal)
                                      .ThenBy(b => b.Sample.EndDateTimeLocal)
                                      .ThenBy(c => c.SampleResult.ParameterName)
                                      .ThenBy(d => d.Sample.CollectionMethodName)
                                      .ToList();

                // Fill data; 
                var firstMonthCell = new Cell();
                for (var i = 0; i < sampleResultExtensions.Count; i++)
                {
                    var sampleResultExtension = sampleResultExtensions[i];
                    row = sampleResultsTable.Rows.Add();
                    if (i == 0)
                    {
                        row.Cells.Add(sampleResultExtension.Sample.StartDateTimeLocal.ToString("MMMM"));
                    }
                    else
                    {
                        row.Cells.Add("");
                    }

                    row.Cells.Add(sampleResultExtension.SampleResult.ParameterName);
                    row.Cells.Add($"{sampleResultExtension.SampleResult.Value} {sampleResultExtension.SampleResult.UnitName}");
                    row.Cells.Add($"{sampleResultExtension.SampleResult.MethodDetectionLimit}");
                    row.Cells.Add(sampleResultExtension.Sample.StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                    row.Cells.Add(sampleResultExtension.Sample.EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                    row.Cells.Add(sampleResultExtension.Sample.CollectionMethodName);
                    row.Cells.Add(sampleResultExtension.Sample.LabSampleIdentifier);
                    row.Cells.Add(sampleResultExtension.SampleResult.AnalysisMethod);
                    row.Cells.Add(sampleResultExtension.SampleResult.IsApprovedEPAMethod.ToString());
                    row.Cells.Add(sampleResultExtension.SampleResult.AnalysisDateTimeLocal?.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                    row.Cells.Add($"{sampleResultExtension.Sample.FlowValue} {sampleResultExtension.Sample.FlowUnitName}");

                    if (sampleResultExtension.SampleResult.IsCalcMassLoading)
                    {
                        row = sampleResultsTable.Rows.Add();
                        row.Cells.Add("");
                        row.Cells.Add(sampleResultExtension.SampleResult.ParameterName);
                        row.Cells.Add($"{sampleResultExtension.SampleResult.MassLoadingValue} {sampleResultExtension.SampleResult.MassLoadingUnitName}");
                        row.Cells.Add($"{sampleResultExtension.SampleResult.MethodDetectionLimit?.ToString() ?? ""}");
                        row.Cells.Add(sampleResultExtension.Sample.StartDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                        row.Cells.Add(sampleResultExtension.Sample.EndDateTimeLocal.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                        row.Cells.Add(sampleResultExtension.Sample.CollectionMethodName);
                        row.Cells.Add(sampleResultExtension.Sample.LabSampleIdentifier);
                        row.Cells.Add(sampleResultExtension.SampleResult.AnalysisMethod);
                        row.Cells.Add(sampleResultExtension.SampleResult.IsApprovedEPAMethod.ToString());
                        row.Cells.Add(sampleResultExtension.SampleResult.AnalysisDateTimeLocal?.ToString("MM/dd/yyyy hh:mm tt").ToLower());
                        row.Cells.Add($"{sampleResultExtension.Sample.FlowValue} {sampleResultExtension.Sample.FlowUnitName}");
                    }
                }
            }
        }

        private void ReportInfoTable(Page pdfPage, TextState reportInfoTableNormalTextState, TextState reportInfoTableBoldTextState)
        {
            var reportInfoTable = new Table();
            reportInfoTable.DefaultCellPadding = new MarginInfo(3, 3, 3, 3);
            reportInfoTable.Margin.Top = 20f;

            pdfPage.Paragraphs.Add(reportInfoTable);
            reportInfoTable.ColumnWidths = "15% 30% 10% 15% 30%";

            reportInfoTable.SetColumnTextState(0, reportInfoTableBoldTextState);
            reportInfoTable.SetColumnTextState(1, reportInfoTableNormalTextState);
            reportInfoTable.SetColumnTextState(2, reportInfoTableNormalTextState);
            reportInfoTable.SetColumnTextState(3, reportInfoTableBoldTextState);
            reportInfoTable.SetColumnTextState(4, reportInfoTableNormalTextState);

            //--------------------------------Row 1 
            var row = reportInfoTable.Rows.Add();
            row.Cells.Add("Report:");
            row.Cells.Add(_reportPackage.Name);

            row.Cells.Add("");
            row.Cells.Add("Period:");
            row.Cells.Add($"{_reportPackage.PeriodStartDateTimeLocal}-{_reportPackage.PeriodEndDateTimeLocal}");

            //--------------------------------Row 2
            // empty row
            row = reportInfoTable.Rows.Add();
            row.MinRowHeight = 30;

            //--------------------------------Row 3
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Industy Name:", reportInfoTableBoldTextState);
            row.Cells.Add(_reportPackage.OrganizationName);

            row.Cells.Add("");

            row.Cells.Add("Submitted Date:", reportInfoTableBoldTextState);
            // todo: PM/AM 
            row.Cells.Add(_reportPackage.SubmissionDateTimeLocal.Value.ToString("MMMM dd, yyyy HH:mm "));

            //--------------------------------Row 4
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Industy Number:");
            row.Cells.Add(_reportPackage.OrganizationReferenceNumber);

            row.Cells.Add("");

            row.Cells.Add("Submitted By:");
            row.Cells.Add(_reportPackage.SubmitterUserName);
            //--------------------------------Row 5
            row = reportInfoTable.Rows.Add();
            row.Cells.Add("Address:");
            if (string.IsNullOrWhiteSpace(_reportPackage.OrganizationAddressLine2))
            {
                _reportPackage.OrganizationAddressLine2 = "";
            }
            row.Cells.Add($"{_reportPackage.OrganizationAddressLine1} {_reportPackage.OrganizationAddressLine2},{_reportPackage.OrganizationZipCode}");

            row.Cells.Add("");

            row.Cells.Add("Title:");
            row.Cells.Add(_reportPackage.SubmitterTitleRole);
        }

        private static void HeaderFooterTable(Page pdfPage, string reportName, string submittedDateTimeString, string authorityName, string industryName)
        {
            Table headerTable = new Table();

            pdfPage.Header = new HeaderFooter();
            var headerMargin = new MarginInfo(10, 20, 10, 5);
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

            headerTable.ColumnWidths = "33.3% 33.3% 33.3%";
            headerTable.DefaultCellPadding = new MarginInfo(2, 3, 2, 3);
            headerTable.Broken = TableBroken.None;
            headerTable.Margin.Top = 5f;

            headerTable.SetColumnTextState(0, leftHeaderFooterTextState);
            headerTable.SetColumnTextState(1, centerHeaderFooterTextState);
            headerTable.SetColumnTextState(2, rightHeaderFooterTextState);

            Row row = headerTable.Rows.Add();
            row.Cells.Add(authorityName);
            row.Cells.Add("Copy Of Record");
            row.Cells.Add(industryName);

            // Footer 
            Table footerTable = new Table();
            pdfPage.Footer = new HeaderFooter();
            pdfPage.Footer.Margin.Left = headerMargin.Left;
            pdfPage.Footer.Margin.Right = headerMargin.Right;
            pdfPage.Footer.Paragraphs.Add(footerTable);

            footerTable.ColumnWidths = "33.3% 33.3% 33.3%";
            footerTable.DefaultCellPadding = new MarginInfo(2, 3, 2, 3);
            footerTable.Broken = TableBroken.None;
            footerTable.Margin.Bottom = -5f;
            footerTable.SetColumnTextState(0, leftHeaderFooterTextState);
            footerTable.SetColumnTextState(1, centerHeaderFooterTextState);
            footerTable.SetColumnTextState(2, rightHeaderFooterTextState);

            TextFragment text = new TextFragment("Page: ($p of $P )");
            text.TextState.FontSize = 8;

            row = footerTable.Rows.Add();
            row.Cells.Add(reportName);
            var cell = row.Cells.Add("");
            cell.Paragraphs.Add(text);
            row.Cells.Add(submittedDateTimeString);
        }
    }
}
