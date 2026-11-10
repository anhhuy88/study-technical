using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using CommonWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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

        var form = await Request.ReadFormAsync();
        var files = form.Files;
        var fileDataRes = _dbContext.Set<FileData>();
        var hostData = $"https://{Request.Host}";
        var fileIds = new List<string>();

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

        var provider = new FileExtensionContentTypeProvider();
        string contentType;
        foreach (var file in files)
        {
            if (!provider.TryGetContentType(file.FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            var fileData = new FileData
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                ContentType = contentType,
                Size = file.Length,
                FileType = fileType,
                CreatedDate = DateTime.Now
            };

            fileData.Data = await file.ToByteArrayAsync();

            await fileDataRes.AddAsync(fileData);

            fileIds.Add(fileData.Id);
        }

        await _dbContext.SaveChangesAsync();
        var links = fileIds.Select(id => $"{hostData}/api/filedata/download/{id}").ToList();
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
        var linkUrl = await _uploadService.SaveFileDataAsync(model);

        if (!linkUrl.StartsWith("http"))
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