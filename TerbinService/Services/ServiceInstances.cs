using System;
using System.Collections.Generic;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinService.Data.References;
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


internal static class ServiceInstances
{
    [TerbinExecutable((byte)CodeServices.Create, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        string name;
        string path;

        ReadOnlySpan<byte> reader = pParameters;
        name = reader.ReadArray<char>().CrString();
        if (reader.Length > ThreeQuartersInt.Space)
            path = reader.ReadArray<char>().CrString();

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        bool succes = Manager.Instances.NewInstance(name, false);
        // TODO: Si hay path crearlo ahí.

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> GetAllInstances(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        List<ReferenceInstance> instances = Manager.Index.GetIndex().Instances;
        Serialineitor s = new();

        if (instances.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest);

        s.Add<ThreeQuartersInt>(instances.Count);
        for (int i = 0; i < instances.Count; i++)
        {
            ReferenceInstanceSerilizable tmp = (ReferenceInstanceSerilizable)instances[i];
            s.AddStruct<ReferenceInstanceSerilizable>(tmp);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();

        var manifest = Manager.Instances.GetStringManifestByName(name);
        if (manifest is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceNotExist));

        byte[] pld = Serialineitor.SerializeArray(manifest.ToCharArray());

        return InfoResponse.CreateSucces(pHead.IdRequest, pld);
    }

    // TODO: Update Instance.
    // TODO: Deleted Instance (Obsoleto), Dinamitar en ServiceNode.
}
