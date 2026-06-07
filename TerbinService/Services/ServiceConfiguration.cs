using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Managers;

namespace TerbinService.Services;
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
// ├─ Al cambiar la ruta toca mover todo.
// ├─ Temporal de Proton
// └─ Steam Instalado

[TODO("Guardar y gestionar:\r\n ├─ Proton Instalado\r\n ├─ Al cambiar la ruta toca mover todo.\r\n ├─ Temporal de Proton\r\n └─ Steam Instalado")]
[TODO("Las instancias no deberia ser una ruta, deberias cambiar su posicion desde el propio servicio que se encarge de mover toda la carpeta.")]
internal static class ServiceConfiguration
{
    //[TerbinExecutable((byte)CodeServices.Create, (byte)CodeServicesSection.Rute)]
    public static async Task<InfoResponse?> UpdateRute(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> recived = pParameters;
        string keyRute = recived.ReadArray<char>().CrString();
        string newRute = recived.ReadArray<char>().CrString();

        if (newRute == null)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        var result = Manager.Configuration.SetConfig(keyRute, newRute);
        pHead.Status = result switch
        {
            CodeAcessJSonSave.Succes => CodeStatus.Succes,
            CodeAcessJSonSave.ErrorSerialize => CodeStatus.SerializeError,
            _ => CodeStatus.AccesNullOrNotExist,
        };

        return InfoResponse.Create(pHead.IdRequest, pHead.Status);
    }

    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.Rute)]
    public static async Task<InfoResponse?> ReadRute(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        byte[] pld;
        string keyRute = new(Serialineitor.DeserializeArray<char>(ref pParameters));
        CodeStatus status;

        if (string.IsNullOrEmpty(keyRute))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);
        }
        if (Manager.Configuration.GetConfg(keyRute) is var rute && rute != null)
        {
            pld = Serialineitor.SerializeArray<char>(rute.ToCharArray());
            status = CodeStatus.Succes;
        }
        else
        {
            pld = [];
            // Farlands no esta instalado.
            status = CodeStatus.AccesNullOrNotExist;
        }
        return InfoResponse.Create(pHead.IdRequest, status, pld);
    }

}
