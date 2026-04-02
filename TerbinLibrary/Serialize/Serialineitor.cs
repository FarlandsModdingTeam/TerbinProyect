using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Communication;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerbinLibrary.Serialize;

public interface IStructSerializable
{
    int GetSize();
    void WriteTo(Span<byte> pBuffer);
    void ReadFrom(ReadOnlySpan<byte> pBuffer);
}


public class Serialineitor
{
    public static byte[] SerializeConst<T>(T pStruct) where T : struct
    {
        int size = Marshal.SizeOf(pStruct);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(pStruct, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }
    public static T DeserializeConst<T>(byte[] pBytes) where T : struct
    {
        T newStruct = default;

        IntPtr ptr = Marshal.AllocHGlobal(pBytes.Length);
        Marshal.Copy(pBytes, 0, ptr, pBytes.Length);

        newStruct = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return newStruct;
    }

    
    public static byte[] Serialize<T>(T pStruct) where T : struct, IStructSerializable
    {
        byte[] buffer = new byte[pStruct.GetSize()]; // sizeof(T) // unsafe
        pStruct.WriteTo(buffer);
        return buffer;
    }
    public static T Deserialize<T>(byte[] pBuffer) where T : struct, IStructSerializable
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
        BinWriter.AddArray<T>(newArray, ref offset, pArray);
        return newArray.ToArray();
    }
    public static T[] DeserializeArray<T>(byte[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        return BinReader.ReadArray<T>(pArray, ref offset);
    }
}
