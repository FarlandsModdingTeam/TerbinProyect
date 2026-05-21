using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinService.Data.References;

namespace TerbinService.Managers;

public static class Maneger
{
    public static class StoragePlugin
    {
        // TerbinConfiguration
        // TerbinServiceConst.MANIFEST_STORAGE

        public static async Task<Guid?> Store(string pPathPlugin, string pNameFile)
        {
            // TODO: Renombrar el archivo de pPathPlugin por pNameFile.
            return await Store(pPathPlugin);
        }
        public static async Task<Guid?> Store(string pPathPlugin)
        {
            string nameFile = Path.GetFileName(pPathPlugin);
            string namePlugin;
            Guid id;

            if (await ExistByFile(nameFile)) return null;

            namePlugin = Manager.Node.GetNameByFile(pPathPlugin);
            id = Guid.NewGuid();

            var reference = new ReferencePluginStore
            {
                Name = namePlugin,
                Guid = $"{id}:N",
                File = nameFile,
            };

        }

        private static async ValueTask<bool> savePlugin(string pPathPlugin)
        {
            string? pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (pathStorage is null) return false;

            File.Move(pPathPlugin, pathStorage);

            return true;
        }

        private static async Task registerPlugin(ReferencePluginStore pReference)
        {

        }

        public static async Task<bool> ExistByFile(string pFile)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (path is null) return false;
            string[] r = Directory.GetFiles(path, pFile);
            return r.Length > 0;
        }

        public static async Task<bool> ExistByFile(string pFile, string pPath)
        {
            string[] r = Directory.GetFiles(pPath, pFile);
            return r.Length > 0;
        }

        public static async Task<bool> Exist(Guid pG)
        {


            return false;
        }
    }
}
