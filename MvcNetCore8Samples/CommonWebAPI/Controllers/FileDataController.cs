using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CommonWebAPI.Controllers;

[Route("api/[controller]")]
public class FileDataController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    private readonly ILogger _logger;

    private readonly IUploadService _uploadService;

    private readonly IHttpClientFactory _httpClientFactory;

    public FileDataController(AppDbContext dbContext,
        ILoggerFactory loggerFactory,
        IUploadService uploadService,
        IHttpClientFactory httpClientFactory)
    {
        _uploadService = uploadService;
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<FileDataController>();
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsync()
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

        var http = _httpClientFactory.CreateClient(VARIABLES.IMAGE_HTTP_CLIENT_NAME);
        var imgUrl = base64Data.ToImageUrl();
        var response = await http.GetAsync(imgUrl);
        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }
        var exension = Path.GetExtension(imgUrl);
        var name = Path.GetFileNameWithoutExtension(imgUrl);
        var contentType = new FileExtensionContentTypeProvider().TryGetContentType(exension, out var type) ? type : VARIABLES.APPLICATION_OCTET_STREAM;
        var bytes = await response.Content.ReadAsByteArrayAsync();
        if (bytes.Length == 0)
        {
            return NotFound();
        }
        return File(bytes, contentType);
    }

}