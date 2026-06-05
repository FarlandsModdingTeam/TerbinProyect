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


public static class ManagerFarlands
{
    public const int KEY_FARLANDS = 2252680;
    public const string FARLANDS_EXE = "Farlands.exe";

    public static bool IsOpenSteam
    {
        get => Process.GetProcessesByName("steam").Length > 0 ||
               Process.GetProcessesByName("steamwebhelper").Length > 0;
    }

    [Obsolete("Lanzar juego de la instancia")]
    public static Status LaunchFarlands(string? pName = null)
    {
        if (pName == null)
            return LaunchFarlandsBySteam();

        // TODO: Lanzar juego de la instancia.
        return Status.Succes;
    }

    public static Status LaunchFarlandsBySteam()
    {
        if (SteamLocator.GetGamePath(KEY_FARLANDS) == null)
            return Status.NotInstaled;
        Process.Start(new ProcessStartInfo
        {
            FileName = "steam://run/2252680",
            UseShellExecute = true
        });
        return Status.Succes;
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


    public static string GetVersion()
    {
        return "0.0.9";
    }





    public enum Status : sbyte
    {
        NotInstaled = 2,

        Succes = 1,
    }
}
