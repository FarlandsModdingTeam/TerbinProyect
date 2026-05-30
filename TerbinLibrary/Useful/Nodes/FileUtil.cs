using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using TerbinLibrary.Data;

namespace TerbinLibrary.Useful.Nodes;
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

/// <summary>
/// ___________________( Español )___________________<br />
/// Enumeración que representa los estados posibles en las utilidades de archivos.<br />
/// ___________________( English )___________________<br />
/// Enum representing the possible states in file utilities.<br />
/// </summary>
public enum StatusFileUtil : sbyte
{
    IsCancelled = 0,
    Succes = 1,

    InvalidSource = 2,
    InvalidFiles = 3,
    InvalidDirectorys = 4,
    InvalidHandwritten = 5,
    InvalidRoot = 6,
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase estática de utilidad para manejar archivos y directorios de forma avanzada.<br />
/// ___________________( English )___________________<br />
/// Static utility class to handle files and directories in an advanced way.<br />
/// </summary>
public static class FileUtil // : File
{
    //  private const ushort _falseSizeFolder = 0xFFFF;

    // Son los unicos que el tamaño no es por el peso de los archivos en bytes.

    // PaVerano:
    // (Permitira actualizar farlands borrando solo el contenido marcado del json y volver a clonar actualizar de version la instancia)
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Clona un directorio copiando sus archivos y subdirectorios a un nuevo destino.<br />
    /// Notas: Genera un registro de lo copiado para facilitar su posterior actualización o borrado.<br />
    /// ___________________( English )___________________<br />
    /// Clones a directory by copying its files and subdirectories to a new destination.<br />
    /// Notes: Generates a record of the copied content to facilitate subsequent updates or deletion.<br />
    /// </summary>
    /// <param name="pSourceDir">Es: Directorio origen a clonar. <br />En: Source directory to clone.</param>
    /// <param name="pDestinationDir">Es: Directorio destino. <br />En: Destination directory.</param>
    /// <param name="pOverwrite">Es: Verdadero para sobrescribir archivos existentes. <br />En: True to overwrite existing files.</param>
    /// <param name="pProgress">Es: Objeto para reportar el progreso. <br />En: Object to report progress.</param>
    /// <param name="pCancellationToken">Es: Token para cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Estado y registro del directorio clonado. <br />En: Status and cloned directory record.</returns>
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
            return (StatusFileUtil.InvalidFiles, null);

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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Borra archivos y directorios registrados en la estructura, actualizando su raíz.<br />
    /// ___________________( English )___________________<br />
    /// Deletes files and directories registered in the structure, updating its root.<br />
    /// </summary>
    /// <param name="pDir">Es: Directorio base de donde eliminar. <br />En: Base directory to delete from.</param>
    /// <param name="pHandwritten">Es: Registro de los contenidos. <br />En: Record of the contents.</param>
    /// <param name="pProgress">Es: Para reportar el progreso. <br />En: To report progress.</param>
    /// <returns>Es: Estado de la operación. <br />En: Operation status.</returns>
    public static StatusFileUtil DeleteFromHandwritten(string pDir, DirectoryHandwritten pHandwritten, IProgress<TerbinInfoProgrss>? pProgress = null)
    {
        pHandwritten.Root = pDir;
        return DeleteFromHandwritten(pHandwritten, pProgress);
    }

