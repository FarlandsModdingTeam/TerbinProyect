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
                                            IProgress<TerbinInfoProgrss>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        List<string>? allFiles;
        List<string>? allDictories;
        bool last = false;
        int previus = -1;
        double? inverse;


        allFiles = GetAllFiles(pSourceDir);
        if (allFiles is null)
            return StatusFileUtil.InvalidSource;

        if (!Directory.Exists(pDestinationDir))
            Directory.CreateDirectory(pDestinationDir);

        inverse = Util.GetInverse(allFiles.Count);
        for (int i = 0; i < allFiles.Count; i++)
        {
            string  file = allFiles[i];
            string  rel = Path.GetRelativePath(pSourceDir, file);
            string  destFile = Path.Combine(pDestinationDir, rel);
            string? destFolder = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destFolder)) Directory.CreateDirectory(destFolder);

            File.Copy(file, destFile, pOverwrite);

            last = (i + 1) < allFiles.Count;
            Util.ReportProgress(i + 1, inverse, pProgress, last, ref previus);
        }

        allDictories = GetAllDirectories(pSourceDir);
        if (allDictories is null)
            return StatusFileUtil.InvalidSource;

        for (int i = 0; i < allDictories.Count; i++)
        {
            string dir = allDictories[i];
            string rel = Path.GetRelativePath(pSourceDir, dir);
            string destSub = Path.Combine(pDestinationDir, rel);
            if (!Directory.Exists(destSub)) Directory.CreateDirectory(destSub);
        }

        return StatusFileUtil.Succes;
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
