using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Communication;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */


public class StreamWritesStruct : StreamBytes
{
    public StreamWritesStruct(Stream pPipeStream) : base(pPipeStream) { }

    public async Task WriteAsycn<T>(T pStruct, CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        byte[] buffer = Serialineitor.SerializeStruct<T>(pStruct);

        // 1. Escribimos la longitud real del paquete primero (4 bytes)
        byte[] lengthPrefix = BitConverter.GetBytes(buffer.Length);
        await PipeStream.WriteAsync(lengthPrefix, pToken);

        // 2. Escribimos el paquete
        await base.WriteAsync(buffer, pToken);
    }
}

public class StreamReadStruct : StreamBytes
{
    public StreamReadStruct(Stream pPipeStream) : base(pPipeStream) { }

    public async Task<T> ReadAsycn<T>(CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        // 1. Leemos los 4 bytes que nos dicen cuánto mide el mensaje
        byte[] lengthBuffer = await base.ReadAsycn(4, pToken);
        int packetLength = BitConverter.ToInt32(lengthBuffer);

        // 2. Leemos EXACTAMENTE la longitud dinámica del paquete
        byte[] buffer = await base.ReadAsycn(packetLength, pToken);

        return Serialineitor.DeserializeStruct<T>(buffer);
    }
}


// TODO: (Mangincian): ver si hace falta MarshalByRefObject;
public abstract class StreamBytes : /*MarshalByRefObject,*/ IDisposable
{
    private Stream _pipeStream;

    // ****************************( Getters, Setters e Indexadores )**************************** //
    public Stream PipeStream
    {
        get => _pipeStream;
    }

    public StreamBytes(Stream pPipeStream)
    {
        this._pipeStream = pPipeStream;
    }

    public virtual async Task<byte[]> ReadAsycn(int pSize, CancellationToken pToken = default)
    {
        byte[] buffer = new byte[pSize];

        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int read = await PipeStream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead),
                pToken
            );

            if (read == 0)
                throw new EndOfStreamException("(StreamBytes>ReadAsycn): Stream closed before reading all bytes");

            totalRead += read;
        }

        return buffer;
    }

    public virtual async Task WriteAsync(byte[] buffer, CancellationToken pToken = default)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        await PipeStream.WriteAsync(buffer.AsMemory(0, buffer.Length), pToken);
        await PipeStream.FlushAsync(pToken);
    }


    // ****************************( Helpers )**************************** //
    [Obsolete(message: "Use: StructSerialineitor.SerializeConst()")]
    public static byte[] StructToBytes<T>(T pStruct) where T : struct
    {
        int size = Marshal.SizeOf(pStruct);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(pStruct, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    [Obsolete(message: "Use: StructSerialineitor.DeserializeConst()")]
    public static T BytesToStruct<T>(byte[] pBytes) where T : struct
    {
        T newStruct = default;

        IntPtr ptr = Marshal.AllocHGlobal(pBytes.Length);
        Marshal.Copy(pBytes, 0, ptr, pBytes.Length);

        newStruct = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return newStruct;
    }


    // ****************************( Implement IDisposable )**************************** //
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            liberateAdministered();
        }
        liberateNotAdministered();

        _disposed = true;
    }

    protected virtual void liberateAdministered()
    {
        // Liberar recursos administrados.
        _pipeStream?.Dispose();

    }
    protected virtual void liberateNotAdministered()
    {
        // Liberar recursos NO administrados aquí (si los hubiera).

    }

    ~StreamBytes()
    {
        Dispose(false);
    }
}
