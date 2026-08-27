using System.IO;

public static class GamePaths
{
    public static string GetSystemLogPath()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        return Path.Combine(desktopPath, "soul_733.txt");
    }

    public static string GetLockPath()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        return Path.Combine(desktopPath, "deleted.lock");
    }
}