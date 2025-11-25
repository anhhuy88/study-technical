using CommonWebAPI.Models;

namespace CommonWebAPI.Interfaces;
public interface IUploadService
{
    Task<string> SaveFileDataAsync(FileDataModel model);
}
