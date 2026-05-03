using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Configuration;
using TerbinService.Data;

namespace TerbinService.Instances;

public partial class InstancesService
{
    // El nombre de la instancia, bool si quieres instalar BepInEx, 
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

    public static bool NewInstance(string pName)
    {
        var dirInstace = MakePathFolder(pName);
        if (dirInstace == null)
            return false;

        if (Directory.Exists(dirInstace))
        {
            if (Directory.EnumerateFileSystemEntries(dirInstace).Any())
                throw new Exception("TODO: Preguntar si quiere sobreescribir");
        }
        else
        {
            Directory.CreateDirectory(dirInstace);
        }


        HandleManifest.CreatePredeterminated(pName);

        HandleManifest.UpdateCoreManifest(pName);
        return true;
    }

}
