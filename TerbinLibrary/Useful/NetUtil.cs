using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO.Compression;
using TerbinLibrary.Serialize;

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

    public static async Task<StatusNetUtil> InstallZipWithProgress(
                                            string pUrl,
                                            string pDestination,
                                            IProgress<TerbinInfoProgrss>? pProgressZip = null,
                                            IProgress<TerbinInfoProgrss>? pProgressDowload = null,
                                            CancellationToken pCancellationToken = default)
    {
        StatusNetUtil result = StatusNetUtil.Succes;
        string tmp = "";

        if (!Directory.Exists(pDestination))
            return StatusNetUtil.DestinationInvalid;

        if (await DownloadAny(pUrl, pProgressDowload) is var r && r.status == StatusNetUtil.Succes)
        {
            tmp = r.tempFilePath;
            try
            {
                var json = await ZipUtil.ExtractWithProgress(r.tempFilePath, pDestination, pProgressZip);
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

    /// <summary>
    /// Descarga un recurso desde la URL especificada y lo guarda en un archivo temporal,
    /// informando opcionalmente del progreso.
    /// </summary>
    /// <param name="pUrl">
    /// URL del recurso a descargar.
    /// </param>
    /// <param name="pProgress">
    /// Objeto opcional para reportar el progreso de la descarga (valores entre 0.0 y 1.0).
    /// </param>
    /// <param name="pCancellationToken">
    /// Token que permite cancelar la operación.
    /// </param>
    /// <returns>
    /// Una tupla donde:
    /// <list type="bullet">
    /// <item>
    /// <description><c>success</c>: Indica si la descarga se completó correctamente.</description>
    /// </item>
    /// <item>
    /// <description><c>tempFilePath</c>: Ruta del archivo temporal descargado. Cadena vacía si falló.</description>
    /// </item>
    /// </list>
    /// </returns>
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
    /// Envía una solicitud HTTP GET y devuelve la respuesta sin descargar
    /// el contenido completo en memoria.
    /// </summary>
    /// <param name="pUrl">URL a solicitar.</param>
    /// <param name="pCancellationToken">Token de cancelación.</param>
    /// <returns>
    /// Un <see cref="HttpResponseMessage"/> con los encabezados disponibles.
    /// </returns>
    /// <remarks>
    /// Usa <see cref="HttpCompletionOption.ResponseHeadersRead"/> para permitir
    /// procesamiento en streaming del contenido.
    /// </remarks>
    public static Task<HttpResponseMessage> GetResponseAsync(string pUrl, CancellationToken pCancellationToken)
    {
        return _httpClient.GetAsync(
            pUrl,
            HttpCompletionOption.ResponseHeadersRead,
            pCancellationToken);
    }
    /// <summary>
    /// Crea un <see cref="FileStream"/> configurado para escritura asíncrona.
    /// </summary>
    /// <param name="pDestination">Ruta completa del archivo a crear.</param>
    /// <returns>
    /// Un flujo de archivo listo para escritura.
    /// </returns>
    /// <remarks>
    /// El archivo se sobrescribe si ya existe.
    /// </remarks>
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
    /// Copia datos desde un flujo origen a un flujo destino reportando
    /// el progreso de la operación.
    /// </summary>
    /// <param name="pSource">Flujo de origen.</param>
    /// <param name="pDestination">Flujo de destino.</param>
    /// <param name="pTotal">
    /// Tamaño total esperado en bytes. Puede ser <see langword="null"/>
    /// si se desconoce.
    /// </param>
    /// <param name="pProgress">
    /// Objeto opcional para reportar el progreso en porcentaje.
    /// </param>
    /// <param name="pCancellationToken">Token de cancelación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
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

    public static async Task<HttpResponseMessage> GetHead(string pUrl, CancellationToken pCancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, pUrl);
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, pCancellationToken);
    }

    // NOTA: no se si borrar estas funciones.
    public static bool IsResponseOk(HttpResponseMessage pResponse)
    {
        return pResponse.IsSuccessStatusCode;
    }
    public static Task<Stream> GetNetworkStreamAsync(HttpResponseMessage pResponse, CancellationToken pCancellationToken)
    {
        return pResponse.Content.ReadAsStreamAsync(pCancellationToken);
    }
}

