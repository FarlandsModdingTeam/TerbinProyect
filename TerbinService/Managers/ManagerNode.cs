using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;

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
    public static class Node
    {
        public static async Task<Task<(StatusFileUtil status, DirectoryHandwritten? json)>?>
                    HandleCloneGame(string pDirSource, string pDirTarjet, IProgress<TerbinInfoProgrss> pProgrss = default)
        {
            if (!ManagerFarlands.IsFarlands(pDirSource))
                return null;

            var result = FileUtil.CloneDirectory(pDirSource, pDirTarjet, true, pProgrss);
            return result;
        }

        public static (long? maxFiles, long? maxDir) GetSizeDir(string pDir)
        {
            long? countFiles = FileUtil.GetCountFiles(pDir);
            long? countDir = FileUtil.GetCountDirectories(pDir);
            return (countFiles, countDir);
        }



        public static async Task HandleCloneDirectory(string pName, byte pIdMemoryGame, string pDirGame, IProgress<TerbinInfoProgrss>? pProgrss = default)
        {
            var dirInstace = Manager.Instances.MakePathFolder(pName);
            if (dirInstace == null)
                return;

            if (!Manager.Instances.IsInstance(dirInstace))
                throw new Exception("TODO: Informar que NO existe la instancia O el manifiesto");

            var (status, json) = await FileUtil.CloneDirectory(pDirGame, dirInstace, true, pProgrss);

            if (status != StatusFileUtil.Succes) // si es Succes, json no es null
                throw new Exception("TODO: Informar de que farlands no se ah podido clonar");

            Manager.Manifest.WriteHandwritten(dirInstace, json);


            var exes = FileUtil.GetAllExeFiles(dirInstace);
            if (exes is null)
                return;

            Manager.Manifest.UpdateInstace(pName, dirInstace, manifest =>
            {
                manifest.Executable = exes[0];
            });
        }
    }
}
