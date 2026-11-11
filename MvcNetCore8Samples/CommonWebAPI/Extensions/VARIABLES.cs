namespace CommonWebAPI.Extensions;

public static class VARIABLES
{
    public const string FILE_SIZE_TOO_LARGE = "File size is too large (2MB).";
    public const string FILE_SIZE_IS_ZERO = "File size is 0.";
    public const string APPLICATION_OCTET_STREAM = "application/octet-stream";
    public const string HTTP_CLIENT_NAME = "http_client_name";
    public const string IMAGE_HTTP_CLIENT_NAME = "image_http_client_name";
    public const string IMGBB_HTTP_CLIENT_NAME = "imgbb_http_client_name";
    public const string IMGBB_UPLOAD_URL = "https://api.imgbb.com/1/upload";
    public const int IMGBB_TIMEOUT_SECONDS = 20;
    public const string FILE_CLEANUP_SECTION_NAME = "FileCleanup";
    public const int IMAGE_MAX_CONCURRENT_DOWNLOADS = 4;
    public const uint IMAGE_MAX_DIMENSION = 1600;
    public const uint IMAGE_QUALITY = 82;
    public const uint IMAGE_LOGO_QUALITY = 92;
}