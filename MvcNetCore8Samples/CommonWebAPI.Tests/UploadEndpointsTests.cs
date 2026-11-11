using CommonWebAPI.Controllers;
using CommonWebAPI.Domain;
using CommonWebAPI.Models;
using CommonWebAPI.Services;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CommonWebAPI.Tests;

public class UploadEndpointsTests
{
    [Fact]
    public async Task MultipartUploadOptimizesLogoImage()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var controller = CreateController(db);
        using var image = new MagickImage(MagickColors.Red, 2000, 1000);
        using var imageStream = new MemoryStream(image.ToByteArray(MagickFormat.Png));
        var files = new FormFileCollection
        {
            new FormFile(imageStream, 0, imageStream.Length, "files", "image.png")
        };
        controller.Request.ContentType = "multipart/form-data; boundary=test";
        controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        var response = await controller.UploadAsync(FileType.Logo);

        Assert.IsType<OkObjectResult>(response);
        var saved = await db.Set<FileData>().AsNoTracking().ToListAsync();
        Assert.Single(saved);
        var storedImage = Assert.Single(saved, f => f.FileName == "image.png");
        using var decoded = new MagickImage(storedImage.Data);
        Assert.Equal(1600u, decoded.Width);
        Assert.Equal(800u, decoded.Height);
        Assert.Equal(FileType.Logo, storedImage.FileType);
        Assert.Equal(storedImage.Data.Length, storedImage.Size);
    }

    [Fact]
    public async Task Base64UploadUsesSameImageProcessingAndReturnsLink()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var controller = CreateController(db);
        using var image = new MagickImage(MagickColors.Red, 1000, 2000);
        var model = new FileDataModel
        {
            FileName = "photo.jpg",
            Base64Data = Convert.ToBase64String(image.ToByteArray(MagickFormat.Jpeg)),
            FileType = FileType.Normal
        };

        var response = await controller.Upload2Async(model);

        var ok = Assert.IsType<OkObjectResult>(response);
        var stored = await db.Set<FileData>().AsNoTracking().SingleAsync();
        Assert.Equal($"https://localhost/api/filedata/download/{stored.Id}", ok.Value);
        using var decoded = new MagickImage(stored.Data);
        Assert.Equal(800u, decoded.Width);
        Assert.Equal(1600u, decoded.Height);
        Assert.Equal("image/jpeg", stored.ContentType);
        Assert.Equal(stored.Data.Length, stored.Size);
    }

    [Fact]
    public async Task InvalidImageDoesNotSaveAnyFiles()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateDb(connection);
        await db.Database.EnsureCreatedAsync();
        var controller = CreateController(db);
        using var textStream = new MemoryStream("document"u8.ToArray());
        using var badImageStream = new MemoryStream("invalid"u8.ToArray());
        var files = new FormFileCollection
        {
            new FormFile(textStream, 0, textStream.Length, "files", "note.txt"),
            new FormFile(badImageStream, 0, badImageStream.Length, "files", "bad.png")
        };
        controller.Request.ContentType = "multipart/form-data; boundary=test";
        controller.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), files);

        Assert.IsType<BadRequestObjectResult>(await controller.UploadAsync());
        Assert.Empty(await db.Set<FileData>().AsNoTracking().ToListAsync());
        Assert.IsType<BadRequestObjectResult>(await controller.Upload2Async(new FileDataModel
        {
            FileName = "bad.png",
            Base64Data = "not-base64"
        }));
    }

    private static AppDbContext CreateDb(SqliteConnection connection)
    {
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
    }

    private static FileDataController CreateController(AppDbContext db)
    {
        var upload = new UploadService(NullLoggerFactory.Instance, db, new TestHttpClientFactory(), new ConfigurationBuilder().Build());
        var controller = new FileDataController(db, NullLoggerFactory.Instance, upload, null);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.Request.Host = new HostString("localhost");
        return controller;
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
