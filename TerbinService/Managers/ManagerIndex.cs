using TerbinLibrary.Configuration;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;
using TerbinService.Data.References;

namespace TerbinService.Managers;

public partial class Manager
{
    public class Index
    {
        private const string _INSTANCES = ".IndexInstances.json";

        // TODO: Doc.
        private static Action<ManifestIndex> deletedInInstance(ReferenceInstance pReference) => ii => { ii.Instances.Remove(pReference); };
        // TODO: Doc.
        private static Action<ManifestIndex> addInInstance(ReferenceInstance pReference) => ii => { ii.Instances.Add(pReference); };


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
            FileUtil.Hide(dir, _INSTANCES);
            return true;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Elimina una instancia del índice de instancias.<br />
        /// ___________________( English )___________________<br />
        /// Removes an instance from the instance index.<br />
        /// </summary>
        /// <param name="pName">Es: El nombre de la instancia a eliminar. <br />En: The name of the instance to remove.</param>
        /// <returns>Es: Verdadero si se elimina con éxito, falso en caso de error. <br />En: True if successfully removed, false on error.</returns>
        public static bool DeleteIndex(ReferenceInstance pReference)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<ManifestIndex>(dir, _INSTANCES, deletedInInstance(pReference));
            FileUtil.Hide(dir, _INSTANCES);
            return true;
        }


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



        // TODO: Documentacion.
        public static ReferenceInstance RegisterNewInstance(string pName, string? pPath = null, bool pOutSide = false)
        {
            ReferenceInstance r = new()
            {
                Name = pName,
                Path = pPath ?? Manager.Instances.MakePathFolder(pName),
                OutSide = pOutSide,
            };
            Manager.Index.UpdateIndex(r);
            return r;
        }

    }
}
