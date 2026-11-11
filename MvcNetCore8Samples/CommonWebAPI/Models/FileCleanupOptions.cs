namespace CommonWebAPI.Models;

public class FileCleanupOptions
{
    public bool Enabled { get; set; } = true;
    public int RetentionDays { get; set; } = 365;
    public int IntervalHours { get; set; } = 24;
    public int ImageCacheRetentionDays { get; set; } = 30;
}
