using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonWebAPI.Services;

public class UploadService : IUploadService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;

    public UploadService(ILoggerFactory loggerFactory,
        AppDbContext dbContext,
        IHttpClientFactory httpFactory,
        IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<UploadService>();
        _dbContext = dbContext;
        _httpFactory = httpFactory;
        _configuration = configuration;
    }

    private async Task<string> TryUploadImgBBAsync(byte[] data, string fileName, CancellationToken cancellationToken)
    {
        var imgBBKey = _configuration.GetValue<string>("ImgBBKEY");
        if (string.IsNullOrWhiteSpace(imgBBKey))
        {
            _logger.LogWarning("ImgBB API key is not configured; saving image to the database.");
            return null;
        }

        try
        {
            var http = _httpFactory.CreateClient(VARIABLES.IMGBB_HTTP_CLIENT_NAME);
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(imgBBKey), "key");
            form.Add(new ByteArrayContent(data), "image", fileName);
            using var res = await http.PostAsync(VARIABLES.IMGBB_UPLOAD_URL, form, cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("ImgBB upload returned HTTP {StatusCode}; saving image to the database.", (int)res.StatusCode);
                return null;
            }

            var json = JObject.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
            var dataObject = json["data"] as JObject;
            var url = dataObject?["url"]?.Type == JTokenType.String ? (string)dataObject["url"] : null;
            if (!string.IsNullOrEmpty(url))
            {
                return url;
            }

            _logger.LogWarning("ImgBB returned an invalid upload response; saving image to the database.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "ImgBB upload failed; saving image to the database.");
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "ImgBB upload response could not be read; saving image to the database.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "ImgBB returned invalid JSON; saving image to the database.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ImgBB upload timed out; saving image to the database.");
        }

        return null;
    }

    public async Task<UploadResult> SaveFileDataAsync(FileDataModel model, CancellationToken cancellationToken)
    {
        var original = Convert.FromBase64String(model.Base64Data);
        var images = new[] { new ImageUpload(original, model.FileName, model.FileType) };
        var results = await SaveImagesAsync(images, cancellationToken);
        return results[0];
    }

    public async Task<IReadOnlyList<UploadResult>> SaveImagesAsync(IReadOnlyList<ImageUpload> images, CancellationToken cancellationToken)
    {
        var prepared = new List<PreparedFileUpload>(images.Count);
        foreach (var image in images)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var optimized = ImageOptimizer.Optimize(image.Data, image.FileName, image.FileType);
            prepared.Add(new PreparedFileUpload(optimized, image.Data, image.FileType));
        }

        var results = new List<UploadResult>(prepared.Count);
        var externalUploadCount = 0;
        foreach (var item in prepared)
        {
            string url = null;
            if (item.FileType != FileType.Logo)
            {
                url = await TryUploadImgBBAsync(item.OriginalData, item.Image.FileName, cancellationToken);
            }

            if (url != null)
            {
                externalUploadCount++;
                results.Add(new UploadResult(url, true));
                continue;
            }

            var file = new FileData
            {
                Id = Guid.NewGuid().ToString(),
                FileName = item.Image.FileName,
                ContentType = item.Image.ContentType,
                Size = item.Image.Data.Length,
                FileType = item.FileType,
                CreatedDate = DateTime.Now,
                Data = item.Image.Data
            };
            _dbContext.Set<FileData>().Add(file);
            results.Add(new UploadResult(file.Id, false));
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database save failed after {ExternalUploadCount} external uploads.", externalUploadCount);
            throw;
        }

        return results;
    }
}
