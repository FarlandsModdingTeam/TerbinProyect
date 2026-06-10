using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinService.Managers;

namespace TerbinService.Services;

internal class ServicePluginStorage
{
    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.PluginStorage)]
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

    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.PluginStorage)]
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

}
