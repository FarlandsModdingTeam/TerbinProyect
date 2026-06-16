using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.NetWork;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Managers;

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


internal static class ServicePlugins
{
    [TODO("Comprobar que el plugin no exista")]
    [TerbinExecutable((byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> DowloadPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string urlPlugin = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;

        long? sizePlugin = await NetUtil.GetContentLength(urlPlugin, pCancellationToken: CancellationToken.None);
        if (sizePlugin is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.PluginNotConect));

        if (useProgress)
        {
            MaxProgressDTO max = new(sizePlugin.Value);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        // TODO: Comprobar que exista y si existe preguntar si quiere sobre-escrbir.
        var r = await Manager.Plugin.DowloadOne(urlPlugin, progress, pToken);

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);
        if (r != Manager.Plugin.Status.Succes)
        {
            var error = TSHelper.GetError(r switch
            {
                Manager.Plugin.Status.NotSuchSpace => CodeInternalErrors.PluginNotSuchSpace,
                Manager.Plugin.Status.InvalidURL => CodeInternalErrors.PluginInvalidURL,
                _ => CodeInternalErrors.PluginOnDowload,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }


    [TerbinExecutable((byte)CodeServices.Install, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> InstallPlugin(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string idPlugin = reader.ReadArray<char>().CrString();
        string relativePath = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        string? pathPlugin;
        string? pathInstance;
        IProgress<TerbinInfoProgrss>? progress = null;

        pathInstance = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(pathInstance))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        pathPlugin = Path.Combine(pathInstance, relativePath);

        if (useProgress)
        {
            MaxProgressDTO max = new(await Manager.StoragePlugin.GetSize(idPlugin));
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var r = await Manager.Plugin.InstallOne(idPlugin, name, pathPlugin, progress, pToken);

        if (r == Manager.Plugin.Status.IsCancelled)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        if (r != Manager.Plugin.Status.Succes)
        {
            // ErrorGetPlugin, ErrorGetPathPlugin, ErrorGetManifest, ErrorOnSaveManifest, GenericError
            var error = TSHelper.GetError(r switch
            {
                Manager.Plugin.Status.ErrorGetPathPlugin => CodeInternalErrors.PluginGetPath,
                Manager.Plugin.Status.ErrorGetManifest => CodeInternalErrors.PluginGetManifest,
                Manager.Plugin.Status.ErrorOnSaveManifest => CodeInternalErrors.PluginOnSave,
                _ => CodeInternalErrors.PluginNotExist,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> GetAll(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();

        ManifestInstance? manifest;
        ManifestPlugin[] manis;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(nameInstance);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        manis = await Manager.Plugin.GetAll(path, manifest, pToken);

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        Serialineitor s = new();

        if (manis.Length <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(manis.Length);
        for (int i = 0; i < manis.Length; i++)
        {
            ManifestPluginDTO tmp = (ManifestPluginDTO)manis[i];
            s.AddStruct<ManifestPluginDTO>(tmp);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string id = reader.ReadArray<char>().CrString();

        ManifestInstance? manifest;
        ManifestPlugin? mani;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        mani = await Manager.Plugin.GetOne(id, name, manifest, pToken);
        if (mani is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        byte[] dto = ((ManifestPluginDTO)mani).Serialize();

        return InfoResponse.CreateSucces(pHead.IdRequest, dto);
    }


    [TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.Plugin)]
    public static async Task<InfoResponse?> Delete(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string name = reader.ReadArray<char>().CrString();
        string id = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;

        ManifestInstance? manifest;
        ManifestPlugin? mani;
        string? path;

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        path = Manager.Instances.GetPathFolder(name);
        if (string.IsNullOrEmpty(path))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        manifest = await Manager.Instances.GetManifestByPath(path);
        if (manifest == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        mani = await Manager.Plugin.GetOne(id, name, manifest, pToken);
        if (mani is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        if (useProgress)
        {
            MaxProgressDTO max = new(mani.HandWritten.GetSize());
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Deleted, (byte)CodeServicesSection.Plugin);
        }

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        // Eh mirado y se supone que no puede fallar, raro.
        StatusNodeUtil r = await Manager.Instances.UnistallPlugin(mani.HandWritten, name, progress, pToken);

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}