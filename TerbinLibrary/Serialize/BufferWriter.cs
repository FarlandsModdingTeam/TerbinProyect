using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Serialize;
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

public class BufferWriter
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
        byte[] strucBytes = Serialineitor.SerializeStruct(pStruct);
        strucBytes.CopyTo(pBuffer[pOffset..]);
        pOffset += strucBytes.Length;
    }
}

public static class BufferWriterExtension
{
    // TODO: darles una vuelta a los sin offset para que:
    // crean un nuevo Span donde pongan lo nuevo y luego sobreEscriban el antiguo Span con el nuevo.
    public static void Write<T>(this ref Span<byte> pBuffer, T pValue)
        where T : unmanaged
    {
        if (pBuffer.Length < Unsafe.SizeOf<T>())
            throw new ArgumentOutOfRangeException(nameof(pBuffer), "Buffer is too small.");

        MemoryMarshal.Write(pBuffer, in pValue);

        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
    }
    public static void WriteArray<T>(this ref Span<byte> pBuffer, T[] pArray)
        where T : unmanaged
    {
        if (pArray?.Length > ushort.MaxValue)
            throw new InvalidOperationException("Array surpasses ushort max");

        pBuffer.Write((ushort)(pArray?.Length ?? 0));

        if (pArray != null && pArray.Length > 0)
        {
            Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());

            if (pBuffer.Length < bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(pBuffer),
                    $"The buffer is too small. More are needed {bytes.Length} bytes.");

            bytes.CopyTo(pBuffer);

            // Avanzamos el buffer
            pBuffer = pBuffer[bytes.Length..];
        }
    }
    public static void WriteStruct<T>(this ref Span<byte> pBuffer, T pStruct)
        where T : struct, IStructSerializable
    {
        byte[] strucBytes = Serialineitor.SerializeStruct(pStruct);

        if (pBuffer.Length < strucBytes.Length)
            throw new ArgumentOutOfRangeException(nameof(pBuffer), "Buffer is too small for this struct.");

        strucBytes.CopyTo(pBuffer);

        pBuffer = pBuffer[strucBytes.Length..];
    }


    public static void WriteArray<T>(this Span<byte> pBuffer, ref int pOffset, T[] pArray)
        where T : unmanaged
    {
        BufferWriter.AddArray<T>(pBuffer, ref pOffset, pArray);
    }
    public static void Write<T>(this Span<byte> pBuffer, ref int pOffset, T pValue)
        where T : unmanaged
    {
        BufferWriter.Add<T>(pBuffer, ref pOffset, pValue);
    }
    public static void WriteStruct<T>(this Span<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        BufferWriter.AddStruct<T>(pBuffer, ref pOffset, pStruct);
    }
}