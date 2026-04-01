using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Serialize;
public class BinReader
{
    public static T[] ReadArray<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        ushort length = Read<ushort>(pBuffer, ref pOffset);

        int byteLength = length * Unsafe.SizeOf<T>();
        var slice = pBuffer.Slice(pOffset, byteLength);

        T[] array = MemoryMarshal.Cast<byte, T>(slice).ToArray();
        pOffset += byteLength;

        return array;
    }
    public static T Read<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
       where T : unmanaged
    {
        T value = MemoryMarshal.Read<T>(pBuffer[pOffset..]);
        pOffset += Unsafe.SizeOf<T>();
        return value;
    }
    public static T ReadStruct<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        var lenth = pStruct.GetSize();
        T newStruct = StructSerialineitor.Deserialize<T>(pBuffer[pOffset..(pOffset+lenth)].ToArray());
        pOffset += lenth;
        return newStruct;
    }
}
