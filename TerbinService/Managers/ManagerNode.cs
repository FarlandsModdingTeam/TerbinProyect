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
}
