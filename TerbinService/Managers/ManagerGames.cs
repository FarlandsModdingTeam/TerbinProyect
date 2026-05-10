using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
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


public static class ManagerGames
{
    public static async Task HandleCloneInInstanceWithProgress(string pName, byte pIdMemoryGame, string pDirGame)
    {
        IProgress<TerbinInfoProgrss> progressBarr = Util.CreateProgessBarrForMemory(Worker.CurrentConst.Value.Communicator, pIdMemoryGame, p => {
            Console.Write($"\rClonando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual | Finalizado: {p.Finish}");
        });
        try
        {
            await HandleCloneInInstance(pName, pIdMemoryGame, pDirGame, progressBarr);
        }
        catch (Exception e)
        {
            string exceptionString = $$"""
                [PitufiChingada] ExceptionError->
                {
                    Message: {{e.Message}};
                    Source: {{e.Source}};
                    Inner: {{e.InnerException?.Message ?? "N/A"}};
                    Trace: {{e.StackTrace}};
                    String: {{e.ToString()}}
                }
                """;
            Console.WriteLine(exceptionString);
        }

    }

    public static async Task HandleCloneInInstance(string pName, byte pIdMemoryGame, string pDirGame, IProgress<TerbinInfoProgrss> pProgrss = default)
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
