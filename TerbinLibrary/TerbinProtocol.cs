using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;

public class TerbinProtocol
{
    public const ushort MAX_PLD = 0xFFF; // ¡Vamos Miura!
    public const ushort FRAGMENT_IN = 0xFFF;

    public const ushort ORDER_SINGLE = ushort.MinValue;

    public const ushort FIRST_PACKET = 1;
    public const ushort FINAL_PACKET = ushort.MaxValue;
}

[Flags]
public enum CodeTypeData : byte
{
    Single = 0 << 0, // 0000
    Array = 1 << 0, // 0001

    Int = 0 << 1,  // 0000
    Float = 1 << 1,  // 0010
    Char = 2 << 1,  // 0100
    Byte = 3 << 1,  // 0110


    // Singles  = 0b0000,
    // Array    = 0b0001,
    // 
    // Int      = 0b0000,
    // Float    = 0b0010,
    // Char     = 0b0100,
    // Byte     = 0b0110,
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
    // Si puede ayudar a ahorrarte bytes.
    // "CRUD": 
    Solicit = 4,
    Create = 5,
    Update = 6,
    Deleted = 7,

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
    OverMaximumTime = 550,
}