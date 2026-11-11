using System.Net;
using CommonWebAPI.Controllers;
using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Models;
using CommonWebAPI.Services;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace CommonWebAPI.Tests;

public class ImgBBUploadTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BothEndpointsReturnExactImgBBImageUrl(bool multipart)
    {
        await using var fixture = await Fixture.CreateAsync();
        const string directUrl = "https://i.ibb.co:443/w04Prt6/c1f64245afb2.gif";
        fixture.Handler.Response = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                {"data":{"id":"2ndCYJK","url_viewer":"https://ibb.co/2ndCYJK",
                "url":"https://i.ibb.co:443/w04Prt6/c1f64245afb2.gif",
                "display_url":"https://i.ibb.co/thumbnail.gif",
                "image":{"url":"https://i.ibb.co/another.gif"}},
                "success":true,"status":200}
                """)
        };
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        var bytes = image.ToByteArray(MagickFormat.Png);

        if (multipart)
        {
            using var stream = new MemoryStream(bytes);
            fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
            fixture.Controller.Request.Form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
                new FormFileCollection { new FormFile(stream, 0, stream.Length, "files", "image.png") });
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync());
            Assert.Equal(directUrl, Assert.Single(Assert.IsType<List<string>>(response.Value)));
        }
        else
        {
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.Upload2Async(Model(bytes)));
            Assert.Equal(directUrl, response.Value);
        }

        Assert.Equal(1, fixture.Handler.Calls);
        Assert.Empty(await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BothEndpointsUseDatabaseWhenImgBBFails(bool multipart)
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Handler.Response = () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        using var image = new MagickImage(MagickColors.Red, 1800, 900);
        var bytes = image.ToByteArray(MagickFormat.Png);
        string url;

        if (multipart)
        {
            using var stream = new MemoryStream(bytes);
            fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
            fixture.Controller.Request.Form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
                new FormFileCollection { new FormFile(stream, 0, stream.Length, "files", "image.png") });
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync());
            url = Assert.Single(Assert.IsType<List<string>>(response.Value));
        }
        else
        {
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.Upload2Async(Model(bytes)));
            url = Assert.IsType<string>(response.Value);
        }

        var stored = await fixture.Db.Set<FileData>().AsNoTracking().SingleAsync();
        Assert.Equal($"https://localhost/api/filedata/download/{stored.Id}", url);
        using var decoded = new MagickImage(stored.Data);
        Assert.Equal(1600u, decoded.Width);
        Assert.Equal(1, fixture.Handler.Calls);
    }

    [Fact]
    public async Task Base64SuccessReturnsExternalUrlWithoutSavingImage()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 1800, 900);
        var original = image.ToByteArray(MagickFormat.Png);
        var model = Model(original);

        var response = await fixture.Controller.Upload2Async(model);

        Assert.Equal("https://i.ibb.co/example.png", Assert.IsType<OkObjectResult>(response).Value);
        Assert.Empty(await fixture.Db.Set<FileData>().ToListAsync());
        Assert.Equal(1, fixture.Handler.Calls);
        Assert.Equal(VARIABLES.IMGBB_HTTP_CLIENT_NAME, fixture.Factory.ClientName);
        Assert.Equal(VARIABLES.IMGBB_UPLOAD_URL, fixture.Handler.RequestUrl);
        Assert.Equal("test-key", fixture.Handler.Key);
        Assert.Equal(original, fixture.Handler.Image);
    }

    [Theory]
    [InlineData(429, "{}")]
    [InlineData(200, "not json")]
    [InlineData(200, "{\"success\":true,\"data\":42}")]
    [InlineData(200, "{\"success\":true,\"data\":{}}")]
    [InlineData(200, "{\"success\":true,\"data\":{\"url\":null}}")]
    [InlineData(200, "{\"success\":true,\"data\":{\"url\":\"\"}}")]
    public async Task FailedUploadFallsBackToOptimizedDatabaseImage(int statusCode, string body)
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Handler.Response = () => new HttpResponseMessage((HttpStatusCode)statusCode) { Content = new StringContent(body) };
        using var image = new MagickImage(MagickColors.Red, 1800, 900);

        var response = await fixture.Controller.Upload2Async(Model(image.ToByteArray(MagickFormat.Png)));

        var stored = await fixture.Db.Set<FileData>().SingleAsync();
        Assert.Equal($"https://localhost/api/filedata/download/{stored.Id}", Assert.IsType<OkObjectResult>(response).Value);
        using var saved = new MagickImage(stored.Data);
        Assert.Equal(1600u, saved.Width);
        Assert.Equal(stored.Data.Length, stored.Size);
    }

    [Theory]
    [InlineData(false, "http://i.ibb.co/example.png")]
    [InlineData(true, "http://i.ibb.co/example.png")]
    [InlineData(false, " ")]
    [InlineData(true, " ")]
    [InlineData(false, "not-an-absolute-url")]
    [InlineData(true, "not-an-absolute-url")]
    public async Task NonEmptyImgBBUrlIsReturnedUnchangedEvenWhenSuccessFlagIsFalse(bool multipart, string url)
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Handler.Response = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new { success = false, data = new { url } }))
        };
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        var bytes = image.ToByteArray(MagickFormat.Png);

        if (multipart)
        {
            using var stream = new MemoryStream(bytes);
            fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
            fixture.Controller.Request.Form = new FormCollection(
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
                new FormFileCollection { new FormFile(stream, 0, stream.Length, "files", "image.png") });
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync());
            Assert.Equal(url, Assert.Single(Assert.IsType<List<string>>(response.Value)));
        }
        else
        {
            var response = Assert.IsType<OkObjectResult>(await fixture.Controller.Upload2Async(Model(bytes)));
            Assert.Equal(url, response.Value);
        }

        Assert.Empty(await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NetworkFailureAndTimeoutFallBackToDatabase(bool timeout)
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Handler.Error = timeout ? new TaskCanceledException("timeout") : new HttpRequestException("network error");
        using var image = new MagickImage(MagickColors.Red, 10, 10);

        Assert.IsType<OkObjectResult>(await fixture.Controller.Upload2Async(Model(image.ToByteArray(MagickFormat.Png))));
        Assert.Single(await fixture.Db.Set<FileData>().ToListAsync());
    }

    [Fact]
    public async Task MissingKeyAndLogoSkipExternalUpload()
    {
        await using var missingKey = await Fixture.CreateAsync("");
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        var bytes = image.ToByteArray(MagickFormat.Png);
        Assert.IsType<OkObjectResult>(await missingKey.Controller.Upload2Async(Model(bytes)));
        Assert.Equal(0, missingKey.Handler.Calls);
        Assert.Single(await missingKey.Db.Set<FileData>().ToListAsync());

        await using var logo = await Fixture.CreateAsync();
        Assert.IsType<OkObjectResult>(await logo.Controller.Upload2Async(Model(bytes, FileType.Logo)));
        Assert.Equal(0, logo.Handler.Calls);
        Assert.Equal(FileType.Logo, (await logo.Db.Set<FileData>().SingleAsync()).FileType);
    }

    [Fact]
    public async Task MultipartPreservesOrderAndStoresOnlyFailedUpload()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Handler.Response = () => fixture.Handler.Calls == 1
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"success\":true,\"data\":{\"url\":\"https://i.ibb.co/example.png\"}}") }
            : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        using var image = new MagickImage(MagickColors.Red, 1800, 900);
        var bytes = image.ToByteArray(MagickFormat.Png);
        using var first = new MemoryStream(bytes);
        using var last = new MemoryStream(bytes);
        var files = new FormFileCollection
        {
            new FormFile(first, 0, first.Length, "files", "first.png"),
            new FormFile(last, 0, last.Length, "files", "last.png")
        };
        fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
        fixture.Controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        var response = Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync());

        var links = Assert.IsType<List<string>>(response.Value);
        var saved = await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync();
        Assert.Equal(2, fixture.Handler.Calls);
        Assert.Single(saved);
        Assert.Equal("https://i.ibb.co/example.png", links[0]);
        var fallback = Assert.Single(saved, f => f.FileName == "last.png");
        Assert.Equal($"https://localhost/api/filedata/download/{fallback.Id}", links[1]);
        using var decoded = new MagickImage(fallback.Data);
        Assert.Equal(1600u, decoded.Width);
    }

    [Theory]
    [InlineData("note.txt")]
    [InlineData("photo.png")]
    [InlineData("unknown.bin")]
    public async Task MultipartRejectsNonImageBeforeUploadingOrSavingOtherFiles(string fileName)
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        using var good = new MemoryStream(image.ToByteArray(MagickFormat.Png));
        using var bad = new MemoryStream("not image data"u8.ToArray());
        var files = new FormFileCollection
        {
            new FormFile(good, 0, good.Length, "files", "good.png"),
            new FormFile(bad, 0, bad.Length, "files", fileName)
        };
        fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
        fixture.Controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        var result = Assert.IsType<BadRequestObjectResult>(await fixture.Controller.UploadAsync());

        Assert.Contains("images are supported", Assert.IsType<string>(result.Value));
        Assert.Equal(0, fixture.Handler.Calls);
        Assert.Empty(await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Base64UploadRejectsNonImageWithoutUploadingOrSaving()
    {
        await using var fixture = await Fixture.CreateAsync();
        var model = new FileDataModel
        {
            FileName = "document.png",
            Base64Data = Convert.ToBase64String("not image data"u8.ToArray())
        };

        var result = Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Upload2Async(model));

        Assert.Contains("images are supported", Assert.IsType<string>(result.Value));
        Assert.Equal(0, fixture.Handler.Calls);
        Assert.Empty(await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImageWithNonImageExtensionIsRecognizedByContent(bool logo)
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        using var stream = new MemoryStream(image.ToByteArray(MagickFormat.Png));
        var files = new FormFileCollection { new FormFile(stream, 0, stream.Length, "files", "image.txt") };
        fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
        fixture.Controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync(logo ? FileType.Logo : FileType.Normal));
        Assert.Equal(logo ? 0 : 1, fixture.Handler.Calls);
        var saved = await fixture.Db.Set<FileData>().AsNoTracking().ToListAsync();
        if (logo)
        {
            Assert.Equal("image.png", Assert.Single(saved).FileName);
        }
        else
        {
            Assert.Empty(saved);
        }
    }

    [Fact]
    public async Task MultipartLogoSkipsImgBBWhenKeyIsConfigured()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 1800, 900);
        using var stream = new MemoryStream(image.ToByteArray(MagickFormat.Png));
        var files = new FormFileCollection { new FormFile(stream, 0, stream.Length, "files", "logo.png") };
        fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
        fixture.Controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        var response = Assert.IsType<OkObjectResult>(await fixture.Controller.UploadAsync(FileType.Logo));

        var saved = await fixture.Db.Set<FileData>().SingleAsync();
        Assert.Equal(0, fixture.Handler.Calls);
        Assert.Equal(FileType.Logo, saved.FileType);
        Assert.Equal($"https://localhost/api/filedata/download/{saved.Id}", Assert.Single(Assert.IsType<List<string>>(response.Value)));
        using var decoded = new MagickImage(saved.Data);
        Assert.Equal(1600u, decoded.Width);
    }

    [Fact]
    public async Task MultipartValidatesAllFilesBeforeExternalUploads()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        using var good = new MemoryStream(image.ToByteArray(MagickFormat.Png));
        using var bad = new MemoryStream("invalid"u8.ToArray());
        var files = new FormFileCollection
        {
            new FormFile(good, 0, good.Length, "files", "good.png"),
            new FormFile(bad, 0, bad.Length, "files", "bad.png")
        };
        fixture.Controller.Request.ContentType = "multipart/form-data; boundary=test";
        fixture.Controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.UploadAsync());
        Assert.Equal(0, fixture.Handler.Calls);
        Assert.Empty(await fixture.Db.Set<FileData>().ToListAsync());
    }

    [Fact]
    public async Task CallerCancellationDoesNotFallBackToDatabase()
    {
        await using var fixture = await Fixture.CreateAsync();
        using var image = new MagickImage(MagickColors.Red, 10, 10);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        fixture.Controller.HttpContext.RequestAborted = cts.Token;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Controller.Upload2Async(Model(image.ToByteArray(MagickFormat.Png))));
        Assert.Empty(await fixture.Db.Set<FileData>().ToListAsync());
    }

    private static FileDataModel Model(byte[] bytes, FileType type = FileType.Normal) =>
        new() { Base64Data = Convert.ToBase64String(bytes), FileName = "image.png", FileType = type };

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public AppDbContext Db { get; }
        public FakeHandler Handler { get; }
        public FakeFactory Factory { get; }
        public FileDataController Controller { get; }

        private Fixture(SqliteConnection connection, string key)
        {
            _connection = connection;
            Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
            Handler = new FakeHandler();
            Factory = new FakeFactory(Handler);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["ImgBBKEY"] = key }).Build();
            var upload = new UploadService(NullLoggerFactory.Instance, Db, Factory, config);
            Controller = new FileDataController(Db, NullLoggerFactory.Instance, upload, null)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            Controller.Request.Host = new HostString("localhost");
        }

        public static async Task<Fixture> CreateAsync(string key = "test-key")
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var fixture = new Fixture(connection, key);
            await fixture.Db.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FakeFactory(FakeHandler handler) : IHttpClientFactory
    {
        public string ClientName { get; private set; }
        public HttpClient CreateClient(string name)
        {
            ClientName = name;
            return new HttpClient(handler, false);
        }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string RequestUrl { get; private set; }
        public string Key { get; private set; }
        public byte[] Image { get; private set; }
        public Exception Error { get; set; }
        public Func<HttpResponseMessage> Response { get; set; } = () =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"success\":true,\"data\":{\"url\":\"https://i.ibb.co/example.png\"}}") };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Calls++;
            RequestUrl = request.RequestUri.ToString();
            var form = Assert.IsType<MultipartFormDataContent>(request.Content);
            foreach (var part in form)
            {
                var name = part.Headers.ContentDisposition?.Name?.Trim('"');
                if (name == "key") Key = await part.ReadAsStringAsync(cancellationToken);
                if (name == "image") Image = await part.ReadAsByteArrayAsync(cancellationToken);
            }
            if (Error != null) throw Error;
            return Response();
        }
    }
}
