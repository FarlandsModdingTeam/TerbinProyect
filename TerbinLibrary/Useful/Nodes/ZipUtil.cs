using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using TerbinLibrary.Data;

namespace TerbinLibrary.Useful.Nodes;

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase que contiene utilidades estáticas relacionadas con la manipulación y extracción de archivos ZIP.<br />
/// ___________________( English )___________________<br />
/// Class containing static utilities related to the manipulation and extraction of ZIP files.<br />
/// </summary>
public class ZipUtil
{
    [Obsolete]
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

        double totalInverse = ProgressUtil.GetInverse(totalEntries);

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

            ProgressUtil.TryReportProgressPercent(currentEntry, totalInverse, pProgress, false, ref previusly);
        }

        ProgressUtil.TryReportProgressPercent(currentEntry, totalInverse, pProgress, true, ref previusly);

        return handwritten;
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae un archivo ZIP de manera asíncrona, genera un registro de extracción y reporta el progreso en porcentaje.<br />
    /// Notas: Crea directorios automáticamente si no existen e implementa protecciones contra vulnerabilidades ZipSlip.<br />
    /// ___________________( English )___________________<br />
    /// Extracts a ZIP file asynchronously, generating an extraction record and reporting progress in percentage.<br />
    /// Notes: Automatically creates directories if they do not exist and implements protections against ZipSlip vulnerabilities.<br />
    /// </summary>
    /// <param name="pSourceZipPath">Es: Ruta directa al archivo .zip origen. <br />En: Direct path to the source .zip file.</param>
    /// <param name="pDestinationDirectory">Es: Directorio donde extraer el contenido. <br />En: Directory where the contents will be extracted.</param>
    /// <param name="pProgress">Es: Objeto empleado para reportar el progreso global. <br />En: Object used to report global progress.</param>
    /// <param name="pOverwrite">Es: Permiso para sobrescribir elementos duplicados. <br />En: Permission to overwrite duplicate items.</param>
    /// <param name="pCancellationToken">Es: Token utilizado para cancelar el proceso en ejecución. <br />En: Token used to cancel the running process.</param>
    /// <returns>Es: Un objeto DirectoryHandwritten con el registro. <br />En: A DirectoryHandwritten object containing the record.</returns>
    [TODO("Controlar que tengas permisos")]
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

        long totalSize = 0; // archive.Entries.Sum(e => e.Length)
        if (pProgress != null)
        {
            for (int i = 0; i < archive.Entries.Count; i++)
            {
                totalSize += archive.Entries[i].Length;
            }
        }
        else
            totalSize = 100;
        long currentSize = 0;
        int previusly = -1;

        double totalInverse = ProgressUtil.GetInverse(totalSize);

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

            currentSize += entry.Length;
            ProgressUtil.TryReportProgressPercent(currentSize, totalInverse, pProgress, false, ref previusly);
        }

        if (pProgress != null)
            ProgressUtil.ReportProgressPercent(100, currentSize, true, pProgress);

        return handwritten;
    }

    public static long GetSize(string pSourceZipPath)
    {
        if (File.Exists(pSourceZipPath))
            return new FileInfo(pSourceZipPath).Length;
        else
            return long.MinValue;
    }
}
