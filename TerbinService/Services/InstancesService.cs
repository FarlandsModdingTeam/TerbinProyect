using System;
using System.Collections.Generic;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinService.Manifests;
using static TerbinService.Managers.InstancesManager;

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


internal static class InstancesService
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();

        NewInstance(name);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [],
        };
    }

    [TerbinExecutable((byte)CodeServices.ReadAllInstances)]
    public static async Task<InfoResponse?> GetAllInstances(Header pHead, byte[] pParameters)
    {
        List<string> instances = HandleManifest.GetIndex();
        Serialineitor s = new();

        if (instances.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest);

        s.Add<ThreeQuartersInt>(instances.Count);
        for (int i = 0; i < instances.Count; i++)
        {
            s.AddArray(instances[i].ToCharArray());
        }

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = s.Serialize(),
        };
    }

    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Read, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> ReadInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();

        var manifest = GetStringManifest(name);
        if (manifest is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceNotExistOrConfigError));

        byte[] pld = Serialineitor.SerializeArray(manifest.ToCharArray());

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = pld,
        };
    }

    // TODO: Update Instance.
    // TODO: Deleted Instance.
}
