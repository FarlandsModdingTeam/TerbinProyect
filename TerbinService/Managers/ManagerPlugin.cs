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
using TerbinService.Data.Manifests;
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
        //-----------------( Dowload/Deleted )-----------------//
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

        
        public static async Task<Status> DeletedOne
            (string pId, CancellationToken pCancellationToken = default)
        {
            bool? r = await Manager.StoragePlugin.Eliminate(pId);
            Status result = r switch
            {
                null => Status.NotFound,
                true => Status.Succes,
                false => Status.GenericError
            };
            return result;
        }


        //-----------------( Install/Unistall )-----------------//
        public static async Task<Status> HandleInstallPlugin(string pPlugin, string pInstance, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = Util.CreateProgessBarr(Worker.CurrentConst.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await InstallOne(pPlugin, pInstance, progress, pCancellationToken);
        }

        public static async Task<Status> InstallOne
            (string pPlugin, string pNameInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            var reference = await Manager.StoragePlugin.Get(pPlugin).ConfigureAwait(false);
            if (reference?.FileName == null)
                return Status.ErrorGetPlugin;

            string? pathPlugin = Manager.StoragePlugin.MakePathPlugin(reference.FileName);
            if (string.IsNullOrEmpty(pathPlugin))
                return Status.ErrorGetPathPlugin;

            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            var result = await Manager.Instances.InstallPlugin(pathPlugin, pNameInstance, true, pProgress, pCancellationToken).ConfigureAwait(false);

            if (pCancellationToken.IsCancellationRequested)
            {
                if (result != null)
                    await Manager.Instances.UnistallPlugin(result, pNameInstance, pCancellationToken: CancellationToken.None);
                return Status.IsCancelled;
            }

            Manager.Manifest.HandleAddPlugin
                (reference.Id ?? $"E:{CodeManifestError.NotAccesId}",
                reference.Name ?? $"E:{CodeManifestError.NotAccesName}",
                pNameInstance, result);

            if (pCancellationToken.IsCancellationRequested)
            {
                if (result != null)
                    await Manager.Instances.UnistallPlugin(result, pNameInstance, pCancellationToken: CancellationToken.None);
                throw new Exception("TODO: Desregistrar.");
            }

            return Status.Succes;
        }


        public static async Task<Status> HandleUnistallOne
            (string pPlugin, string pNameInstance, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = Util.CreateProgessBarr(Worker.CurrentConst.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await UnistallOne(pPlugin, pNameInstance, progress, pCancellationToken);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Desinstala un plugin de una instancia específica.<br />
        /// Recupera el manifiesto del plugin y ejecuta el proceso de desinstalación.<br />
        /// Notas: Puede devolver un estado de error si el manifiesto no existe o la operación es cancelada.<br />
        /// Tips: Asegúrate de proporcionar un nombre de instancia válido para evitar errores de tipo 'InstanceNotExist'.<br />
        /// ___________________( English )___________________<br />
        /// Uninstalls a plugin from a specific instance.<br />
        /// Retrieves the plugin manifest and executes the uninstallation process.<br />
        /// Notes: It can return an error status if the manifest does not exist or the operation is cancelled.<br />
        /// Tips: Make sure to provide a valid instance name to avoid 'InstanceNotExist' errors.<br />
        /// </summary>
        /// <param name="pPlugin">Es: Identificador o nombre del plugin a desinstalar.<br />En: Identifier or name of the plugin to uninstall.</param>
        /// <param name="pNameInstance">Es: Nombre de la instancia de la cual se desinstalará el plugin.<br />En: Name of the instance from which the plugin will be uninstalled.</param>
        /// <param name="pProgress">Es: Proveedor de progreso opcional para reportar el avance de la desinstalación.<br />En: Optional progress provider to report the uninstallation progress.</param>
        /// <param name="pCancellationToken">Es: Token para monitorear las solicitudes de cancelación.<br />En: Token to monitor for cancellation requests.</param>
        /// <returns>Es: El estado final de la operación de desinstalación.<br />En: The final status of the uninstallation operation.</returns>
        public static async Task<Status> UnistallOne
            (string pPlugin, string pNameInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            var (status, manifest) = await GetOne(pPlugin, pNameInstance, pCancellationToken);

            if (status != Status.Succes)
                return status;

            if (manifest?.HandWritten == null)
                return Status.ManifestNotExit;

            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            StatusFileUtil r = await Manager.Instances.UnistallPlugin(manifest.HandWritten, pNameInstance, pProgress, pCancellationToken);
            Status result = r switch
            {
                StatusFileUtil.Succes => Status.Succes,
                StatusFileUtil.IsCancelled => Status.IsCancelled,
                StatusFileUtil.InvalidSource => Status.InstanceNotExist,

                _ => Status.GenericError,
            };
            return result;
        }

        //-----------------( Gets )-----------------//
        // TODO: Mover a Instance.
        public static async Task<(Status status, PluginManifest? manifest)> GetOne(string pPlugin, string pNameInstance, CancellationToken pCancellationToken = default)
        {
            if (pCancellationToken.IsCancellationRequested)
                return (Status.IsCancelled, null);

            var manifest = Manager.Instances.GetManifest(pNameInstance);
            if (manifest == null)
                return (Status.InstanceNotExist, null);

            var information = Manager.Instances.MakePathFolderInformation(pNameInstance);
            if (information == null)
                return (Status.InformationNotExist, null);

            for (int i = 0; i < manifest.Plugins.Count; i++)
            {
                if (pCancellationToken.IsCancellationRequested)
                    return (Status.IsCancelled, null);

                var refe = manifest.Plugins[i];
                if (refe.IdLocal == pPlugin)
                {
                    if (refe.Path == null) continue;

                    string pathJson = Path.IsPathFullyQualified(refe.Path)
                        ? refe.Path
                        : Path.Combine(information, refe.Path);

                    PluginManifest? man = JSonUtil.AcessDirect<PluginManifest>(pathJson);
                    if (man == null)
                        return (Status.ManifestNotExit, null);
                    return (Status.Succes, man);
                }
            }
            return (Status.NotFound, null);
        }
        // TODO: Mover a Instance.
        public static async Task<(Status status, List<PluginManifest>? manifests)> GetAll(string pNameInstance, CancellationToken pCancellationToken = default)
        {
            if (pCancellationToken.IsCancellationRequested)
                return (Status.IsCancelled, null);

            var manifest = Manager.Instances.GetManifest(pNameInstance);
            if (manifest == null)
                return (Status.InstanceNotExist, null);

            var information = Manager.Instances.MakePathFolderInformation(pNameInstance);
            if (information == null)
                return (Status.InformationNotExist, null);

            List<PluginManifest> manis = new();

            for (int i = 0; i < manifest.Plugins.Count; i++)
            {
                if (pCancellationToken.IsCancellationRequested)
                    return (Status.IsCancelled, null);

                var refe = manifest.Plugins[i];
                if (refe.Path == null) continue;

                string pathJson = Path.IsPathFullyQualified(refe.Path)
                    ? refe.Path
                    : Path.Combine(information, refe.Path);

                PluginManifest? man = JSonUtil.AcessDirect<PluginManifest>(pathJson);

                if (man == null) continue;
                manis.Add(man);
            }
            return (Status.Succes, manis);
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

            GenericError = 2,
            ErrorGetPathInstance = 3,
            ErrorGetPlugin = 4,
            ErrorGetPathPlugin = 5,
            ErrorOnDowload = 6,
            InstanceNotExist = 7,
            InformationNotExist = 8,
            ManifestNotExit = 9,

            NotFound = 10,
        }
    }
}
