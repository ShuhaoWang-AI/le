using System.IO;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Report;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class CorController : Controller
    {
        private IReportPackageService _reportPackageService;

        public CorController(IReportPackageService reportPackageService)
        {
            _reportPackageService = reportPackageService;
        }

        [Route("Core/{reportPackageId:int}")]
        public FileResult DownloadCor(int reportPackageId)
        {
            var copyOfRecordDto = _reportPackageService.GetCopyOfRecordByReportPackageId(reportPackageId: reportPackageId);
            var contentType = "application/zip";
            var fileDownloadName = copyOfRecordDto.DownloadFileName;
            var dataStream = new MemoryStream(copyOfRecordDto.Data) { Position = 0 };
            return File(dataStream, contentType, fileDownloadName);
        }

        [Route("Core/{reportPackageId:int}/verify")]
        public ViewResult VerifyCopyOfRecord(int reportPackageId)
        {
            var verifyResult = _reportPackageService.VerififyCopyOfRecord(reportPackageId);
            return View(verifyResult);
        }
    }
}