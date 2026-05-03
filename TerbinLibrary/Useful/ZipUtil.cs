using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace TerbinLibrary.Useful;

public class ZipUtil
{
    public static async Task<DirectoryHandwritten> ExtractWithProgressAndReportAsync(string pSourceZipPath, string pDestinationDirectory, IProgress<TerbinInfoProgrss> pProgress, bool pOverwrite = true)
    {
        //var extractedFiles = new List<string>();
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

            // Evitar una vulnerabilidad de ZipSlip asegurando que la ruta de destino está dentro del directorio esperado
            if (!destinationPath.StartsWith(Path.GetFullPath(pDestinationDirectory), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                handwritten.Directories.Add(destinationPath);
            }
            else
            {
                string rel = Path.GetRelativePath(pSourceDir, destinationPath);

                if (!Directory.Exists(destinationPath))
                {
                    string? dir = Path.GetDirectoryName(destinationPath);
                    if (dir != null)
                        Directory.CreateDirectory(dir); // Problema: ¿Esto no creara una carpeta en la raiz del disco o proyecto?
                }

                entry.ExtractToFile(destinationPath, pOverwrite);
                extractedFiles.Add(entry.FullName);
                handwritten.Files.Add(destinationPath);
            }

            currentEntry++;

            Util.TryReportProgressPercent(currentEntry, totalInverse, pProgress, false, ref previusly);
        }

        Util.TryReportProgressPercent(currentEntry, totalInverse, pProgress, true, ref previusly);

        // Guardar el registro de archivos extraídos en un JSON
        // string jsonFilePath = Path.Combine(pDestinationDirectory, "extracted_content_report.json");
        // string jsonContent = JsonSerializer.Serialize(extractedFiles, new JsonSerializerOptions { WriteIndented = true });
        // await File.WriteAllTextAsync(jsonFilePath, jsonContent);

        return handwritten;
    }
}
