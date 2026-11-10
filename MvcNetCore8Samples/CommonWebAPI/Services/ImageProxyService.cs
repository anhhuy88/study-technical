using System.Security.Cryptography;
using System.Text;
using CommonWebAPI.Extensions;
using Microsoft.AspNetCore.StaticFiles;

namespace CommonWebAPI.Services;

public enum ImageFetchStatus
{
    Ok,
    NotFound,
    Timeout,
    Busy,
    Failed
}

public record ImageFetchResult(ImageFetchStatus Status, string FilePath = null, string ContentType = null);

public interface IImageProxyService
{
    Task<ImageFetchResult> GetAsync(string imgUrl, CancellationToken token);
}

public class ImageProxyService : IImageProxyService
{
    private const int MaxConcurrentDownloads = 4;
    private static readonly TimeSpan WaitSlotTimeout = TimeSpan.FromSeconds(10);
    private static readonly SemaphoreSlim Slots = new(MaxConcurrentDownloads, MaxConcurrentDownloads);

    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ImageProxyService> _logger;
    private readonly string _cacheFolder;
    private readonly FileExtensionContentTypeProvider _typeProvider = new();

    public ImageProxyService(IHttpClientFactory httpFactory,
        ILogger<ImageProxyService> logger,
        ImageCacheOptions options)
    {
        _httpFactory = httpFactory;
        _logger = logger;
        _cacheFolder = options.Folder;
        Directory.CreateDirectory(_cacheFolder);
    }

    public async Task<ImageFetchResult> GetAsync(string imgUrl, CancellationToken token)
    {
        var extension = GetSafeExtension(imgUrl);
        var contentType = _typeProvider.TryGetContentType($"x{extension}", out var type)
            ? type
            : VARIABLES.APPLICATION_OCTET_STREAM;

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(imgUrl)));
        var cachePath = Path.Combine(_cacheFolder, hash + extension);

        if (File.Exists(cachePath))
        {
            return new ImageFetchResult(ImageFetchStatus.Ok, cachePath, contentType);
        }

        if (!await Slots.WaitAsync(WaitSlotTimeout, token))
        {
            _logger.LogWarning("Image proxy is busy, skip {Url}", imgUrl);
            return new ImageFetchResult(ImageFetchStatus.Busy);
        }

        try
        {
            // Another request may have cached it while waiting for a slot.
            if (File.Exists(cachePath))
            {
                return new ImageFetchResult(ImageFetchStatus.Ok, cachePath, contentType);
            }

            var http = _httpFactory.CreateClient(VARIABLES.IMAGE_HTTP_CLIENT_NAME);
            using var response = await http.GetAsync(imgUrl, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
            {
                return new ImageFetchResult(ImageFetchStatus.NotFound);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(token);
            if (bytes.Length == 0)
            {
                return new ImageFetchResult(ImageFetchStatus.NotFound);
            }

            var tempPath = $"{cachePath}.{Guid.NewGuid():N}.tmp";
            await File.WriteAllBytesAsync(tempPath, bytes, token);
            File.Move(tempPath, cachePath, overwrite: true);

            return new ImageFetchResult(ImageFetchStatus.Ok, cachePath, contentType);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            // HttpClient.Timeout elapsed.
            _logger.LogWarning("Timeout while fetching image {Url}", imgUrl);
            return new ImageFetchResult(ImageFetchStatus.Timeout);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch image {Url}", imgUrl);
            return new ImageFetchResult(ImageFetchStatus.Failed);
        }
        finally
        {
            Slots.Release();
        }
    }

    private static string GetSafeExtension(string imgUrl)
    {
        var path = Uri.TryCreate(imgUrl, UriKind.Absolute, out var uri) ? uri.AbsolutePath : imgUrl;
        var ext = Path.GetExtension(path);
        return ext.Length is > 1 and <= 6 && ext.Skip(1).All(char.IsLetterOrDigit)
            ? ext.ToLowerInvariant()
            : ".img";
    }
}

public record ImageCacheOptions(string Folder);
