using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Web.ViewModels.Attachment;

namespace Linko.LinkoExchange.Web.Controllers
{
    //Demo controller for attachment upload and download
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

        [Route(template: "Attachment/FileStoreInfo/{fileStoreId:int}")]
        public ActionResult FileStoreInfo(int fileStoreId)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId);
            var model = new AttachmentModel
            {
                FileName = fileStore.Name,
                OriginFileName = fileStore.OriginalFileName,
                AttachmentTypeName = fileStore.ReportElementTypeName,
                Description = fileStore.Description,
                FileStoreId = fileStore.FileStoreId.Value
            };


            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Route(template: "Attachment/{fileStoreId}/Update")]
        public ActionResult Update(AttachmentModel model, int fileStoreId)
        {
            var fileStoreDto = _fileStoreService.GetFileStoreById(fileStoreId);

            fileStoreDto.ReportElementTypeName = model.AttachmentTypeName;
            fileStoreDto.Description = model.Description;

            _fileStoreService.UpdateFileStore(fileStoreDto);

            var param = new RouteValueDictionary();
            param.Add("fileStoreId", fileStoreId);
            return RedirectToAction("FileStoreInfo", param);
        }

        [Route(template: "Attachment/Download/{fileStoreId:int}")]
        public FileResult Download(int fileStoreId)
        {
            var fileStore = _fileStoreService.GetFileStoreById(fileStoreId, includingFileData: true);
            var fileDownloadName = fileStore.OriginalFileName;
            var extension = fileDownloadName.Substring(fileDownloadName.IndexOf(".") + 1);
            var contentType = $"application/${extension}";

            var fileStream = new MemoryStream(fileStore.Data) { Position = 0 };
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