using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Data;
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
    public static class Manifest
    {
        private const string _INSTANCES = ".IndexInstances.json";

        public static bool UpdateIndex(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<List<string>>(dir, _INSTANCES, ii => { ii.Add(pName); });
            return true;
        }
        public static bool DeleteIndex(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<List<string>>(dir, _INSTANCES, ii => { ii.Remove(pName); });
            return true;
        }


        public static List<string> GetIndex()
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return new List<string>();
            return JSonUtil.AcessDirect<List<string>>(dir, _INSTANCES) ?? new List<string>();
        }



        public static void CreatePredeterminated(string pName)
        {
            string? dirInfo = Manager.Instances.MakePathFolderInformation(pName);
            if (dirInfo == null)
                return;
            DirectoryInfo directoryInfo = Directory.CreateDirectory(dirInfo);
            directoryInfo.Attributes |= FileAttributes.Hidden;
            CreatePredeterminatedInstance(pName, dirInfo);
        }

        public static void CreatePredeterminatedInstance(string pName, string pDir)
        {
            var manifest = new InstanceManifest
            {
                Name = pName,
                Version = TerbinLibrary.SteamFarlands.ManagerFarlands.GetVersion(),
                Plugins = []
            };
            JSonUtil.SaveDirect(pDir, TerbinServiceConst.MANIFEST_INSTANCE, manifest);
        }


        public static bool UpdateInstace(string pName, Action<InstanceManifest> updateAction)
        {
            var pathInstance = Manager.Instances.MakePathFolder(pName);
            if (pathInstance == null)
                return false;

            return UpdateInstace(pName, pathInstance, updateAction);
        }

        public static bool UpdateInstace(string pName, string pPathInstance, Action<InstanceManifest> updateAction)
        {
            var pathInformation = Manager.Instances.MakePathFolderInformation(pName);
            if (pathInformation is null)
                return false;

            JSonUtil.UpdateDirect<InstanceManifest>(pathInformation, TerbinServiceConst.MANIFEST_INSTANCE, updateAction);
            return true;
        }


        public static void HandleAddPlugin(string pNameInstace, DirectoryHandwritten? pHandwritten)
        {
            var information = Manager.Instances.MakePathFolderInformation(pNameInstace);
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
            JSonUtil.SaveDirect(information, file, manifest);

            var reference = new ReferencePlugin
            {
                Name = name,
                GUID = name,
                Path = pathManifest,
            };

            Manager.Manifest.UpdateInstace(pNameInstace, m => { m.Plugins.Add(reference); });
        }


        public static void WriteHandwritten(string pPath, DirectoryHandwritten? pJson)
        {
            if (pJson == null)
                return;

            pPath = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE, TerbinServiceConst.HANDWRITTEN);
            File.WriteAllText(pPath, pJson.ToJson());
        }

    }
}
