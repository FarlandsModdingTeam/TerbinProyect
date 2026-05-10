using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;
using TerbinService.BepInEx;

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


internal static class PluginsServices
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Plugin)]
    public static async Task<InfoResponse?> InstallPluginService(Header pHead, byte[] pParameters)
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
            //pathPlugin = MakePathPluginByInstance(pathInstance);
            pathPlugin = BepInExService.GetBepInExFolderPlugin(pathInstance);
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
