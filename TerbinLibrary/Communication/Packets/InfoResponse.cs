using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication.Packets;
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


public struct InfoResponse
{
    public ushort IdRequest { get => field; set => field = value; }
    public CodeStatus Status { get => field; set => field = value; }
    public byte[] ActionMethod { get => field; set => field = value; }
    public byte[] Payload { get => field; set => field = value; }

    public InfoResponse()
    {
        IdRequest = TerbinProtocol.ORDER_SINGLE;
        Status = CodeStatus.NotAsign;
        ActionMethod = [(byte)CodeTerbinProtocol.Response];
        Payload = [];
    }


    public static InfoResponse Create(ushort pIdRequest, CodeStatus pStatus)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = pStatus,
        };
    }


    public static InfoResponse CreateInteralError(ushort pIdRequest, params byte[] pPld)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.InternalWorkerError,
            Payload = pPld,
        };
    }


    public static InfoResponse CreateSucces(ushort pIdRequest)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.Succes,
        };
    }
    public static InfoResponse CreateSucces(ushort pIdRequest, byte[] pPLD)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.Succes,
            Payload = pPLD,
        };
    }
}
