using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Store;
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
    // ReferencePluginStoreDTO
    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.PluginStorage)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        List<ReferencePluginStore> plugin;
        Serialineitor s = new();

        plugin = await Manager.StoragePlugin.GetAll();

        if (plugin.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(plugin.Count);
        for (int i = 0; i < plugin.Count; i++)
            s.AddStruct<ReferencePluginStoreDTO>(plugin[i].ToDTO());

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    // ReferencePluginStoreDTO
    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.PluginStorage)]
    public static async Task<InfoResponse?> GetAll(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string id = reader.ReadArray<char>().CrString();

        ReferencePluginStore? plugin;

        plugin = await Manager.StoragePlugin.Get(id);
        if (plugin is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.PluginNotExist));

        return InfoResponse.CreateSucces(pHead.IdRequest, plugin.ToSerilizeDTO());
    }
}