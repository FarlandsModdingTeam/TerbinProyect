using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace TerbinLibrary.Useful;

public class ZipUtil
{
    public static async Task<DirectoryHandwritten> ExtractWithProgressAndReportAsync(
                            string pSourceZipPath,
                            string pDestinationDirectory,
                            IProgress<TerbinInfoProgrss>? pProgress = default,
                            bool pOverwrite = true)
    {
        DirectoryHandwritten handwritten = new();

        if (!Directory.Exists(pDestinationDirectory))
            Directory.CreateDirectory(pDestinationDirectory);

        using ZipArchive archive = ZipFile.OpenRead(pSourceZipPath);
        int totalEntries = archive.Entries.Count;
        int currentEntry = 0;
        int previusly = -1;

        double totalInverse = Util.GetInverse(totalEntries);

        for (int i = 0; i < totalEntries; i++)
        {
            ZipArchiveEntry entry = archive.Entries[i];
            string destinationPath = Path.GetFullPath(Path.Combine(pDestinationDirectory, entry.FullName));
            string destinationRelative = Path.GetRelativePath(pDestinationDirectory, destinationPath);

            // Evitar una vulnerabilidad de ZipSlip asegurando que la ruta de destino está dentro del directorio esperado
            if (!destinationPath.StartsWith(Path.GetFullPath(pDestinationDirectory), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                handwritten.Directories.Add(destinationRelative);
            }
            else
            {
                if (!Directory.Exists(destinationPath))
                {
                    string? dir = Path.GetDirectoryName(destinationPath);
                    if (dir != null)
                        Directory.CreateDirectory(dir); // Problema: ¿Esto no creara una carpeta en la raiz del disco o proyecto?
                }

                entry.ExtractToFile(destinationPath, pOverwrite);
                handwritten.Files.Add(destinationRelative);
            }

            currentEntry++;

            Util.TryReportProgressPercent(currentEntry, totalInverse, pProgress, false, ref previusly);
        }

        Util.TryReportProgressPercent(currentEntry, totalInverse, pProgress, true, ref previusly);

        return handwritten;
    }

    public static async Task<DirectoryHandwritten> ExtractWithProgress(
                                    string pSourceZipPath,
                                    string pDestinationDirectory,
                                    IProgress<TerbinInfoProgrss>? pProgress = default,
                                    bool pOverwrite = true,
                                    CancellationToken pCancellationToken = default)
    {
        DirectoryHandwritten handwritten = new();
        string fullDestDir = Path.GetFullPath(pDestinationDirectory);

        if (!Directory.Exists(fullDestDir))
            Directory.CreateDirectory(fullDestDir);

        // Task.Run se puede usar aquí si abrir el ZIP grande congela el hilo inicial, 
        // pero la iteración asíncrona debajo es lo que realmente evita bloqueos de I/O.
        using ZipArchive archive = ZipFile.OpenRead(pSourceZipPath);
        int totalEntries = archive.Entries.Count;
        int currentEntry = 0;
        int previusly = -1;

        double totalInverse = Util.GetInverse(totalEntries);

        for (int i = 0; i < totalEntries; i++)
        {
            if (pCancellationToken.IsCancellationRequested)
                break;

            ZipArchiveEntry entry = archive.Entries[i];
            string destinationPath = Path.GetFullPath(Path.Combine(fullDestDir, entry.FullName));
            string destinationRelative = Path.GetRelativePath(fullDestDir, destinationPath);

            // Evitar vulnerabilidad de ZipSlip
            if (!destinationPath.StartsWith(fullDestDir, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Detectar si es un directorio (los directorios terminan en '/' en los ZIP)
            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                handwritten.Directories.Add(destinationRelative);
            }
            else
            {
                // Es un archivo. Asegurar que su directorio contenedor existe.
                string? dir = Path.GetDirectoryName(destinationPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Extracción Asíncrona Real
                if (pOverwrite || !File.Exists(destinationPath))
                {
                    using Stream entryStream = entry.Open();
                    // useAsync: true es vital para aprovechar el I/O asíncrono subyacente del SO
                    using FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

                    await entryStream.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                handwritten.Files.Add(destinationRelative);
            }

            currentEntry++;
            Util.TryReportProgressPercent(currentEntry, totalInverse, pProgress, false, ref previusly);
        }

        if (pProgress != null)
            Util.ReportProgressPercent(100, currentEntry, true, pProgress);

        return handwritten;
    }
}
