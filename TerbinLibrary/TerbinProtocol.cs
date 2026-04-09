using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;

public class TerbinProtocol
{
    public const ushort MAX_PLD = 0xFFF; // ¡Vamos Miura!
    public const ushort FRAGMENT_IN = 0xFFF;
    public const double FRAGMENT_IN_MULTIPLICATE_INVERSE = 1.0D / FRAGMENT_IN;

    public const ushort ORDER_SINGLE = ushort.MinValue;

    public const ushort FIRST_PACKET = 1;
    public const ushort FINAL_PACKET = ushort.MaxValue;

    public const ushort MAXIMUS_RESPONSE_TIME = 180; // ¿Se necesita un short?
}

[Flags]
public enum CodeTypeData : byte
{
    //Single = 0 << 0, // 0000
    //Array = 1 << 0, // 0001

    //Int = 0 << 1,  // 0000
    //Float = 1 << 1,  // 0010
    //Char = 2 << 1,  // 0100
    //Byte = 3 << 1,  // 0110


    Singles = 0b000_0,
    Array   = 0b000_1,

    Int     = 0b000_0,
    Float   = 0b001_0,
    Char    = 0b010_0,
    Byte    = 0b011_0,
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
    Solicit = 4,
    Undefine5 = 5,

    // Si puede ayudar a ahorrarte fuciones.
    // C.R.U.D for you: 
    Create = 6,
    Read = 7,
    Update = 8,
    Deleted = 9,
}

public enum CodeTerbinMemory : byte
{
    None = 0,
    NotAsign = 1,
    New = 2,
    Undefined = 3, // Literaly

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