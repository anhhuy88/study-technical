namespace EfCoreSamples.Domains;

public class FileData
{
    public string Id { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime CreatedDate { get; set; }
}
