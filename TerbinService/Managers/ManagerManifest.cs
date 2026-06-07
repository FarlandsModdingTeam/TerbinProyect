using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Data.Manifests;
using TerbinLibrary.Data.References;
using TerbinLibrary.SteamFarlands;
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
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Clase que gestiona los manifiestos, índices y configuración de instancias y plugins.<br />
    /// ___________________( English )___________________<br />
    /// Class that manages manifests, indexes, and configuration for instances and plugins.<br />
    /// </summary>
    public static class Manifest
    {
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea el manifiesto predeterminado para una instancia específica en el directorio dado.<br />
        /// ___________________( English )___________________<br />
        /// Creates the default manifest for a specific instance in the given directory.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <param name="pDirInfo">Es: Directorio donde se creará el manifiesto. <br />En: Directory where the manifest will be created.</param>
        public static void CreateInstance(string pName, string pDirInfo)
        {
            ManifestInstance? manifest;
            // manifest = JSonUtil.AcessDirect<InstanceManifest>(pDirInfo, TerbinServiceConst.MANIFEST_INSTANCE);

            manifest = new ManifestInstance
            {
                Name = pName,
                Version = Games.GetVersion(),
                Plugins = []
            };
            JSonUtil.SaveDirect(pDirInfo, TerbinServiceConst.MANIFEST_INSTANCE, manifest);
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Actualiza el manifiesto de una instancia dado su nombre y una acción de modificación.<br />
        /// ___________________( English )___________________<br />
        /// Updates the manifest of an instance given its name and a modification action.<br />
        /// </summary>
        /// <param name="pName">Es: Nombre de la instancia. <br />En: Name of the instance.</param>
        /// <param name="updateAction">Es: Acción a realizar sobre el manifiesto. <br />En: Action to perform on the manifest.</param>
        /// <returns>Es: Verdadero si la ruta es válida y se puede actualizar. <br />En: True if the path is valid and can be updated.</returns>
        public static bool UpdateInstaceByName(string pName, Action<ManifestInstance> updateAction)
        {
            var pathInstance = Manager.Instances.GetPathFolder(pName);
            if (pathInstance == null)
                return false;

            return UpdateInstaceByPath(pathInstance, updateAction);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Actualiza el manifiesto de una instancia dado su nombre, ruta y una acción de modificación.<br />
        /// ___________________( English )___________________<br />
        /// Updates the manifest of an instance given its name, path, and a modification action.<br />
        /// </summary>
        /// <param name="pName">Es: Nombre de la instancia. <br />En: Name of the instance.</param>
        /// <param name="pPathInstance">Es: Ruta directa a la instancia. <br />En: Direct path to the instance.</param>
        /// <param name="updateAction">Es: Acción a realizar sobre el manifiesto. <br />En: Action to perform on the manifest.</param>
        /// <returns>Es: Verdadero si se encuentra la información y se actualiza. <br />En: True if information is found and updated.</returns>
        public static bool UpdateInstaceByPath(string pPathInstance, Action<ManifestInstance> updateAction)
        {
            string? pathInformation = Manager.Instances.MakePathFolderInformationByPath(pPathInstance);
            if (string.IsNullOrEmpty(pathInformation))
                return false;

            JSonUtil.UpdateDirect<ManifestInstance>(pathInformation, TerbinServiceConst.MANIFEST_INSTANCE, updateAction);
            return true;
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Gestiona la adición de un plugin a una instancia, generando su manifiesto correspondiente con un GUID normal.<br />
        /// ___________________( English )___________________<br />
        /// Handles adding a plugin to an instance, generating its corresponding manifest using a normal GUID.<br />
        /// </summary>
        /// <param name="pGuid">Es: El identificador único del plugin. <br />En: The unique identifier of the plugin.</param>
        /// <param name="pNamePlugin">Es: El nombre del plugin. <br />En: The name of the plugin.</param>
        /// <param name="pNameInstace">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <param name="pHandwritten">Es: Datos escritos a mano del directorio. <br />En: Handwritten data of the directory.</param>
        /// <returns>Es: El estado de la operación. <br />En: The status of the operation.</returns>
        public static (Status status, string local) HandleAddPlugin(Guid pGuid, string pNamePlugin, string pNameInstace, bool pOutSideIntence, DirectoryHandwritten? pHandwritten)
        {
            return HandleAddPlugin($"{pGuid:N}", pNamePlugin, pNameInstace, pOutSideIntence, pHandwritten);
        }
        

        // TODO: Actualizar Documentacion.
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Gestiona la adición de un plugin a una instancia definiendo el GUID como cadena.<br />
        /// ___________________( English )___________________<br />
        /// Handles adding a plugin to an instance by defining the GUID as a string.<br />
        /// </summary>
        /// <param name="pGuid">Es: El identificador único del plugin en formato cadena. <br />En: The unique identifier of the plugin as a string.</param>
        /// <param name="pNamePlugin">Es: El nombre del plugin. <br />En: The name of the plugin.</param>
        /// <param name="pNameInstace">Es: El nombre de la instancia de destino. <br />En: The name of the target instance.</param>
        /// <param name="pHandwritten">Es: Datos escritos a mano u opcionales del directorio. <br />En: Handwritten or optional directory data.</param>
        /// <returns>Es: El resultado de la operación (ej. Éxito, Error al obtener manifiesto). <br />En: The result of the operation (e.g., Success, Error getting manifest).</returns>
        public static (Status status, string local) HandleAddPlugin(string pGuid, string pNamePlugin, string pNameInstace, bool pOutSideIntence, DirectoryHandwritten? pHandwritten)
        {
            string? information = Manager.Instances.GetPathFolderInformationByName(pNameInstace);
            if (string.IsNullOrEmpty(information))
                return (Status.ErrorInstanceNotExist, "");

            string local = $"{Guid.NewGuid:N}";
            string name = pNamePlugin;
            string file = makeNameFieldPlugin(name, local);

            string pathRelativeManifest = MakePathRelativeManifest(information, file, pNameInstace);

            var manifest = new ManifestPlugin
            {
                Name = name,
                Id = pGuid,
                IdLocal = local,
                OutSideIntance = pOutSideIntence,
                HandWritten = pHandwritten,
            };
            var r = JSonUtil.SaveDirect(information, file, manifest);

            if (r != CodeAcessJSonSave.Succes)
                return (Status.ErrorOnSaveManifest, "");

            var reference = new ReferencePlugin
            {
                Name = name,
                Id = name,
                IdLocal = local,
                Path = pathRelativeManifest,
            };

            Manager.Manifest.UpdateInstaceByName(pNameInstace, m => { m.Plugins.Add(reference); });
            return (Status.Succes, local);
        }
        
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Gestiona la eliminación de un plugin de una instancia.<br />
        /// ___________________( English )___________________<br />
        /// Handles the removal of a plugin from an instance.<br />
        /// </summary>
        /// <param name="pIdLocal">Es: El ID local del plugin. <br />En: The local ID of the plugin.</param>
        /// <param name="pNameInstace">Es: El nombre de la instancia. <br />En: The name of the instance.</param>
        /// <returns>Es: El estado de la eliminación. <br />En: The status of the removal.</returns>
        public static Status HandleRemovePlugin(string pIdLocal, string pNameInstace)
        {
            string? information = Manager.Instances.GetPathFolderInformationByName(pNameInstace);
            if (string.IsNullOrEmpty(information))
                return Status.ErrorInstanceNotExist;

            ReferencePlugin? reference = null;

            Manager.Manifest.UpdateInstaceByName(pNameInstace, m =>
            {
                reference = m.Plugins.FirstOrDefault(p => p.IdLocal == pIdLocal);
                if (reference != null)
                    m.Plugins.Remove(reference);
            });

            if (reference == null)
                return Status.ErrorGetReference;

            string pathManifest = Path.Combine(information, makeNameFieldPlugin(reference.Name, reference.IdLocal));
            if (File.Exists(pathManifest))
                File.Delete(pathManifest);
            return Status.Succes;
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea el nombre de archivo combinado utilizando el nombre y GUID del plugin.<br />
        /// ___________________( English )___________________<br />
        /// Creates the combined file name using the plugin's name and GUID.<br />
        /// </summary>
        /// <param name="pName">Es: Nombre del plugin. <br />En: Plugin name.</param>
        /// <param name="pGUID">Es: Identificador GUID del plugin. <br />En: Plugin GUID identifier.</param>
        /// <returns>Es: Nombre del archivo del plugin. <br />En: Plugin file name.</returns>
        private static string makeNameFieldPlugin(string? pName, string? pGUID)
        {
            string guid = ""; 
            if (pName == null)
                guid = pGUID ?? $"{DateAndTime.Now}";
            pName ??= $"E:{CodeManifestError.NotAccesName}::{guid}";
            pGUID ??= $"E:{CodeManifestError.NotAccesIdLocal}::{guid}";
            return $"{pName}_{pGUID}.json";
        }


        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Genera la ruta relativa del archivo de manifiesto hacia el directorio base de la instancia.<br />
        /// ___________________( English )___________________<br />
        /// Generates the manifest file's relative path to the instance base directory.<br />
        /// </summary>
        /// <param name="pPathInformation">Es: Ruta con información. <br />En: Path with information.</param>
        /// <param name="pFile">Es: Nombre del archivo de configuración. <br />En: Configuration file name.</param>
        /// <param name="pNameInstace">Es: Nombre de la instancia base. <br />En: Name of the base instance.</param>
        /// <returns>Es: Cadena con la ruta relativa calculada. <br />En: String with the calculated relative path.</returns>
        public static string MakePathRelativeManifest(string pPathInformation, string pFile, string pNameInstace)
        {
            string pathManifest = Path.Combine(pPathInformation, pFile);
            string instance = Manager.Instances.MakePathFolder(pNameInstace);
            string pathRelativeManifest = Path.GetRelativePath(instance, pathManifest);
            return pathRelativeManifest;
        }


        public static DirectoryHandwritten? GetHandwritten(string pPath)
        {
            pPath = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);

            return JSonUtil.AcessDirect<DirectoryHandwritten>(pPath, TerbinServiceConst.HANDWRITTEN);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Escribe el contenido manual o JSON proporcionado en la ruta específica.<br />
        /// ___________________( English )___________________<br />
        /// Writes the handwritten or JSON content provided to the specific path.<br />
        /// </summary>
        /// <param name="pPath">Es: Ruta base de la instancia. <br />En: Base path of the instance.</param>
        /// <param name="pJson">Es: Objeto de contenido manual a guardar. <br />En: Handwritten content object to save.</param>
        public static bool WriteHandwritten(string pPath, DirectoryHandwritten? pJson)
        {
            if (pJson == null)
                return false;
            if (!Manager.Instances.IsInstance(pPath))
                return false;

            pPath = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE, TerbinServiceConst.HANDWRITTEN);
            File.WriteAllText(pPath, pJson.ToJson());
            return true;
        }


        public static bool RemoveHandwritten(string pPath)
        {
            string path = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE, TerbinServiceConst.HANDWRITTEN);
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }



        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Representa los diferentes estados de resultado tras realizar operaciones de manifiestos.<br />
        /// ___________________( English )___________________<br />
        /// Represents the different outcome statuses after performing manifest operations.<br />
        /// </summary>
        public enum Status : byte
        {
            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Operación cancelada.<br />
            /// ___________________( English )___________________<br />
            /// Operation cancelled.<br />
            /// </summary>
            IsCancelled = 0,
            
            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Operación exitosa.<br />
            /// ___________________( English )___________________<br />
            /// Successful operation.<br />
            /// </summary>
            Succes = 1,

            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Error genérico.<br />
            /// ___________________( English )___________________<br />
            /// Generic error.<br />
            /// </summary>
            GenericError = 2,
            
            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Error al obtener el manifiesto.<br />
            /// ___________________( English )___________________<br />
            /// Error reading or getting the manifest.<br />
            /// </summary>
            ErrorGetManifest = 3,
            
            /// <summary>
            /// ___________________( Español )___________________<br />
            /// Error al obtener la referencia.<br />
            /// ___________________( English )___________________<br />
            /// Error reading or getting the reference.<br />
            /// </summary>
            ErrorGetReference = 4,

            ErrorOnSaveManifest = 5,

            ErrorInstanceNotExist = 6,
        }
    }
}
