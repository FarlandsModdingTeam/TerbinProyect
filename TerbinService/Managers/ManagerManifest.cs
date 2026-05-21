using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data;
using TerbinService.Data.Manifests;
using TerbinService.Data.References;
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


internal static partial class Manager
{
    internal static class Manifest
    {
        private const string _INSTANCES = ".IndexInstances.json";

        public static async Task<bool> UpdateIndex(string pName)
        {
            var dir = await Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            await JSonUtil.UpdateDirectAsync<List<string>>(dir, _INSTANCES, ii => { ii.Add(pName); });
            return true;
        }
        public static async Task<bool> DeleteIndex(string pName)
        {
            var dir = await Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            await JSonUtil.UpdateDirectAsync<List<string>>(dir, _INSTANCES, ii => { ii.Remove(pName); });
            return true;
        }


        public static async Task<List<string>> GetIndex()
        {
            var dir = await Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return new List<string>();
            return await JSonUtil.AcessDirect<List<string>>(dir, _INSTANCES) ?? new List<string>();
        }



        public static async Task CreatePredeterminated(string pName)
        {
            string? dirInfo = await Manager.Instances.MakePathFolderInformation(pName);
            if (dirInfo == null)
                return;
            DirectoryInfo directoryInfo = Directory.CreateDirectory(dirInfo);
            directoryInfo.Attributes |= FileAttributes.Hidden;
            await CreatePredeterminatedInstance(pName, dirInfo);
        }

        public static async Task CreatePredeterminatedInstance(string pName, string pDir)
        {
            var manifest = new InstanceManifest
            {
                Name = pName,
                Version = TerbinLibrary.SteamFarlands.ManagerFarlands.GetVersion(),
                Plugins = []
            };
            await JSonUtil.SaveDirectAsync(pDir, TerbinServiceConst.MANIFEST_INSTANCE, manifest);
        }


        public static async Task<bool> UpdateInstace(string pName, Action<InstanceManifest> updateAction)
        {
            var pathInstance = await Manager.Instances.MakePathFolder(pName);
            if (pathInstance == null)
                return false;

            return await UpdateInstace(pName, pathInstance, updateAction);
        }

        public static async Task<bool> UpdateInstace(string pName, string pPathInstance, Action<InstanceManifest> updateAction)
        {
            var pathInformation = await Manager.Instances.MakePathFolderInformation(pName);
            if (pathInformation is null)
                return false;

            await JSonUtil.UpdateDirectAsync<InstanceManifest>(pathInformation, TerbinServiceConst.MANIFEST_INSTANCE, updateAction);
            return true;
        }


        public static async Task HandleAddPlugin(string pNameInstace, DirectoryHandwritten? pHandwritten)
        {
            var information = await Manager.Instances.MakePathFolderInformation(pNameInstace);
            if (information is null)
                throw new Exception("TODO: informar de que no se pudo conseguir la information en manifest");

            Guid g = Guid.NewGuid();
            string name = $"{g:N}";
            string file = $"{g:N}.json";
            string pathManifest = Path.Combine(information, $"{g:N}.json");

            var manifest = new PluginManifest
            {
                Name = name,
                Content = pHandwritten,
            };
            await JSonUtil.SaveDirectAsync(information, file, manifest);

            var reference = new ReferencePlugin
            {
                Name = name,
                GUID = name,
                Path = pathManifest,
            };

            await Manager.Manifest.UpdateInstace(pNameInstace, m => { m.Plugins.Add(reference); });
        }


        public static async Task WriteHandwritten(string pPath, DirectoryHandwritten? pJson)
        {
            if (pJson == null)
                return;

            pPath = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE, TerbinServiceConst.HANDWRITTEN);
            await File.WriteAllTextAsync(pPath, pJson.ToJson());
        }

    }
}
