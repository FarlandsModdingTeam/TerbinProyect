using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using TerbinLibrary.Id;
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


// TODO: Hacerles getter y setters + valor predeterminado, con eso nos olvidamos del constructor.
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Header // la memoria es constante es unmanaged.
{
    public ushort IdRequest;
    public ushort OrderRequest; // 0 = solo uno, 1 = es el primero, ushort.MaxValue = es el ultimo.
    public CodeStatus Status;
    //public ushort MaximusTime; // ¿Aqui realmente se necesita?
    public byte IdMemory;

    public Header()
    {
        IdRequest = 0;
        OrderRequest = 0;
        Status = CodeStatus.NotAsign;
        //MaximusTime = TerbinProtocol.MAXIMUS_RESPONSE_TIME;
        IdMemory = (byte)CodeTerbinMemory.Undefined;
    }

    public Header(
        ushort pIdRequest = 0,
        ushort? pOrderRequest = null,
        CodeStatus pStatus = CodeStatus.NotAsign,
        /*ushort pMaximusTime = TerbinProtocol.MAXIMUS_RESPONSE_TIME,*/
        byte pIdMemory = (byte)CodeTerbinMemory.Undefined)
    {
        IdRequest = (pIdRequest == 0) ? (ushort)1 : pIdRequest;
        OrderRequest = pOrderRequest ?? TerbinProtocol.ORDER_SINGLE;
        Status = pStatus;
        //MaximusTime = pMaximusTime;
        IdMemory = pIdMemory;
    }
}


// TODO: Hacerles getter y setters + valor predeterminado, con eso nos olvidamos del constructor.
[StructLayout(LayoutKind.Sequential)]
public struct PacketRequest : IStructSerializable
{
    public Header Head;
    public byte ActionMethod;
    // Deberia hacer que IdMemory sea un byte y tener una RAM (almacen de memorias xd) o tener un almacen por funcion /
    // y que IdMemory sea un CodeAction y guardar los datos en la memoria especifica de la funcion y ya no tengo /
    // que especificarlo pero IdMemory se quedaria vacio pero si es byte el cliente tiene que medio gestionar la memoria.
    //public byte IdMemory;
    public byte[] Payload;

    public PacketRequest()
    {
        Head = new Header();
        ActionMethod = (byte)CodeTerbinProtocol.None;
        //IdMemory = (byte)CodeTerbinMemory.Undefined;
        Payload = [];
    }

    public PacketRequest(
        Header? pHead = null,
        byte pActionMethod = (byte)CodeTerbinProtocol.None,
        byte pIdMemory = (byte)CodeTerbinMemory.Undefined,
        byte[]? pPayload = null)
    {
        Head = pHead ?? new Header();
        ActionMethod = pActionMethod;
        //IdMemory = (pIdMemory < 10) ? (byte)10 : pIdMemory;
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ushort + byte[]
    // 7 + 1 + 0 + 2 + Length
    public ThreeQuartersInt GetSize() => 10 + (Payload?.Length ?? 0);
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<Header>(ref offset, Head);
        pBuffer.Write<byte>(ref offset, ActionMethod);
        //pBuffer.Write<byte>(ref offset, IdMemory);
        pBuffer.WriteArray<byte>(ref offset, Payload);
    }
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head =          pBuffer.Read<Header>(ref offset);
        ActionMethod =  pBuffer.Read<byte>(ref offset);
        //IdMemory =      pBuffer.Read<byte>(ref offset);
        Payload =       pBuffer.ReadArray<byte>(ref offset);
    }

    public void ClearPLD()
    {
        Head.OrderRequest = TerbinProtocol.ORDER_SINGLE;
        Head.IdMemory = (byte)CodeTerbinMemory.NotAsign;
        Payload = [];
    }

    // ¿?
    public static explicit operator PacketRequest(Task<PacketRequest?> v)
    {
        if (v != null)
            return (PacketRequest)v;
        else
            return new PacketRequest();
    }
}

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
