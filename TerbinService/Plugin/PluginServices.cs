using System;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;
using TerbinService.BepInEx;
using TerbinService.Data;
using TerbinService.Instances;
using TerbinService.Manifests;

namespace TerbinService.Plugin;

public partial class PluginServices
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Plugin)]
    public static async Task<InfoResponse?> InstallPlugin(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        AmongInfoThreads info = Worker.CurrentConst.Value;

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string urlPlugin = reader.ReadArray<char>().CrString();
        bool requierBepInEx = reader.Read<bool>();

        string? pathInstance;
        string pathPlugin;
        pathInstance = InstancesService.MakePathFolder(name);
        if (pathInstance is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceNotExit));
        if (requierBepInEx)
        {
            if (!BepInExService.CheckInstallBepInEx(pathInstance))
                return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.BepInExNotInstall));
            pathPlugin = MakePathPluginByInstance(pathInstance);
        }
        else
        {
            pathPlugin = pathInstance;
        }


        long? sizePlugin = await NetUtil.GetContentLength(urlPlugin);
        if (sizePlugin is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.PluginNotConect));

        // Solicitar id de memoria.
        var rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte memoryDownload = rId.Payload[0];

        rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte memoryExtract = rId.Payload[0];

        _ = HandleInstallPluginWithProgress(name, memoryDownload, memoryExtract, pathPlugin, urlPlugin);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = new Serialineitor()
                        .Add(memoryDownload)
                        .Add(memoryExtract)
                        .Add(sizePlugin.Value)
                        .Serialize(),
        };
    }


    public static async Task HandleInstallPluginWithProgress(string pNameInstace, byte pIdDownload, byte pIdExtract, string pPathPlugin, string pUrl)
    {
        IProgress<TerbinInfoProgrss> progressBarrDownload = new Progress<TerbinInfoProgrss>(p =>
        {
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdDownload, p.Serialize());
            Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        IProgress<TerbinInfoProgrss> progressBarrExtract = new Progress<TerbinInfoProgrss>(p =>
        {
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdExtract, p.Serialize());
            Console.Write($"\rInstalando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        StatusNetUtil r = await HandleInstallPlugin(pNameInstace, pUrl, pPathPlugin, progressBarrExtract, progressBarrDownload);
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
                .Serialize();
            _ = info.Communicator.Send((byte)CodeTerbinProtocol.Info, pld);
        } 
    }
    public static async Task<StatusNetUtil?> SimpleInstallPlugin(
        string pNameInstance,
        string pUrl,
        IProgress<TerbinInfoProgrss>? pProgressDownload = default,
        IProgress<TerbinInfoProgrss>? pProgressExtract = default)
    {
        StatusNetUtil r = StatusNetUtil.Succes;
        string? pathInstance = InstancesService.MakePathFolder(pNameInstance);

        if (pathInstance is null) return null;
        if (!BepInExService.CheckInstallBepInEx(pathInstance)) return null;

        r = await HandleInstallPlugin(pNameInstance, pUrl, pathInstance, pProgressExtract, pProgressDownload);
        return r;
    }

    public static async Task<StatusNetUtil> HandleInstallPlugin(
                                            string pNameInstace,
                                            string pUrl,
                                            string pPathPlugin,
                                            IProgress<TerbinInfoProgrss>? pProgressZip = null,
                                            IProgress<TerbinInfoProgrss>? pProgressDowload = null,
                                            CancellationToken pCancellationToken = default)
    {
        var (status, json) = await NetUtil.InstallZipWithProgress(pUrl, pPathPlugin, pProgressZip, pProgressDowload);

        HandleManifest.HandleAddPlugin(pNameInstace, json);

        return status;
    }



    public static string? MakePathPluginByName(string pNameInstance)
    {
        string? pathInstance;
        string pathPlugin;
        pathInstance = InstancesService.MakePathFolder(pNameInstance);
        if (pathInstance is null)
            return null;
        pathPlugin = Path.Combine(pathInstance, TerbinServiceConst.PATH_BEPINEX_PLUGIN);
        if (!Directory.Exists(pathPlugin))
            Directory.CreateDirectory(pathPlugin);
        return pathPlugin;
    }
    public static string MakePathPluginByInstance(string pPathInstance)
    {
        string pathPlugin;
        pathPlugin = Path.Combine(pPathInstance, TerbinServiceConst.PATH_BEPINEX_PLUGIN);
        if (!Directory.Exists(pathPlugin))
            Directory.CreateDirectory(pathPlugin);
        return pathPlugin;
    }
}