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

        // TODO: De carpeta de Farlands.
        // ├─Coger Dir Carpeta.
        // └─Comprobar si esta el ejecutable <todo: mover dentro del metodo>.

        // TODO: Del nombre carpeta destino.
        // └─Comprobar si la carpeta esta vacia.

        // HandleCreateInstance(); // Tengo sueño.
        // HandleCloneFarlands();

        // TODO: Instalar BepInEx si el entrante es que si.

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    public static async Task HandleCreateInstance(string pName)
    {
        // TODO: De la carpeta destino.
        // ├─Comprobar si esta vacia.
        // └─Comprobar si ya hay una instancia.
    }
    public static async Task<(Task<StatusFileUtil> result, long? maxFiles, long? maxDir)?>
                HandleCloneFarlands(string pDir, IProgress<TerbinInfoProgrss> pProgrss = default)
    {
        string? dirFarlands = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_FARLANDS);

        if (dirFarlands is null)
            return null;

        if (!ManagerFarlands.IsFarlands(dirFarlands))
                return null; // (StatusFileUtil.InvalidSource, null, null)

        var countDir = FileUtil.GetCountDirectories(dirFarlands);
        var countFiles = FileUtil.GetCountFiles(dirFarlands);
        var result = FileUtil.CloneDirectory(dirFarlands, pDir, true, pProgrss);

        // TODO:Crear manifiesto.

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
