using System.Text;

namespace CommonWebAPI.Extensions;

public static class CommonExtensions
{
    public static async Task<byte[]> ToByteArrayAsync(this IFormFile file, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public static string ToImageUrl(this string base64Data)
    {
        var bytes = Convert.FromBase64String(base64Data);
        return Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
    }

    public static string ToDecodeURIComponent(this string urlData)
    {
        return Uri.UnescapeDataString(urlData);
    }
}
