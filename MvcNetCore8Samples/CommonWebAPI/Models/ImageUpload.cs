using CommonWebAPI.Domain;

namespace CommonWebAPI.Models;

public sealed record ImageUpload(byte[] Data, string FileName, FileType FileType);
