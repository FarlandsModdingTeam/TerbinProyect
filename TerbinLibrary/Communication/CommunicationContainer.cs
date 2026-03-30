using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
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
    PlaceHolder = 2,
    // etc...

    // methods:
    CreateInstance = 10,
    DeleteInstance = 11,
}
/* -- Parecida a HTTP pero no la voy a seguir a raja tabla.
    1xx → Informativos
    2xx → Éxito (ej. 200 OK) ✅
    3xx → Redirecciones 🔁
    4xx → Error del cliente (ej. 404 Not Found) ❌
    5xx → Error del worker (ej. 500 Internal Server Error)
 */
public enum CodeStatus : short
{
    NotAsign = -1,

    Succes = 200,

    BadRequest = 400,
    NotFound = 404,

    GenericWorkerError = 500,
}

[StructLayout(LayoutKind.Sequential)]
public struct Header
{
    public Guid IdClient;
    public CodeStatus Status;
}

[StructLayout(LayoutKind.Sequential)]
public struct Capsule
{
    public Header Head;
    public CodeAction ActionMethod;
    // TODO: DesSerializar los parametros en los tipos de parametros requeridos.
    public byte[] Parameters;
    // TODO: Pasar Guid de tuberia para parametros;
    // ¿Si no pasas Guid sigue pasando bytes?
}

// TODO: Todo el tema del TerbinCommandAttribute deberia estar en TerbinProyect no en TerbinLibrary.
// TODO: Como mucho aqui deberia estar el tema de serializar y deserializar los parametros.
