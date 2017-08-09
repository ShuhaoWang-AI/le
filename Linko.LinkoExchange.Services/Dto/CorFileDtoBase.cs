namespace Linko.LinkoExchange.Services.Dto
{
    public abstract class CorFileDtoBase
    {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }

    }
}