    // TODO: Hacerla asincrona.
    // TODO: Que no salte excepcion.
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Borra secuencialmente los archivos y directorios basándose en los registros.<br />
    /// Notas: Borra directorios solo si están vacíos tras eliminar sus archivos.<br />
    /// ___________________( English )___________________<br />
    /// Sequentially deletes files and directories based on the records.<br />
    /// Notes: Deletes directories only if they are empty after removing their files.<br />
    /// </summary>
    /// <param name="pHandwritten">Es: Registro de directorio con su raíz configurada. <br />En: Directory record with its root configured.</param>
    /// <param name="pProgress">Es: Progreso de eliminación. <br />En: Deletion progress.</param>
    /// <returns>Es: Resultado de la operación. <br />En: Operation result.</returns>
    [TODO("Hacer asincrono DeleteFromHandwritten")]
    public static StatusFileUtil DeleteFromHandwritten(DirectoryHandwritten pHandwritten, IProgress<TerbinInfoProgrss>? pProgress = null)
    {
        int previus = -1;
        double? inverse;
        string? root = pHandwritten.Root;

        inverse = (pProgress != null) ? Util.GetInverse(pHandwritten.Files.Count) : null;
        for (int i = 0; i < pHandwritten.Files.Count; i++)
        {
            string file = pHandwritten.Files[i];
            string destFile =
                Path.IsPathFullyQualified(file) ? file
                : (root != null) ? Path.Combine(root, file) 
                : throw new NullReferenceException("Try acces Root null in path Handwritten relative");

            if (File.Exists(destFile))
                File.Delete(destFile);

            Util.TryReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        // Borrar directorios vacíos (de más profundos a más superficiales)
        // Al ordenar por longitud descendente, procesamos "A/B/C" antes que "A/B"
        var orderedDirectories = pHandwritten.Directories.OrderByDescending(d => d.Length).ToList();

        inverse = (pProgress != null) ? Util.GetInverse(orderedDirectories.Count) : null;
        for (int i = 0; i < orderedDirectories.Count; i++)
        {
            string dir = orderedDirectories[i];
            string destSub =
                Path.IsPathFullyQualified(dir) ? dir
                : (root != null) ? Path.Combine(root, dir)
                : throw new NullReferenceException("Try acces Root null in path Handwritten relative");

            if (Directory.Exists(destSub))
            {
                if (!Directory.EnumerateFileSystemEntries(destSub).Any())
                    Directory.Delete(destSub, false); // NO borrado recursivo
            }
            Util.TryReportProgressPercent(i + 1, inverse, pProgress, false, ref previus);
        }

        if (pProgress != null)
            Util.ReportProgressPercent(100, previus, true, pProgress);

        return StatusFileUtil.Succes;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Oculta un archivo combinando atributos, aplicando solo en sistemas Windows.<br />
    /// ___________________( English )___________________<br />
    /// Hides a file by combining attributes, applying only on Windows systems.<br />
    /// </summary>
    /// <param name="pDir">Es: O carpeta contenedora. <br />En: Or containing folder.</param>
    /// <param name="pFileName">Es: Nombre de archivo a ocultar. <br />En: Name of the file to hide.</param>
    public static void Hide(string pDir, string pFileName)
    {
        if (!OperatingSystem.IsWindows())
            return;

        string filePath = Path.Combine(pDir, pFileName);
        if (File.Exists(filePath))
            File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene todas las rutas de archivos iterando recursivamente por el directorio principal y subdirectorios.<br />
    /// ___________________( English )___________________<br />
    /// Gets all file paths iterating recursively through the main directory and subdirectories.<br />
    /// </summary>
    /// <param name="pDir">Es: Directorio a mapear. <br />En: Directory to map.</param>
    /// <returns>Es: Lista de archivos encontrados o null si no existe. <br />En: List of found files or null if it doesn't exist.</returns>
    public static List<string>? GetAllFiles(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;
        return Directory.EnumerateFiles(pDir, "*", SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene subdirectorios de manera recursiva a partir del directorio dado.<br />
    /// ___________________( English )___________________<br />
    /// Gets subdirectories recursively from the given directory.<br />
    /// </summary>
    /// <param name="pDir">Es: Directorio fuente a explorar. <br />En: Source directory to explore.</param>
    /// <returns>Es: Lista de carpetas o null. <br />En: List of folders or null.</returns>
    public static List<string>? GetAllDirectories(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;
        return Directory.EnumerateDirectories(pDir, "*", SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza una busqueda recursiva y retorna un listado con aquellos ficheros con extensión .exe.<br />
    /// ___________________( English )___________________<br />
    /// Performs a recursive search and returns a list with those files ending in .exe.<br />
    /// </summary>
    /// <param name="pDir">Es: Directorio a consultar. <br />En: Directory to query.</param>
    /// <returns>Es: Colección de ejecutables o null. <br />En: Executable paths collection or null.</returns>
    public static List<string>? GetAllExeFiles(string pDir)
    {
        if (!Directory.Exists(pDir))
            return null;

        return Directory.EnumerateFiles(pDir, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Cuenta el número total de archivos recursivamente en el árbol de directorios.<br />
    /// ___________________( English )___________________<br />
    /// Counts the total number of files recursively in the directory tree.<br />
    /// </summary>
    /// <param name="pDir">Es: Ruta directa del directorio. <br />En: Direct directory path.</param>
    /// <returns>Es: Cantidad total identificada. <br />En: Total quantity identified.</returns>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Cuenta la cantidad de subcarpetas o niveles contenidos de forma recursiva.<br />
    /// ___________________( English )___________________<br />
    /// Counts the amount of subfolders or levels contained recursively.<br />
    /// </summary>
    /// <param name="pDir">Es: Ruta base. <br />En: Base path.</param>
    /// <returns>Es: Total de directorios encontrados. <br />En: Total directories found.</returns>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Retorna las sumatorias (conteo) de tanto archivos como de subcarpetas en uso.<br />
    /// Notas: No devuelve tamaño en bytes en este método.<br />
    /// ___________________( English )___________________<br />
    /// Returns the sum totals (count) of both files and subfolders in use.<br />
    /// Notes: It doesn't return size in bytes in this method.<br />
    /// </summary>
    /// <param name="pDir">Es: Raíz a escanear. <br />En: Root to scan.</param>
    /// <returns>Es: Tupla (archivos totales, directorios totales). <br />En: Tuple (total files, total directories).</returns>
    public static (long? files, long? direc) GetSizeDir(string pDir)
    {
        long? countFiles = GetCountFiles(pDir);
        long? countDir = GetCountDirectories(pDir);
        return (countFiles, countDir);
    }

}
