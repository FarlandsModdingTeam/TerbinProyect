using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Id;


/// <summary>
/// 
/// </summary>
public static class MiniID
{
    public const ushort MAX_Short = 0xFFFD;
    public const byte MAX_Byte = 0xFD;
    public const byte MIN = 8;

    public static ushort IncrementalShort
    {
        get => field;
        private set
        {
            value = Math.Min(value, MAX_Short);
            value = Math.Max(value, MIN);
            field = value;
        }
    } = MIN;
    public static byte IncrementalByte
    {
        get => field;
        private set
        {
            value = Math.Min(value, MAX_Byte);
            value = Math.Max(value, MIN);
            field = value;
        }
    } = MIN;

    public static ushort NewS
    {
        get => NewShortId();
    }
    public static byte NewB
    {
        get => NewByteId();
    }

    public static ushort NewShortId()
    {
        var now = DateTime.Now;
        var timeId = now.Millisecond + now.Minute;
        int total = nextShortId() + (timeId < 0 ? 0 : timeId);
        return (ushort)total;
    }
    public static byte NewByteId()
    {
        int total = nextBytetId() + DateTime.Now.Minute;
        return (byte)total;
    }

    private static ushort nextShortId()
    {
        if (IncrementalShort >= MAX_Short)
        {
            IncrementalShort = MIN;
            return IncrementalShort;
        }
        return IncrementalShort++;
    }
    private static byte nextBytetId()
    {
        if (IncrementalByte >= MAX_Byte)
        {
            IncrementalByte = MIN;
            return IncrementalByte;
        }
        return IncrementalByte++;
    }
}
