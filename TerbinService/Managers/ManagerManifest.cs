using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
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
            FileUtil.Hide(dir, _INSTANCES);
            return true;
        }
        public static bool DeleteIndex(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return false;

            JSonUtil.UpdateDirect<List<string>>(dir, _INSTANCES, ii => { ii.Remove(pName); });
            FileUtil.Hide(dir, _INSTANCES);
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


        public static Status HandleAddPlugin(Guid pGuid, string pNamePlugin, string pNameInstace, DirectoryHandwritten? pHandwritten)
        {
            return HandleAddPlugin($"{pGuid:N}", pNamePlugin, pNameInstace, pHandwritten);
        }
        public static Status HandleAddPlugin(string pGuid, string pNamePlugin, string pNameInstace, DirectoryHandwritten? pHandwritten)
        {
            var information = Manager.Instances.MakePathFolderInformation(pNameInstace);
            if (string.IsNullOrEmpty(information))
                return Status.ErrorGetManifest;

            string local = $"{Guid.NewGuid:N}";
            string name = pNamePlugin;
            string file = makeNameFieldPlugin(name, local);

            string pathRelativeManifest = MakePathRelativeManifest(information, file, pNameInstace);

            var manifest = new PluginManifest
            {
                Name = name,
                Id = pGuid,
                IdLocal = local,
                HandWritten = pHandwritten,
            };
            JSonUtil.SaveDirect(information, file, manifest);

            var reference = new ReferencePlugin
            {
                Name = name,
                Id = name,
                IdLocal = local,
                Path = pathRelativeManifest,
            };

            Manager.Manifest.UpdateInstace(pNameInstace, m => { m.Plugins.Add(reference); });
            return Status.Succes;
        }
        public static Status HandleRemovePlugin(string pIdLocal, string pNameInstace)
        {
            var information = Manager.Instances.MakePathFolderInformation(pNameInstace);
            if (string.IsNullOrEmpty(information))
                return Status.ErrorGetManifest;

            ReferencePlugin? reference = null;

            Manager.Manifest.UpdateInstace(pNameInstace, m =>
            {
                reference = m.Plugins.FirstOrDefault(p => p.IdLocal == pIdLocal);
                if (reference != null)
                    m.Plugins.Remove(reference);
            });

            if (reference != null)
            {
                string pathManifest = Path.Combine(information, makeNameFieldPlugin(reference.Name, reference.IdLocal));
                if (File.Exists(pathManifest))
                    File.Delete(pathManifest);
                return Status.Succes;
            }
            return Status.ErrorGetReference;
        }


        private static string makeNameFieldPlugin(string? pName, string? pGUID)
        {
            string guid = ""; 
            if (pName == null)
                guid = pGUID ?? $"{Guid.NewGuid:N}";
            pName ??= $"E:{CodeManifestError.NotAccesName}::{guid}";
            pGUID ??= $"E:{CodeManifestError.NotAccesIdLocal}::{guid}";
            return $"{pName}_{pGUID}.json";
        }


        public static string MakePathRelativeManifest(string pPathInformation, string pFile, string pNameInstace)
        {
            string pathManifest = Path.Combine(pPathInformation, pFile);
            var instance = Manager.Instances.MakePathFolder(pNameInstace);
            if (string.IsNullOrEmpty(instance))
                throw new Exception("TODO: informar de que no se pudo conseguir la information en manifest");
            string pathRelativeManifest = Path.GetRelativePath(instance, pathManifest);
            return pathRelativeManifest;
        }


        public static void WriteHandwritten(string pPath, DirectoryHandwritten? pJson)
        {
            if (pJson == null)
                return;

            pPath = Path.Combine(pPath, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE, TerbinServiceConst.HANDWRITTEN);
            File.WriteAllText(pPath, pJson.ToJson());
        }





        public enum Status : byte
        {
            IsCancelled = 0,
            Succes = 1,

            GenericError = 2,
            ErrorGetManifest = 3,
            ErrorGetReference = 4,
        }
    }
}
