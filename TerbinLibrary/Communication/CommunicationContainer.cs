using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

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

[StructLayout(LayoutKind.Sequential)]
public struct Header
{
    public Guid IdRequest; // ¿Realmente es necesario?
    public ushort OrderRequest;
    public CodeStatus Status;
}

[StructLayout(LayoutKind.Sequential)]
public struct Capsule
{
    public Header Head;
    public CodeAction ActionMethod;
    // Deberia hacer que IdMemory sea un byte y tener una RAM (almacen de memorias xd) o tener un almacen por funcion /
    // y que IdMemory sea un CodeAction y guardar los datos en la memoria especifica de la funcion y ya no tengo /
    // que especificarlo pero IdMemory se quedaria vacio pero si es byte el cliente tiene que medio gestionar la memoria.
    public byte IdMemory;
    public byte Payload;
    // TODO: Pasar Guid de tuberia para parametros;
    // ¿Si no pasas Guid sigue pasando bytes?
}

// 16 bytes es el guid y los restantes 7 es el resto de mensaje XD.

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/

// TODO: Todo el tema del TerbinCommandAttribute deberia estar en TerbinService no en TerbinLibrary.
// TODO: Como mucho aqui deberia estar el tema de serializar y deserializar los parametros.
