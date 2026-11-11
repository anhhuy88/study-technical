using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommonWebAPI.Controllers;

[Route("api/[controller]")]
public class FileDataController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    private readonly ILogger _logger;

    private readonly IUploadService _uploadService;

    private readonly IImageProxyService _imageProxy;

    public FileDataController(AppDbContext dbContext,
        ILoggerFactory loggerFactory,
        IUploadService uploadService,
        IImageProxyService imageProxy)
    {
        _uploadService = uploadService;
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<FileDataController>();
        _imageProxy = imageProxy;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsync([FromQuery] FileType fileType = FileType.Normal)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest();
        }

        var cancellationToken = HttpContext.RequestAborted;
        var form = await Request.ReadFormAsync(cancellationToken);
        var files = form.Files;
        var hostData = $"https://{Request.Host}";
        var images = new List<ImageUpload>();

        // check file max 2MB
        var maxFileSize = 2 * 1024 * 1024;
        var isInvalid = files.Any(f => f.Length > maxFileSize);
        if (isInvalid)
        {
            _logger.LogInformation("File size is too large (2MB).");
            return BadRequest("File size is too large (2MB).");
        }

        if (files.Where(f => f.Length == 0).Any())
        {
            _logger.LogInformation("File size is 0.");
            return BadRequest("File size is 0.");
        }

        foreach (var file in files)
        {
            images.Add(new ImageUpload(await file.ToByteArrayAsync(cancellationToken), file.FileName, fileType));
        }

        IReadOnlyList<UploadResult> results;
        try
        {
            results = await _uploadService.SaveImagesAsync(images, cancellationToken);
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Invalid image upload.");
            return BadRequest(ex.Message);
        }

        var links = results.Select(result => result.IsExternal
            ? result.Value
            : $"{hostData}/api/filedata/download/{result.Value}").ToList();
        return Ok(links);
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadAsync(string id)
    {
        var fileData = await _dbContext.Set<FileData>().FindAsync(id);

        if (fileData == null)
        {
            return NotFound();
        }

        return File(fileData.Data, fileData.ContentType, fileData.FileName);
    }

    [HttpPost("upload2")]
    public async Task<IActionResult> Upload2Async([FromBody] FileDataModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.FileName) || string.IsNullOrWhiteSpace(model.Base64Data))
        {
            return BadRequest("File name and Base64 image data are required.");
        }

        UploadResult uploadResult;
        try
        {
            uploadResult = await _uploadService.SaveFileDataAsync(model, HttpContext.RequestAborted);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid Base64 image upload.");
            return BadRequest("Invalid Base64 image data.");
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Invalid image upload.");
            return BadRequest(ex.Message);
        }

        var linkUrl = uploadResult.Value;
        if (!uploadResult.IsExternal)
        {
            var hostData = $"https://{Request.Host}";
            linkUrl = $"{hostData}/api/filedata/download/{linkUrl}";
        }
        return Ok(linkUrl);
    }

    [HttpGet("image/{base64Data}")]
    public async Task<IActionResult> GetImageAsync(string base64Data)
    {
        // javascript:var test = unescape(encodeURIComponent(data)); base64Data = btoa(test);

        string imgUrl;
        try
        {
            imgUrl = base64Data.ToDecodeURIComponent().ToImageUrl();
        }
        catch (FormatException)
        {
            return BadRequest();
        }

        var result = await _imageProxy.GetAsync(imgUrl, HttpContext.RequestAborted);
        switch (result.Status)
        {
            case ImageFetchStatus.Ok:
                Response.Headers.CacheControl = "public, max-age=2592000";
                return PhysicalFile(result.FilePath, result.ContentType);
            case ImageFetchStatus.Timeout:
                return StatusCode(StatusCodes.Status504GatewayTimeout);
            case ImageFetchStatus.Busy:
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            case ImageFetchStatus.Failed:
                return StatusCode(StatusCodes.Status502BadGateway);
            default:
                return NotFound();
        }
    }

}