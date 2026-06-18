using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.NetWork;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Managers;

namespace TerbinService.Services;
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

/// <summary>
/// ___________________( Español )___________________<br />
/// Servicio encargado de exponer las operaciones de gestión y despliegue de plugins de forma ejecutable.<br />
/// Actúa como puente entre las peticiones de red y la lógica interna de los mánagers de almacenamiento e instancias.<br />
/// Notas: Las clases de servicio son instanciadas dinámicamente por el despachador de comandos de Terbin.<br />
/// Tips: Asegúrese de que los mánagers subyacentes se encuentren completamente inicializados antes de habilitar este servicio.<br />
/// ___________________( English )___________________<br />
/// Service responsible for exposing plugin management and deployment operations in an executable manner.<br />
/// Acts as a bridge between network requests and the internal logic of storage and instance managers.<br />
/// Notes: Service classes are dynamically instantiated by the Terbin command dispatcher.<br />
/// Tips: Ensure that underlying managers are fully initialized before enabling this service.<br />
/// </summary>
internal static class ServicePlugins
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio encargado de procesar la solicitud de descarga de un plugin desde la red.<br />
    /// Valida el tamaño del recurso remoto y delega la tarea de descarga al gestor de plugins (Manager.Plugin).<br />
    /// Notas: Si la descarga falla por falta de espacio o URL inválida, se devuelve un error interno categorizado.<br />
    /// Tips: Asegúrese de tener conexión a la red o permisos en el firewall antes de invocar este comando.<br />
    /// ___________________( English )___________________<br />
    /// Service in charge of processing the plugin download request from the network.<br />
    /// Validates the remote resource size and delegates the download task to the plugin manager (Manager.Plugin).<br />
    /// Notes: If the download fails due to lack of space or an invalid URL, a categorized internal error is returned.<br />
    /// Tips: Ensure you have network connectivity or firewall permissions before invoking this command.<br />
    /// </summary>
    /// <param name="urlPlugin">Es: (Obligatorio) Cadena de caracteres que contiene la URL directa desde donde se descargará el plugin. <br />En: (Mandatory) Character string containing the direct URL from where the plugin will be downloaded.</param>
    /// <param name="useProgress">Es: (Opcional) Valor booleano que indica si se debe inicializar un canal para reportar visualmente el progreso de la descarga. <br />En: (Optional) Boolean value indicating whether to initialize a channel to visually report the download progress.</param>
    /// <returns>Es: Un InfoResponse de éxito, o un error interno detallando el motivo del fallo en la descarga. <br />En: A success InfoResponse, or an internal error detailing the reason for the download failure.</returns>
    [TODO("Mover a ServicePluginStorage")]
    [TODO("Comprobar que el plugin no exista")]
    [TerbinExecutable((byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> DowloadPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string urlPlugin = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;

        long? sizePlugin = await NetUtil.GetContentLength(urlPlugin, pCancellationToken: CancellationToken.None);
        if (sizePlugin is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.PluginNotConect));

        if (useProgress)
        {
            MaxProgressDTO max = new(sizePlugin.Value);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        // TODO: Comprobar que exista y si existe preguntar si quiere sobre-escrbir.
        var r = await Manager.Plugin.DowloadOne(urlPlugin, progress, pToken);

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);
        if (r != Manager.Plugin.Status.Succes)
        {
            var error = TSHelper.GetError(r switch
            {
                Manager.Plugin.Status.NotSuchSpace => InternalErrors.PluginNotSuchSpace,
                Manager.Plugin.Status.InvalidURL => InternalErrors.PluginInvalidURL,
                _ => InternalErrors.PluginOnDowload,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio responsable de instalar un plugin previamente almacenado en una instancia de destino.<br />
    /// Descomprime y ubica los archivos en la ruta relativa proporcionada dentro del directorio de la instancia.<br />
    /// Notas: Valida la existencia de la instancia y genera una entrada en el manifiesto al finalizar con éxito.<br />
    /// Tips: La ruta relativa puede ser útil para organizar mods o plugins en diferentes subcarpetas soportadas por BePinEx u otros loaders.<br />
    /// ___________________( English )___________________<br />
    /// Service responsible for installing a previously stored plugin into a target instance.<br />
    /// Extracts and places the files in the provided relative path within the instance's directory.<br />
    /// Notes: Validates the existence of the instance and generates a manifest entry upon successful completion.<br />
    /// Tips: The relative path can be useful for organizing mods or plugins in different subfolders supported by BePinEx or other loaders.<br />
    /// </summary>
    /// <param name="name">Es: (Obligatorio) Cadena de caracteres con el nombre de la instancia donde se realizará la instalación. <br />En: (Mandatory) Character string with the name of the instance where the installation will take place.</param>
    /// <param name="idPlugin">Es: (Obligatorio) Identificador único en el almacenamiento (Storage) que hace referencia al plugin descargado. <br />En: (Mandatory) Unique identifier in the Storage referencing the downloaded plugin.</param>
    /// <param name="relativePath">Es: (Obligatorio) Ruta relativa dentro de la instancia donde se desempaquetarán los archivos. <br />En: (Mandatory) Relative path within the instance where the files will be unpacked.</param>
    /// <param name="useProgress">Es: (Opcional) Valor booleano que habilita la emisión de paquetes de progreso durante la extracción del plugin. <br />En: (Optional) Boolean value that enables the emission of progress packets during the plugin extraction.</param>
    /// <returns>Es: Un InfoResponse exitoso o el código de error correspondiente a fallos de lectura, escritura o falta del plugin. <br />En: A successful InfoResponse or the corresponding error code for reading, writing, or missing plugin failures.</returns>
    [TODO("Crear Enum (Flag) que permita algo de configuracion como extrar en una carpeta con el nombre del mod")]
    [TerbinExecutable((byte)CodeServices.Install, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> InstallPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string idPlugin = reader.ReadArray<char>().CrString();
        string relativePath = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        string? pathPlugin;
        string? pathInstance;
        IProgress<TerbinInfoProgrss>? progress = null;

        pathInstance = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(pathInstance))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        pathPlugin = Path.Combine(pathInstance, relativePath);

        if (useProgress)
        {
            MaxProgressDTO max = new(await Manager.StoragePlugin.GetSize(idPlugin));
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var r = await Manager.Plugin.InstallOne(idPlugin, name, pathPlugin, progress, pToken);

        if (r == Manager.Plugin.Status.IsCancelled)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        if (r != Manager.Plugin.Status.Succes)
        {
            // ErrorGetPlugin, ErrorGetPathPlugin, ErrorGetManifest, ErrorOnSaveManifest, GenericError
            var error = TSHelper.GetError(r switch
            {
                Manager.Plugin.Status.ErrorGetPathPlugin => InternalErrors.PluginGetPath,
                Manager.Plugin.Status.ErrorGetManifest => InternalErrors.PluginGetManifest,
                Manager.Plugin.Status.ErrorOnSaveManifest => InternalErrors.PluginOnSave,
                _ => InternalErrors.PluginNotExist,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio que recupera la lista completa de todos los plugins registrados e instalados en una instancia.<br />
    /// Lee el manifiesto de la instancia solicitada, procesa los manifiestos individuales y los empaca en una estructura serializada.<br />
    /// Notas: Si la instancia no contiene plugins, devolverá una respuesta exitosa con una carga útil equivalente a un array vacío.<br />
    /// Tips: Utiliza este servicio para renderizar interfaces de inventario o catálogos de mods de los usuarios.<br />
    /// ___________________( English )___________________<br />
    /// Service that retrieves the complete list of all registered and installed plugins in an instance.<br />
    /// Reads the requested instance's manifest, processes the individual manifests, and packs them into a serialized structure.<br />
    /// Notes: If the instance contains no plugins, it will return a successful response with a payload equivalent to an empty array.<br />
    /// Tips: Use this service to render inventory interfaces or user mod catalogs.<br />
    /// </summary>
    /// <param name="nameInstance">Es: (Obligatorio) Cadena de caracteres que define el nombre de la instancia a escanear. <br />En: (Mandatory) Character string defining the name of the instance to scan.</param>
    /// <returns>Es: InfoResponse que contiene la cantidad de plugins instalados y sus respectivos datos serializados (ManifestPluginDTO). <br />En: InfoResponse containing the number of installed plugins and their respective serialized data (ManifestPluginDTO).</returns>
    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> GetAll(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();

        ManifestInstance? manifest;
        ManifestPlugin[] manis;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(nameInstance);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.ManifestGet));

        manis = await Manager.Plugin.GetAll(path, manifest, pToken);

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        Serialineitor s = new();

        if (manis.Length <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(manis.Length);
        for (int i = 0; i < manis.Length; i++)
        {
            ManifestPluginDTO tmp = (ManifestPluginDTO)(manis[i] ?? new());
            s.AddStruct<ManifestPluginDTO>(tmp);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio enfocado en obtener la información detallada (manifiesto local) de un plugin en particular dentro de una instancia.<br />
    /// Valida las rutas de configuración de la instancia y localiza el archivo JSON asociado al identificador proporcionado.<br />
    /// Notas: Es una operación de lectura pasiva; no altera el estado de la instancia ni de los archivos en disco.<br />
    /// Tips: Ideal para verificar el estado, versión o metadatos de un mod antes de actualizarlo o modificarlo.<br />
    /// ___________________( English )___________________<br />
    /// Service focused on obtaining the detailed information (local manifest) of a particular plugin within an instance.<br />
    /// Validates the instance configuration paths and locates the JSON file associated with the provided identifier.<br />
    /// Notes: This is a passive read operation; it does not alter the state of the instance or disk files.<br />
    /// Tips: Ideal for verifying the state, version, or metadata of a mod before updating or modifying it.<br />
    /// </summary>
    /// <param name="name">Es: (Obligatorio) Cadena de caracteres correspondiente al nombre de la instancia objetivo. <br />En: (Mandatory) Character string corresponding to the target instance name.</param>
    /// <param name="id">Es: (Obligatorio) Identificador local (IdLocal) que señala unívocamente al plugin que se desea leer. <br />En: (Mandatory) Local identifier (IdLocal) that uniquely points to the plugin to be read.</param>
    /// <returns>Es: InfoResponse con el objeto ManifestPluginDTO serializado, o un error si no se encuentra la instancia o el plugin. <br />En: InfoResponse with the serialized ManifestPluginDTO object, or an error if the instance or plugin is not found.</returns>
    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string id = reader.ReadArray<char>().CrString();

        ManifestInstance? manifest;
        ManifestPlugin? mani;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.ManifestGet));

        mani = await Manager.Plugin.GetOne(id, path, manifest, pToken);
        if (mani is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.PluginGet));

        byte[] dto = ((ManifestPluginDTO)mani).Serialize();

        return InfoResponse.CreateSucces(pHead.IdRequest, dto);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio destinado a desinstalar y purgar de manera física o lógica un plugin asociado a una instancia de ejecución específica.<br />
    /// Resuelve la ruta raíz de la instancia, valida el manifiesto de la misma y remueve los archivos del plugin, permitiendo opcionalmente reportar el progreso del borrado.<br />
    /// Notas: Evalúa activamente el estado del token de cancelación en puntos críticos del flujo para evitar corrupciones de datos en disco.<br />
    /// Tips: Activar el indicador de progreso generará llamadas en tiempo real al backend de comunicación para actualizar la interfaz del cliente.<br />
    /// ___________________( English )___________________<br />
    /// Service intended to physically or logically uninstall and purge a plugin associated with a specific execution instance.<br />
    /// Resolves the root folder path of the instance, validates its manifest, and removes the plugin files, optionally allowing progress reporting.<br />
    /// Notes: Actively evaluates the status of the cancellation token at critical flow checkpoints to avoid disk data corruption.<br />
    /// Tips: Enabling the progress flag will trigger real-time updates through the communication backend to refresh the client interface.<br />
    /// </summary>
    /// <param name="name">Es: (Obligatorio) Cadena de caracteres que indica el nombre de la instancia de la cual se va a desinstalar el recurso. <br />En: (Mandatory) Character string indicating the name of the instance from which the resource will be uninstalled.</param>
    /// <param name="id">Es: (Obligatorio) Cadena identificadora que localiza unívocamente el plugin específico dentro del manifiesto de la instancia. <br />En: (Mandatory) Identifier string that uniquely locates the specific plugin inside the instance's manifest.</param>
    /// <param name="useProgress">Es: (Opcional) Valor booleano que define si se inicializará un canal de reporte visual del progreso de eliminación. <br />En: (Optional) Boolean value defining whether a visual progress reporting channel will be initialized for the deletion.</param>
    /// <returns>Es: Un InfoResponse de éxito si el proceso concluye correctamente, o errores internos correspondientes a InstanceNotExist, ManifestGet o PluginGet. <br />En: A success InfoResponse if the process concludes cleanly, or internal errors relating to InstanceNotExist, ManifestGet, or PluginGet.</returns>[TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> Delete(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string id = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;

        ManifestInstance? manifest;
        ManifestPlugin? mani;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.ManifestGet));

        mani = await Manager.Plugin.GetOne(id, path, manifest, pToken);
        if (mani is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.PluginGet));

        if (useProgress)
        {
            MaxProgressDTO max = new(mani.HandWritten.GetSize());
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Deleted, (byte)CodeServicesSection.Plugin);
        }

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        var r = await Manager.Plugin.UnistallOne(mani, path, name, progress, pToken);

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}