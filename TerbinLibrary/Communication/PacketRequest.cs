using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TerbinLibrary.Memory;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Communication;
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


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Header // la memoria es constante es unmanaged.
{
    public ushort IdRequest;
    public ushort OrderRequest; // 0 = solo uno, 1 = es el primero, ushort.MaxValue = es el ultimo.
    public CodeStatus Status;
    public byte IdMemory;

    public Header()
    {
        IdRequest = 0;
        OrderRequest = 0;
        Status = CodeStatus.NotAsign;
        IdMemory = (byte)CodeTerbinMemory.Undefined;
    }

    public Header(
        ushort pIdRequest = 0,
        ushort pOrderRequest = TerbinProtocol.ORDER_SINGLE,
        CodeStatus pStatus = CodeStatus.NotAsign,
        byte pIdMemory = (byte)CodeTerbinMemory.Undefined)
    {
        IdRequest = (pIdRequest == 0) ? (ushort)1 : pIdRequest;
        OrderRequest = pOrderRequest;
        Status = pStatus;
        IdMemory = pIdMemory;
    }
}


[StructLayout(LayoutKind.Sequential)]
public struct PacketRequest : IStructSerializable
{
    public Header Head;
    public byte[] ActionMethod;
    public byte[] Payload;

    public PacketRequest()
    {
        Head = new Header();
        ActionMethod = [(byte)CodeTerbinProtocol.Response];
        Payload = [];
    }

    public PacketRequest(
        Header? pHead = null,
        byte[]? pActionMethod = null,
        byte[]? pPayload = null)
    {
        Head = pHead ?? new Header();
        ActionMethod = pActionMethod ?? [(byte)CodeTerbinProtocol.Response];
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ThreeQuartersInt + byte[]
    // 7 + 1 + 0 + 2 + Length
    public ushort GetSize() => (ushort)(8 + TerbinProtocol.LENGTH_ARRAY + (Payload?.Length ?? 0));
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<Header>(ref offset, Head);
        pBuffer.WriteArray<byte>(ref offset, ActionMethod);
        pBuffer.WriteArray<byte>(ref offset, Payload);
    }
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head =          pBuffer.Read<Header>(ref offset);
        ActionMethod =  pBuffer.ReadArray<byte>(ref offset);
        Payload =       pBuffer.ReadArray<byte>(ref offset);
    }



    public void ClearPacket()
    {
        Head.OrderRequest = TerbinProtocol.ORDER_SINGLE;
        Head.IdMemory = (byte)CodeTerbinMemory.NotAsign;
        Payload = [];
    }


    public static PacketRequest CreateResponseError(ushort pIdRequest, CodeStatus pError)
    {
        Header h = new(pIdRequest: pIdRequest);
        return CreateResponseError(h, pError);
    }
    public static PacketRequest CreateResponseSucces(ushort pIdRequest)
    {
        Header h = new(pIdRequest: pIdRequest);
        return CreateResponseSucces(h);
    }
    public static PacketRequest CreateResponseError(Header pHead, CodeStatus pError)
    {
        pHead.Status = pError;
        return CreateResponse(pHead);
    }
    public static PacketRequest CreateResponseSucces(Header pHead)
    {
        pHead.Status = CodeStatus.Succes;
        return CreateResponse(pHead);
    }
    public static PacketRequest CreateResponse(Header pHead, byte[]? pPayload = null)
    {
        pHead.IdMemory = (byte)CodeTerbinMemory.NotAsign;
        pHead.OrderRequest = TerbinProtocol.ORDER_SINGLE;
        return new PacketRequest(pHead, [(byte)CodeTerbinProtocol.Response], pPayload);
    }

    /*
    public static explicit operator PacketRequest(Task<PacketRequest?> v)
    {
        if (v != null)
            return (PacketRequest)v;
        else
            return new PacketRequest();
    }*/
}

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


public struct InfoPacket
{
    public ushort? IdRequest { get => field; set => field = value; }
    public byte? ActionMethod { get => field; set => field = value; }
    public byte[]? Payload { get => field; set => field = value; }
    public CodeStatus? Status { get => field; set => field = value; }
    public bool Recuperate { get => field; set => field = value; }

    public InfoPacket()
    {
        ActionMethod = null;
        Payload      = null;
        IdRequest    = null;
        Status       = null;
        Recuperate   = false;
    }
}

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
