using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Data;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.NetWork;
using TerbinLibrary.Useful.Nodes;
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


public static partial class Manager
{
    public static class Plugin
    {
        [Obsolete]
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
            string? pathInstance = Manager.Instances.MakePathFolder(pNameInstance);

            if (pathInstance is null) return null;
            if (!Manager.BepInEx.CheckInstallBepInEx(pathInstance)) return null;

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

            Manager.Manifest.HandleAddPlugin(pNameInstace, json);

            return status;
        }

        public static async Task<Status> HandleInstallPlugin(string pPlugin, string pInstance, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = Util.CreateProgessBarr(Worker.CurrentConst.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await InstallOne(pPlugin, pInstance, progress, pCancellationToken);
        }
        public static async Task<Status> HandleDowloadPlugin(string pUrl, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = Util.CreateProgessBarr(Worker.CurrentConst.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await DowloadOne(pUrl, progress, pCancellationToken);
        }

        public static async Task<Status> DowloadOne
            (string pUrl, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            if (await NetUtil.DownloadAny(pUrl, pProgress) is var r && r.status != StatusNetUtil.Succes)
                return Status.ErrorOnDowload;   

            Guid? id = null;
            string nameFile = NetUtil.GetFileName(pUrl);
            if (!pCancellationToken.IsCancellationRequested)
                id = await Manager.StoragePlugin.Store(r.tempFilePath, nameFile, false).ConfigureAwait(false);
            try
            {
                File.Delete(r.tempFilePath);
            }
            catch
            {
                return Status.ExceptionOnDeteledTmp;
            }
            finally
            {
                if (pCancellationToken.IsCancellationRequested && id is not null)
                    await Manager.StoragePlugin.Eliminate($"{id:N}").ConfigureAwait(false);
            }
            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;
            return Status.Succes;
        }

        public static async Task<Status> InstallOne
            (string pPlugin, string pInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            string? pathInstance = Manager.Instances.MakePathFolder(pInstance);
            if (string.IsNullOrEmpty(pathInstance))
                return Status.ErrorGetPathInstance;

            var reference = await Manager.StoragePlugin.Get(pPlugin).ConfigureAwait(false);
            if (reference?.FileName == null)
                return Status.ErrorGetPlugin;

            string? pathPlugin = Manager.StoragePlugin.MakePathPlugin(reference.FileName);
            if (string.IsNullOrEmpty(pathPlugin))
                return Status.ErrorGetPathPlugin;

            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            var result = await ZipUtil.ExtractWithProgress(pathPlugin, pathInstance, pProgress, true, pCancellationToken).ConfigureAwait(false);

            if (pCancellationToken.IsCancellationRequested)
                throw new Exception("TODO: Desinstalar.");

            Manager.Manifest.HandleAddPlugin(pInstance, result);

            if (pCancellationToken.IsCancellationRequested)
                throw new Exception("TODO: Desinstalar completo.");

            return Status.Succes;
        }




        public static string? MakePathPluginByName(string pNameInstance)
        {
            string? pathInstance;
            string pathPlugin;
            pathInstance = Manager.Instances.MakePathFolder(pNameInstance);
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



        public enum Status : sbyte
        {
            ExceptionOnDeteledTmp = -1,

            IsCancelled = 0,
            Succes = 1,

            ErrorGetPathInstance = 3,
            ErrorGetPlugin = 4,
            ErrorGetPathPlugin = 5,
            ErrorOnDowload = 6,
            ErrroDeletedTmp = 7,
        }
    }
}
