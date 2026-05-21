using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.Extension;
using TerbinService.Managers;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Protocol;
using TerbinLibrary.Useful.NetWork;

namespace TerbinService.Services;
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


internal static class ServicesPlugins
{
    [TerbinExecutable((byte)CodeServices.Install, (byte)CodeSubServices.Plugin)]
    public static async Task<InfoResponse?> InstallPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
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
        pathInstance = Manager.Instances.MakePathFolder(name);
        if (pathInstance is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceNotExit));
        if (requierBepInEx)
        {
            if (!Manager.BepInEx.CheckInstallBepInEx(pathInstance))
                return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.BepInExNotInstall));
            //pathPlugin = MakePathPluginByInstance(pathInstance);
            pathPlugin = Manager.BepInEx.GetBepInExFolderPlugin(pathInstance);
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

        _ = Manager.Plugin.HandleInstallPluginWithProgress(name, memoryDownload, memoryExtract, pathPlugin, urlPlugin);

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


    [TerbinExecutable((byte)CodeServices.Dowload, (byte)CodeSubServices.Plugin)]
    public static async Task<InfoResponse?> DowloadPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        AmongInfoThreads info = Worker.CurrentConst.Value;

        ReadOnlySpan<byte> reader = pParameters;
        string urlPlugin = reader.ReadArray<char>().CrString();
        bool requierBepInEx = reader.Read<bool>();


        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = new Serialineitor()
                        .Add('a')
                        .Serialize(),
        };
    }
}