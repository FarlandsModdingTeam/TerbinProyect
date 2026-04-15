using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;
public static class BoolExtension
{
    public static sbyte ToSByte(this bool pBool)
    {
        return (sbyte)(pBool ? 1 : 0);
    }
    public static byte ToByte(this bool pBool)
    {
        return (byte)(pBool ? 1 : 0);
    }
    public static sbyte ToSByte(this bool? pBool)
    {
        return (sbyte)(pBool != null ? ToSByte(pBool.Value) : -1);
    }

    public static bool ToBool(this sbyte pSByte)
    {
        return pSByte == 1;
    }
    public static bool ToBool(this byte pByte)
    {
        return pByte == 1;
    }
    public static bool? ToBoolUk(this sbyte pSByte)
    {
        return (pSByte == -1) ? null : ToBool(pSByte);
    }
}
