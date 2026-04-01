using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Serialize;
public class BinWriter
{
    public static void AddArray<T>(Span<byte> pBuffer, ref int pOffset, T[] pArray)
        where T : unmanaged
    {
        if (pArray.Length > ushort.MaxValue)
            throw new InvalidOperationException("Array surpasses ushort max");

        BitConverter.TryWriteBytes(pBuffer[pOffset..], (ushort)pArray.Length);
        pOffset += 2;

        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());
        bytes.CopyTo(pBuffer[pOffset..]);
        pOffset += bytes.Length;
    }
    public static void Add<T>(Span<byte> pBuffer, ref int pOffset, T pValue)
        where T : unmanaged
    {
        MemoryMarshal.Write(pBuffer[pOffset..], in pValue); // ref
        pOffset += Unsafe.SizeOf<T>();
    }
    public static void AddStruct<T>(Span<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        byte[] strucBytes = Serialineitor.Serialize(pStruct);
        strucBytes.CopyTo(pBuffer[pOffset..]);
        pOffset += strucBytes.Length;
    }
}
