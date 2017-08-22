namespace Linko.LinkoExchange.Services.Dto
{
    public abstract class CorFileDtoBase
    {
        #region public properties

        public string FileName { get; set; }
        public byte[] FileData { get; set; }

        #endregion
    }
}