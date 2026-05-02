using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;

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

// TODO: Guardar y gestionar:
// ├─ Proton Instalado
// ├─ Temporal de Proton
// └─ Steam Instalado

public class ConfiguratonService
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Update, (byte)CodeSubServices.Rute)]
    public static async Task<InfoResponse?> UpdateRute(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> recived = pParameters;
        string keyRute = recived.ReadArray<char>().CrString();
        string newRute = recived.ReadArray<char>().CrString();

        Console.WriteLine($"Memo: {newRute}");
        if (newRute == null)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        var result = ManagerConfiguration.SetConfig(keyRute, newRute);
        pHead.Status = result switch
        {
            CodeAcessJSonSave.Succes => CodeStatus.Succes,
            CodeAcessJSonSave.ErrorSerialize => CodeStatus.SerializeError,
            _ => CodeStatus.AccesNullOrNotExist,
        };

        return InfoResponse.Create(pHead.IdRequest, pHead.Status);
    }

    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Read, (byte)CodeSubServices.Rute)]
    public static async Task<InfoResponse?> ReadRute(Header pHead, byte[] pParameters)
    {
        byte[] pld;
        string keyRute = new(Serialineitor.DeserializeArray<char>(pParameters));
        if (string.IsNullOrEmpty(keyRute))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);
        }
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
        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = pHead.Status,
            Payload = pld
        };
    }

}
