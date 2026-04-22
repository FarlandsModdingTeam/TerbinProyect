using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ThreeQuartersInt
{
    private byte _byte1;
    private byte _byte2;
    private byte _byte3;

    public const int MaxValue = 0xFF_FF_FF;
    public const int MinValue = 0x0;

    public static implicit operator ThreeQuartersInt(int pValue)
    {
        ThreeQuartersInt result = new ThreeQuartersInt();

        result._byte1 = (byte)(pValue & 0xFF);
        result._byte2 = (byte)((pValue >> 8) & 0xFF);
        result._byte3 = (byte)((pValue >> 16) & 0xFF);

        return result;
    }
    public static implicit operator ThreeQuartersInt(uint pValue)
    {
        ThreeQuartersInt result = new ThreeQuartersInt();

        result._byte1 = (byte)(pValue & 0xFF);
        result._byte2 = (byte)((pValue >> 8) & 0xFF);
        result._byte3 = (byte)((pValue >> 16) & 0xFF);

        return result;
    }

    public static implicit operator int(ThreeQuartersInt pValue)
    {
        return pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16);
    }
    public static implicit operator uint(ThreeQuartersInt pValue)
    {
        return (uint)(pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16));
    }

    public static implicit operator Index(ThreeQuartersInt pValue)
    {
        int intValue = pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16);
        return new Index(intValue);
    }

    public readonly byte[] ToArray()
    {
        return new byte[]{ _byte1, _byte2, _byte3 };
    }
    public readonly override string ToString()
    {
        return ((int)this).ToString();
    }
}