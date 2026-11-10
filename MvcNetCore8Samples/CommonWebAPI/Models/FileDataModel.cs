using CommonWebAPI.Domain;
using Microsoft.AspNetCore.StaticFiles;

namespace CommonWebAPI.Models;
public class FileDataModel
{
    public string Base64Data { get; set; }
    public string FileName { get; set; }
    public FileType FileType { get; set; } = FileType.Normal;
    public string ContentType
    {
        get
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType = null;
            if (!provider.TryGetContentType(FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
