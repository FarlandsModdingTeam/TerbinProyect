using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinService.Configuration;
using System.Text.Json;

namespace TerbinService.Instances;

public partial class InstancesService
{
    [TerbinExecutable((byte)CodeServices.ReadAllInstances)]
    public static async Task<InfoResponse?> GetAllInstances(Header pHead, byte[] pParameters)
    {
        List<string> instances = HandleManifest.GetCore();
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
            Payload = s.ToArray(),
        };
    }

    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Read, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> ReadInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();

        var manifest = GetManifestIntance(name);
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


    public static string? GetManifestIntance(string pName)
    {
        string? dir = GetIntance(pName);
        if (dir == null)
            return null;

        string file = Path.Combine(dir, pName);
        if (!File.Exists(file))
            return null;

        return File.ReadAllText(file);
    }

    public static string? GetIntance(string pName)
    {
        var dir = ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_INSTANCES);
        if (dir == null)
            return null;

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return Path.Combine(dir, pName);
    }
}

