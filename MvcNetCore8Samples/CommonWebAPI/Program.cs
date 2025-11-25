using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Interfaces;
using CommonWebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Console.WriteLine($"Data folder: {dataFolder}");
if (!Directory.Exists(dataFolder))
{
    Directory.CreateDirectory(dataFolder);
}

var dataFile = Path.Combine(dataFolder, "appData.db"); // Directory.GetCurrentDirectory()

builder.Services.AddHttpClient(VARIABLES.HTTP_CLIENT_NAME);
builder.Services.AddHttpClient(VARIABLES.IMAGE_HTTP_CLIENT_NAME);

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite($"Filename={dataFile}"));

builder.Services.AddScoped<IUploadService, UploadService>();

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

if (builder.Environment.IsDevelopment() == false)
{
    app.UseHttpsRedirection();
}
// Configure the HTTP request pipeline.
app.UseCors("AllowAll");

app.MapControllers();

await app.RunAsync();


