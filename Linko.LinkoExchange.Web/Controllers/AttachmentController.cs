using System;
using System.Collections.Generic;
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


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(AttachmentModel model, HttpPostedFileBase upload)
        {
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

                    var id = _fileStoreService.CreateFileStore(fileStoreDto);
                }
            }

            return View();
        }
    }
}