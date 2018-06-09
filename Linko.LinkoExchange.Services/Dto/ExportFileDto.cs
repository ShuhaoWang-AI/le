using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ExportFileDto
    {
        #region public properties
        
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }

        #endregion
    }
}