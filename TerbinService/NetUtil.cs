using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO.Compression;

namespace TerbinService;

/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayuscula = publica.
  empieza: minuscula = privada.
 */

public static class NetUtil
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<bool> InstallZip(string pUrl, string pDestination, string pNameArchive)
    {
        bool result = true;
        string tmp = "";
        // TODO: Comprobar que carpetas al destino existen.

        try
        {
            if (await DownloadAny(pUrl) is var r && r.success)
            {
                tmp = r.tempFilePath;
                ZipFile.ExtractToDirectory(sourceArchiveFileName: r.tempFilePath,
                                           destinationDirectoryName: pDestination,
                                           overwriteFiles: true);
            }
            else
            {
                result = false;
            }
        }
        catch (Exception e)
        {
            // TODO: guardar error -> DebugLogger.
            result = false;
        }
        finally
        {
            try
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
            catch
            {
                // TODO: guardar error -> DebugLogger.
            }
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
    public static async Task<(bool success, string tempFilePath)> DownloadAny(
                                            string pUrl,
                                            IProgress<double>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        string tmp = Path.Combine(Path.GetTempPath(), $"tmp_{Guid.NewGuid():N}");

        using var response = await GetResponseAsync(pUrl, pCancellationToken);

        if (!response.IsSuccessStatusCode)
            return (false, "");

        var total = response.Content.Headers.ContentLength;

        await using var networkStream = await response.Content.ReadAsStreamAsync(pCancellationToken);
        await using var fileStream = CreateFileStream(tmp);

        await CopyStreamWithProgressAsync(
            networkStream,
            fileStream,
            total,
            pProgress,
            pCancellationToken);

        return (true, tmp);
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
            bufferSize: 81920,
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
                                            IProgress<double>? pProgress,
                                            CancellationToken pCancellationToken)
    {
        var buffer = new byte[81920];
        long totalRead = 0;
        int read;

        while ((read = await pSource.ReadAsync(
                   buffer.AsMemory(0, buffer.Length),
                   pCancellationToken)) > 0)
        {
            await pDestination.WriteAsync(
                buffer.AsMemory(0, read),
                pCancellationToken);

            totalRead += read;

            ReportProgress(totalRead, pTotal, pProgress);
        }
    }
    /// <summary>
    /// Calcula y reporta el porcentaje de progreso de la operación.
    /// </summary>
    /// <param name="pTotalRead">Cantidad total de bytes leídos.</param>
    /// <param name="pTotal">Cantidad total esperada de bytes.</param>
    /// <param name="pProgress">
    /// Objeto opcional para reportar el progreso.
    /// </param>
    /// <remarks>
    /// Si el tamaño total es desconocido o no se proporcionó un
    /// objeto de progreso, no se reporta nada.
    /// </remarks>
    public static void ReportProgress(long pTotalRead, long? pTotal, IProgress<double>? pProgress)
    {
        if (!pTotal.HasValue || pProgress == null)
            return;

        double percent = (double)pTotalRead / pTotal.Value * 100;
        pProgress.Report(percent);
    }



    // NOTA: no se si borrar estas funciones.
    public static bool IsResponseOk(HttpResponseMessage pResponse)
    {
        return pResponse.IsSuccessStatusCode;
    }
    public static long? GetContentLength(HttpResponseMessage pResponse)
    {
        return pResponse.Content.Headers.ContentLength;
    }
    public static Task<Stream> GetNetworkStreamAsync(HttpResponseMessage pResponse, CancellationToken pCancellationToken)
    {
        return pResponse.Content.ReadAsStreamAsync(pCancellationToken);
    }
}

