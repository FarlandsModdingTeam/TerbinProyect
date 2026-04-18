using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Data;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;

namespace TerbinService.Configuration;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */


public class ConfiguratonFarlands
{
    private const string RUTE_FARLANDS = "rute_farlands";
    private const string RUTE_INSTANCES = "rute_instances";
    // TODO: Tener ruta predeterminada para ambos.

    [TerbinCRUD(CodeTerbinProtocol.Update, (byte)CodeServices.FarlandsRute)]
    public static async Task<PacketRequest> UpdateRuteFarlands(Header pHead, MemoryStream pParameters)
    { 
        if (pParameters.Length <= 0)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }

        string? newRute = Serialineitor.DeserializeArray<char>(pParameters.ToArray()).ToString();
        if (newRute == null)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }

        if (ManagerConfiguration.SetConfig(RUTE_FARLANDS, newRute))
            pHead.Status = CodeStatus.Succes;
        else
            pHead.Status = CodeStatus.SerializeError;

        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            []);
    }

    [TerbinCRUD(CodeTerbinProtocol.Read, (byte)CodeServices.FarlandsRute)]
    public static async Task<PacketRequest> ReadRuteFarlands(Header pHead, MemoryStream pParameters)
    {
        byte[] pld;
        if (ManagerConfiguration.GetConfg(RUTE_FARLANDS) is var rute && rute != null)
        {
            pld = Serialineitor.SerializeArray<char>(rute.ToCharArray());
            pHead.Status = CodeStatus.Succes;
        }
        else
        {
            pld = [];
            pHead.Status = CodeStatus.SerializeError;
        }
        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            pld);

    }


    [TerbinExecutable((byte)CodeMethodsTerbinService.ChangeInstancesRute)]
    public static async Task<PacketRequest> ChangeRuteInstances(Header pHead, MemoryStream pPLD)
    {


        throw new NotImplementedException("Ñe");
    }
}
