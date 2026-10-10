namespace EkoaGym.Mobile.Tests.Localization;

internal static class RepoPaths
{
    public static string Root { get; } = FindRoot();

    public static string MobileProject => Path.Combine(Root, "src", "EkoaGym.Mobile");

    public static string EnglishResx => Path.Combine(MobileProject, "Resources", "SharedResources.resx");

    public static string PortugueseResx => Path.Combine(MobileProject, "Resources", "SharedResources.pt-BR.resx");

    public static string Backend(params string[] segments) => Path.Combine([Root, "src", .. segments]);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "EkoaGym.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate EkoaGym.slnx above the test output folder.");
    }
}
