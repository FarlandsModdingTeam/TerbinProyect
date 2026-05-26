using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Data.Manifests;
using static TerbinService.Managers.Manager;
using static TerbinService.Managers.Manager.Plugin;

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

// TODO: Controlar que por cada instancia solo pueda tocar un hilo a la vez.
public static partial class Manager
{
    public static class Instances
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _instanceLocks = new(StringComparer.OrdinalIgnoreCase);


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

        public static bool ExistInIndex(string pName)
        {
            List<string> index = Manager.Manifest.GetIndex();
            string? mani = index.FirstOrDefault(manifest => manifest == pName);
            return !string.IsNullOrEmpty(mani);
        }
        public static bool? ExistInPhisic(string pName)
        {
            var dir = Manager.Configuration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
            if (string.IsNullOrEmpty(dir))
                return null;

            if (!Directory.Exists(dir))
                return null;

            var all = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            if (all == null)
                return false;

            return all.Contains(pName);
        }


        public static async Task<DirectoryHandwritten?> InstallPlugin(string pPathPlugin, string pNameInstance, bool pOverwrite, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            string? pathInstance = Manager.Instances.MakePathFolder(pNameInstance);
            if (string.IsNullOrEmpty(pathInstance))
                return null;

            SemaphoreSlim instanceLock = _instanceLocks.GetOrAdd(pNameInstance, _ => new SemaphoreSlim(1, 1));
            await instanceLock.WaitAsync(pCancellationToken).ConfigureAwait(false);
            try
            {
                var result = await ZipUtil.ExtractWithProgress(pPathPlugin, pathInstance, pProgress, pOverwrite, pCancellationToken).ConfigureAwait(false);
                return result;
            }
            finally
            {
                instanceLock.Release();
            }
        }

        public static async Task<StatusFileUtil> UnistallPlugin(DirectoryHandwritten pPlugin, string pNameInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            //ArgumentNullException.ThrowIfNull(pPlugin.Root, "Root is not asing, need root in DirectoryHandwritten");

            string? pathInstance = Manager.Instances.MakePathFolder(pNameInstance);
            if (string.IsNullOrEmpty(pathInstance))
                return StatusFileUtil.InvalidSource;

            SemaphoreSlim instanceLock = _instanceLocks.GetOrAdd(pNameInstance, _ => new SemaphoreSlim(1, 1));
            await instanceLock.WaitAsync(pCancellationToken).ConfigureAwait(false);
            try
            {
                var result = FileUtil.DeleteFromHandwritten(pathInstance, pPlugin, pProgress);
                return result;
            }
            finally
            {
                instanceLock.Release();
            }
        }
    }
}
