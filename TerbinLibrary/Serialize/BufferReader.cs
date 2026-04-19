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

public class BufferReader
{
    public static T[] GetArray<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        ushort length = Get<ushort>(pBuffer, ref pOffset);

        //int byteLength = length * Unsafe.SizeOf<T>();
        var slice = pBuffer.Slice(pOffset, length);

        T[] array = MemoryMarshal.Cast<byte, T>(slice).ToArray();
        pOffset += length;

        return array;
    }
    public static T Get<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
       where T : unmanaged
    {
        T value = MemoryMarshal.Read<T>(pBuffer[pOffset..]);
        pOffset += Unsafe.SizeOf<T>();
        return value;
    }
    public static T GetStruct<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        var lenth = pStruct.GetSize();
        T newStruct = Serialineitor.DeserializeStruct<T>(pBuffer[pOffset..(pOffset+lenth)].ToArray());
        pOffset += lenth;
        return newStruct;
    }
}

// TODO: Usar "out" para devolver el byte[] y asin funcionar directamente con arrays.
public static class BufferReaderExtension
{
    public static T[] ReadArray<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : unmanaged
    {
        ushort length = pBuffer.Read<ushort>();
        //int byteLength = length * Unsafe.SizeOf<T>();

        T[] newArray = MemoryMarshal.Cast<byte, T>(pBuffer[..length]).ToArray();
        pBuffer = pBuffer[length..];

        return newArray;
    }
    public static T Read<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : unmanaged
    {
        T newValue = MemoryMarshal.Read<T>(pBuffer);
        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
        return newValue;
    }
    public static T ReadStruct<T>(this ref ReadOnlySpan<byte> pBuffer, T pStruct)
        where T : struct, IStructSerializable
    {
        var length = pStruct.GetSize();
        T newStruct = Serialineitor.DeserializeStruct<T>(pBuffer[..length].ToArray());
        pBuffer = pBuffer[length..];
        return newStruct;
    }


    public static T[] ReadArray<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        return BufferReader.GetArray<T>(pBuffer, ref pOffset);
    }
    public static T Read<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
       where T : unmanaged
    {
        return BufferReader.Get<T>(pBuffer, ref pOffset);
    }
    public static T ReadStruct<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        return BufferReader.GetStruct<T>(pBuffer, ref pOffset, pStruct);
    }
}
