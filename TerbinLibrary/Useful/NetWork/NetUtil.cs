using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO.Compression;
using TerbinLibrary.Data;
using TerbinLibrary.Useful.Nodes;

namespace TerbinLibrary.Useful.NetWork;
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
/// Enumeración que representa los posibles estados de las operaciones de red.<br />
/// ___________________( English )___________________<br />
/// Enum representing the possible statuses of network operations.<br />
/// </summary>
public enum StatusNetUtil : sbyte
{
    ExceptionOnExtractZip = -12,
    ExceptionDeleteTemporalFile = -11,
    ExceptionOnDownload = -10,

    Succes = 1,

    InvalidURL = 2,
    ErrorOnDownload = 3,
    NotSuchSpace = 4,
    DestinationInvalid = 5,
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase de utilidad estática para realizar distintas operaciones de red y descargas.<br />
/// ___________________( English )___________________<br />
/// Static utility class to perform various network operations and downloads.<br />
/// </summary>
public static class NetUtil
{
    public const int BUFFER_SIZE = 81920;

    // Descomentar para http y comentar para https 
    //static NetUtil() // Constructor estático para configurar el cliente
    //{
    //    _httpClient.DefaultRequestHeaders.Add("User-Agent", "TerbinService-Downloader/0.0.9");
    //}

    // TODO: tener uno en configuracion y pasarlo por funcion.
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Descarga un archivo ZIP desde una URL y lo extrae en el destino indicado.<br />
    /// Notas: Los contenidos existentes en el destino serán sobrescritos.<br />
    /// ___________________( English )___________________<br />
    /// Downloads a ZIP file from a URL and extracts it to the indicated destination.<br />
    /// Notes: Existing contents in the destination will be overwritten.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL remota del archivo ZIP a descargar. <br />En: Remote URL of the ZIP file to download.</param>
    /// <param name="pDestination">Es: Ruta local para la extracción. <br />En: Local path for extraction.</param>
    /// <param name="pProgress">Es: Objeto para reportar progreso. <br />En: Object to report progress.</param>
    /// <param name="pCancellationToken">Es: Token para cancelación. <br />En: Token for cancellation.</param>
    /// <returns>Es: Un indicador de estado. <br />En: A status indicator.</returns>
    public static async Task<StatusNetUtil> InstallZip(
                                            string pUrl,
                                            string pDestination,
                                            IProgress<TerbinInfoProgrss>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        StatusNetUtil result = StatusNetUtil.Succes;
        string tmp = "";

        if (!Directory.Exists(pDestination))
            return StatusNetUtil.DestinationInvalid;

        if (await DownloadAny(pUrl, pProgress) is var r && r.status == StatusNetUtil.Succes)
        {
            tmp = r.tempFilePath;
            try
            {
                ZipFile.ExtractToDirectory(sourceArchiveFileName: r.tempFilePath,
                                           destinationDirectoryName: pDestination,
                                           overwriteFiles: true);
            }
            catch
            {
                result = StatusNetUtil.ExceptionOnExtractZip;
            }
        }
        else
        {
            result = r.status;
        }

        try
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
        catch
        {
            result = StatusNetUtil.ExceptionDeleteTemporalFile;
        }

        return result;
    }


