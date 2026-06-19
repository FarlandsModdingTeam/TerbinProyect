using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Protocol;
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


// ==========================================
// Resources
// ==========================================
public class TerbinProtocolResource
{
    public static byte[] UniteFlag(byte pCRUD, params byte[] pMethods)
    {
        byte first = (byte)(pCRUD | pMethods[0]);
        pMethods[0] = first;
        return pMethods;
    }

    public static (TerbinFlagCRUD crud, byte method) TakeCRUD(byte pMethod)
    {
        return ((TerbinFlagCRUD)(pMethod & 0xC0), (byte)(pMethod & 0x3F));
    }
    /*
    public static (TerbinFlagCRUD crud, byte method) TakeCRUD(byte pMethod)
    {
        TerbinFlagCRUD crud = (TerbinFlagCRUD)(pMethod & 0b0011_1111);
        byte method = (byte)(pMethod & 0b1100_0000);
        return (crud, method);
    }
     */
}

// TODO: Invertarme mi propito tipos de petición.
// https://developer.mozilla.org/es/docs/Web/HTTP/Reference/Methods


public enum TerbinCRUD : byte
{
    ReadAll = 250,
    Duplicate = 251,

    Create = 252,
    Read = 253,
    Update = 254,
    Deleted = 255,
}

[Flags]
public enum TerbinFlagCRUD : byte
{
    Create = 0 << 6,
    Read = 1 << 6,
    Update = 2 << 6,
    Deleted = 3 << 6,
}
