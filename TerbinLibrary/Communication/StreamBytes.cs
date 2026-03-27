using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Communication;


// TODO: (Mangincian): ver si hace falta MarshalByRefObject;
public class StreamReadBytes : StreamBytes
{
    public StreamReadBytes(Stream pPipeStream) : base(pPipeStream)
    {
    }



    public async Task<T> ReadAsycn<T>() where T : struct
    {
        byte[] buffer = new byte[Marshal.SizeOf<T>()];

        await PipeStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
        T data = BytesToStruct<T>(buffer);


    }
}
public class StreamWritesBytes : StreamBytes
{
    public StreamWritesBytes(Stream pPipeStream) : base(pPipeStream)
    {
    }

}


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
