using CommonWebAPI.Models;
using ImageMagick;

namespace CommonWebAPI.Extensions;

public static class CommonExtensions
{
    public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static string ToResizeBase64Image(this FileDataModel model, uint maxW, uint maxH)
    {
        using var image = MagickImage.FromBase64(model.Base64Data);
        if (image.Width > maxW || image.Height > maxH)
        {
            image.Resize(maxW, maxH);
        }
        return image.ToBase64();
    }

    public static byte[] ToResizeImageBytes(this FileDataModel model, uint maxW, uint maxH)
    {
        using var image = MagickImage.FromBase64(model.Base64Data);
        if (image.Width > maxW || image.Height > maxH)
        {
            image.Resize(maxW, maxH);
        }
        return image.ToByteArray();
    }

    public static byte[] ToImageBytes(this FileDataModel model)
    {
        using var image = MagickImage.FromBase64(model.Base64Data);
        return image.ToByteArray();
    }
}
