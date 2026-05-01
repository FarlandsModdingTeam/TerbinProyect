using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Configuration;

namespace TerbinService.Instances;

public partial class InstancesService
{
    // El nombre de la instancia, bool si quieres instalar BepInEx, 
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        // HandleCreateInstance(); // Tengo sueño.
        // HandleCloneFarlands();

        // TODO: Instalar BepInEx si el entrante es que si.

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    public static async Task<(Task<(StatusFileUtil status, string? json)> result, long? maxFiles, long? maxDir)?>
        HandleCreateInstance(string pName, byte pIdMemory)
    {
        string? dir = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
        if (dir == null)
            return null;

        var newInstace = Path.Combine(dir, pName);
        if (!Directory.Exists(newInstace))
            Directory.CreateDirectory(newInstace);
        else
            if (Directory.EnumerateFileSystemEntries(newInstace).Any())
                return null; // TODO: Preguntar si quiere sobreescribir
                        // Worker.CurrentConst.Value.Communicator.Send();

        // TODO: Comprobar si existe un manifest y IsFarlands()


        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            AmongInfoThreads info = Worker.CurrentConst.Value;

            var Content = p.ToArray();
            _ = info.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, Content);

            Console.Write($"\rClonando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        var result = await HandleCloneFarlands(newInstace, progressBarr);
        // TODO:Crear manifiesto.
        return result; 

        // TODO: De carpeta de Farlands.
        // ├─Coger Dir Carpeta.
        // └─Comprobar si esta el ejecutable <todo: mover dentro del metodo>.
        // TODO: De la carpeta destino.
        // ├─Comprobar si esta vacia.
        // └─Comprobar si ya hay una instancia.
    }
    public static async Task<(Task<(StatusFileUtil status, string? json)> result, long? maxFiles, long? maxDir)?>
                HandleCloneFarlands(string pDir, IProgress<TerbinInfoProgrss> pProgrss = default)
    {
        string? dirFarlands = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_FARLANDS);

        if (dirFarlands is null)
            return null;

        if (!ManagerFarlands.IsFarlands(dirFarlands))
                return null; // (StatusFileUtil.InvalidSource, null, null)

        var countDir = FileUtil.GetCountDirectories(dirFarlands);
        var countFiles = FileUtil.GetCountFiles(dirFarlands);
        // TODO: Gurdar Json.
        var result = FileUtil.CloneDirectory(dirFarlands, pDir, true, pProgrss);

        return (result, countFiles, countDir);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool CreateManifest()
    {
        // TODO: Crear manifiesto de la instancia con todos los mods.

        throw new NotImplementedException();
    }
}
