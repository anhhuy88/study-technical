using CommonWebAPI.Models;

namespace CommonWebAPI.Interfaces;
public interface IUploadService
{
    Task<UploadResult> SaveFileDataAsync(FileDataModel model, CancellationToken cancellationToken);
    Task<IReadOnlyList<UploadResult>> SaveImagesAsync(IReadOnlyList<ImageUpload> images, CancellationToken cancellationToken);
}
