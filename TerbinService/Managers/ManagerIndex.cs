using TerbinLibrary.Configuration;
using TerbinLibrary.Extension;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;
using TerbinService.Data.References;

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


public partial class Manager
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Gestiona el índice general de las referencias de instancias en el administrador.<br />
    /// Proporciona utilidades para registrar, actualizar, eliminar y obtener el manifiesto de las instancias configuradas en el sistema.<br />
    /// ___________________( English )___________________<br />
    /// Manages the general index of instance references in the manager.<br />
    /// Provides utilities to register, update, delete, and retrieve the manifest of configured instances in the system.<br />
    /// </summary>
    public static class Index
    {
        private const string _INSTANCES = ".IndexInstances.json";

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea una acción para eliminar una referencia de instancia del índice de manifiestos.<br />
        /// Se utiliza en la actualización directa del índice mediante operaciones JSON.<br />
        /// ___________________( English )___________________<br />
        /// Creates an action to remove an instance reference from the manifest index.<br />
        /// It is used in direct index updating via JSON operations.<br />
        /// </summary>
        /// <param name="pReference">Es: La referencia de instancia que se desea eliminar. <br />En: The instance reference to be removed.</param>
        /// <returns>Es: Una acción que remueve la instancia dada. <br />En: An action that removes the given instance.</returns>
        private static Action<ManifestIndex> deletedInInstance(ReferenceInstance pReference) => ii => { ii.Instances.Remove(pReference); };

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea una acción para añadir una referencia de instancia al índice de manifiestos.<br />
        /// Se utiliza en la actualización directa del índice mediante operaciones JSON.<br />
        /// ___________________( English )___________________<br />
        /// Creates an action to add an instance reference to the manifest index.<br />
        /// It is used in direct index updating via JSON operations.<br />
        /// </summary>
        /// <param name="pReference">Es: La referencia de instancia que se desea añadir. <br />En: The instance reference to be added.</param>
        /// <returns>Es: Una acción que agrega la instancia dada. <br />En: An action that adds the given instance.</returns>
        private static Action<ManifestIndex> addInInstance(ReferenceInstance pReference) => ii => { ii.Instances.Add(pReference); };

        private static Action<ManifestIndex> deletedInstanceByName(string pName) => ii =>
        {
            for (int i = 0; i < ii.Instances.Count; i++)
            {
                var inst = ii.Instances[i];
                if (inst.Name == pName)
                    ii.Instances.RemoveAt(i);
            }
        };

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Actualiza el índice de instancias añadiendo un nuevo nombre.<br />
        /// ___________________( English )___________________<br />
        /// Updates the instance index by adding a new name.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia a agregar. <br />En: The name of the instance to add.</param>
        /// <returns>Es: Verdadero si se actualiza con éxito, falso en caso de error. <br />En: True if successfully updated, false on error.</returns>
        public static bool UpdateIndex(ReferenceInstance pReference)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<ManifestIndex>(dir, _INSTANCES, addInInstance(pReference));
            NodeUtil.HideFile(dir, _INSTANCES);
            return true;
        }

        // TODO: Doc.
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Elimina una instancia del índice de instancias.<br />
        /// ___________________( English )___________________<br />
        /// Removes an instance from the instance index.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia a eliminar. <br />En: The name of the instance to remove.</param>
        /// <returns>Es: Verdadero si se elimina con éxito, falso en caso de error. <br />En: True if successfully removed, false on error.</returns>
        public static bool DeleteIndex(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<ManifestIndex>(dir, _INSTANCES, deletedInstanceByName(pName));
            NodeUtil.HideFile(dir, _INSTANCES);
            return true;
        }

        // TODO: Doc.
        public static ReferenceInstance? GetInstance(string pName)
        {
            List<ReferenceInstance> all = GetAllInstances();
            for (int i = 0; i < all.Count; i++)
            {
                var ins = all[i];
                if ((ins.Name?.Equals(pName)).ToBool())
                    return ins;
            }
            return null;
        }

        // TODO: Doc.
        public static List<ReferenceInstance> GetAllInstances() =>
            Manager.Index.GetIndex().Instances ?? new();
        

        // TODO: Actualizar Doc.
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Obtiene la lista de instancias registradas en el índice.<br />
        /// ___________________( English )___________________<br />
        /// Retrieves the list of instances registered in the index.<br />
        /// </summary>
        /// <returns>Es: Una lista con los nombres de las instancias. <br />En: A list containing the instance names.</returns>
        public static ManifestIndex GetIndex()
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return new ManifestIndex();
            return JSonUtil.AcessDirect<ManifestIndex>(dir, _INSTANCES) ?? new ManifestIndex();
        }



        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Registra una nueva instancia en el administrador.<br />
        /// Crea una referencia para la instancia y actualiza el índice general para reflejar la adición.<br />
        /// Notas: Si no se proporciona una ruta, se generará una automáticamente basada en el nombre.<br />
        /// ___________________( English )___________________<br />
        /// Registers a new instance in the manager.<br />
        /// Creates a reference for the instance and updates the general index to reflect the addition.<br />
        /// Notes: If no path is provided, one will be generated automatically based on the name.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la nueva instancia. <br />En: The name of the new instance.</param>
        /// <param name="pPath">Es: La ruta opcional donde residirá la instancia. <br />En: The optional path where the instance will reside.</param>
        /// <param name="pOutSide">Es: Indica si la instancia se encuentra fuera de la estructura predeterminada. <br />En: Indicates whether the instance is outside the default structure.</param>
        /// <returns>Es: La referencia de la instancia creada. <br />En: The reference to the created instance.</returns>
        public static ReferenceInstance RegisterInstance(string pName, string? pPath = null, bool pOutSide = false)
        {
            ReferenceInstance r = new()
            {
                Name = pName,
                Path = pPath ?? Manager.Instances.MakePathFolderFromConfig(pName),
                OutSide = pOutSide,
            };
            Manager.Index.UpdateIndex(r);
            return r;
        }
        public static bool UnregisterInstance(string pName)
        {
            return Manager.Index.DeleteIndex(pName);
        }
    }
}
