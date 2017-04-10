using System.IO;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.CopyOrRecord;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class CorController : Controller
    {
        private ICopyOfRecordService _copyOfRecordService;

        public CorController(ICopyOfRecordService copyOfRecordService)
        {
            _copyOfRecordService = copyOfRecordService;
        }

        [Route("Core/{reportPackageId:int}")]
        public FileResult DownloadCor(int reportPackageId)
        {
            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackageId(reportPackageId: reportPackageId);
            var contentType = "application/zip";
            var fileDownloadName = copyOfRecordDto.DownloadFileName;
            var dataStream = new MemoryStream(copyOfRecordDto.Data) { Position = 0 };
            return File(dataStream, contentType, fileDownloadName);
        }
    }
}