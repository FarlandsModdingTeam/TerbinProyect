using System;
using System.Collections.Generic;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.HelperData;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
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

[TODO("Update Instance")]
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
        if (reader.Length > ThreeQuartersInt.Size)
            path = reader.ReadArray<char>().CrString();

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        // TODO: Si hay path crearlo ahí.
        bool succes = Manager.Instances.NewInstance(name, false);
        if (!succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceCreate));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> DeleteInstances(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        string name;
        ReadOnlySpan<byte> reader = pParameters;
        name = reader.ReadArray<char>().CrString();

        var r = await Manager.Instances.Delete(name, pToken);

        if (r == Manager.Instances.Status.IsCancelled)
            return InfoResponse.CreateCancelled(pHead.IdRequest);
        if (r != Manager.Instances.Status.Succes)
        {
            var error = TSHelper.GetError(r switch
            {
                Manager.Instances.Status.ErrorNotExist => CodeInternalErrors.InstanceNotExist,
                Manager.Instances.Status.ErrorIsNotInstance => CodeInternalErrors.InstanceIsNotInstance,
                Manager.Instances.Status.ErrorUnregistInstance => CodeInternalErrors.InstanceUnregister,
                _ => CodeInternalErrors.NodeDinamite,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> GetAllInstances(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        List<ReferenceInstance> instances = Manager.Index.GetAllInstances();
        Serialineitor s = new();

        if (instances.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(instances.Count);
        for (int i = 0; i < instances.Count; i++)
        {
            ReferenceInstanceDTO tmp = (ReferenceInstanceDTO)instances[i];
            s.AddStruct<ReferenceInstanceDTO>(tmp);
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

        ManifestInstance? manifest;

        manifest = await Manager.Instances.GetManifestByName(name);
        if (manifest is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstanceNotExist));

        byte[] dto = ((ManifestInstanceDTO)manifest).Serialize();

        return InfoResponse.CreateSucces(pHead.IdRequest, dto);
    }
}
