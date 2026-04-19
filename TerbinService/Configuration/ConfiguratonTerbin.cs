using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
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


public class ConfiguratonTerbin
{
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
        
        string newRute = new(Serialineitor.DeserializeArray<char>(pParameters.ToArray()));
        if (newRute == null)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }
        var result = ManagerConfiguration.SetConfig(TerbinConfiguration.RUTE_FARLANDS, newRute);
        if (result == CodeAcessJSonSave.Succes)
            pHead.Status = CodeStatus.Succes;
        else if (result == CodeAcessJSonSave.ErrorSerialize)
            pHead.Status = CodeStatus.SerializeError;
        else
            pHead.Status = CodeStatus.AccesNullOrNotExist;

        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            []);
    }

    [TerbinCRUD(CodeTerbinProtocol.Read, (byte)CodeServices.FarlandsRute)]
    public static async Task<PacketRequest> ReadRuteFarlands(Header pHead, MemoryStream pParameters)
    {
        byte[] pld;
        if (ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_FARLANDS) is var rute && rute != null)
        {
            pld = Serialineitor.SerializeArray<char>(rute.ToCharArray());
            pHead.Status = CodeStatus.Succes;
        }
        else
        {
            pld = [];
            // Farlands no esta instalado.
            pHead.Status = CodeStatus.AccesNullOrNotExist;
        }
        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            pld);
    }


    [TerbinCRUD(CodeTerbinProtocol.Update, (byte)CodeServices.InstancesRute)]
    public static async Task<PacketRequest> UpdateInstancesFarlands(Header pHead, MemoryStream pParameters)
    {
        if (pParameters.Length <= 0)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }

        string newRute = new(Serialineitor.DeserializeArray<char>(pParameters.ToArray()));
        if (newRute == null)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }
        var result = ManagerConfiguration.SetConfig(TerbinConfiguration.RUTE_INSTANCES, newRute);
        if (result == CodeAcessJSonSave.Succes)
            pHead.Status = CodeStatus.Succes;
        else if (result == CodeAcessJSonSave.ErrorSerialize)
            pHead.Status = CodeStatus.SerializeError;
        else
            pHead.Status = CodeStatus.AccesNullOrNotExist;

        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            []);
    }

    [TerbinCRUD(CodeTerbinProtocol.Read, (byte)CodeServices.InstancesRute)]
    public static async Task<PacketRequest> ReadInstancesFarlands(Header pHead, MemoryStream pParameters)
    {
        byte[] pld;
        if (ManagerConfiguration.GetConfg(TerbinConfiguration.RUTE_INSTANCES) is var rute && rute != null)
        {
            pld = Serialineitor.SerializeArray<char>(rute.ToCharArray());
            pHead.Status = CodeStatus.Succes;
        }
        else
        {
            pld = [];
            pHead.Status = CodeStatus.AccesNullOrNotExist;
        }
        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            pld);
    }




    // Pruebas:
    [TerbinCRUD(CodeTerbinProtocol.Update, (byte)CodeServices.Rute)]
    public static async Task<PacketRequest> UpdateRute(Header pHead, MemoryStream pParameters)
    {
        if (pParameters.Length <= 0)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }

        ReadOnlySpan<byte> recived = pParameters.ToArray();
        string keyRute = recived.ReadArray<char>().CrString();
        string newRute = recived.ReadArray<char>().CrString();

        if (newRute == null)
        {
            pHead.Status = CodeStatus.ErrorNotPayload;
            return new PacketRequest(pHead,
                (byte)CodeTerbinProtocol.Update,
                (byte)CodeTerbinMemory.None,
                pParameters.ToArray());
        }
        var result = ManagerConfiguration.SetConfig(keyRute, newRute);
        if (result == CodeAcessJSonSave.Succes)
            pHead.Status = CodeStatus.Succes;
        else if (result == CodeAcessJSonSave.ErrorSerialize)
            pHead.Status = CodeStatus.SerializeError;
        else
            pHead.Status = CodeStatus.AccesNullOrNotExist;

        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            []);
    }

    [TerbinCRUD(CodeTerbinProtocol.Read, (byte)CodeServices.Rute)]
    public static async Task<PacketRequest> ReadRute(Header pHead, MemoryStream pParameters)
    {
        byte[] pld;
        string keyRute = new(Serialineitor.DeserializeArray<char>(pParameters.ToArray()));
        if (ManagerConfiguration.GetConfg(keyRute) is var rute && rute != null)
        {
            pld = Serialineitor.SerializeArray<char>(rute.ToCharArray());
            pHead.Status = CodeStatus.Succes;
        }
        else
        {
            pld = [];
            // Farlands no esta instalado.
            pHead.Status = CodeStatus.AccesNullOrNotExist;
        }
        return new PacketRequest(pHead,
            (byte)CodeTerbinProtocol.Update,
            (byte)CodeTerbinMemory.None,
            pld);
    }
}
