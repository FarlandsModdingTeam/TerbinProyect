using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

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
public class DirectoryHandwritten
{
    public List<string> Directories { get; set; } = new();
    public List<string> Files { get; set; } = new();
}

public static class FileUtil
{
    // PaVerano:
    // TODO: Devolver un json con todos los archivos y carpetas clonadas.
    // (Permitira actualizar farlands borrando solo el contenido marcado del json y volver a clonar actualizar de version la instancia)
    public static async Task<(StatusFileUtil status, string? json)> CloneDirectory(
                                            string pSourceDir,
                                            string pDestinationDir,
                                            bool pOverwrite,
                                            IProgress<TerbinInfoProgrss>? pProgress = null)
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

            if (pProgress != null)
                Util.ReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        allDictories = GetAllDirectories(pSourceDir);
        if (allDictories is null)
            return (StatusFileUtil.InvalidSource, null);

        inverse = (pProgress != null) ? Util.GetInverse(allDictories.Count) : null;
        previus = -1;

        for (int i = 0; i < allDictories.Count; i++)
        {
            string dir = allDictories[i];
            string rel = Path.GetRelativePath(pSourceDir, dir);
            string destSub = Path.Combine(pDestinationDir, rel);
            if (!Directory.Exists(destSub)) Directory.CreateDirectory(destSub);

            handwritten.Directories.Add(rel);

            if (pProgress != null)
                Util.ReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        if (pProgress != null)
            Util.ReportProgressPercent(previus, inverse, pProgress, true, ref previus);

        string handwrittenJson = JsonSerializer.Serialize(handwritten, new JsonSerializerOptions { WriteIndented = true });

        return (StatusFileUtil.Succes, handwrittenJson);
    }

    // TODO: metodo que le dar una direccion y un DirectoryHandwritten en json (string) y te lo borra.
    // └─Luego borra directorios vacios.

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
