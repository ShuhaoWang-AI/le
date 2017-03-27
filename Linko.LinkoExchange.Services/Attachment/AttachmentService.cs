using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Services.Attachment
{
    public class AttachmentService : IAttachmentService
    {
        private readonly DbContext _dbContext;
        private readonly IMapHelper _mapHelper;

        private readonly string[] _validExtensions =
        {
            "docx", "doc", "xls", "xlsx", "pdf", "tif",
            "jpg", "jpeg", "bmp", "png", "txt", "csv"
        };

        public AttachmentService(
            DbContext dbContext,
            IMapHelper mapHelper)
        {
            _dbContext = dbContext;
            _mapHelper = mapHelper;
        }

        public List<string> GetValidAttachmentFileExtensions()
        {
            return new List<string>(_validExtensions);
        }

        public bool IsValidFileExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext))
            {
                return false;
            }

            if (ext.StartsWith("."))
            {
                ext = ext.Substring(1);
            }

            return _validExtensions.Contains(ext);
        }

        public IList<string> GetUserAttachmentFiles()
        {
            throw new NotImplementedException();
        }
    }
}