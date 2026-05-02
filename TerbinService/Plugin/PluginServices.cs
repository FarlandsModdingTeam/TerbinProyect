using System;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;
using TerbinLibrary.Extension;
using TerbinService.Instances;

namespace TerbinService.Plugin;

public partial class PluginServices
{
    private static string _ñññ = "https://github.com/FarlandsModdingTeam/FarlandsCoreMod/releases/download/v0.1.2/FCM_0.1.2.zip";
    private static string _ññ = "https://github.com/FarlandsModdingTeam/UnityExplorer/releases/download/v4.9.0/com.sinai.unityexplorer.zip";

    [TerbinExecutable((byte)CodeServices.WIP_NewService)]
    public static async Task<InfoResponse?> WIP_NewService(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        long? sizeBepInEx = await NetUtil.GetContentLength(TerbinURLs.BepInEx);
        if (sizeBepInEx is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.BepInExNotConect));

        AmongInfoThreads info = Worker.CurrentConst.Value;

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();
        var urlPlugin = reader.ReadArray<char>().CrString();

        // Habra alguna forma de saber si es un direccion valida?

        if (!Directory.Exists(rute))
            Directory.CreateDirectory(rute);

        byte idMemory = 0;
        var rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.IdSoliciteError));

        idMemory = rId.Payload[0];
        _ = HandleInstallPluginWithProgress(idMemory, name, urlPlugin);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [idMemory, .. Serialineitor.Serialize<long>(sizeBepInEx.Value)],
        };
    }


    public static async Task HandleInstallPluginWithProgress(byte pIdMemory, string pNameInstance, string pUrl)
    {
        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            var Content = p.ToArray();
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, Content);
            Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        StatusNetUtil? r = await HandleInstallPlugin(pNameInstance, pUrl, progressBarr);
        if (r is null) throw new Exception("TODO: informar de que BepInEx ya esta instalado");
        if (r != StatusNetUtil.Succes)
        {
            CodeInternalErrors error = r switch
            {
                StatusNetUtil.ExceptionOnExtractZip => CodeInternalErrors.ZipExtractException,
                StatusNetUtil.ExceptionDeleteTemporalFile => CodeInternalErrors.ZipDeletedTempException,
                _ => CodeInternalErrors.ZipExtractError
            };
            throw new Exception($"TODO: informar de {error}");

            // Prototipo del funcionamiento de Info
            AmongInfoThreads info = Worker.CurrentConst.Value;
            byte[] pld = new Serialineitor()
                .Add(TypeService.Service)
                .Add(CodeServices.InstallBepInEx)
                .Add(error)
                .ToArray();
            _ = info.Communicator.Send((byte)CodeTerbinProtocol.Info, pld);
        }
    }

    public static async Task<StatusNetUtil?> HandleInstallPlugin(string pNameInstance, string pUrl, IProgress<TerbinInfoProgrss>? pProgress = default)
    {
        StatusNetUtil r = StatusNetUtil.Succes;
        string? dir = InstancesService.MakePathFolder(pNameInstance);
        if (dir is null) return null;

        if (!BepInExService.CheckInstallBepInEx(dir)) return null;
        r = await NetUtil.InstallZip(pUrl, dir, pProgress);
        return r;
    }
}