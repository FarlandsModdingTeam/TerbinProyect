using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;

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


public static class ManagerNode
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
        var dirInstace = ManagerInstances.MakePathFolder(pName);
        if (dirInstace == null)
            return;

        if (!ManagerInstances.IsInstance(dirInstace))
            throw new Exception("TODO: Informar que NO existe la instancia O el manifiesto");

        var (status, json) = await FileUtil.CloneDirectory(pDirGame, dirInstace, true, pProgrss);

        if (status != StatusFileUtil.Succes) // si es Succes, json no es null
            throw new Exception("TODO: Informar de que farlands no se ah podido clonar");

        ManagerManifest.WriteHandwritten(dirInstace, json);


        var exes = FileUtil.GetAllExeFiles(dirInstace);
        if (exes is null)
            return;

        ManagerManifest.UpdateInstace(pName, dirInstace, manifest =>
        {
            manifest.Executable = exes[0];
        });
    }
}
