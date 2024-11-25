namespace CommonWebAPI.Extensions;

public static class CommonExtensions
{
    public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
