using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.SteamFarlands;

public static class SteamLocator
{
    // private static object _lockManifest = new();
    // private static object _lockLibraryForders = new();

    public static string? GetGamePath(int pAppId)
    {
        foreach (var library in GetSteamLibraries())
        {
            var manifest = Path.Combine(
                library,
                "steamapps",
                $"appmanifest_{pAppId}.acf"
            );

            if (!File.Exists(manifest))
                continue;

            foreach (var line in File.ReadLines(manifest))
            {
                if (line.Contains("installdir"))
                {
                    var dir = line.Split('"')[3];
                    return Path.Combine(library, "steamapps", "common", dir);
                }
            }
        }

        return null;
    }

    static IEnumerable<string> GetSteamLibraries()
    {
        var paths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (key?.GetValue("SteamPath") is string steamPath)
                paths.Add(steamPath);
        }
        else
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            paths.Add(Path.Combine(home, ".steam/steam"));
            paths.Add(Path.Combine(home, ".local/share/Steam"));
        }

        foreach (var path in paths)
        {
            var vdf = Path.Combine(path, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(vdf))
                continue;

            foreach (var line in File.ReadLines(vdf))
            {
                if (line.Contains("\"path\""))
                {
                    var p = line.Split('"')[3]
                                .Replace("\\\\", "\\");

                    yield return p;
                }
            }

            yield return path;
        }
    }
}
