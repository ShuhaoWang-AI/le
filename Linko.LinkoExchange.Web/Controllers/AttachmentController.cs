using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Attachment;

namespace Linko.LinkoExchange.Web.Controllers
{
    public partial class AttachmentController : Controller
    {
        private IFileStoreService _fileStoreService;

        public AttachmentController(IFileStoreService fileStoreService)
        {
            _fileStoreService = fileStoreService;
        }

        // GET: Attachment
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult FileStores()
        {
            var fileStores = _fileStoreService.GetFileStores();
            return View(fileStores);
        }

        [Route(template: "Attachment/Download/{fileStoreId:int}")]
        public FileResult Download(int fileStoreId)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId);
            var fileDownloadName = fileStore.OriginalFileName;
            var extension = fileDownloadName.Substring(fileDownloadName.IndexOf(".") + 1);
            var contentType = $"application/${extension}";

            var fileStream = new MemoryStream(fileStore.Data);
            fileStream.Position = 0;
            return File(fileStream, contentType, fileDownloadName);
        }

        [HttpPost]
        public ActionResult Create(AttachmentModel model, HttpPostedFileBase upload)
        {
            int fileStoreId = -1;

            if (upload != null && upload.ContentLength > 0)
            {
                using (var reader = new System.IO.BinaryReader(upload.InputStream))
                {
                    var content = reader.ReadBytes(upload.ContentLength);

                    var fileStoreDto = new FileStoreDto();
                    fileStoreDto.OriginalFileName = model.OriginFileName;
                    fileStoreDto.ReportElementTypeName = model.AttachmentTypeName;
                    fileStoreDto.Description = model.Description;
                    fileStoreDto.Data = content;
                    fileStoreDto.FileStoreId = null;

                    fileStoreId = _fileStoreService.CreateFileStore(fileStoreDto);
                }
            }

            return RedirectToAction("FileStores");
        }
    }
}