    [Obsolete]
    public static async Task<(StatusNetUtil status, DirectoryHandwritten? json)> InstallZipWithProgress(
                                            string pUrl,
                                            string pDestination,
                                            IProgress<TerbinInfoProgrss>? pProgressZip = null,
                                            IProgress<TerbinInfoProgrss>? pProgressDowload = null,
                                            CancellationToken pCancellationToken = default)
    {
        StatusNetUtil result = StatusNetUtil.Succes;
        string tmp = "";
        DirectoryHandwritten? json = null;

        if (!Directory.Exists(pDestination))
            Directory.CreateDirectory(pDestination);//return(StatusNetUtil.DestinationInvalid, null);

        if (await DownloadAny(pUrl, pProgressDowload) is var r && r.status == StatusNetUtil.Succes)
        {
            tmp = r.tempFilePath;
            try
            {
                json = await ZipUtil.ExtractWithProgress(r.tempFilePath, pDestination, pProgressZip);
            }
            catch
            {
                result = StatusNetUtil.ExceptionOnExtractZip;
            }
        }
        else
        {
            result = r.status;
        }

        try
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
        catch
        {
            result = StatusNetUtil.ExceptionDeleteTemporalFile;
        }

        return (result, json);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Descarga un recurso desde la URL especificada y lo guarda temporalmente.<br />
    /// ___________________( English )___________________<br />
    /// Downloads a resource from the specified URL and saves it temporarily.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL del recurso. <br />En: URL of the resource.</param>
    /// <param name="pProgress">Es: Reporta el progreso (0 a 100). <br />En: Reports the progress (0 a 100).</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Tupla con estado y ruta del archivo. <br />En: Tuple with status and file path.</returns>
    public static async Task<(StatusNetUtil status, string tempFilePath)> DownloadAny(
                                            string pUrl,
                                            IProgress<TerbinInfoProgrss>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        string tmp = Path.Combine(Path.GetTempPath(), $"terbin_tmp_{Guid.NewGuid():N}");

        if (!Uri.TryCreate(pUrl, UriKind.Absolute, out _))
            return (StatusNetUtil.InvalidURL, "");
        try
        {
            using var response = await GetResponseAsync(pUrl, pCancellationToken);

            if (!response.IsSuccessStatusCode)
                return (StatusNetUtil.ErrorOnDownload, "");

            var total = response.Content.Headers.ContentLength;

            var driveInfo = new DriveInfo(tmp);
            if (total.HasValue && driveInfo.AvailableFreeSpace < total.Value)
                return (StatusNetUtil.NotSuchSpace, "");

            await using var networkStream = await response.Content.ReadAsStreamAsync(pCancellationToken);
            await using var fileStream = CreateFileStream(tmp);

            await CopyStreamWithProgressAsync(
                networkStream,
                fileStream,
                total,
                pProgress,
                pCancellationToken);

            return (StatusNetUtil.Succes, tmp);
        }
        catch (Exception e)
        {
            return (StatusNetUtil.ExceptionOnDownload, e.Message);
        }
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Envía una petición HTTP GET recibiendo la respuesta sin descargarla.<br />
    /// Notas: Resulta útil para streaming utilizando ResponseHeadersRead.<br />
    /// ___________________( English )___________________<br />
    /// Sends an HTTP GET request receiving the response without downloading it.<br />
    /// Notes: Useful for streaming using ResponseHeadersRead.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL a solicitar. <br />En: URL to request.</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Respuesta HTTP con encabezados. <br />En: HTTP response with headers.</returns>
    public static Task<HttpResponseMessage> GetResponseAsync(string pUrl, CancellationToken pCancellationToken)
    {
        return _httpClient.GetAsync(
            pUrl,
            HttpCompletionOption.ResponseHeadersRead,
            pCancellationToken);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea un FileStream configurado para escritura asíncrona.<br />
    /// Notas: El archivo se sobrescribe si ya existe.<br />
    /// ___________________( English )___________________<br />
    /// Creates a FileStream configured for asynchronous writing.<br />
    /// Notes: The file is overwritten if it already exists.<br />
    /// </summary>
    /// <param name="pDestination">Es: Ruta donde crear o sobrescribir el archivo. <br />En: Path where to create or overwrite the file.</param>
    /// <returns>Es: Flujo de archivo para escritura. <br />En: File stream for writing.</returns>
    public static FileStream CreateFileStream(string pDestination)
    {
        return new FileStream(
            pDestination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: BUFFER_SIZE,
            useAsync: true);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Copia de un flujo a otro, reportando el progreso asíncronamente.<br />
    /// ___________________( English )___________________<br />
    /// Copies from one stream to another, reporting progress asynchronously.<br />
    /// </summary>
    /// <param name="pSource">Es: Flujo de entrada. <br />En: Input stream.</param>
    /// <param name="pDestination">Es: Flujo de salida. <br />En: Output stream.</param>
    /// <param name="pTotal">Es: Tamaño total (bytes), si es conocido. <br />En: Total size (bytes), if known.</param>
    /// <param name="pProgress">Es: Objeto que recibe el porcentaje. <br />En: Object receiving the percentage.</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Tarea asíncrona. <br />En: Asynchronous task.</returns>
    public static async Task CopyStreamWithProgressAsync(
                                            Stream pSource,
                                            Stream pDestination,
                                            long? pTotal,
                                            IProgress<TerbinInfoProgrss>? pProgress,
                                            CancellationToken pCancellationToken)
    {
        var buffer = new byte[BUFFER_SIZE];
        long currentRead = 0;
        int read;
        bool last = false;

        double? totalInverse = Util.GetInverse(pTotal);
        int lastPercentage = -1;
        while ((read = await pSource.ReadAsync(
                   buffer.AsMemory(0, buffer.Length),
                   pCancellationToken)) > 0)
        {
            await pDestination.WriteAsync(
                buffer.AsMemory(0, read),
                pCancellationToken);

            currentRead += read;

            last = (pTotal.HasValue) ? (currentRead >= pTotal.Value) : false;

            Util.TryReportProgressPercent(currentRead, totalInverse, pProgress, last, ref lastPercentage);
        }
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene la longitud del contenido (en bytes) de la URL proporcionada.<br />
    /// Notas: Interamente utiliza una petición HEAD.<br />
    /// ___________________( English )___________________<br />
    /// Gets the content length (in bytes) of the provided URL.<br />
    /// Notes: Internally uses a HEAD request.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL del archivo a verificar. <br />En: URL of the file to verify.</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Tamaño (bytes) o null si falla. <br />En: Size (bytes) or null if it fails.</returns>
    public static async Task<long?> GetContentLength(string pUrl, CancellationToken pCancellationToken = default)
    {
        if (!Uri.TryCreate(pUrl, UriKind.Absolute, out _))
            return null;

        try
        {
            using var response = await GetHead(pUrl, pCancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response.Content.Headers.ContentLength;
            }
        }
        catch (Exception)
        {
            
        }

        return null;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza una solicitud HTTP HEAD y obtiene sus cabeceras sin procesar todo el cuerpo.<br />
    /// ___________________( English )___________________<br />
    /// Performs an HTTP HEAD request and retrieves its headers without fetching the entire body.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL de destino. <br />En: Destination URL.</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Respuesta HTTP con encabezados. <br />En: HTTP response with headers.</returns>
    public static async Task<HttpResponseMessage> GetHead(string pUrl, CancellationToken pCancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, pUrl);
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, pCancellationToken);
    }

    // NOTA: no se si borrar estas funciones.

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Comprueba si la respuesta HTTP indica éxito (2xx).<br />
    /// ___________________( English )___________________<br />
    /// Checks if the HTTP response indicates success (2xx).<br />
    /// </summary>
    /// <param name="pResponse">Es: Mensaje HTTP de respuesta. <br />En: HTTP response message.</param>
    /// <returns>Es: True en caso de éxito. <br />En: True on success.</returns>
    public static bool IsResponseOk(HttpResponseMessage pResponse)
    {
        return pResponse.IsSuccessStatusCode;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el flujo de datos (Stream) desde una respuesta HTTP.<br />
    /// ___________________( English )___________________<br />
    /// Gets the data stream from an HTTP response.<br />
    /// </summary>
    /// <param name="pResponse">Es: Mensaje HTTP de respuesta. <br />En: HTTP response message.</param>
    /// <param name="pCancellationToken">Es: Token de cancelación. <br />En: Cancellation token.</param>
    /// <returns>Es: Flujo con los datos. <br />En: Stream with the data.</returns>
    public static Task<Stream> GetNetworkStreamAsync(HttpResponseMessage pResponse, CancellationToken pCancellationToken)
    {
        return pResponse.Content.ReadAsStreamAsync(pCancellationToken);
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el nombre del archivo ubicado al final de una URL.<br />
    /// ___________________( English )___________________<br />
    /// Gets the file name located at the end of a URL.<br />
    /// </summary>
    /// <param name="pUrl">Es: URL que contiene el archivo. <br />En: URL containing the file.</param>
    /// <returns>Es: Nombre final del recurso/archivo. <br />En: Final name of the resource/file.</returns>
    public static string GetFileName(string pUrl)
    {
        string rute;
        Uri uri = new Uri(pUrl);

        if (pUrl.EndsWith('/'))
            rute = uri.AbsolutePath.TrimEnd('/');
        else
            rute = uri.AbsolutePath;

        return Path.GetFileName(rute);
    }
}

