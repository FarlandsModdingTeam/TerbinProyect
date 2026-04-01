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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerbinLibrary.Communication;

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

/// <summary>
/// 
/// Reserved the first 10.
/// </summary>
public enum CodeAction : byte
{
    Stop = 0,
    None = 1,
    Load = 2,
    PlaceHolder = 3,
    // etc...

    // methods:
    CreateInstance = 10,
    DeleteInstance = 11,
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

    BadRequest = 400,
    NotFound = 404,

    ActionNotFound = 440,


    GenericWorkerError = 500,
}

// TODO: Hacerles getter y setters + valor predeterminado, con eso nos olvidamos del constructor.
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Header // la memoria es constante es unmanaged.
{
    public ushort IdClient;
    public ushort OrderRequest;
    public CodeStatus Status;

    public Header(
        ushort pIdClient = 0,
        ushort? pOrderRequest = null,
        CodeStatus pStatus = CodeStatus.NotAsign)
    {
        IdClient = (pIdClient == 0) ? ShortId.NewShortId() : pIdClient;
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

    public PacketRequest(
        Header pHead = new(),
        byte pActionMethod = 2,
        byte pIdMemory = 0,
        byte[]? pPayload = null)
    {
        Head = pHead;
        ActionMethod = pActionMethod;
        IdMemory = (pIdMemory < 10) ? (byte)10 : pIdMemory;
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ushort + byte[]
    // 6 + 1 + 1 + 2 + Length
    public int GetSize() => 10 + Payload.Length;
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        BinWriter.Add<Header>(pBuffer, ref offset, Head);
        BinWriter.Add<byte>(pBuffer, ref offset, ActionMethod);
        BinWriter.Add<byte>(pBuffer, ref offset, IdMemory);
        BinWriter.AddArray<byte>(pBuffer, ref offset, Payload);
    }
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head =          BinReader.Read<Header>(pBuffer, ref offset);
        ActionMethod =  BinReader.Read<byte>(pBuffer, ref offset);
        IdMemory =      BinReader.Read<byte>(pBuffer, ref offset);
        Payload =       BinReader.ReadArray<byte>(pBuffer, ref offset);
    }

}

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
