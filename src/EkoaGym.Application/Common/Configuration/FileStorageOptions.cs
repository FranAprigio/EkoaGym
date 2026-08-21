namespace EkoaGym.Application.Common.Configuration;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    // Absolute path, computed at startup from IHostEnvironment.ContentRootPath (not read from a config value).
    public string UploadsRootPath { get; set; } = string.Empty;
}
