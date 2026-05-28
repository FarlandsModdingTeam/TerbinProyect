using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;
using TerbinService.Data.References;
using static TerbinService.Managers.Manager;
using static TerbinService.Managers.Manager.Plugin;

namespace TerbinService.Managers;

public static partial class Manager
{
    // No esta protegido el ExistsByFile y el guardar como uno.
    public static class StoragePlugin
    {
        // TerbinConfiguration
        // TerbinServiceConst.MANIFEST_STORAGE
        private static readonly SemaphoreSlim _semaphoreOperate = new(1, 1);
        private static readonly SemaphoreSlim _semaphoreManifest = new(1, 1);

        public static async ValueTask<Guid?> Store(string pPathPlugin, string pNameFile, bool pDuplicate = false)
        {
            string newPath = Path.Combine(Path.GetDirectoryName(pPathPlugin) ?? string.Empty, pNameFile);
            File.Move(pPathPlugin, newPath);

            return await Store(newPath, pDuplicate);
        }
        public static async ValueTask<Guid?> Store(string pPathPlugin, bool pDuplicate = false)
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
            };

            if (await ExistsByFile(nameFile).ConfigureAwait(false)) return null; 
            if (pDuplicate)
            {
                if (!await operatePlugin(pPathPlugin, (p, d) => { File.Copy(p, d); }).ConfigureAwait(false))
                    return null;
            }
            else
            {
                if (!await operatePlugin(pPathPlugin, (p, d) => { File.Move(p, d); }).ConfigureAwait(false))
                    return null;
            }

            if (!await registerPlugin(reference).ConfigureAwait(false))
            {
                await operatePlugin(nameFile, (p, d) => { File.Delete(d); }).ConfigureAwait(false);
                return null;
            }

            return id;
        }

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
        private static async ValueTask<bool> unregisterPlugin(ReferencePluginStore pReference)
        {
            if (pReference.Id is null)
                return false;
            return await unregisterPlugin(pReference.Id);
        }
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


        public static async Task<bool> ExistsByFile(string pFile)
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
        public static async Task<List<ReferencePluginStore>?> GetAll()
        {
            var man = await getManifest().ConfigureAwait(false);
            if (man is null) return null;
            return man.References;
        }


        private static async ValueTask<ManifestStorage?> getManifest()
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (string.IsNullOrEmpty(path)) return null;

            var man = JSonUtil.AcessDirect<ManifestStorage>(path, TerbinServiceConst.MANIFEST_STORAGE);
            //if (man is null) return null;

            return man;
        }

        public static string? MakePathPlugin(string pName)
        {
            string? path = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_STORAGE_PLUGINS);
            if (string.IsNullOrEmpty(path)) return null;

            return Path.Combine(path, pName);
        }

        // TODO: Un TryGetVersion, A partir del nombre del archivo intentar detectar un patron de versionado.
    }
}
