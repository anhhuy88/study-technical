using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using CommonWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonWebAPI.Services;

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
        _options = configuration.GetSection(VARIABLES.FILE_CLEANUP_SECTION_NAME).Get<FileCleanupOptions>() ?? new FileCleanupOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Expired file cleanup is disabled.");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(1, _options.IntervalHours));
        DateTimeOffset? nextRun = null;
        _logger.LogInformation("File cleanup starts only between 22:00 and 03:00 server local time, with an interval of {Interval}.", interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var delay = CleanupSchedule.GetDelay(DateTime.Now, now, nextRun);
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                continue;
            }

            nextRun = now.Add(interval);
            await CleanupAsync(stoppingToken);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            CleanupImageCache();
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
