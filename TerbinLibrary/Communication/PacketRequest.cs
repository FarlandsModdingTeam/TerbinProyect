using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using TerbinLibrary.Id;
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


[Flags]
public enum CodeTypeData : byte
{
    Single = 0 << 0, // 0000
    Array  = 1 << 0, // 0001

    Int   = 0 << 1,  // 0000
    Float = 1 << 1,  // 0010
    Char  = 2 << 1,  // 0100
    Byte  = 3 << 1,  // 0110
}

/// <summary>
/// 0 -> 9 | 
/// Reserved the first 10.
/// </summary>
public enum CodeTerbinProtocol : byte
{
    Stop = 0,
    None = 1,
    Load = 2,
    Cancel = 3,
     // "CRUD":
    Solicit = 4,
    Create = 5,
    Update = 6,

    Undefined7 = 7,
    Undefined8 = 8,
    Undefined9 = 9,
}

public enum CodeTerbinMemory : byte
{
    New = 0,
    NotAsign = 1,
    Undefined2 = 2,
    Undefined3 = 3,
    Undefined4 = 4,
    Undefined5 = 5,
    Undefined6 = 6,
    Undefined7 = 7,
    Undefined8 = 8,
    Undefined9 = 9,
}

/* -- Parecida a HTTP pero no la voy a seguir a raja tabla.
    1xx → Informativos
    2xx → Éxito (ej. 200 OK)
    3xx → Redirecciones
    4xx → Error del cliente (ej. 404 Not Found)
    5xx → Error del worker (ej. 500 Internal Server Error)
 */
public enum CodeStatus : short
{
    NotAsign = -1,

    Succes = 200,

    Execute = 300, // Hace 0 falta.

    BadRequest = 400,
    NotFound = 404,

    ActionNotFound = 440,


    GenericWorkerError = 500,
}

// TODO: Hacerles getter y setters + valor predeterminado, con eso nos olvidamos del constructor.
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Header // la memoria es constante es unmanaged.
{
    public ushort IdRequest;
    public ushort OrderRequest; // 0 = solo uno, 1 = es el primero, ushort.MaxValue = es el ultimo.
    public CodeStatus Status;

    public Header()
    {
        IdRequest = 1;
        OrderRequest = 0;
        Status = CodeStatus.NotAsign;
    }

    public Header(
        ushort pIdClient = 0,
        ushort? pOrderRequest = null,
        CodeStatus pStatus = CodeStatus.NotAsign)
    {
        IdRequest = (pIdClient == 0) ? (ushort)1 : pIdClient;
        OrderRequest = pOrderRequest ?? 0;
        Status = pStatus;
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
    public byte IdMemory; // Reservarse los 10 primeros
    public byte[] Payload;

    public PacketRequest()
    {
        Head = new Header();
        ActionMethod = (byte)CodeTerbinProtocol.None;
        IdMemory = 0;
        Payload = [];
    }

    public PacketRequest(
        Header? pHead = null,
        byte pActionMethod = 2,
        byte pIdMemory = 0,
        byte[]? pPayload = null)
    {
        Head = pHead ?? new Header();
        ActionMethod = pActionMethod;
        IdMemory = (pIdMemory < 10) ? (byte)10 : pIdMemory;
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ushort + byte[]
    // 6 + 1 + 1 + 2 + Length
    public int GetSize() => 10 + (Payload?.Length ?? 0);
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<Header>(ref offset, Head);
        pBuffer.Write<byte>(ref offset, ActionMethod);
        pBuffer.Write<byte>(ref offset, IdMemory);
        pBuffer.WriteArray<byte>(ref offset, Payload);
    }
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head =          pBuffer.Read<Header>(ref offset);
        ActionMethod =  pBuffer.Read<byte>(ref offset);
        IdMemory =      pBuffer.Read<byte>(ref offset);
        Payload =       pBuffer.ReadArray<byte>(ref offset);
    }

}

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
