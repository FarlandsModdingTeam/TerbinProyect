using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Communication;


public class StreamReadStruct : StreamBytes
{
    public StreamReadStruct(Stream pPipeStream) : base(pPipeStream)
    {
    }

    public async Task<T> ReadAsycn<T>() where T : struct
    {
        byte[] buffer = await base.ReadAsycn(Marshal.SizeOf<T>());
        return BytesToStruct<T>(buffer);
    }
}
public class StreamWritesStruct : StreamBytes
{
    public StreamWritesStruct(Stream pPipeStream) : base(pPipeStream)
    {
    }

    public async Task WriteAsycn<T>(T pStruct) where T : struct
    {
        byte[] buffer = StructToBytes<T>(pStruct);
        await base.WriteAsync(buffer);
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

    public virtual async Task<byte[]> ReadAsycn(int pSize)
    {
        byte[] buffer = new byte[pSize];

        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int read = await PipeStream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead)
            );

            if (read == 0)
                throw new EndOfStreamException("(StreamBytes>ReadAsycn): Stream closed before reading all bytes");

            totalRead += read;
        }

        return buffer;
    }

    public virtual async Task WriteAsync(byte[] buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (PipeStream == null) throw new InvalidOperationException("(StreamBytes>ReadAsycn): PipeStream is Null.");

        await PipeStream.WriteAsync(buffer.AsMemory(0, buffer.Length));
        await PipeStream.FlushAsync();
    }


    // ****************************( Helpers )**************************** //
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
