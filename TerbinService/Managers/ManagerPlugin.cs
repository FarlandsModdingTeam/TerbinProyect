using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinService.Services;

namespace TerbinService.Managers;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


public static class ManagerPlugin
{


    public static async Task HandleInstallPluginWithProgress(string pNameInstace, byte pIdDownload, byte pIdExtract, string pPathPlugin, string pUrl)
    {
        IProgress<TerbinInfoProgrss> progressBarrDownload = new Progress<TerbinInfoProgrss>(p =>
        {
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdDownload, p.Serialize());
            Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        IProgress<TerbinInfoProgrss> progressBarrExtract = new Progress<TerbinInfoProgrss>(p =>
        {
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdExtract, p.Serialize());
            Console.Write($"\rInstalando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        try
        {
            StatusNetUtil r = await HandleInstallPlugin(pNameInstace, pUrl, pPathPlugin, progressBarrExtract, progressBarrDownload);
            if (r != StatusNetUtil.Succes)
            {
                CodeInternalErrors error = r switch
                {
                    StatusNetUtil.ExceptionOnExtractZip => CodeInternalErrors.ZipExtractException,
                    StatusNetUtil.ExceptionDeleteTemporalFile => CodeInternalErrors.ZipDeletedTempException,
                    _ => CodeInternalErrors.ZipExtractError
                };
                throw new Exception($"TODO: informar de {error}, {r}");

                // Prototipo del funcionamiento de Info
                AmongInfoThreads info = Worker.CurrentConst.Value;
                byte[] pld = new Serialineitor()
                    .Add(TypeService.Service)
                    .Add(CodeServices.Dowload)
                    .Add(error)
                    .Serialize();
                _ = info.Communicator.Send(new((byte)CodeTerbinProtocol.ExceptionAlert), pld);
            }
        }
        catch (Exception e)
        {
            e.PrintException("HandleInstallPluginWithProgress");
        }
    }
    [Obsolete]
    public static async Task<StatusNetUtil?> SimpleInstallPlugin(
        string pNameInstance,
        string pUrl,
        IProgress<TerbinInfoProgrss>? pProgressDownload = default,
        IProgress<TerbinInfoProgrss>? pProgressExtract = default)
    {
        StatusNetUtil r = StatusNetUtil.Succes;
        string? pathInstance = ManagerInstances.MakePathFolder(pNameInstance);

        if (pathInstance is null) return null;
        if (!ManagerBepInEx.CheckInstallBepInEx(pathInstance)) return null;

        r = await HandleInstallPlugin(pNameInstance, pUrl, pathInstance, pProgressExtract, pProgressDownload);
        return r;
    }
    [Obsolete]
    public static async Task<StatusNetUtil> HandleInstallPlugin(
                                            string pNameInstace,
                                            string pUrl,
                                            string pPathPlugin,
                                            IProgress<TerbinInfoProgrss>? pProgressZip = null,
                                            IProgress<TerbinInfoProgrss>? pProgressDowload = null,
                                            CancellationToken pCancellationToken = default)
    {
        if (!Directory.Exists(pPathPlugin))
            Directory.CreateDirectory(pPathPlugin);

        var (status, json) = await NetUtil.InstallZipWithProgress(pUrl, pPathPlugin, pProgressZip, pProgressDowload);

        ManagerManifest.HandleAddPlugin(pNameInstace, json);

        return status;
    }



    public static async Task DowloadOne
        (string pUrl, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
    {
        
    }

    public static async Task InstallOne
        (Guid pPlugin, string pInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
    {

    }






    public static string? MakePathPluginByName(string pNameInstance)
    {
        string? pathInstance;
        string pathPlugin;
        pathInstance = ManagerInstances.MakePathFolder(pNameInstance);
        if (pathInstance is null)
            return null;
        pathPlugin = Path.Combine(pathInstance, TerbinServiceConst.PATH_BEPINEX_PLUGIN);
        if (!Directory.Exists(pathPlugin))
            Directory.CreateDirectory(pathPlugin);
        return pathPlugin;
    }

    public static string MakePathPluginByInstance(string pPathInstance)
    {
        string pathPlugin;
        pathPlugin = Path.Combine(pPathInstance, TerbinServiceConst.PATH_BEPINEX_PLUGIN);
        if (!Directory.Exists(pathPlugin))
            Directory.CreateDirectory(pathPlugin);
        return pathPlugin;
    }

    public static string MakeNameByFile(string pFile)
    {
        if (string.IsNullOrWhiteSpace(pFile))
            return string.Empty;

        string fileName = Path.GetFileNameWithoutExtension(pFile);

        fileName = fileName.Replace('_', ' ').Replace('-', ' ');

        return string.Join(' ', fileName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
