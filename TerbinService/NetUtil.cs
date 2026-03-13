using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

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

    public static (bool succes, string archive) DowloadAny(string pDest, string pArchive)
    {
        // TODO: Comprobar que carpetas al destino existen.

        string tmp = Path.Combine(Path.GetTempPath(), $"temporal_{Guid.NewGuid():N}");
        //try ()
        //{

        //}
        //catch ()
        //{

        //}

        throw new NotImplementedException();
    }


    public static async Task<bool> DownloadWithProgress(
                                            string pUrl,
                                            string pDestination,
                                            IProgress<double>? pProgress = null,
                                            CancellationToken pCancellationToken = default)
    {
        using var response = await GetResponseAsync(pUrl, pCancellationToken);

        if (!IsResponseOk(response))
            return false;

        var total = GetContentLength(response);

        await using var networkStream = await GetNetworkStreamAsync(response, pCancellationToken);
        await using var fileStream = CreateFileStream(pDestination);

        await CopyStreamWithProgressAsync(
            networkStream,
            fileStream,
            total,
            pProgress,
            pCancellationToken);

        return true;
    }

    public static bool IsResponseOk(HttpResponseMessage pResponse)
    {
        return pResponse.IsSuccessStatusCode;
    }
    public static Task<HttpResponseMessage> GetResponseAsync(string pUrl, CancellationToken pCancellationToken)
    {
        return _httpClient.GetAsync(
            pUrl,
            HttpCompletionOption.ResponseHeadersRead,
            pCancellationToken);
    }
    public static long? GetContentLength(HttpResponseMessage pResponse)
    {
        return pResponse.Content.Headers.ContentLength;
    }
    public static Task<Stream> GetNetworkStreamAsync(HttpResponseMessage pResponse, CancellationToken pCancellationToken)
    {
        return pResponse.Content.ReadAsStreamAsync(pCancellationToken);
    }
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
    public static async Task CopyStreamWithProgressAsync(Stream pSource, Stream pDestination, long? pTotal, IProgress<double>? pProgress, CancellationToken pCancellationToken)
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
    public static void ReportProgress(long pTotalRead, long? pTotal, IProgress<double>? pProgress)
    {
        if (!pTotal.HasValue || pProgress == null)
            return;

        double percent = (double)pTotalRead / pTotal.Value * 100;
        pProgress.Report(percent);
    }





    public static string DownloadString(string eUrl)
    {
        using var response = _httpClient.GetAsync(eUrl).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}

