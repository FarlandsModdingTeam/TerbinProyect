using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;
using TerbinService.Data.References;

namespace TerbinService.Managers;

public static class Maneger
{
    public static class StoragePlugin
    {
        // TerbinConfiguration
        // TerbinServiceConst.MANIFEST_STORAGE

        public static async ValueTask<Guid?> Store(string pPathPlugin, string pNameFile)
        {
            string newPath = Path.Combine(Path.GetDirectoryName(pPathPlugin) ?? string.Empty, pNameFile);
            File.Move(pPathPlugin, newPath);

            return await Store(newPath);
        }
        public static async ValueTask<Guid?> Store(string pPathPlugin)
        {
            string nameFile = Path.GetFileName(pPathPlugin);
            string namePlugin;
            Guid id;

            if (await ExistByFile(nameFile).ConfigureAwait(false)) return null;

            namePlugin = Manager.Node.GetNameByFile(pPathPlugin);
            id = Guid.NewGuid();

            var reference = new ReferencePluginStore
            {
                Name = namePlugin,
                Id = $"{id:N}",
                FileName = nameFile,
            };

            if (!await savePlugin(pPathPlugin).ConfigureAwait(false))
                return null;

            if (!await registerPlugin(reference).ConfigureAwait(false))
            {
                await removePlugin(nameFile).ConfigureAwait(false);
                return null;
            }

            return id;
        }

        public static async ValueTask<bool?> Eliminate(string pId)
        {
            var plugin = await Get(pId).ConfigureAwait(false);
            if (plugin is null) return null;
            if (plugin.Name is null) return null;
            if (plugin.Id is null) return null;

            if (!await removePlugin(plugin.Name))
                return false;

            if (!await unregisterPlugin(plugin.Id).ConfigureAwait(false))
                return false;
            return true;
        }

        private static async ValueTask<bool> savePlugin(string pPathPlugin)
        {
            string? pathStorage;
            string destination;
            
            pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (pathStorage is null) return false;

            destination = Path.Combine(pathStorage, Path.GetFileName(pPathPlugin));

            File.Move(pPathPlugin, destination);

            return true;
        }

        private static async ValueTask<bool> registerPlugin(ReferencePluginStore pReference)
        {
            string? pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (pathStorage == null)
                return false;

            var r = JSonUtil.UpdateDirect<ManifestStorage>(
                pathStorage,
                TerbinServiceConst.MANIFEST_STORAGE,
                ii => { ii.References?.Add(pReference); }
            );

            return r == CodeAcessJSonSave.Succes;
        }
        private static async ValueTask<bool> removePlugin(string pFileName)
        {
            string? pathStorage;
            string destination;

            pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (pathStorage is null) return false;

            destination = Path.Combine(pathStorage, Path.GetFileName(pFileName));

            File.Delete(destination);

            return true;
        }

        private static async ValueTask<bool> unregisterPlugin(ReferencePluginStore pReference)
        {
            if (pReference.Id is null)
                return false;
            return await unregisterPlugin(pReference.Id);
        }
        private static async ValueTask<bool> unregisterPlugin(string pId)
        {
            string? pathStorage = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (pathStorage == null)
                return false;

            var r = JSonUtil.UpdateDirect<ManifestStorage>(
                pathStorage,
                TerbinServiceConst.MANIFEST_STORAGE,
                ii => {
                    if (ii.References is null) return;
                    for (int i = 0; i < ii.References.Count; i++)
                    {
                        if (ii.References[i].Id == pId)
                            ii.References.RemoveAt(i);
                    }
                }
            );

            return r == CodeAcessJSonSave.Succes;
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

        public static async Task<bool> Exist(string pId)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (path is null) return false;

            var man = JSonUtil.AcessDirect<ManifestStorage>(path, TerbinServiceConst.MANIFEST_STORAGE);
            if (man is null) return false;
            if (man.References is null) return false;

            for (int i = 0; i < man.References.Count; i++)
            {
                if (man.References[i].Id == pId)
                    return true;
            }
            return false;
        }

        public static async Task<ReferencePluginStore?> Get(string pId)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (path is null) return null;

            var man = JSonUtil.AcessDirect<ManifestStorage>(path, TerbinServiceConst.MANIFEST_STORAGE);
            if (man is null) return null;
            if (man.References is null) return null;

            for (int i = 0; i < man.References.Count; i++)
            {
                if (man.References[i].Id == pId)
                    return man.References[i];
            }
            return null;
        }


        // TODO: Un TryGetVersion, A partir del nombre del archivo intentar detectar un patron de versionado.
    }
}
