using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Communication;

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

public interface IStructSerializable
{
    int GetSize();
    void WriteTo(Span<byte> pBuffer);
    void ReadFrom(ReadOnlySpan<byte> pBuffer);
}

// TODO: CreateArray: Le puedas pasar todo lo que quieras en orden y lo serializara y añadira a un array que te devolvera.
public class Serialineitor
{
    public static byte[] SerializeStructConst<T>(T pStruct) where T : struct
    {
        int size = Marshal.SizeOf(pStruct);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(pStruct, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }
    public static T DeserializeStructConst<T>(byte[] pBytes) where T : struct
    {
        T newStruct = default;

        IntPtr ptr = Marshal.AllocHGlobal(pBytes.Length);
        Marshal.Copy(pBytes, 0, ptr, pBytes.Length);

        newStruct = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return newStruct;
    }

    
    public static byte[] SerializeStruct<T>(T pStruct) where T : struct, IStructSerializable
    {
        byte[] buffer = new byte[pStruct.GetSize()]; // sizeof(T) // unsafe
        pStruct.WriteTo(buffer);
        return buffer;
    }
    public static T DeserializeStruct<T>(byte[] pBuffer) where T : struct, IStructSerializable
    {
        T newStruct = new();
        newStruct.ReadFrom(pBuffer);
        return newStruct;
    }


    public static byte[] SerializeArray<T>(T[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        byte[] newArray = new byte[pArray.Length * Unsafe.SizeOf<T>() + 2];
        BufferWriter.AddArray<T>(newArray, ref offset, pArray);
        return newArray.ToArray();
    }
    public static T[] DeserializeArray<T>(byte[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        return BufferReader.GetArray<T>(pArray, ref offset);
    }


    public static ushort GetArraySize<T>(ushort pLength) where T : unmanaged
    {
        return (ushort)(pLength * Unsafe.SizeOf<T>());
    }
    public static ushort GetArraySize<T>(int pLength) where T : unmanaged
    {
        return (ushort)(pLength * Unsafe.SizeOf<T>());
    }



    public static byte[] Serialize<T>(T pValue)
    {

        throw new NotImplementedException("Ñe");
    }
}
