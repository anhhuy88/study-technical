using Microsoft.AspNetCore.StaticFiles;

namespace CommonWebAPI.Models;
public class FileDataModel
{
    public string Base64Data { get; set; }
    public string FileName { get; set; }
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
