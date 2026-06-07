using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data.Manifests;
using TerbinLibrary.Data.References;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;

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
    // No esta protegido el ExistsByFile y el guardar como uno.
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Clase secundaria estática dedicada a gestionar el almacenamiento de plugins en disco.<br />
    /// Encargada de leer y escribir de los manifiestos localizando referencias y almacenamientos físicos de forma segura y concurrente.<br />
    /// Notas: Todas las operaciones físicas están controladas por un lock o semaphore.<br />
    /// Tips: Evitar manipular o mover archivos listados en el manifiesto manualmente.<br />
    /// ___________________( English )___________________<br />
    /// Static secondary class dedicated to managing on-disk plugin storage.<br />
    /// Responsible for reading and writing manifests, securely finding references and physical storages concurrently.<br />
    /// Notes: All physical operations are controlled by a lock or semaphore.<br />
    /// Tips: Avoid manually manipulating or moving files listed in the manifest.<br />
    /// </summary>
    [TODO("Actualizar plugin, ahunque ¿Que vas ah cambiar?")]
    public static class StoragePlugin
    {
        // TerbinConfiguration
        // TerbinServiceConst.MANIFEST_STORAGE
        private static readonly SemaphoreSlim _semaphoreOperate = new(1, 1);
        private static readonly SemaphoreSlim _semaphoreManifest = new(1, 1);

        private static readonly Lock _lockRename = new();

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Almacena un plugin en el almacén de seguridad, permitiendo renombrarlo en el proceso.<br />
        /// Notas: Usa un bloqueo para evitar que el mismo archivo se manipule en paralelo al moverlo.<br />
        /// Tips: Útil cuando el archivo que recibes requiere normalizar su nombre de guardado.<br />
        /// ___________________( English )___________________<br />
        /// Stores a plugin in the secure warehouse, allowing it to be renamed in the process.<br />
        /// Notes: Uses a lock to prevent the same file from being manipulated in parallel while moving.<br />
        /// Tips: Useful when the file you receive requires normalizing its save name.<br />
        /// </summary>
        /// <param name="pPathPlugin">Es: Ruta fuente donde se encuentra el plugin actualmente. <br />En: Source path where the plugin is currently located.</param>
        /// <param name="pNameFile">Es: Nuevo nombre con el que se va a guardar este plugin. <br />En: New name with which this plugin will be saved.</param>
        /// <param name="pDuplicate">Es: Indica si se debe duplicar el archivo en lugar de moverse. <br />En: Indicates if the file should be duplicated instead of moved.</param>
        /// <returns>Es: Retorna un identificador único Guid para este registro, o null si fue fallido. <br />En: Returns a unique Guid identifier for this record, or null if failed.</returns>
        public static ValueTask<Guid?> Store(string pPathPlugin, string pNameFile, bool pDuplicate = false)
        {
            string newPath = Path.Combine(Path.GetDirectoryName(pPathPlugin) ?? string.Empty, pNameFile);
            lock (_lockRename)
            {
                File.Move(pPathPlugin, newPath); // Renombrar.
            }
            if (pDuplicate)
                return StoreDuplicate(newPath);
            else
                return Store(newPath);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Almacena un plugin existente manteniendo su nombre original intacto.<br />
        /// Notas: Crea el registro pertinente en el archivo del manifiesto vinculado al archivo movido.<br />
        /// Tips: Siempre usar la bandera pDuplicate si el archivo fuente se debe retener.<br />
        /// ___________________( English )___________________<br />
        /// Stores an existing plugin while keeping its original name intact.<br />
        /// Notes: Creates the appropriate record in the manifest file linked to the moved file.<br />
        /// Tips: Always use the pDuplicate flag if the source file must be retained.<br />
        /// </summary>
        /// <param name="pPathPlugin">Es: Ruta directa al plugin a guardar. <br />En: Direct path to the plugin to save.</param>
        /// <param name="pDuplicate">Es: Indiferencia temporal para clonar o trasladar el archivo. <br />En: Temporary indifference to clone or move the file.</param>
        /// <returns>Es: Un código único validado (Guid en texto). <br />En: A validated unique code (text Guid).</returns>
        public static async ValueTask<Guid?> Store(string pPathPlugin)
        {
            string nameFile = Path.GetFileName(pPathPlugin);
            string namePlugin;
            Guid id;

            namePlugin = Manager.Node.GetNameByFile(pPathPlugin);
            id = Guid.NewGuid();

            var reference = new ReferencePluginStore
            {
                Name = namePlugin,
                Id = $"{id:N}",
                FileName = nameFile,
                Version = PluginUtil.ExtratVersion(nameFile),
            };

            if (await ExistsByFile(nameFile).ConfigureAwait(false)) return null;
            if (!await operatePlugin(pPathPlugin, (p, d) => { File.Move(p, d); }).ConfigureAwait(false))
                return null;

            if (!await registerPlugin(reference).ConfigureAwait(false))
            {
                await operatePlugin(nameFile, (p, d) => { File.Delete(d); }).ConfigureAwait(false);
                return null;
            }

            return id;
        }

        // TODO: Doc.
        public static async ValueTask<Guid?> StoreDuplicate(string pPathPlugin)
        {
            string nameFile = Path.GetFileName(pPathPlugin);
            string namePlugin;
            Guid id;

            namePlugin = Manager.Node.GetNameByFile(pPathPlugin);
            id = Guid.NewGuid();

            var reference = new ReferencePluginStore
            {
                Name = namePlugin,
                Id = $"{id:N}",
                FileName = nameFile,
                Version = PluginUtil.ExtratVersion(nameFile),
            };

            if (await ExistsByFile(nameFile).ConfigureAwait(false)) return null;
            if (!await operatePlugin(pPathPlugin, (p, d) => { File.Copy(p, d); }).ConfigureAwait(false))
                return null;

            if (!await registerPlugin(reference).ConfigureAwait(false))
            {
                await operatePlugin(nameFile, (p, d) => { File.Delete(d); }).ConfigureAwait(false);
                return null;
            }

            return id;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Elimina un registro de plugin y su archivo enlazado según el Guid registrado.<br />
        /// Notas: El uso es permanente, el fichero se borra por acción delegada.<br />
        /// Tips: Empléalo en tareas de limpieza controladas.<br />
        /// ___________________( English )___________________<br />
        /// Deletes a plugin registry and its linked file based on the registered Guid.<br />
        /// Notes: It's permanent, the file is deleted by delegated action.<br />
        /// Tips: Use it in controlled clean-up tasks.<br />
        /// </summary>
        /// <param name="pId">Es: El ID identificador en texto de dicho registro. <br />En: The text identifier ID of that record.</param>
        /// <param name="pCancellationToken">Es: Token interno para detener la acción de lectura. <br />En: Internal token to stop the reading action.</param>
        /// <returns>Es: Null o falso si no prosperó, verdadero tras limpiar con éxito. <br />En: Null or false if unsuccessful, true after successful wipe.</returns>
        public static async ValueTask<bool?> Eliminate(string pId, CancellationToken pCancellationToken = default)
        {
            var plugin = await Get(pId).ConfigureAwait(false);
            if (plugin is null) return null;
            if (plugin.Name is null) return null;
            if (plugin.Id is null) return null;

            if (pCancellationToken.IsCancellationRequested)
                return null;

            if (!await operatePlugin(plugin.Name, (p, d) => { File.Delete(d); }).ConfigureAwait(false))
                return false;

            if (!await unregisterPlugin(plugin.Id).ConfigureAwait(false))
                return false;
            return true;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Operación interna segura para mover/copiar archivos en disco usando semáforos.<br />
        /// Notas: Retiene hasta que finalice cualquiera de las demás tareas en disco para archivos.<br />
        /// Tips: No exportar jamás esta función para la API pública.<br />
        /// ___________________( English )___________________<br />
        /// Safe internal operation to move/copy files on disk using semaphores.<br />
        /// Notes: Blocks until any other pending disk file tasks finish.<br />
        /// Tips: Never export this function to the public API.<br />
        /// </summary>
        /// <param name="pPathPlugin">Es: Ruta principal de archivo inicial. <br />En: Initial file main path.</param>
        /// <param name="pOperate">Es: Delegado que realiza un IO (File.Copy o File.Move). <br />En: Delegate performing an IO (File.Copy or File.Move).</param>
        /// <returns>Es: Verdadero tras soltar el recurso de disco con éxito. <br />En: True after successfully releasing the disk resource.</returns>
        private static async ValueTask<bool> operatePlugin(string pPathPlugin, Action<string, string> pOperate)
        {
            string? pathStorage;
            string destination;

            await _semaphoreOperate.WaitAsync().ConfigureAwait(false);
            try
            {
                pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
                if (pathStorage is null) return false;

                destination = Path.Combine(pathStorage, Path.GetFileName(pPathPlugin));

                pOperate(pPathPlugin, destination);

            }
            finally
            {
                _semaphoreOperate.Release();
            }
            return true;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Registra la referencia construida formalmente en el fichero ManifestStorage.<br />
        /// Notas: Bloqueado a un solo acceso para evitar corrupción del manifiesto JSON.<br />
        /// Tips: Es de uso interno tras el store físico.<br />
        /// ___________________( English )___________________<br />
        /// Registers the formally built reference into the ManifestStorage file.<br />
        /// Notes: Locked to a single access to prevent JSON manifest corruption.<br />
        /// Tips: Internal use after physical store.<br />
        /// </summary>
        /// <param name="pReference">Es: El modelo de referencia que contiene la metadata clave. <br />En: The reference model containing key metadata.</param>
        /// <returns>Es: Devuelve un estado booleano de éxito según la escritura JSON. <br />En: Returns a boolean success state based on the JSON writing.</returns>
        private static async ValueTask<bool> registerPlugin(ReferencePluginStore pReference)
        {
            CodeAcessJSonSave r;
            await _semaphoreManifest.WaitAsync().ConfigureAwait(false);
            try
            {
                string? pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
                if (pathStorage == null)
                    return false;

                r = JSonUtil.UpdateDirect<ManifestStorage>(
                    pathStorage,
                    TerbinServiceConst.MANIFEST_STORAGE,
                    ii => { ii.References?.Add(pReference); }
                );
            }
            finally
            {
                _semaphoreManifest.Release();
            }

            return r == CodeAcessJSonSave.Succes;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Anula el registro interno evaluando un modelo previamente existente.<br />
        /// Notas: Es una función envolvente para sobreescribir la eliminación.<br />
        /// Tips: Úselo cuando el objeto a eliminar ya esté cargado enteramente.<br />
        /// ___________________( English )___________________<br />
        /// Nullifies internal registration evaluating a previously existing model.<br />
        /// Notes: It's a wrapper function to overwrite unregistration.<br />
        /// Tips: Use when the target object is already fully loaded.<br />
        /// </summary>
        /// <param name="pReference">Es: Modelo directo del plugin persistido. <br />En: Direct model of persisted plugin.</param>
        /// <returns>Es: Booleano según si la eliminación persistió. <br />En: Boolean indicating if deletion persisted.</returns>
        private static async ValueTask<bool> unregisterPlugin(ReferencePluginStore pReference)
        {
            if (pReference.Id is null)
                return false;
            return await unregisterPlugin(pReference.Id);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Elimina físicamente el fragmento local del JSON referido en el manifiesto.<br />
        /// Notas: Retiene cualquier iteración asíncrona hasta terminar el I/O JSON.<br />
        /// Tips: Preferible emplear esta sobrecarga si solo posees la llave Guid.<br />
        /// ___________________( English )___________________<br />
        /// Physically removes the local JSON fragment referred in the manifest.<br />
        /// Notes: Pauses any asynchronous iteration until JSON I/O finishes.<br />
        /// Tips: Prefer this overload if you only own the Guid key.<br />
        /// </summary>
        /// <param name="pId">Es: Identificador Guid mapeado a ser purgado. <br />En: Guid mapped identifier to be purged.</param>
        /// <returns>Es: Si la variable asíncrona fue guardada correctamente. <br />En: If the async variable correctly saved.</returns>
        private static async ValueTask<bool> unregisterPlugin(string pId)
        {
            CodeAcessJSonSave r;
            await _semaphoreManifest.WaitAsync().ConfigureAwait(false);
            try
            {
                string? pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
                if (pathStorage == null)
                    return false;

                r = JSonUtil.UpdateDirect<ManifestStorage>(
                    pathStorage,
                    TerbinServiceConst.MANIFEST_STORAGE,
                    ii =>
                    {
                        for (int i = 0; i < ii.References.Count; i++)
                        {
                            if (ii.References[i].Id == pId)
                                ii.References.RemoveAt(i);
                        }
                    }
                );
            }
            finally
            {
                _semaphoreManifest.Release();
            }
            return r == CodeAcessJSonSave.Succes;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Determina rápidamente si un nombre de fichero ya existe en el núcleo de almacenamiento de plugins.<br />
        /// Notas: No interactúa con el manifiesto, hace una query directa de IO del sistema.<br />
        /// Tips: Útil como comprobación temprana de colisiones.<br />
        /// ___________________( English )___________________<br />
        /// Swiftly determines if a file name already exists in the plugin storage core.<br />
        /// Notes: Doesn't interact with the manifest, doing a direct OS I/O query.<br />
        /// Tips: Useful as an early collision test.<br />
        /// </summary>
        /// <param name="pFile">Es: Nombre de archivo y extensión a comparar. <br />En: File name and extension to match.</param>
        /// <returns>Es: True en caso de encontrar el fichero. <br />En: True when file is found.</returns>
        public static async Task<bool> ExistsByFile(string pFile)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (path is null) return false;
            string[] r = Directory.GetFiles(path, pFile);
            return r.Length > 0;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Determina la preexistencia de un documento exacto dada una ruta especifica de contexto.<br />
        /// Notas: Realiza el recorrido del `GetFiles` sin filtros pesados.<br />
        /// Tips: No lo limites a los plugins, es genérico paramétrico.<br />
        /// ___________________( English )___________________<br />
        /// Evaluates the pre-existence of a precise document given a context route.<br />
        /// Notes: Executes `GetFiles` browsing without heavy filtering.<br />
        /// Tips: Don't limit it to plugins, it's parametric generic.<br />
        /// </summary>
        /// <param name="pFile">Es: Criterio final con nombre o extensión de fichero. <br />En: Match criteria with name or file extension.</param>
        /// <param name="pPath">Es: Ruta directa donde iterar. <br />En: Target direct iterating route.</param>
        /// <returns>Es: True si el buffer encuentra una o más devoluciones en el glob. <br />En: True if buffer returns one or more results from glob.</returns>
        public static async Task<bool> ExistByFile(string pFile, string pPath)
        {
            string[] r = Directory.GetFiles(pPath, pFile);
            return r.Length > 0;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Consulta la pertenencia de un Guid en el index de referencia del manifiesto de plugins.<br />
        /// Notas: Usa GetAll() y lee su integridad por ende tarda tanto en consultar como el I/O del JSON lo dicte.<br />
        /// Tips: Trate esto como carga pesada si la lista es grande.<br />
        /// ___________________( English )___________________<br />
        /// Queries Guid association within the plugin manifest reference index.<br />
        /// Notes: Calls GetAll() verifying integrity, so it takes as long as JSON I/O dictates.<br />
        /// Tips: Handle this as heavy-load if the list grows large.<br />
        /// </summary>
        /// <param name="pId">Es: El valor Id guardado en el archivo base. <br />En: Saved Id value in the base string.</param>
        /// <returns>Es: True si la lista expone positivamente tu Key. <br />En: True if list safely exposes your Key.</returns>
        public static async Task<bool> Exists(string pId)
        {
            var references = await GetAll().ConfigureAwait(false);
            if (references is null) return false;

            for (int i = 0; i < references.Count; i++)
            {
                if (references[i].Id == pId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Retorna el tipo referenciado del plugin según el manifiesto general si se logra relacionar.<br />
        /// Notas: Navega secuencialmente el manifiesto JSON cargado mapeando contra tu string Id.<br />
        /// Tips: Si el resultado es null asuma que el modelo ha sido borrado o ignorado por corrupción.<br />
        /// ___________________( English )___________________<br />
        /// Returns the referenced plugin object according to general manifest if linked correctly.<br />
        /// Notes: Navigates loaded JSON manifest mapping explicitly with your string Id.<br />
        /// Tips: When null assume missing due to erase or corruption.<br />
        /// </summary>
        /// <param name="pId">Es: Referencia clave exacta Guid a consultar. <br />En: Precise referential Guid to fetch.</param>
        /// <returns>Es: Objeto mapeado del plugin completo en base de datos. <br />En: Database fully mapped plugin entity.</returns>
        public static async Task<ReferencePluginStore?> Get(string pId)
        {
            var references = await GetAll().ConfigureAwait(false);
            if (references is null) return null;

            for (int i = 0; i < references.Count; i++)
            {
                if (references[i].Id == pId)
                    return references[i];
            }
            return null;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Interconecta el manifiesto local decodificado entregando listado completo de todos los fragmentos.<br />
        /// Notas: Usa un cargador de JSON directo por tanto es carga síncrona/asíncrona pesada.<br />
        /// Tips: Cachear o manejar mediante tareas de fondo para evitar trabas a rendimiento.<br />
        /// ___________________( English )___________________<br />
        /// Interconnects the local decoded manifest yielding out complete listing chunks.<br />
        /// Notes: Loads a pure JSON engine hence being a blocking-heavy call respectively.<br />
        /// Tips: Buffer it or map via background queues to drop runtime hitches.<br />
        /// </summary>
        /// <returns>Es: Una Lista completa mapeada que ilustra el inventario del store. <br />En: Complete mapped listing that paints the warehouse inventory.</returns>
        public static async Task<List<ReferencePluginStore>?> GetAll()
        {
            var man = await getManifest().ConfigureAwait(false);
            if (man is null) return null;
            return man.References;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Deserializa estrictamente todo el index local ManifestStorage en un modelo persistencial base.<br />
        /// Notas: Base cruda de uso interno sin validar referenciales contra disco físico.<br />
        /// Tips: Restringir a lectura solamente, cualquier mutación debe realizarse por update de JSON.<br />
        /// ___________________( English )___________________<br />
        /// Strictly parses the whole local ManifestStorage layout on a generic persistance format.<br />
        /// Notes: Raw internal utility without mapping directly reference models towards local hard-drive.<br />
        /// Tips: Enclose this within read boundary rules, logic changes belong to update JSON tools only.<br />
        /// </summary>
        /// <returns>Es: Objeto manifiesto cargado al ecosistema actual asincrono. <br />En: Loaded manifest object to asynchronous layer core.</returns>
        private static async ValueTask<ManifestStorage?> getManifest()
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (string.IsNullOrEmpty(path)) return null;

            var man = JSonUtil.AcessDirect<ManifestStorage>(path, TerbinServiceConst.MANIFEST_STORAGE);
            //if (man is null) return null;

            return man;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Ensambla y deduce limpiamente el trayecto exacto que requiere un archivo para funcionar en el Storage en base local.<br />
        /// Notas: Usado como utilería de parseo string contra directorio activo.<br />
        /// Tips: Funciona de pre-proceso para chequeos y loggers.<br />
        /// ___________________( English )___________________<br />
        /// Cleanly constructs and deduces the exact path logic needed for a file to run off local Storage space.<br />
        /// Notes: Simple parsing helper pointing toward active layout scope.<br />
        /// Tips: Behaves well at pre-processing logic for loggers and sanity checks.<br />
        /// </summary>
        /// <param name="pName">Es: Entrada nombre final de tu extensión. <br />En: Your final input extension label.</param>
        /// <returns>Es: Salida mapeada C://Ruta//Almacen... o null si falta configuración matriz. <br />En: Built local C://Path... result missing core setup nullish flag.</returns>
        public static string? MakePathPlugin(string pName)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (string.IsNullOrEmpty(path)) return null;

            return Path.Combine(path, pName);
        }

    }
}
