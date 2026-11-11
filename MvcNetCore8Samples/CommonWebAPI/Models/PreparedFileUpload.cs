using CommonWebAPI.Domain;

namespace CommonWebAPI.Models;

public sealed record PreparedFileUpload(OptimizedImage Image, byte[] OriginalData, FileType FileType);
