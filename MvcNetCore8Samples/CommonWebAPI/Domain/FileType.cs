namespace CommonWebAPI.Domain;

public enum FileType
{
    Normal = 0,
    // Logo files are never removed by the background cleanup.
    Logo = 1
}
