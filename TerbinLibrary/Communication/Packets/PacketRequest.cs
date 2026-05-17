using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

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

[StructLayout(LayoutKind.Sequential)]
public struct PacketRequest : IStructSerializable
{
    public Header Head;
    public IdArray ActionMethod;
    public byte[] Payload;

    public PacketRequest()
    {
        Head = new Header();
        ActionMethod = new IdArray([(byte)CodeTerbinProtocol.Response]);
        Payload = [];
    }

    public PacketRequest(Header? pHead = null)
        : this (pHead, (byte[]?)null, (byte[]?)null)
    { }
    public PacketRequest(
        Header? pHead = null,
        byte[]? pActionMethod = null,
        byte[]? pPayload = null)
        : this (pHead, new IdArray(pActionMethod ?? [(byte)CodeTerbinProtocol.Response]), pPayload)
    { }
    public PacketRequest(
        Header? pHead = null,
        IdArray? pActionMethod = null,
        byte[]? pPayload = null)
    {
        Head = pHead ?? new Header();
        ActionMethod = pActionMethod ?? new IdArray([(byte)CodeTerbinProtocol.Response]);
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ThreeQuartersInt + byte[]
    // 7 + 1 + 0 + 2 + Length
    public readonly ushort GetSize() => (ushort)(8 + TerbinProtocol.LENGTH_ARRAY + (Payload?.Length ?? 0));
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<Header>(ref offset, Head);
        pBuffer.WriteStruct<IdArray>(ref offset, ActionMethod);
        pBuffer.WriteArray<byte>(ref offset, Payload);
    }
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head = pBuffer.Read<Header>(ref offset);
        ActionMethod = pBuffer.ReadStruct<IdArray>(ref offset);
        Payload = pBuffer.ReadArray<byte>(ref offset);
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


/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
