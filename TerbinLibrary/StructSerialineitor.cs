using System;
using System.Collections.Generic;
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


public class StructSerialineitor
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

    public static void AddArray<T>(Span<byte> pBuffer, int pOffset, T[] pArray) where T : unmanaged
    {
        if (pArray.Length > ushort.MaxValue)
            throw new InvalidOperationException("Array surpasses ushort max");

        BitConverter.TryWriteBytes(pBuffer[pOffset..], (ushort)pArray.Length);
        pOffset += 2;

        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());
        bytes.CopyTo(pBuffer[pOffset..]);
        pOffset += bytes.Length;
    }

    public static void AddUnmaged<T>(Span<byte> pBuffer, int pOffset, T pData) where T : unmanaged
    {

    }
}
