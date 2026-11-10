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
}

public class ExpiredFileCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredFileCleanupService> _logger;
    private readonly FileCleanupOptions _options;

    public ExpiredFileCleanupService(IServiceScopeFactory scopeFactory,
        ILogger<ExpiredFileCleanupService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
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
