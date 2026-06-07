using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Async;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Data.Manifests;
using TerbinLibrary.Data.References;
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
        public const string ROOT = /*Game*/"/";
        public static readonly string BEPINEX_PLUGINS = Path.Combine(ROOT, "BepInEx/plugins");
        public static readonly string MELONLOADER_MODS = Path.Combine(ROOT, "Mods");

        public static string Root
        {
            get => ROOT;
        }
        public static string BepInExPlugin
        {
            get => Path.Combine(ROOT);
        }



        private static readonly SemaphoreByKey<string> _locks = new(StringComparer.OrdinalIgnoreCase);


        //-----------------( Dowload/Deleted )-----------------//
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Maneja la descarga de un plugin usando su URL.<br />
        /// Crea una barra de progreso y luego procede con la descarga.<br />
        /// Notas: Envía la información de progreso mediante un comunicador.<br />
        /// Tips: Asegúrate de que el idRequest sea único para cada solicitud.<br />
        /// ___________________( English )___________________<br />
        /// Handles the downloading of a plugin using its URL.<br />
        /// Creates a progress bar and then proceeds with the download.<br />
        /// Notes: Sends progress information via a communicator.<br />
        /// Tips: Ensure that idRequest is unique for each request.<br />
        /// </summary>
        /// <param name="pUrl">Es: URL desde donde se descargará el plugin.<br />En: URL from where the plugin will be downloaded.</param>
        /// <param name="pIdRequest">Es: Identificador de la solicitud para el progreso.<br />En: Request identifier for progress tracking.</param>
        /// <param name="pCancellationToken">Es: Token para monitorear las solicitudes de cancelación.<br />En: Token to monitor for cancellation requests.</param>
        /// <param name="pMethod">Es: Parámetros opcionales para la creación del progreso.<br />En: Optional parameters for progress creation.</param>
        /// <returns>Es: El estado de la operación de descarga.<br />En: The status of the download operation.</returns>
        public static async Task<Status> HandleDowloadPlugin(string pUrl, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = ProgressUtil.CreateProgessBarr(Worker.CurrentContext.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await DowloadOne(pUrl, progress, pCancellationToken);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Realiza la descarga de un plugin.<br />
        /// Descarga el archivo de la URL dada, lo guarda en el almacenador y borra el archivo temporal.<br />
        /// Notas: Si se cancela, se elimina el archivo descargado.<br />
        /// Tips: Puede fallar si la URL es incorrecta o no hay acceso a internet.<br />
        /// ___________________( English )___________________<br />
        /// Performs the download of a plugin.<br />
        /// Downloads the file from the given URL, stores it in storage, and deletes the temp file.<br />
        /// Notes: If cancelled, the downloaded file is eliminated.<br />
        /// Tips: Can fail if the URL is invalid or there's no internet access.<br />
        /// </summary>
        /// <param name="pUrl">Es: URL de descarga del plugin.<br />En: Download URL of the plugin.</param>
        /// <param name="pProgress">Es: Proveedor de progreso de la descarga.<br />En: Download progress provider.</param>
        /// <param name="pCancellationToken">Es: Token de cancelación.<br />En: Cancellation token.</param>
        /// <returns>Es: El estado tras la descarga.<br />En: The status after downloading.</returns>
        public static async Task<Status> DowloadOne
            (string pUrl, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            if (await NetUtil.DownloadAny(pUrl, pProgress) is var r && r.status != StatusNetUtil.Succes)
                return r.status switch
                {
                    StatusNetUtil.NotSuchSpace => Status.NotSuchSpace,
                    StatusNetUtil.InvalidURL => Status.InvalidURL,
                    _ => Status.ErrorOnDowload,
                };

            Guid? id = null;
            try
            {
                string nameFile = NetUtil.GetFileName(pUrl);
                if (!pCancellationToken.IsCancellationRequested)
                    id = await Manager.StoragePlugin.Store(r.tempFilePath, nameFile, false).ConfigureAwait(false);
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

        
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Elimina un plugin guardado en el gestor de almacenamiento.<br />
        /// Utiliza el identificador asociado al plugin para removerlo por completo del sistema.<br />
        /// Notas: El proceso es definitivo y elimina el archivo físico si se encuenta.<br />
        /// Tips: Asegúrate de que el ID pertenezca a un plugin que ciertamente deba ser eliminado.<br />
        /// ___________________( English )___________________<br />
        /// Deletes a plugin saved in the storage manager.<br />
        /// Uses the identifier associated with the plugin to completely remove it from the system.<br />
        /// Notes: The process is final and deletes the physical file if found.<br />
        /// Tips: Make sure the ID belongs to a plugin that certainly should be deleted.<br />
        /// </summary>
        /// <param name="pId">Es: Identificador único del plugin a eliminar.<br />En: Unique identifier of the plugin to delete.</param>
        /// <param name="pCancellationToken">Es: Token para monitorear las solicitudes de cancelación.<br />En: Token to monitor for cancellation requests.</param>
        /// <returns>Es: El resultado de la operación de borrado.<br />En: The result of the deletion operation.</returns>
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
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Maneja la inicialización del proceso de instalación de un plugin para una instancia.<br />
        /// Prepara la barra de progreso usando el comunicador configurado y procede con la instalación.<br />
        /// Notas: Intercepta las credenciales de comunicación según el worker actual.<br />
        /// Tips: Ideal para ser llamado directamente desde los handlers de red o IPC.<br />
        /// ___________________( English )___________________<br />
        /// Handles the initialization of the plugin installation process for an instance.<br />
        /// Prepares the progress bar using the configured communicator and proceeds with the installation.<br />
        /// Notes: Intercepts communication credentials according to the current worker.<br />
        /// Tips: Ideal to be called directly from network or IPC handlers.<br />
        /// </summary>
        /// <param name="pPlugin">Es: El nombre o identificador del plugin.<br />En: The name or identifier of the plugin.</param>
        /// <param name="pInstance">Es: El nombre de la instancia en la que instalar.<br />En: The name of the instance where to install.</param>
        /// <param name="pIdRequest">Es: Id de la solicitud que origina esta acción.<br />En: Request Id that originated this action.</param>
        /// <param name="pCancellationToken">Es: Token para monitorear las solicitudes de cancelación.<br />En: Token to monitor for cancellation requests.</param>
        /// <param name="pMethod">Es: Parámetros de progreso adicionales.<br />En: Additional progress parameters.</param>
        /// <returns>Es: Estado resultante de la operación.<br />En: Resulting status of the operation.</returns>
        public static async Task<Status> HandleInstallPlugin(string pPlugin, string pInstance, string pTarjetPath, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = ProgressUtil.CreateProgessBarr(Worker.CurrentContext.Value.Communicator, pIdRequest, pMethod: pMethod);

            return await InstallOne(pPlugin, pInstance, pTarjetPath, progress, pCancellationToken);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Realiza la instalación de un plugin previamente almacenado en una instancia específica.<br />
        /// Extrae la referencia del plugin, calcula su ruta e invoca la instalación física. Si algo falla o se cancela, se intenta revertir los cambios.<br />
        /// Notas: Actualiza el manifiesto de la instancia si la instalación es satisfactoria.<br />
        /// Tips: Verifica que el plugin ya haya sido descargado y figure en el Storage.<br />
        /// ___________________( English )___________________<br />
        /// Performs the installation of a previously stored plugin in a specific instance.<br />
        /// Extracts the plugin reference, calculates its path, and invokes physical installation. If something fails or is canceled, changes are rolled back.<br />
        /// Notes: Updates the instance manifest if installation is successful.<br />
        /// Tips: Verify that the plugin has already been downloaded and is in Storage.<br />
        /// </summary>
        /// <param name="pPlugin">Es: Identificador del plugin dentro del almacenamiento.<br />En: Identifier of the plugin within the storage.</param>
        /// <param name="pNameInstance">Es: Nombre de la instancia objetivo.<br />En: Target instance name.</param>
        /// <param name="pProgress">Es: Rastreador de progreso para la operación.<br />En: Progress tracker for the operation.</param>
        /// <param name="pCancellationToken">Es: Token para cancelar la instalación.<br />En: Token to cancel the installation.</param>
        /// <returns>Es: Estado de la operación tras intentar la instalación.<br />En: Status of the operation after attempting the installation.</returns>
        public static async Task<Status> InstallOne
            (string pPlugin, string pNameInstance, string pTarjetPath, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            var reference = await Manager.StoragePlugin.Get(pPlugin).ConfigureAwait(false);
            if (reference?.FileName == null)
                return Status.ErrorGetPlugin;

            string? pathPlugin = Manager.StoragePlugin.MakePathPlugin(reference.FileName);
            if (string.IsNullOrEmpty(pathPlugin))
                return Status.ErrorGetPathPlugin;

            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            var result = await Manager.Instances.InstallPlugin
                (pathPlugin, pNameInstance, pTarjetPath, true, pProgress, pCancellationToken).ConfigureAwait(false);

            if (pCancellationToken.IsCancellationRequested)
            {
                if (result != null)
                    await Manager.Instances.UnistallPlugin(result, pNameInstance, pCancellationToken: CancellationToken.None);
                return Status.IsCancelled;
            }

            string id = reference.Id ?? $"E:{CodeManifestError.NotAccesId}";
            string namePlugin = reference.Name ?? $"E:{CodeManifestError.NotAccesName}";
            bool inInstance = Manager.Instances.InsideConfig(pNameInstance, pTarjetPath);

            var (status, local) = Manager.Manifest.HandleAddPlugin
                (id, namePlugin, pNameInstance, inInstance, result);

            if (status != Manager.Manifest.Status.Succes)
                return status switch
                {
                    Manifest.Status.ErrorGetManifest => Status.ErrorGetManifest,
                    Manifest.Status.ErrorOnSaveManifest => Status.ErrorOnSaveManifest,
                    _ => Status.GenericError,
                };

            if (pCancellationToken.IsCancellationRequested)
            {
                if (result != null)
                    await Instances.UnistallPlugin(result, pNameInstance, pCancellationToken: CancellationToken.None);
                Manifest.HandleRemovePlugin(local, pNameInstance);
                return Status.IsCancelled;
            }

            return Status.Succes;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Maneja el proceso de desinstalación creando la interfaz de progreso y llamando al método subyacente.<br />
        /// Enlaza el proceso de cancelación y el tracker al Worker actual.<br />
        /// Notas: Usa el comunicador disponible en Worker.CurrentConst.<br />
        /// Tips: Evita llamadas concurrentes sobre el mismo plugin y la misma instancia para evitar bloqueos.<br />
        /// ___________________( English )___________________<br />
        /// Handles the uninstallation process by creating the progress UI and calling the underlying method.<br />
        /// Binds the cancellation process and the tracker to the current Worker.<br />
        /// Notes: Uses the communicator available in Worker.CurrentConst.<br />
        /// Tips: Avoid concurrent calls on the same plugin and the same instance to prevent deadlocks.<br />
        /// </summary>
        /// <param name="pPlugin">Es: ID o nombre del plugin.<br />En: Plugin ID or name.</param>
        /// <param name="pNameInstance">Es: Nombre de la instancia principal.<br />En: Name of the main instance.</param>
        /// <param name="pIdRequest">Es: Identificador de la petición.<br />En: Request identifier.</param>
        /// <param name="pCancellationToken">Es: Token de cancelación.<br />En: Cancellation token.</param>
        /// <param name="pMethod">Es: Métodos para la creación del progreso en array binario.<br />En: Methods for progress creation in binary array.</param>
        /// <returns>Es: El estado de la desinstalación.<br />En: The status of the uninstallation.</returns>
        public static async Task<Status> HandleUnistallOne
            (string pPlugin, string pNameInstance, ushort pIdRequest, CancellationToken pCancellationToken = default, params byte[] pMethod)
        {
            var progress = ProgressUtil.CreateProgessBarr(Worker.CurrentContext.Value.Communicator, pIdRequest, pMethod: pMethod);

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

            StatusNodeUtil r = await Manager.Instances.UnistallPlugin(manifest.HandWritten, pNameInstance, pProgress, pCancellationToken);
            Status result = r switch
            {
                StatusNodeUtil.Succes => Status.Succes,
                StatusNodeUtil.IsCancelled => Status.IsCancelled,
                StatusNodeUtil.InvalidSource => Status.InstanceNotExist,

                _ => Status.GenericError,
            };
            return result;
        }

        //-----------------( Gets )-----------------//
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Obtiene el manifiesto de un plugin en particular instalado en una instancia.<br />
        /// Busca la información JSON local del plugin correspondiente.<br />
        /// Notas: Combina rutas relativas y absolutas según corresponda, y comprueba que existan manifiestos.<br />
        /// Tips: Útil para validar el estado de un plugin antes de desinstalar o inicializar.<br />
        /// ___________________( English )___________________<br />
        /// Gets the manifest of a particular plugin installed in an instance.<br />
        /// Searches for the corresponding plugin's local JSON information.<br />
        /// Notes: Combines relative and absolute paths as appropriate, and checks if manifests exist.<br />
        /// Tips: Useful for validating a plugin's state before uninstalling or initializing.<br />
        /// </summary>
        /// <param name="pPlugin">Es: Identificador local del plugin a consultar.<br />En: Local identifier of the plugin to check.</param>
        /// <param name="pNameInstance">Es: La instancia asignada a revisar.<br />En: The assigned instance to review.</param>
        /// <param name="pCancellationToken">Es: Token de la operación.<br />En: Operation token.</param>
        /// <returns>Es: Tupla con estado de operación y el manifiesto si se encontró.<br />En: Tuple with the operation status and the manifest if found.</returns>
        public static async Task<(Status status, ManifestPlugin? manifest)>
            GetOne(string pPlugin, string pNameInstance, CancellationToken pCancellationToken = default)
        {
            ManifestInstance? manifest;
            string information;
            string? path;

            if (pCancellationToken.IsCancellationRequested)
                return (Status.IsCancelled, null);

            path = Manager.Instances.GetPathFolder(pNameInstance);
            if (string.IsNullOrEmpty(path))
                return (Status.InstanceNotExist, null);

            manifest = await Manager.Instances.GetManifestByPath(path);
            if (manifest == null)
                return (Status.InstanceNotExist, null);

            information = Manager.Instances.MakePathFolderInformationByPath(path);

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

                    ManifestPlugin? man = JSonUtil.AcessDirect<ManifestPlugin>(pathJson);
                    if (man == null)
                        return (Status.ManifestNotExit, null);
                    return (Status.Succes, man);
                }
            }
            return (Status.NotFound, null);
        }
        
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Recopila todos los manifiestos de los plugins pertenecientes a una sola instancia.<br />
        /// Itera la carpeta de plugins y carga los archivos manifiesto disponibles.<br />
        /// Notas: Puede regresar una lista vacía si la instancia no tiene plugins.<br />
        /// Tips: Las rutas absolutas y relativas serán manejadas en automático.<br />
        /// ___________________( English )___________________<br />
        /// Collects all plugin manifests belonging to a single instance.<br />
        /// Iterates inside the plugins folder and loads available manifest files.<br />
        /// Notes: Might return an empty list if the instance has no plugins.<br />
        /// Tips: Absolute and relative paths will be managed automatically.<br />
        /// </summary>
        /// <param name="pNameInstance">Es: Nombre de la instancia a escanear.<br />En: Name of the instance to scan.</param>
        /// <param name="pCancellationToken">Es: Token de cancelación de la iteración.<br />En: Iteration cancellation token.</param>
        /// <returns>Es: Tupla con el estado de la tarea y la lista de todos los manifiestos hallados.<br />En: Tuple containing the task status and the list of all found manifests.</returns>
        public static async Task<(Status status, List<ManifestPlugin>? manifests)>
            GetAll(string pNameInstance, CancellationToken pCancellationToken = default)
        {
            ManifestInstance? manifest;
            string information;
            List<ManifestPlugin> manis;
            string? path;

            if (pCancellationToken.IsCancellationRequested)
                return (Status.IsCancelled, null);

            path = Manager.Instances.GetPathFolder(pNameInstance);
            if (string.IsNullOrEmpty(path))
                return (Status.InstanceNotExist, null);

            manifest = await Manager.Instances.GetManifestByPath(path);
            if (manifest == null)
                return (Status.InstanceNotExist, null);

            information = Manager.Instances.MakePathFolderInformationByPath(path);

            manis = new();
            for (int i = 0; i < manifest.Plugins.Count; i++)
            {
                if (pCancellationToken.IsCancellationRequested)
                    return (Status.IsCancelled, null);

                var refe = manifest.Plugins[i];
                if (refe.Path == null) continue;

                string pathJson = Path.IsPathFullyQualified(refe.Path)
                    ? refe.Path
                    : Path.Combine(information, refe.Path);

                ManifestPlugin? man = JSonUtil.AcessDirect<ManifestPlugin>(pathJson);

                if (man == null) continue;
                manis.Add(man);
            }
            return (Status.Succes, manis);
        }

        // TODO: Doc.
        public static async Task BrowseIn(ManifestInstance pMani, Action<ReferencePlugin?> pPredicate, CancellationToken pCancellationToken = default)
        {
            for (int i = 0; i < pMani.Plugins.Count; i++)
            {
                if (pCancellationToken.IsCancellationRequested)
                    return;

                ReferencePlugin? refe = pMani.Plugins[i];

                using (_locks.LockAsync(refe.IdLocal ?? pMani.Name ?? "BrowseIn", pCancellationToken))
                {
                    pPredicate(refe);
                }
            }
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea internamente la ruta de la carpeta de plugins para una instancia en caso de no existir o la recupera.<br />
        /// Busca por nombre de instancia y construye el path en función a BePinEx.<br />
        /// Notas: Si la carpeta de la instancia no existe, devuelve nulo.<br />
        /// Tips: Puede generar una nueva carpeta de plugin en el disco local de ser necesario.<br />
        /// ___________________( English )___________________<br />
        /// Internally creates the plugin folder path for an instance in case it doesn't exist, or retrieves it.<br />
        /// Searches by instance name and builds the path based on BePinEx.<br />
        /// Notes: If the instance folder does not exist, it returns null.<br />
        /// Tips: Can generate a new plugin directory on the local disk if deemed necessary.<br />
        /// </summary>
        /// <param name="pNameInstance">Es: Nombre base de la instancia.<br />En: Base name of the instance.</param>
        /// <returns>Es: Ruta absoluta de la carpeta de plugins de la instancia o nulo.<br />En: Absolute path for the instance's plugins folder or null.</returns>
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

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Construye internamente y verifica la ruta de plugins usando el path explícito de la instancia.<br />
        /// Si la carpeta base para BePinEx no existe, esta se generará.<br />
        /// Notas: Esta sobrecarga obvia la búsqueda de la instancia mediante nombre asumiéndose ya resuelto.<br />
        /// Tips: Útil al enviar llamadas locales ya procesadas para evitar refactorización extra.<br />
        /// ___________________( English )___________________<br />
        /// Internally constructs and verifies the plugins path using the explicit instance path.<br />
        /// If the base BePinEx folder doesn't exist, it will be generated.<br />
        /// Notes: This overload bypasses instance name resolution, assuming it has already been resolved.<br />
        /// Tips: Useful when sending already processed local calls to avoid extra refactoring.<br />
        /// </summary>
        /// <param name="pPathInstance">Es: Ruta directa hacia la instancia.<br />En: Direct path towards the instance.</param>
        /// <returns>Es: Ruta final a la carpeta de plugins.<br />En: Final path to the plugins folder.</returns>
        public static string MakePathPluginByInstance(string pPathInstance)
        {
            string pathPlugin;
            pathPlugin = Path.Combine(pPathInstance, TerbinServiceConst.PATH_BEPINEX_PLUGIN);
            if (!Directory.Exists(pathPlugin))
                Directory.CreateDirectory(pathPlugin);
            return pathPlugin;
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Enumerador de códigos de estado de operaciones de los plugins.<br />
        /// Contiene todos los retornos posibles de resoluciones correctas o errores de proceso.<br />
        /// Notas: Utiliza tipos base sbyte para un menor consumo en memoria y serialización.<br />
        /// Tips: Verifica Succes (1) para validaciones rápidas.<br />
        /// ___________________( English )___________________<br />
        /// Status codes enumerator for plugin operations.<br />
        /// Contains all possible returns of proper resolutions or process failures.<br />
        /// Notes: Uses an sbyte base type for lower memory footprint and serialization overhead.<br />
        /// Tips: Check for Succes (1) for fast validations.<br />
        /// </summary>
        public enum Status : sbyte
        {
            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Ocurrió una excepción al intentar eliminar un archivo temporal.<br />
            /// ___________________( English )___________________<br />
            /// An exception occurred while attempting to delete a temporary file.<br />
            /// </summary>
            ExceptionOnDeteledTmp = -1,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// La operación fue cancelada por el usuario o el token de cancelación.<br />
            /// ___________________( English )___________________<br />
            /// The operation was cancelled by the user or the cancellation token.<br />
            /// </summary>
            IsCancelled = 0,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// La operación se completó con éxito sin inconvenientes.<br />
            /// ___________________( English )___________________<br />
            /// The operation completed successfully without issues.<br />
            /// </summary>
            Succes = 1,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Ocurrió un error genérico o no especificado durante el proceso.<br />
            /// ___________________( English )___________________<br />
            /// A generic or unspecified error occurred during the process.<br />
            /// </summary>
            GenericError = 2,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Ocurrió un error al intentar obtener o resolver la ruta de la instancia.<br />
            /// ___________________( English )___________________<br />
            /// An error occurred while trying to obtain or resolve the instance path.<br />
            /// </summary>
            ErrorGetPathInstance = 3,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Ocurrió un error al recuperar la información del plugin desde el almacenamiento.<br />
            /// ___________________( English )___________________<br />
            /// An error occurred while retrieving the plugin information from storage.<br />
            /// </summary>
            ErrorGetPlugin = 4,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Error al tratar de generar u obtener la ruta física del plugin.<br />
            /// ___________________( English )___________________<br />
            /// Error when trying to generate or retrieve the physical path of the plugin.<br />
            /// </summary>
            ErrorGetPathPlugin = 5,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Se produjo un fallo en la red o servidor durante la descarga del plugin.<br />
            /// ___________________( English )___________________<br />
            /// A network or server failure occurred during the plugin download.<br />
            /// </summary>
            ErrorOnDowload = 6,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// La instancia indicada o solicitada no existe o no figura configurada en el sistema.<br />
            /// ___________________( English )___________________<br />
            /// The specified or requested instance does not exist or is not configured in the system.<br />
            /// </summary>
            InstanceNotExist = 7,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// No se pudo localizar la carpeta o los datos de información requeridos de la instancia.<br />
            /// ___________________( English )___________________<br />
            /// The required information folder or data of the instance could not be located.<br />
            /// </summary>
            InformationNotExist = 8,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// El archivo de manifiesto no existe, no es accesible o no puedo ser construido.<br />
            /// ___________________( English )___________________<br />
            /// The manifest file does not exist, is not accessible, or could not be constructed.<br />
            /// </summary>
            ManifestNotExit = 9,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// El recurso, archivo o elemento buscado no ha sido encontrado.<br />
            /// ___________________( English )___________________<br />
            /// The required resource, file, or item has not been found.<br />
            /// </summary>
            NotFound = 10,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Error al obtener el manifiesto.<br />
            /// ___________________( English )___________________<br />
            /// Error reading or getting the manifest.<br />
            /// </summary>
            ErrorGetManifest = 11,

            ErrorOnSaveManifest = 12,

            InvalidURL = 13,

            NotSuchSpace = 14,
        }
    }
}
