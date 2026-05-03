using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Configuration;
using TerbinService.Data;
using TerbinService.Instances;

namespace TerbinService.Game;

public partial class GameService
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Game)]
    public static async Task<InfoResponse?> CloneGame(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();
        string dirGame = buffer.ReadArray<char>().CrString();

        var sizes = GetSizeDir(dirGame);
        if (sizes.maxFiles == null || sizes.maxDir == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceGetSizeError));

        var rId = await Worker.CurrentConst.Value.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rId.Payload[0];

        _ = HandleCloneInInstanceWithProgress(nameInstance, id, dirGame);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = new Serialineitor()
                        .Add(id)
                        .Add(sizes.maxFiles.Value)
                        .Add(sizes.maxDir.Value)
                        .Serialize(),
        };
    }


    public static async Task HandleCloneInInstanceWithProgress(string pName, byte pIdMemoryGame, string pDirGame)
    {
        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            var Content = p.ToArray();
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemoryGame, Content);

            Console.Write($"\rClonando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual  | Finalizado: {p.Finish}");
        });
        await HandleCloneInInstance(pName, pIdMemoryGame, pDirGame, progressBarr);
    }

    public static async Task HandleCloneInInstance(string pName, byte pIdMemoryGame, string pDirGame, IProgress<TerbinInfoProgrss> pProgrss = default)
    {
        var dirInstace = InstancesService.MakePathFolder(pName);
        if (dirInstace == null)
            return;

        if (InstancesService.IsInstance(dirInstace))
            throw new Exception("TODO: Informar que NO existe la instancia O el manifiesto");

        var (status, json) = await FileUtil.CloneDirectory(pDirGame, dirInstace, true, pProgrss);

        if (status != StatusFileUtil.Succes) // si es Succes, json no es null
            throw new Exception("TODO: Informar de que farlands no se ah podido clonar");

        File.WriteAllText(dirInstace, json.ToJson());


        var exes = FileUtil.GetAllExeFiles(dirInstace);
        if (exes is null)
            return;

        HandleManifest.UpdateInstace(pName, dirInstace, manifest =>
        {
            manifest.Executable = exes[0];
        });
    }


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
