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


    public static async Task<bool> DownloadWithProgress(string eUrl, string eDestination)
    {
        using var response = await _httpClient.GetAsync(eUrl, HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }

        var total = response.Content.Headers.ContentLength ?? -1L;
        using var stream = await response.Content.ReadAsStreamAsync();
        using var fs = new FileStream(eDestination, FileMode.Create, FileAccess.Write, FileShare.None);


        var buffer = new byte[81920];
        long totalRead = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, read));
            totalRead += read;
            if (total > 0)
            {
                // ESTO LEE.
            }
        }

        return true;
    }

    public static async Task<bool> DownloadWithProgress(
        HttpClient httpClient,
        string url,
        string destination,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        var total = response.Content.Headers.ContentLength;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var fs = new FileStream(
            destination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        var buffer = new byte[81920];
        long totalRead = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, read), cancellationToken);

            totalRead += read;

            if (total.HasValue && progress != null)
            {
                double percent = (double)totalRead / total.Value * 100;
                progress.Report(percent);
            }
        }

        return true;
    }

    public static string DownloadString(string eUrl)
    {
        using var response = _httpClient.GetAsync(eUrl).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}

