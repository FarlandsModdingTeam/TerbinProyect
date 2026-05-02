using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Configuration;
using TerbinService.Data;

namespace TerbinService.Instances;

public partial class InstancesService
{
    [Obsolete]
    // El nombre de la instancia, bool si quieres instalar BepInEx, 
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        string? dirFarlands = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_FARLANDS);
        if (dirFarlands == null) 
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.FarlandRuteNotExist));
        var sizes = GetSizeDir(dirFarlands);

        if (sizes.maxFiles == null || sizes.maxDir == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceGetSizeError));

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();
        var instalBepInEx = reader.Read<bool>();

        AmongInfoThreads info = Worker.CurrentConst.Value;

        byte idBepInEx = 0;
        var rIdB = await info.Communicator.SoliciteRequestMemory();
        if (rIdB.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        idBepInEx = rIdB.Payload[0];

        byte idFarlands = 0;
        if (instalBepInEx)
        {
            var rIdF = await info.Communicator.SoliciteRequestMemory();
            if (rIdF.Head.Status != CodeStatus.Succes)
                return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
            idFarlands = rIdF.Payload[0];
        }
        else
            idFarlands = (byte)CodeTerbinMemory.None;

        _ = HandleCreateInstance(name, idFarlands, idBepInEx, instalBepInEx);

        byte[] pld = new Serialineitor()
            .Add(idFarlands)
            .Add(idBepInEx)
            .Add(sizes.maxFiles.Value)
            .Add(sizes.maxDir.Value)
            .ToArray();

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = pld,
        };
    }

    [Obsolete]
    public static async Task
        HandleCreateInstance(string pName, byte pIdMemoryGame, byte pIdMemoryBepInEx, bool pInstallBepInEx)
    {
        var dirInstace = GetIntance(pName);
        if (dirInstace == null)
            return;

        if (!Directory.Exists(dirInstace))
            Directory.CreateDirectory(dirInstace);
        else
            if (Directory.EnumerateFileSystemEntries(dirInstace).Any())
                throw new Exception("TODO: Preguntar si quiere sobreescribir");

        // TODO: Comprobar si existe un manifest y IsFarlands()
        if (ManagerFarlands.IsFarlands(dirInstace))
            throw new Exception("TODO: Solicitar si quiere reinstalar");


        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            var Content = p.ToArray();
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemoryGame, Content);

            Console.Write($"\rClonando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual  | Finalizado: {p.Finish}");
        });
        var result = await HandleCloneFarlands(dirInstace, progressBarr);
        if (result == null)
            throw new Exception("TODO: Informar Algo ah pasado al coger a farlands");

        var (status, json) = await result;
        if (status != StatusFileUtil.Succes)
            throw new Exception("TODO: Informar de que farlands no se ah podido clonar");


        File.WriteAllText(dirInstace, json);
        var manifest = new InstanceManifest
        {
            Name = pName,
            Version = ManagerFarlands.GetVersion(),
            Plugins = []
        };
        AcessJSon.SaveDirect(dirInstace, TerbinServiceConst.NAME_OF_MANIFEST, manifest);

        HandleManifest.UpdateCoreManifest(pName);

        if (!pInstallBepInEx || pIdMemoryBepInEx <= TerbinProtocol.RESERVE_MEMORY)
            return;

        _ = BepInExService.HandleInstallBepInExWithProgress(pIdMemoryBepInEx, dirInstace);
    }
    [Obsolete]
    public static async Task<Task<(StatusFileUtil status, string? json)>?>
                HandleCloneFarlands(string pDir, IProgress<TerbinInfoProgrss> pProgrss = default)
    {
        string? dirFarlands = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_FARLANDS);

        if (dirFarlands is null)
            return null;

        if (!ManagerFarlands.IsFarlands(dirFarlands))
                return null; 

        var result = FileUtil.CloneDirectory(dirFarlands, pDir, true, pProgrss);
        return result;
    }
    [Obsolete]
    public static (long? maxFiles, long? maxDir) GetSizeDir(string pDir)
    {
        long? countFiles = FileUtil.GetCountFiles(pDir);
        long? countDir = FileUtil.GetCountDirectories(pDir);
        return (countFiles, countDir);
    }


    public static void NewInstance(string pName)
    {
        var dirInstace = GetIntance(pName);
        if (dirInstace == null)
            return;

        if (Directory.Exists(dirInstace))
        {
            if (Directory.EnumerateFileSystemEntries(dirInstace).Any())
                throw new Exception("TODO: Preguntar si quiere sobreescribir");
        }
        else
        {
            Directory.CreateDirectory(dirInstace);
        }

        string dirInfo = Path.Combine(dirInstace, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);
        Directory.CreateDirectory(dirInfo);

        createPredeterminatedInstanceManifest(pName, dirInfo);

        HandleManifest.UpdateCoreManifest(pName);
    }

    private static void createPredeterminatedInstanceManifest(string pName, string pDir, string? pGame = null)
    {
        var manifest = new InstanceManifest
        {
            Name = pName,
            Version = ManagerFarlands.GetVersion(),
            Plugins = []
        };
        AcessJSon.SaveDirect(pDir, TerbinServiceConst.NAME_OF_MANIFEST, manifest);
    }
}
