using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Models;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CommonWebAPI.Controllers;

[Route("api/[controller]")]
public class FileDataController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;

    public FileDataController(AppDbContext dbContext,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<FileDataController>();
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
        var hostData = $"https://{Request.Host}";
        var contentType = model.ContentType;
        uint maxW = 1024;
        uint maxH = 1024;
        byte[] bytes = null;
        //var bytes = Convert.FromBase64String(model.Base64Data);
        //using var ms = new MemoryStream(bytes);
        //using var image = FreeImageBitmap.FromStream(ms);
        //var w = image.Width;
        //var h = image.Height;

        //if (w > maxW || h > maxH)
        //{
        //    var ratio = Math.Min(maxW / (double)w, maxH / (double)h);
        //    w = (int)(w * ratio);
        //    h = (int)(h * ratio);
        //    image.Rescale(w, h, FREE_IMAGE_FILTER.FILTER_BICUBIC);
        //    using var ms2 = new MemoryStream();
        //    image.Save(ms2, FREE_IMAGE_FORMAT.FIF_JPEG, FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYNORMAL);
        //    bytes = ms2.ToArray();
        //    contentType = "image/jpeg";
        //}

        // for using ImageMagick
        using var image = MagickImage.FromBase64(model.Base64Data);
        if (image.Width > maxW || image.Height > maxH)
        {
            image.Resize(maxW, maxH);
        }

        bytes = image.ToByteArray();

        var fileData = new FileData
        {
            Id = Guid.NewGuid().ToString(),
            FileName = model.FileName,
            ContentType = contentType,
            Size = bytes.Length,
            CreatedDate = DateTime.Now,
            Data = bytes
        };

        await _dbContext.Set<FileData>().AddAsync(fileData);

        await _dbContext.SaveChangesAsync();

        var linkUrl = $"{hostData}/api/filedata/download/{fileData.Id}";
        return Ok(linkUrl);
    }

}