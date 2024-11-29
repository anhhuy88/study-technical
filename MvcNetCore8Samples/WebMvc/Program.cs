using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Serilog;
using WebMvc.Domains;
using WebMvc.Interfaces;
using WebMvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
var demoDataFile = Path.Combine(dataFolder, "demo.db");

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite($"Filename={demoDataFile}"));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add services to the container.
builder.Services.AddMvc()
    .AddApplicationPart(typeof(WebMvcLib.Areas.Admin.Controllers.HomeController).Assembly)
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

var app = builder.Build();

// Migrate database
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();
await context.Database.MigrateAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "areaRoute", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
//app.MapAreaControllerRoute(name: "admin-default", areaName: "Admin", pattern: "/Admin/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
