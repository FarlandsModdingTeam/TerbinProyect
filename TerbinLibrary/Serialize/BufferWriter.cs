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

// TODO: Que estos si permitan ampliar automaticamente y adaptarlos a BufferErrorCode.
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



    public static void EnsureWrite<T>(ref byte[] buffer, int offset, T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (buffer.Length - offset < size)
        {
            // Crear uno más grande y copiar el contenido
            var newBuffer = new byte[buffer.Length * 2 + size];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            buffer = newBuffer;
        }
        // Escribir el valor
        MemoryMarshal.Write(buffer.AsSpan(offset), in value);
    }
}

public static class BufferWriterExtension
{
    // NOTA: Mi todo es fisicamente implosible, XD
    // TODO: darles una vuelta a los sin offset para que:
    // crean un nuevo Span donde pongan lo nuevo y luego sobreEscriban el antiguo Span con el nuevo.
    // Usar: Buffer.BlockCopy
    public static BufferErrorCode Write<T>(this ref Span<byte> pBuffer, T pValue)
        where T : unmanaged
    {
        if (pBuffer.Length < Unsafe.SizeOf<T>())
            return BufferErrorCode.BufferSmall;

        MemoryMarshal.Write(pBuffer, in pValue);

        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
        return BufferErrorCode.None;
    }
    public static BufferErrorCode WriteArray<T>(this ref Span<byte> pBuffer, T[] pArray)
        where T : unmanaged
    {
        if (pArray?.Length > ushort.MaxValue)
            return BufferErrorCode.SurpassesMax;

        pBuffer.Write((ushort)(pArray?.Length ?? 0));

        if (pArray != null && pArray.Length > 0)
        {
            Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());

            if (pBuffer.Length < bytes.Length)
                return BufferErrorCode.BufferSmall;

            bytes.CopyTo(pBuffer);

            pBuffer = pBuffer[bytes.Length..];
        }
        return BufferErrorCode.None;
    }
    public static BufferErrorCode WriteStruct<T>(this ref Span<byte> pBuffer, T pStruct)
        where T : struct, IStructSerializable
    {
        byte[] strucBytes = Serialineitor.SerializeStruct(pStruct);

        if (pBuffer.Length < strucBytes.Length)
            return BufferErrorCode.BufferSmall;

        strucBytes.CopyTo(pBuffer);

        pBuffer = pBuffer[strucBytes.Length..];
        return BufferErrorCode.None;
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



public enum BufferErrorCode : byte
{
    None = 0,

    BufferSmall = 1,
    SurpassesMax = 2,
}