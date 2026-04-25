using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinService.Useful;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */

public enum StatusFileUtil : sbyte
{
    Succes = 1,

    InvalidSource = 2,
}

public static class FileUtil
{
    public static async Task<StatusFileUtil> CloneDirectory(
                                            string pSourceDir,
                                            string pDestinationDir,
                                            bool pOverwrite,
                                            IProgress<byte>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        List<string>? allFiles;
        int totalFiles;

        allFiles = GetAllFiles(pSourceDir);
        if (allFiles is null)
            return StatusFileUtil.InvalidSource;

        totalFiles = allFiles.Count;



        throw new NotImplementedException("Ñe");
    }

    public static List<string>? GetAllFiles(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;
        return Directory.EnumerateFiles(pDir, "*", SearchOption.AllDirectories).ToList();
    }
    public static List<string>? GetAllDirectories(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;
        return Directory.EnumerateDirectories(pDir, "*", SearchOption.AllDirectories).ToList();
    }

    public static long? GetCountFiles(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;

        long count = 0;
        foreach (var file in Directory.EnumerateFiles(pDir, "*", SearchOption.AllDirectories))
        {
            count++;
        }
        return count;
    }
    public static long? GetCountDirectories(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;

        long count = 0;
        foreach (var file in Directory.EnumerateDirectories(pDir, "*", SearchOption.AllDirectories))
        {
            count++;
        }
        return count;
    }
}
