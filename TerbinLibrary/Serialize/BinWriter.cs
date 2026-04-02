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
        if (pArray?.Length > ushort.MaxValue)
            throw new InvalidOperationException("Array surpasses ushort max");

        // Validar que hay al menos 2 bytes para escribir la longitud
        if (pBuffer.Length - pOffset < 2)
            throw new ArgumentOutOfRangeException(nameof(pBuffer),
                "There is not enough space in the buffer to write the length of the array.");

        BitConverter.TryWriteBytes(pBuffer[pOffset..], (ushort)(pArray?.Length ?? 0));
        pOffset += 2;

        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());

        // Validar que hay suficiente espacio en el búfer para los bytes del array
        if (pBuffer.Length - pOffset < bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(pBuffer),
                $"The buffer is too small. More are needed {bytes.Length} bytes, but only bytes remain {pBuffer.Length - pOffset}.");

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
