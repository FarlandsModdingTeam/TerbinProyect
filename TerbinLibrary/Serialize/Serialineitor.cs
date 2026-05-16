using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Protocol;

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
    // TODO: no tiene sentido que sea un ushort si un array es un tresCuartosInt.
    ushort GetSize();
    void WriteTo(Span<byte> pBuffer);
    void ReadFrom(ReadOnlySpan<byte> pBuffer);
}

// TODO: La parte estatica solo deberia contener Raw y no añadir largo.
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
        ArgumentNullException.ThrowIfNull(pArray);
        
        int elementsBytes = pArray.Length * Unsafe.SizeOf<T>();
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

    public byte[] Serialize()
    {
        if (_content != null)
            return _content.AsSpan(0, _offset).ToArray();
        else
            return [];
    }

    public byte[] ToArray()
    {
        return Serialize();
    }


    public void Clear()
    {
        if (_content != null)
        {
            Array.Clear(_content, 0, _content.Length);
        }
        _offset = 0;
    }


    // ******************************( Parte Estatic )****************************** //
    // TODO: Que no dependa de los Buffers sino al reve.
    // TODO: Solo deberia contener Raw y no añadir largo.

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

    
    public static byte[] SerializeStructRaw<T>(T pStruct) where T : struct, IStructSerializable
    {
        byte[] buffer = new byte[pStruct.GetSize()]; // sizeof(T) // unsafe
        pStruct.WriteTo(buffer);
        return buffer;
    }
    public static T DeserializeStructRaw<T>(byte[] pBuffer) where T : struct, IStructSerializable
    {
        T newStruct = new();
        newStruct.ReadFrom(pBuffer);
        return newStruct;
    }

    [Obsolete("utilice Raw o Buffer")]
    public static byte[] SerializeArray<T>(T[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        byte[] newArray = new byte[pArray.Length * Unsafe.SizeOf<T>() + TerbinProtocol.LENGTH_ARRAY];
        BufferWriter.AddArray<T>(newArray, ref offset, pArray);
        return newArray;
    }
    public static byte[] SerializeArrayRaw<T>(T[] pArray, int pOffset = 0) // TODO: Un deserialize Raw.
        where T : unmanaged
    {
        byte[] newArray = new byte[pArray.Length * Unsafe.SizeOf<T>()];
        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());
        bytes.CopyTo(newArray.AsSpan()[pOffset..]);
        return newArray;
    }
    [Obsolete("utilice Raw o Buffer")]
    public static T[] DeserializeArray<T>(byte[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        return BufferReader.GetArray<T>(pArray, ref offset);
    }
    [Obsolete("utilice Raw o Buffer")]
    public static T[] DeserializeArray<T>(ref byte[] pArray)
        where T : unmanaged
    {
        ReadOnlySpan<byte> buffer = pArray;
        return buffer.ReadArray<T>();
    }
    public static T[] DeserializeArrayRaw<T>(ReadOnlySpan<byte> pArray, int pLenght = 0)
        where T : unmanaged
    {
        T[] newArray = MemoryMarshal.Cast<byte, T>(pArray[..pLenght]).ToArray();
        return newArray;
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




    public static byte[] Splice(byte[] pFirst, byte[] pSecond)
    {
        byte[] buffer = new byte[pFirst.Length + pSecond.Length];
        Array.Copy(pFirst, 0, buffer, 0, pFirst.Length);
        Array.Copy(pSecond, 0, buffer, pFirst.Length, pSecond.Length);
        return buffer;
    }
    public static byte[] Splice(params byte[][] pArrays)
    {
        byte[] buffer;
        int offset = 0;
        int size = 0;

        for (int i = 0; i < pArrays.Length; i++)
        {
            checked { size += pArrays[i].Length; }
        }

        buffer = new byte[size];

        for (int i = 0; i < pArrays.Length; i++)
        {
            Array.Copy(pArrays[i], 0, buffer, offset, pArrays[i].Length);
            offset += pArrays[i].Length;
        }

        return buffer;
    }


    
}




public enum BufferErrorCode : sbyte
{
    Succes = 1,

    SurpassesMax = 2,
    BufferSmall = 3,
}