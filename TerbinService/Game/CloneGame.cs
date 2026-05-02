using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Configuration;
using TerbinService.Data;
using TerbinService.Instances;

namespace TerbinService.Game;

public partial class GameService
{




    public static async Task HandleCloneInInstance(string pName, byte pIdMemoryGame, string pDirGame)
    {
        var dirInstace = InstancesService.GetIntance(pName);
        if (dirInstace == null)
            return;

        if (!Directory.Exists(dirInstace))
            throw new Exception("TODO: Informar que no existe la instancia");

        // TODO: Comprobar si existe un manifest


        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            var Content = p.ToArray();
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemoryGame, Content);

            Console.Write($"\rClonando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual  | Finalizado: {p.Finish}");
        });
        //var result = await HandleCloneGame(pDirGame, dirInstace, progressBarr);
        var (status, json) = await FileUtil.CloneDirectory(pDirGame, dirInstace, true, progressBarr);

        if (status != StatusFileUtil.Succes)
            throw new Exception("TODO: Informar de que farlands no se ah podido clonar");

        File.WriteAllText(dirInstace, json);

        // TODO: Actualizar manifiesto.
    }


    public static async Task<Task<(StatusFileUtil status, string? json)>?>
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
