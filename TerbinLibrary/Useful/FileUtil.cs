using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace TerbinLibrary.Useful;
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
    private const ushort _falseSizeFolder = 0xFFFF;

    // Son los unicos que el tamaño no es por el peso de los archivos en bytes.

    // PaVerano:
    // TODO: 
    // (Permitira actualizar farlands borrando solo el contenido marcado del json y volver a clonar actualizar de version la instancia)
    public static async Task<(StatusFileUtil status, DirectoryHandwritten? json)> CloneDirectory(
                                            string pSourceDir,
                                            string pDestinationDir,
                                            bool pOverwrite,
                                            IProgress<TerbinInfoProgrss>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        List<string>? allFiles;
        List<string>? allDictories;
        int previus = -1;
        double? inverse;

        DirectoryHandwritten handwritten = new();

        allFiles = GetAllFiles(pSourceDir);
        if (allFiles is null)
            return (StatusFileUtil.InvalidSource, null);

        if (!Directory.Exists(pDestinationDir))
            Directory.CreateDirectory(pDestinationDir);

        inverse = (pProgress != null) ? Util.GetInverse(allFiles.Count) : null;
        for (int i = 0; i < allFiles.Count; i++)
        {
            if (pCancellationToken.IsCancellationRequested)
                break;
            string  file = allFiles[i];
            string  rel = Path.GetRelativePath(pSourceDir, file);
            string  destFile = Path.Combine(pDestinationDir, rel);
            string? destFolder = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destFolder))
            {
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
            }

            File.Copy(file, destFile, pOverwrite);

            handwritten.Files.Add(rel);

            Util.TryReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        allDictories = GetAllDirectories(pSourceDir);
        if (allDictories is null)
            return (StatusFileUtil.InvalidSource, null);

        inverse = (pProgress != null) ? Util.GetInverse(allDictories.Count) : null;
        previus = -1;

        for (int i = 0; i < allDictories.Count; i++)
        {
            if (pCancellationToken.IsCancellationRequested)
                break;
            string dir = allDictories[i];
            string rel = Path.GetRelativePath(pSourceDir, dir);
            string destSub = Path.Combine(pDestinationDir, rel);
            if (!Directory.Exists(destSub)) Directory.CreateDirectory(destSub);

            handwritten.Directories.Add(rel);

            Util.TryReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        if (pProgress != null)
            Util.ReportProgressPercent(100, previus, true, pProgress);

        return (StatusFileUtil.Succes, handwritten);
    }

    // TODO: metodo que le dar una direccion y un DirectoryHandwritten y te lo borra.
    // └─Luego borra directorios vacios, solo vacios.

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

    public static List<string>? GetAllExeFiles(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;

        return Directory.EnumerateFiles(pDir, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .ToList();
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

    public static (long? maxFiles, long? maxDir) GetSizeDir(string pDir)
    {
        long? countFiles = GetCountFiles(pDir);
        long? countDir = GetCountDirectories(pDir);
        return (countFiles, countDir);
    }



}
