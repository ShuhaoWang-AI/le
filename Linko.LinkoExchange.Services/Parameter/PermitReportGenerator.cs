using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Services.Parameter
{
	internal class PermitReportGenerator
	{
		#region static fields and constants

		private const string ReportFootLeft = "LinkoExchange";

		#endregion

		#region fields

		private readonly Document _pdfDocument;
		private readonly Page _pdfPage;
		private readonly PermitLimitReport _permitLimitReport;
		private readonly TextState _sectionTextSize10 = new TextState(fontFamily:"Arial", bold:false, italic:false);
		private readonly TextState _sectionTitleBoldSize12 = new TextState(fontFamily:"Arial", bold:true, italic:false);
		private readonly ITimeZoneService _timeZoneService;
		private readonly OrganizationRegulatoryProgramDto _organizationRegulatoryProgram;
		#endregion

		#region constructors and destructor

		public PermitReportGenerator(OrganizationRegulatoryProgramDto organizationRegulatoryProgram,
		                             OrganizationRegulatoryProgramDto authorityRegulatoryProgramDto,
		                             List<ParameterLimitsByMonitoringPoint> parameterLimitsByMonitoringPoint,
									 ITimeZoneService timeZoneService
									 )
		{
			var pdflicense = new License();
			pdflicense.SetLicense(licenseName:@"Aspose.Pdf.lic");
			pdflicense.Embedded = true;

			_sectionTitleBoldSize12.FontSize = 12;
			_sectionTitleBoldSize12.FontStyle = FontStyles.Bold;
			_sectionTextSize10.FontSize = 10;

			_pdfDocument = new Document();
			_pdfPage = _pdfDocument.Pages.Add();

			if (organizationRegulatoryProgram == null)
			{
				throw new NullReferenceException(message:"organizationRegulatoryProgram");
			}

			if (authorityRegulatoryProgramDto == null)
			{
				throw new NullReferenceException(message:"authorityRegulatoryProgramDto");
			}

			_timeZoneService = timeZoneService;
			_organizationRegulatoryProgram = organizationRegulatoryProgram; 

			_permitLimitReport = new PermitLimitReport(parammeterByMonitoringPoints:parameterLimitsByMonitoringPoint,
			                                           orp:organizationRegulatoryProgram,
			                                           authorityOrp:authorityRegulatoryProgramDto);
		}

		#endregion

		public byte[] CreateDischargePermitLimitPdf(out string industryNumber)
		{
			_pdfPage.SetPageSize(width:PageSize.PageLetter.Width, height:PageSize.PageLetter.Height);
			_pdfPage.PageInfo.IsLandscape = true;
			_pdfPage.PageInfo.Margin.Top = 20;
			_pdfPage.PageInfo.Margin.Left = 10;
			_pdfPage.PageInfo.Margin.Bottom = 20;
			_pdfPage.PageInfo.Margin.Right = 10;

			// header and footer 
			var authorityName = _permitLimitReport.AuthorityName;
			var companyName = _permitLimitReport.CompanyName;

			//TODO: footerTimeStamp = ? need to get localized data time? 
			var timeStamp = _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(DateTime.Now.ToUniversalTime(), _organizationRegulatoryProgram.OrganizationRegulatoryProgramId);

			var footerTimeStamp = timeStamp.ToString(format:"MMMM dd, yyyy hh:mm tt").ToUpper();
			var reportName = _permitLimitReport.ReportName;

			HeaderFooterTable(pdfPage:_pdfPage, authorityName:authorityName, reportName:reportName, companyName:companyName, footerTimeStamp:footerTimeStamp);

			// Add report info part  
			ReportInfoTable();

			// Add limits group by monitoring points
			foreach (var monitoringPoint in _permitLimitReport.MonitoringPoints)
			{
				PrintMonitoringPointLimits(monitoringPoint:monitoringPoint);
			}

			// Add Note table
			PrintNotePart();

			var mStream = new MemoryStream();
			_pdfDocument.Save(outputStream:mStream, format:SaveFormat.Pdf);
			industryNumber = _permitLimitReport.IndustryNumber; 
			return mStream.ToArray();
		}

		private void PrintNotePart()
		{
			var leftTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
			                    {
				                    FontSize = 8,
				                    HorizontalAlignment = HorizontalAlignment.Left
			                    };

			var tableOrder = new BorderInfo(borderSide:BorderSide.All, borderWidth:0.0F);

			var noteTable = new Table
			                {
				                Border = tableOrder,
				                DefaultCellBorder = tableOrder,
				                DefaultCellPadding = new MarginInfo(left:2, bottom:2, right:2, top:2),
				                Margin = {Top = 20f}
			                };

			_pdfPage.Paragraphs.Add(paragraph:noteTable);

			noteTable.ColumnWidths = "100%";

			// Monitoring Point row 
			var row = noteTable.Rows.Add();
			row.DefaultCellTextState = leftTextSize8;
			row.Cells.Add(text:"-- No Limit defined");

			row = noteTable.Rows.Add();
			row.DefaultCellTextState = leftTextSize8;
			row.Cells.Add(text:"* F = Four Day, M = Monthly ");

			noteTable.Rows.Add();
		}

		private void PrintMonitoringPointLimits(LimitReportMonitoringPoint monitoringPoint)
		{
			var tableBorder = new BorderInfo(borderSide:BorderSide.All, borderWidth:0.1F);

			//Limit table  
			var permitLimitTable = new Table
			                       {
				                       Border = tableBorder,
				                       DefaultCellBorder = tableBorder,
				                       DefaultCellPadding = new MarginInfo(left:5, bottom:2, right:2, top:2),
				                       Margin = {Top = 20},
				                       IsBroken = true,
				                       RepeatingRowsCount = 2
			                       };

			_pdfPage.Paragraphs.Add(paragraph:permitLimitTable);

			// 10 columns
			// Parameter, EffectiveDate, ExpirationDate, ConcentrationDailyLimit, ConcentrationAverageLimit
			// ConcentrationUnits, Average Type, MassDailyLimit, MassEverageLimit, MassUnits 
			permitLimitTable.ColumnWidths = "20% 9% 9% 9% 9% 9% 6% 10% 10% 9%";

			var monitoringPointName = monitoringPoint.MonitoringPointName;

			// table header text font
			var leftTextBoldSize9 = new TextState(fontFamily:"Arial", bold:false, italic:false)
			                        {
				                        FontSize = 9,
				                        FontStyle = FontStyles.Bold,
				                        HorizontalAlignment = HorizontalAlignment.Left
			                        };

			var centerTextBoldSize9 = new TextState(fontFamily:"Arial", bold:false, italic:false)
			                          {
				                          FontSize = 9,
				                          FontStyle = FontStyles.Bold,
				                          HorizontalAlignment = HorizontalAlignment.Center
			                          };

			// cell text font
			var centerTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
			                      {
				                      FontSize = 8,
				                      HorizontalAlignment = HorizontalAlignment.Center
			                      };

			// cell text font
			var leftTextSize8 = new TextState(fontFamily:"Arial", bold:false, italic:false)
			                    {
				                    FontSize = 8,
				                    HorizontalAlignment = HorizontalAlignment.Left
			                    };

			// Monitoring Point row 
			var row = permitLimitTable.Rows.Add();
			row.DefaultCellTextState = leftTextBoldSize9;
			row.BackgroundColor = Color.LightGray;
			row.Border = tableBorder;
			var cell = row.Cells.Add(text:$"Monitoring Point:{monitoringPointName}");
			cell.ColSpan = 10;
			cell.Margin = new MarginInfo(bottom:10.0, left:5.0, right:5.0, top:5.0);

			row = permitLimitTable.Rows.Add();
			row.BackgroundColor = Color.LightGray;

			row.Cells.Add(text:"Parameter", ts:leftTextBoldSize9);
			row.Cells.Add(text:"Effective Date", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Expiration Date", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Concentration Daily Limit", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Concentration Average Limit", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Concentration Units", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Average Type*", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Mass Daily Limit", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Mass Average Limit", ts:centerTextBoldSize9);
			row.Cells.Add(text:"Mass Units", ts:centerTextBoldSize9);

			var limits = monitoringPoint.LimitReportParameterLimits.OrderBy(i => i.ParameterName).ThenBy(j => j.EffectiveDate);

			foreach (var limit in limits)
			{
				row = permitLimitTable.Rows.Add();

				row.Cells.Add(text:limit.ParameterName, ts:leftTextSize8);

				row.Cells.Add(text:limit.EffectiveDate.ToString(format:"MM/dd/yyyy").ToLower(), ts:centerTextSize8);
				row.Cells.Add(text:limit.ExpirationDate.ToString(format:"MM/dd/yyyy").ToLower(), ts:centerTextSize8);

				row.Cells.Add(text:limit.ConcentrationDailyLimit.GetValueOrEmptyString(), ts:centerTextSize8);
				row.Cells.Add(text:limit.ConcentrationAverageLimit.GetValueOrEmptyString(), ts:centerTextSize8);

				row.Cells.Add(text:limit.ConcentrationUnits.GetValueOrEmptyString(), ts:centerTextSize8);
				row.Cells.Add(text:limit.AverageType.GetValueOrEmptyString(), ts:centerTextSize8);
				row.Cells.Add(text:limit.MassDailyLimit.GetValueOrEmptyString(), ts:centerTextSize8);
				row.Cells.Add(text:limit.MassAverageLimit.GetValueOrEmptyString(), ts:centerTextSize8);

				row.Cells.Add(text:limit.MassUnits.GetValueOrEmptyString(), ts:centerTextSize8);
			}
		}

		private void ReportInfoTable()
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

			_pdfPage.Paragraphs.Add(paragraph:reportInfoTable);
			reportInfoTable.ColumnWidths = "15% 85%";

			reportInfoTable.SetColumnTextState(colNumber:0, textState:boldTextStateSize10);
			reportInfoTable.SetColumnTextState(colNumber:1, textState:textStateSize10);

			//--------------------------------Row 1: Report
			var row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"Report:", ts:boldTextStateSize10);
			row.Cells.Add(text:_permitLimitReport.ReportName, ts:boldTextStateSize10);

			//--------------------------------Row 2
			// empty row
			row = reportInfoTable.Rows.Add();
			row.MinRowHeight = 10;

			//--------------------------------Row 3: Industry Name
			row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"Industry Name:", ts:boldTextStateSize10);
			row.Cells.Add(text:_permitLimitReport.IndustryName, ts:textStateSize10);

			//--------------------------------Row 4: Industry Number
			row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"Industry Number:", ts:boldTextStateSize10);
			row.Cells.Add(text:_permitLimitReport.IndustryNumber, ts:textStateSize10);

			//--------------------------------Row 5: Address
			row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"Address:", ts:boldTextStateSize10);

			var addressLine1 = _permitLimitReport.AddressLine1.GetValueOrEmptyString();
			var addressLine2 = _permitLimitReport.AddressLine2.GetValueOrEmptyString();
			var cityName = _permitLimitReport.City.GetValueOrEmptyString();
			var jurisdictionName = _permitLimitReport.State.GetValueOrEmptyString();
			var zipCode = _permitLimitReport.Zip.GetValueOrEmptyString();

			var address1 = string.IsNullOrWhiteSpace(value:addressLine2) ? $"{addressLine1}," : $"{addressLine1} {addressLine2},";

			row.Cells.Add(text:address1, ts:textStateSize10);

			//--------------------------------Row 6: Address
			// Add another row for city, jurisdiction and zip code 
			var address2 = $"{cityName}, {jurisdictionName} {zipCode}";
			row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"");
			row.Cells.Add(text:address2);

			//--------------------------------Row 7: Report
			row = reportInfoTable.Rows.Add();
			row.Cells.Add(text:"Authority:", ts:boldTextStateSize10);
			row.Cells.Add(text:_permitLimitReport.AuthorityName, ts:boldTextStateSize10);
		}

		private static void HeaderFooterTable(Page pdfPage, string authorityName, string reportName, string companyName, string footerTimeStamp)
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
			row.Cells.Add(text:reportName);
			row.Cells.Add(text:companyName);

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
			row.Cells.Add(text:ReportFootLeft);
			var cell = row.Cells.Add(text:"");
			cell.Paragraphs.Add(paragraph:text);
			row.Cells.Add(text:footerTimeStamp);
		}
	}
}