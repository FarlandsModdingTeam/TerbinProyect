using System;
using System.Collections.Generic;
using System.Diagnostics;
using TerbinLibrary.Useful;

namespace TerbinLibrary.SteamFarlands;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: menorculas = es privada local.
  empieza: "p"en menorculas = parametro entrante local.
  empieza: mayorculas = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */

[TODO("Convertir en GameUtil y sacar de aqui")]
public static class ManagerFarlands
{
    public const int KEY_FARLANDS = 2252680;
    public const string FARLANDS_EXE = "Farlands.exe";

    public static bool LaunchGame(string pPath)
    {
        if (string.IsNullOrEmpty(pPath) || !File.Exists(pPath))
            return false;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = pPath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(pPath)
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool LaunchFarlandsBySteam()
    {
        if (SteamLocator.GetGamePath(KEY_FARLANDS) == null)
            return false;
        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://run/2252680",
            UseShellExecute = true
        });
        return true;
    }

    public static List<string> GetDLCs(string pManifestPath)
    {
        var dlcs = new List<string>();
        foreach (var line in File.ReadLines(pManifestPath))
        {
            if (line.Trim().StartsWith("\"") && line.Contains("\""))
            {
                var parts = line.Split('"');
                if (parts.Length > 1 && int.TryParse(parts[1], out _))
                    dlcs.Add(parts[1]);
            }
        }
        return dlcs;
    }
    public static long GetDirectorySize(string pPath)
    {
        long size = 0;
        foreach (var file in Directory.EnumerateFiles(pPath, "*", SearchOption.AllDirectories))
        {
            try { size += new FileInfo(file).Length; }
            catch { }
        }
        return size;
    }
    public static string? GetRuteSteamFarlands()
    {
        return SteamLocator.GetGamePath(KEY_FARLANDS);
    }

    public static bool IsFarlands(string pDir)
    {
        if (!Directory.Exists(pDir))
            return false;

        string pFilePath = Path.Combine(pDir, FARLANDS_EXE);
        return File.Exists(pFilePath);
    }

    [TODO("Solucionar que da la version de Unity en vez del juego XD")]
    public static string GetVersion()
    {
        var ruteFarlands = GetRuteSteamFarlands();
        if (ruteFarlands == null)
            return string.Empty;

        var farlandsExePath = Path.Combine(ruteFarlands, FARLANDS_EXE);

        if (!File.Exists(farlandsExePath))
            return string.Empty;

        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(farlandsExePath);
            return fileInfo.FileVersion ?? fileInfo.ProductVersion ?? "0.0.0";
        }
        catch
        {
            return string.Empty;
        }
    }





    public enum Status : sbyte
    {
        NotInstaled = 2,

        Succes = 1,
    }
}
