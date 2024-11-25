namespace CommonWebAPI.Domain;

public class FileData : BaseEntity
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
    public long Size { get; set; }
    public string Description { get; set; }
}
