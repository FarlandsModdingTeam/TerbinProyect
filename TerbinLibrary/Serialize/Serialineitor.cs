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
    ThreeQuartersInt GetSize();
    void WriteTo(Span<byte> pBuffer);
    void ReadFrom(ReadOnlySpan<byte> pBuffer);
}

public class Serialineitor
{
    private byte[] _content;
    private int _offset;

    public Serialineitor() : this(2) { }
    public Serialineitor(int pSize) : this(null, pSize) { }
    public Serialineitor(byte[]? pInitialContent, int pSize = 2)
    {
        this._content = pInitialContent ?? new byte[pSize];
        this._offset = pInitialContent?.Length ?? 0;
    }

    private void ensureCapacity(int pNeededBytes)
    {
        if (_content.Length - _offset < pNeededBytes)
        {
            int newCapacity = Math.Max(_content.Length * 2, _content.Length + pNeededBytes);
            Array.Resize(ref _content, newCapacity);
        }
    }


    public Serialineitor Add<T>(T pValue) where T : unmanaged
    {
        ensureCapacity(Unsafe.SizeOf<T>());

        BufferWriter.Add<T>(_content, ref _offset, pValue);

        return this; 
    }

    public Serialineitor AddArray<T>(T[] pArray) where T : unmanaged
    {
        int elementsBytes = (pArray?.Length ?? 0) * Unsafe.SizeOf<T>();
        ensureCapacity(TerbinProtocol.LENGTH_ARRAY + elementsBytes);

        BufferWriter.AddArray<T>(_content, ref _offset, pArray);

        return this;
    }

    public Serialineitor AddStruct<T>(T pStruct) where T : struct, IStructSerializable
    {
        int structSize = (int)pStruct.GetSize();
        ensureCapacity(structSize);

        BufferWriter.AddStruct<T>(_content, ref _offset, pStruct);

        return this;
    }

    public byte[] ToArray()
    {
        return _content.AsSpan(0, _offset).ToArray();
    }





    // ******************************( Parte Estatic )****************************** //
    // TODO: Que no dependa de los Buffers sino al reve.

    public static byte[] SerializeStructConst<T>(T pStruct) where T : struct
    {
        int size = Marshal.SizeOf(pStruct);
        byte[] arr = new byte[size];

        nint ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(pStruct, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }
    public static T DeserializeStructConst<T>(byte[] pBytes) where T : struct
    {
        T newStruct = default;

        nint ptr = Marshal.AllocHGlobal(pBytes.Length);
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
        byte[] newArray = new byte[pArray.Length * Unsafe.SizeOf<T>() + TerbinProtocol.LENGTH_ARRAY];
        BufferWriter.AddArray<T>(newArray, ref offset, pArray);
        return newArray;
    }
    public static T[] DeserializeArray<T>(byte[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        return BufferReader.GetArray<T>(pArray, ref offset);
    }
    public static T[] DeserializeArray<T>(ref byte[] pArray)
        where T : unmanaged
    {
        ReadOnlySpan<byte> buffer = pArray;
        return buffer.ReadArray<T>();
    }

    public static ThreeQuartersInt GetArraySize<T>(ThreeQuartersInt pLength) where T : unmanaged
    {
        return (ThreeQuartersInt)(pLength * Unsafe.SizeOf<T>());
    }
    public static ThreeQuartersInt GetArraySize<T>(int pLength) where T : unmanaged
    {
        return (ThreeQuartersInt)(pLength * Unsafe.SizeOf<T>());
    }



    public static byte[] Serialize<T>(T pValue) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        byte[] buffer = new byte[size];

        MemoryMarshal.Write(buffer.AsSpan(), in pValue);
        
        return buffer;
    }

    public static T Deserialize<T>(byte[] pBuffer) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(pBuffer.AsSpan());
    }
    public static T Deserialize<T>(byte[] pBuffer, int pOffset) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(pBuffer[pOffset..]);
    }
}




public enum BufferErrorCode : sbyte
{
    Succes = 1,

    SurpassesMax = 2,
    BufferSmall = 3,
}