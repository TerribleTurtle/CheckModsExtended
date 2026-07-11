using System;
using System.IO;

namespace CheckModsExtended.Utils;

public static class AppConstants
{
    public static string AppDataDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SptCheckModsExtended");
}
