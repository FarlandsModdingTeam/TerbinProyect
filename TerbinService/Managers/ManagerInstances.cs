using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;

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
    public static class Instances
    {
        public static bool NewInstance(string pName)
        {
            var dirInstace = MakePathFolder(pName);
            if (dirInstace == null)
                return false;

            if (Directory.Exists(dirInstace))
            {
                if (Directory.EnumerateFileSystemEntries(dirInstace).Any())
                    throw new Exception("TODO: Preguntar si quiere sobreescribir");
            }
            else
            {
                Directory.CreateDirectory(dirInstace);
            }


            Manager.Manifest.CreatePredeterminated(pName);

            Manager.Manifest.UpdateIndex(pName);
            return true;
        }
        public static bool IsInstance(string pDir)
        {
            string information;
            string manifest;

            information = Path.Combine(pDir, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);

            if (!Directory.Exists(information)) return false;

            manifest = Path.Combine(information, TerbinServiceConst.MANIFEST_INSTANCE);

            return File.Exists(manifest);
        }
        public static string? GetStringManifest(string pName)
        {
            string? dir = MakePathFolder(pName);
            if (dir == null)
                return null;

            string file = Path.Combine(dir, pName);
            if (!File.Exists(file))
                return null;

            return File.ReadAllText(file);
        }

        public static InstanceManifest? GetManifest(string pName)
        {
            string? dir = MakePathFolder(pName);
            if (dir == null)
                return null;

            return JSonUtil.AcessDirect<InstanceManifest>(dir, TerbinServiceConst.MANIFEST_INSTANCE);
        }

        public static string? MakePathFolderInformation(string pName)
        {
            var dir = MakePathFolder(pName);
            if (dir == null)
                return null;

            return Path.Combine(dir, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);
        }

        public static string? CreatePathFolder(string pName)
        {
            var dir = MakePathFolder(pName);
            if (dir == null)
                return null;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
        public static string? MakePathFolder(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (dir == null)
                return null;

            return Path.Combine(dir, pName);
        }
    }
}
