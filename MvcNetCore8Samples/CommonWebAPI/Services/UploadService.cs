using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using ImageMagick;
using Newtonsoft.Json;

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

    private async Task<string> SaveToDbAsync(FileDataModel model)
    {
        var contentType = model.ContentType;
        byte[] bytes = model.ToImageBytes();

        var fileData = new FileData
        {
            Id = Guid.NewGuid().ToString(),
            FileName = model.FileName,
            ContentType = contentType,
            Size = bytes.Length,
            CreatedDate = DateTime.Now,
            Data = bytes
        };

        _dbContext.Set<FileData>().Add(fileData);

        await _dbContext.SaveChangesAsync();

        return fileData.Id;
    }

    private async Task<string> UploadImgBBAsync(FileDataModel model)
    {
        string result = null;
        try
        {
            var imgBBKey = _configuration.GetValue<string>("ImgBBKEY");
            var http = _httpFactory.CreateClient(VARIABLES.HTTP_CLIENT_NAME);
            var base64ToBytes = Convert.FromBase64String(model.Base64Data);
            using var form = new MultipartFormDataContent();
            using var imageContent = new ByteArrayContent(base64ToBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            form.Add(imageContent, "image", model.FileName);
            var res = await http.PostAsync($"https://api.imgbb.com/1/upload?key={imgBBKey}", form);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resData = JsonConvert.DeserializeObject<dynamic>(await res.Content.ReadAsStringAsync());
                result = resData.data.url;
            }
            else
            {
                _logger.LogError($"UploadImgBB: {res.StatusCode}. ${await res.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadImgBB");
        }

        return result;
    }

    public async Task<string> SaveFileDataAsync(FileDataModel model)
    {
        uint maxW = 1024;
        uint maxH = 1024;
        model.Base64Data = model.ToResizeBase64Image(maxW, maxH);
        string result = await UploadImgBBAsync(model);

        if (string.IsNullOrEmpty(result))
            result = await SaveToDbAsync(model);

        return result;
    }
}
