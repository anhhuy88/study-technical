using CommonWebAPI.Models;

namespace CommonWebAPI.Interfaces;

public interface IImageProxyService
{
    Task<ImageFetchResult> GetAsync(string imgUrl, CancellationToken token);
}
