namespace CommonWebAPI.Models;

public enum ImageFetchStatus
{
    Ok,
    NotFound,
    Timeout,
    Busy,
    Failed
}

public record ImageFetchResult(ImageFetchStatus Status, string FilePath = null, string ContentType = null);
