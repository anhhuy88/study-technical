using CommonWebAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CommonWebAPI.Services;

public class FileCleanupOptions
{
    public const string SectionName = "FileCleanup";

    public bool Enabled { get; set; } = true;

    // Files older than this number of days are deleted.
    public int RetentionDays { get; set; } = 365;

    public int IntervalHours { get; set; } = 24;

    // Cached proxied images not accessed/created for this many days are deleted.
    public int ImageCacheRetentionDays { get; set; } = 30;
}

public class ExpiredFileCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredFileCleanupService> _logger;
    private readonly FileCleanupOptions _options;
    private readonly string _imageCacheFolder;

    public ExpiredFileCleanupService(IServiceScopeFactory scopeFactory,
        ILogger<ExpiredFileCleanupService> logger,
        IConfiguration configuration,
        ImageCacheOptions imageCacheOptions)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _imageCacheFolder = imageCacheOptions.Folder;
        _options = configuration.GetSection(FileCleanupOptions.SectionName).Get<FileCleanupOptions>() ?? new FileCleanupOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Expired file cleanup is disabled.");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(1, _options.IntervalHours));
        using var timer = new PeriodicTimer(interval);

        do
        {
            await CleanupAsync(stoppingToken);
            CleanupImageCache();
        }
        while (await WaitNextTickAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitNextTickAsync(PeriodicTimer timer, CancellationToken token)
    {
        try
        {
            return await timer.WaitForNextTickAsync(token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private void CleanupImageCache()
    {
        try
        {
            if (!Directory.Exists(_imageCacheFolder))
            {
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, _options.ImageCacheRetentionDays));
            var deleted = 0;
            foreach (var path in Directory.EnumerateFiles(_imageCacheFolder))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(path) < cutoff)
                    {
                        File.Delete(path);
                        deleted++;
                    }
                }
                catch (IOException ex)
                {
                    // File may be in use by a request; retry on next run.
                    _logger.LogDebug(ex, "Skip cache file {Path}", path);
                }
            }

            if (deleted > 0)
            {
                _logger.LogInformation("Deleted {Count} cached images older than {Cutoff:u}.", deleted, cutoff);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image cache cleanup failed.");
        }
    }

    private async Task CleanupAsync(CancellationToken token)
    {
        try
        {
            // CreatedDate is stored with DateTime.Now, so compare using local time.
            var cutoff = DateTime.Now.AddDays(-Math.Max(1, _options.RetentionDays));

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deleted = await db.Set<FileData>()
                .Where(f => f.CreatedDate < cutoff && f.FileType != FileType.Logo)
                .ExecuteDeleteAsync(token);

            if (deleted > 0)
            {
                _logger.LogInformation("Deleted {Count} files created before {Cutoff:u}.", deleted, cutoff);
                // SQLite does not shrink the db file on delete.
                await db.Database.ExecuteSqlRawAsync("VACUUM;", token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Expired file cleanup failed.");
        }
    }
}
