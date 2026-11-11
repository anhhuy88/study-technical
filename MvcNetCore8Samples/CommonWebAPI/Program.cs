using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Models;
using CommonWebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Resolve the configured log path against the content root so it does not depend on the working directory.
const string LogPathKey = "Serilog:WriteTo:FileSink:Args:path";
var logPath = Path.GetFullPath(configuration[LogPathKey] ?? "logs/log-.txt", builder.Environment.ContentRootPath);
configuration[LogPathKey] = logPath;
try
{
    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Cannot create log folder for '{logPath}': {ex.Message}");
}
Console.WriteLine($"Log path: {logPath}");
// Serilog swallows sink errors (e.g. missing write permission) unless SelfLog is enabled.
Serilog.Debugging.SelfLog.Enable(Console.Error);
builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Console.WriteLine($"Data folder: {dataFolder}");
if (!Directory.Exists(dataFolder))
{
    Directory.CreateDirectory(dataFolder);
}

var dataFile = Path.Combine(dataFolder, "appData.db"); // Directory.GetCurrentDirectory()

builder.Services.AddHttpClient(VARIABLES.HTTP_CLIENT_NAME);
builder.Services.AddHttpClient(VARIABLES.IMGBB_HTTP_CLIENT_NAME, client =>
    client.Timeout = TimeSpan.FromSeconds(VARIABLES.IMGBB_TIMEOUT_SECONDS));
builder.Services.AddHttpClient(VARIABLES.IMAGE_HTTP_CLIENT_NAME, client =>
{
    // Must stay below the reverse proxy read timeout (nginx default 60s).
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36");
    client.DefaultRequestHeaders.Accept.ParseAdd("image/avif,image/webp,image/*,*/*;q=0.8");
});

builder.Services.AddSingleton(new ImageCacheOptions(Path.Combine(dataFolder, "image-cache")));
builder.Services.AddSingleton<IImageProxyService, ImageProxyService>();

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite($"Filename={dataFile}"));

builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddHostedService<ExpiredFileCleanupService>();

builder.Services.AddCors(options =>
{
    options
        .AddPolicy(name: "AllowAll", policy =>
        {
            policy//.WithOrigins(configuration["AllowedHosts"] ?? "*")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");

app.MapControllers();

await app.RunAsync();
