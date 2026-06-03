using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;

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
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Clase responsable de gestionar las diferentes instancias dentro de la aplicación.<br />
    /// Proporciona funcionalidades para crear, verificar, y gestionar manifiestos e instalación de plugins por instancia.<br />
    /// Notas: Usa mecanismos de bloqueo (semáforos) para evitar condiciones de carrera durante la modificación de instancias.<br />
    /// Tips: Asegúrate de comprobar siempre que las rutas estén bien formadas antes de operar.<br />
    /// ___________________( English )___________________<br />
    /// Class responsible for managing the different instances within the application.<br />
    /// Provides functionalities to create, verify, and manage manifests, and plugin installation per instance.<br />
    /// Notes: Uses locking mechanisms (semaphores) to prevent race conditions during instance modifications.<br />
    /// Tips: Always ensure paths are well-formed before operating.<br />
    /// </summary>
    public static class Instances
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _instanceLocks = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Lock _lockCreatingInstance = new();

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea una nueva instancia con el nombre especificado.<br />
        /// Configura el directorio correspondiente, crea el manifiesto predeterminado y actualiza el índice general.<br />
        /// Notas: Si la carpeta de la instancia ya existe y tiene contenido, se lanza una excepción.<br />
        /// Tips: Asegúrate de atrapar la excepción si se implementa una lógica para preguntar por la sobrescritura.<br />
        /// ___________________( English )___________________<br />
        /// Creates a new instance with the specified name.<br />
        /// Sets up the corresponding directory, creates the default manifest, and updates the general index.<br />
        /// Notes: If the instance folder already exists and contains files, an exception is thrown.<br />
        /// Tips: Catch the exception if you implement logic to ask for overwrite confirmation.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la nueva instancia a crear. <br />En: The name of the new instance to create.</param>
        /// <returns>Es: <c>true</c> si se creó con éxito, de lo contrario <c>false</c>. <br />En: <c>true</c> if created successfully, otherwise <c>false</c>.</returns>
        public static bool NewInstance(string pName, bool pOverwrite)
        {
            string dirInstace = Manager.Instances.MakePathFolder(pName);

            if (!pOverwrite)
            {
                if (Manager.Instances.ExistInIndex(pName))
                    return false;
            }

            lock (_lockCreatingInstance)
            {
                if (Directory.Exists(dirInstace))
                    return false;
                else
                    Directory.CreateDirectory(dirInstace);

                Manager.Instances.CreatePredeterminated(pName, pOverwrite);

                Manager.Manifest.RegisterNewInstance(pName);
            }
            return true;
        }



        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea los archivos y directorios predeterminados para una instancia (ocultando el directorio de información).<br />
        /// ___________________( English )___________________<br />
        /// Creates the default files and directories for an instance (hiding the information directory).<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <param name="pOverwrite">Es: Indica si se deben sobrescribir los datos si la instancia ya existe. <br />En: Indicates whether to overwrite data if the instance already exists.</param>
        public static void CreatePredeterminated(string pName, bool pOverwrite)
        {
            string dirInfo = Manager.Instances.MakePathFolderInformation(pName);

            if (Directory.Exists(dirInfo))
            {
                if (!pOverwrite)
                    return;

                throw new NotImplementedException("TODO: Reiniciar Instancia.");
            }

            DirectoryInfo directoryInfo = Directory.CreateDirectory(dirInfo);
            directoryInfo.Attributes |= FileAttributes.Hidden;

            Manager.Manifest.CreateInstance(pName, dirInfo);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Verifica si un directorio específico corresponde a una instancia válida y estructurada.<br />
        /// Comprueba que exista la carpeta de información y que contenga un archivo de manifiesto de instancia.<br />
        /// Notas: Es una revisión rápida y pasiva de los archivos locales.<br />
        /// Tips: Útil para validaciones antes de asumir que un directorio suelto es una instancia operante.<br />
        /// ___________________( English )___________________<br />
        /// Verifies whether a specific directory corresponds to a valid, well-structured instance.<br />
        /// Checks that the information folder exists and contains an instance manifest file.<br />
        /// Notes: This is a fast and passive check on local files.<br />
        /// Tips: Useful for validations before assuming a random directory is an operational instance.<br />
        /// </summary>
        /// <param name="pPath">Es: La ruta completa o relativa al directorio que se desea comprobar. <br />En: The full or relative path to the directory to check.</param>
        /// <returns>Es: <c>true</c> si el directorio es una instancia válida. <br />En: <c>true</c> if the directory is a valid instance.</returns>
        public static bool IsInstance(string pPath)
        {
            string information;
            string manifest;

            information = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);

            if (!Directory.Exists(information)) return false;

            manifest = Path.Combine(information, TerbinServiceConst.MANIFEST_INSTANCE);

            return File.Exists(manifest);
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Obtiene el contenido en formato texto (string) del manifiesto asociado a una instancia.<br />
        /// Intenta construir la ruta de la instancia y leer directamente el archivo del manifiesto.<br />
        /// Notas: Devuelve <c>null</c> si no se puede construir la ruta o si el archivo no existe.<br />
        /// Tips: Usa esto cuando solo ocupes leer el manifiesto bruto (ej. para imprimirlo o procesarlo manualmente).<br />
        /// ___________________( English )___________________<br />
        /// Gets the textual content (string) of the manifest associated with an instance.<br />
        /// Attempts to build the instance path and reads the manifest file directly.<br />
        /// Notes: Returns <c>null</c> if the path cannot be built or the file does not exist.<br />
        /// Tips: Use this when you only need to read the raw manifest (e.g. for printing or manual parsing).<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: El contenido del manifiesto como string, o null si falla. <br />En: The manifest content as a string, or null on failure.</returns>
        public static string? GetStringManifest(string pName)
        {
            string dir = Manager.Instances.MakePathFolder(pName);

            var mani = JSonUtil.AcessDirect<ManifestInstance>(dir, TerbinServiceConst.MANIFEST_INSTANCE);

            if (mani == null)
                return null;

            return JSonUtil.ToJson(mani, Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Obtiene el objeto deseseriaizado del manifiesto de la instancia solicitada.<br />
        /// Utiliza librerías JSON locales para convertir el archivo a un objeto <see cref="ManifestInstance"/>.<br />
        /// Notas: Retorna <c>null</c> si la instancia no tiene un directorio válido o si ocurre un fallo al deserializar.<br />
        /// Tips: Recomendado si necesitas leer las propiedades y operar lógicamente con la configuración de la instancia.<br />
        /// ___________________( English )___________________<br />
        /// Gets the deserialized manifest object for the requested instance.<br />
        /// Uses local JSON libraries to convert the file to an <see cref="ManifestInstance"/> object.<br />
        /// Notes: Returns <c>null</c> if the instance lacks a valid directory or if deserialization fails.<br />
        /// Tips: Recommended if you need to read properties and act logically upon the instance configuration.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: Objeto del manifiesto o null. <br />En: Manifest object or null.</returns>
        public static ManifestInstance? GetManifest(string pName)
        {
            string dir = Manager.Instances.MakePathFolder(pName);

            return JSonUtil.AcessDirect<ManifestInstance>(dir, TerbinServiceConst.MANIFEST_INSTANCE);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Construye y retorna la ruta de la carpeta de información para una instancia dada.<br />
        /// Esta ruta aloja metadatos y configuraciones no manipulables por el usuario.<br />
        /// Notas: No crea la carpeta, solamente construye el string con su potencial ruta base.<br />
        /// Tips: Propenso a devolver <c>null</c> si la configuración raíz de instancias no está asimilada.<br />
        /// ___________________( English )___________________<br />
        /// Builds and returns the path to the information folder for a given instance.<br />
        /// This path hosts metadata and non-user manipulable configurations.<br />
        /// Notes: It does not create the folder; it only constructs the string with the potential base path.<br />
        /// Tips: Prone to returning <c>null</c> if the root instances configuration is not assimilated.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: Ruta a la carpeta de información o null. <br />En: Path to the information folder or null.</returns>
        public static string MakePathFolderInformation(string pName)
        {
            string dir = Manager.Instances.MakePathFolder(pName);

            return Path.Combine(dir, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Construye la ruta para la instancia específica y la crea si no existe.<br />
        /// Su objetivo es proveer acceso seguro en disco y confirmar que el directorio base se encuentra funcional.<br />
        /// Notas: Si la carpeta no existía, la crea automáticamente con permisos por defecto.<br />
        /// Tips: Útil para inicializar áreas antes de descargar o descomprimir archivos pesados.<br />
        /// ___________________( English )___________________<br />
        /// Builds the path for the specific instance and creates it if it doesn't exist.<br />
        /// Its goal is to provide safe disk access and confirm that the root directory is functional.<br />
        /// Notes: If the folder did not exist, it automatically creates it with default permissions.<br />
        /// Tips: Useful for initializing areas before downloading or extracting heavy files.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: Ruta del directorio raíz de la instancia creado/verificado. <br />En: Instance root directory path created/verified.</returns>
        public static string CreatePathFolder(string pName)
        {
            string dir = Manager.Instances.MakePathFolder(pName);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }


        public static string? GetPathFolder(string pName)
        {
            if (!Manager.Instances.ExistInIndex(pName))
                return null;
            return Manager.Instances.MakePathFolder(pName);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Construye la ruta en disco base en donde se debería ubicar una determinada instancia.<br />
        /// Combina la configuración general de directorios de instancias con el nombre proveído.<br />
        /// Notas: No verifica la existencia física del directorio recién ensamblado.<br />
        /// Tips: Utiliza <see cref="CreatePathFolder"/> si aparte de la ruta necesitas materializar el directorio.<br />
        /// ___________________( English )___________________<br />
        /// Builds the base disk path where a specific instance should be located.<br />
        /// Combines the general instances directory configuration with the provided name.<br />
        /// Notes: It does not verify the physical existence of the newly assembled directory.<br />
        /// Tips: Use <see cref="CreatePathFolder"/> if besides the path you also need to physically create the directory.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia a enrutar. <br />En: The name of the instance to route.</param>
        /// <returns>Es: La ruta resultante a la instancia o null si no se halla configuración. <br />En: Resulting path to the instance or null if config is absent.</returns>
        public static string MakePathFolder(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                throw new Exception($"The key TerbinConfiguration.RUTE_INSTANCES is not defined: ({TerbinConfiguration.RUTE_INSTANCES})");

            return Path.Combine(dir, pName);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Consulta el índice global de manifiestos y verifica si la instancia está registrada ahí.<br />
        /// Sirve como validación en catálogos en vez de revisar en disco de golpe.<br />
        /// Notas: Solamente compara el registro, el cual podría estar desfasado de los archivos físicos.<br />
        /// Tips: Considera unirlo con <see cref="ExistInPhisic"/> para una validación doblemente certera.<br />
        /// ___________________( English )___________________<br />
        /// Queries the global manifest index and checks if the instance is registered there.<br />
        /// Acts as a validation in catalogs rather than forcefully checking the hard drive right away.<br />
        /// Notes: It only compares the registry, which could be out of sync with physical files.<br />
        /// Tips: Consider combining it with <see cref="ExistInPhisic"/> for a doubly accurate validation.<br />
        /// </summary>
        /// <param name="pName">Es: Nombre de la instancia. <br />En: Name of the instance.</param>
        /// <returns>Es: <c>true</c> si existe en el índice. <br />En: <c>true</c> if it exists within the index.</returns>
        public static bool ExistInIndex(string pName)
        {
            ManifestIndex index = Manager.Manifest.GetIndex();
            for (int i = 0; i < index.Instances.Count; i++)
            {
                var instance = index.Instances[i];
                if (instance != null && instance.Name == pName)
                    return true;
            }
            return false;
        }

        //public static bool Exist(string pName)
        //{
        //    var instances = Manager.Manifest.GetIndex().Instances;
        //    string? mani = instances?.FirstOrDefault(manifest => manifest.Name == pName)?.Name;
        //    return !string.IsNullOrEmpty(mani);
        //}

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Comprueba si la carpeta de la instancia existe de forma palpable y directa en el sistema de archivos.<br />
        /// Revisa entre los directorios del destino padre asimilado en la variable de configuración.<br />
        /// Notas: Retorna <c>null</c> si el contexto de la ruta general no es obtenible o no existe.<br />
        /// Tips: Llamada moderadamente costosa ya que utiliza llamadas al sistema operativo (I/O).<br />
        /// ___________________( English )___________________<br />
        /// Checks if the instance folder exists palpably and directly in the file system.<br />
        /// Searches among the directories in the parent destination ingested via configuration.<br />
        /// Notes: Returns <c>null</c> if the overall routing context is unachievable or fully missing.<br />
        /// Tips: Moderately expensive call since it relies on I/O OS APIs to fetch top directory structures.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: <c>true</c> o <c>false</c> según la existencia; <c>null</c> bajo fallo de contexto base. <br />En: <c>true</c> or <c>false</c> based on existence; <c>null</c> on base context failure.</returns>
        public static bool? ExistInPhisic(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (string.IsNullOrEmpty(dir))
                return null;

            if (!Directory.Exists(dir))
                return null;

            var all = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            if (all == null)
                return false;

            return all.Contains(pName);
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Descomprime e instala un plugin desde un archivo comprimido a los directorios de una instancia.<br />
        /// Emplea un progreso para monitorear el trabajo de extracción, y semáforos para no interrumpir otra operación paralela.<br />
        /// Notas: El proceso es asíncrono y soporta cancelaciones del usuario.<br />
        /// Tips: La variable sobreescribir define cómo resolver colisiones de archivos que ya existían previamente en esa ubicación.<br />
        /// ___________________( English )___________________<br />
        /// Extracts and installs a plugin from an archive into an instance's directories.<br />
        /// Uses a progress provider to track extraction, and semaphores to prevent interfering with parallel operations.<br />
        /// Notes: The process is asynchronous and supports user cancellation.<br />
        /// Tips: The overwrite variable defines how backward file collisions are resolved gracefully via the extraction tool.<br />
        /// </summary>
        /// <param name="pPathPlugin">Es: Ruta directa al contenedor ZIP del plugin. <br />En: Direct path to the plugin ZIP container.</param>
        /// <param name="pNameInstance">Es: Nombre de la instancia objetivo. <br />En: Target instance name.</param>
        /// <param name="pOverwrite">Es: Booleano para forzar reemplazo si el archivo ya existe. <br />En: Boolean to force replacement if the file exists.</param>
        /// <param name="pProgress">Es: Proveedor iterativo del progreso del volcado. <br />En: Iterative progress provider for dumping process.</param>
        /// <param name="pCancellationToken">Es: Token para frenar en seco la instalación. <br />En: Token to halt the installation immediately.</param>
        /// <returns>Es: Un directorio estructurado (Handwritten) con su rastro o null. <br />En: A structured directory (Handwritten) trace or null.</returns>
        public static async Task<DirectoryHandwritten?> InstallPlugin
            (string pPathPlugin, string pNameInstance, string pTarjetPath, bool pOverwrite, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            if (!Directory.Exists(pTarjetPath))
                Directory.CreateDirectory(pTarjetPath);

            SemaphoreSlim instanceLock = _instanceLocks.GetOrAdd(pNameInstance, _ => new SemaphoreSlim(1, 1));
            await instanceLock.WaitAsync(pCancellationToken).ConfigureAwait(false);
            try
            {
                if (pCancellationToken.IsCancellationRequested)
                    return null;
                var result = await ZipUtil.ExtractWithProgress(pPathPlugin, pTarjetPath, pProgress, pOverwrite, pCancellationToken).ConfigureAwait(false);
                return result;
            }
            finally
            {
                instanceLock.Release();
            }
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Remueve los trazos y archivos generados por un plugin previamente instalado en la instancia.<br />
        /// Opera a base de un esquema 'Handwritten' creado en el momento de la instalación inicial.<br />
        /// Notas: Usa protecciones asíncronas de bloqueo para evitar choques con lecturas.<br />
        /// Tips: Ten cuidado, una desinstalación errónea puede dañar archivos nativos de la instancia si coinciden sus rutas.<br />
        /// ___________________( English )___________________<br />
        /// Removes the traces and files generated by a plugin previously installed into the instance.<br />
        /// Works fundamentally off a 'Handwritten' tree schema mapped initially during installation.<br />
        /// Notes: Employs asynchronous locking safeguards to sidestep read collisions.<br />
        /// Tips: Be warned, an erratic uninstallation can shred native instance files if their paths match identically.<br />
        /// </summary>
        /// <param name="pPlugin">Es: El árbol referencial con los archivos que colocó el plugin. <br />En: Referential tree showcasing files placed by the plugin.</param>
        /// <param name="pNameInstance">Es: El nombre de la instancia en la mira. <br />En: Targeted instance name.</param>
        /// <param name="pProgress">Es: Manejador de notificaciones visuales/backend de progreso. <br />En: Visual or backend notification handler for progress ticks.</param>
        /// <param name="pCancellationToken">Es: Token cancelatorio. <br />En: Cancellation token.</param>
        /// <returns>Es: Un enum indicando el estado final de la limpieza. <br />En: An enum noting the cleanup ending status.</returns>
        public static async Task<StatusFileUtil> UnistallPlugin
            (DirectoryHandwritten pPlugin, string pNameInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            //ArgumentNullException.ThrowIfNull(pPlugin.Root, "Root is not asing, need root in DirectoryHandwritten");

            string pathInstance = Manager.Instances.MakePathFolder(pNameInstance);

            SemaphoreSlim instanceLock = _instanceLocks.GetOrAdd(pNameInstance, _ => new SemaphoreSlim(1, 1));
            await instanceLock.WaitAsync(pCancellationToken).ConfigureAwait(false);
            try
            {
                if (pCancellationToken.IsCancellationRequested)
                    return StatusFileUtil.IsCancelled;
                var result = FileUtil.DeleteFromHandwritten(pathInstance, pPlugin, pProgress);
                return result;
            }
            finally
            {
                instanceLock.Release();
            }
        }
    }
}
