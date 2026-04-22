using Microsoft.CodeAnalysis.CSharp.Syntax;
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
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


public class StreamWriteStruct : StreamBytes
{
    public StreamWriteStruct(Stream pPipeStream) : base(pPipeStream) { }

    public async Task WriteAsycn<T>(T pStruct, CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        if (pStruct.GetSize() > ushort.MaxValue)
            throw new ArgumentOutOfRangeException("(StreamWriteStruct>WriteAsycn): Struct large overflow ushort max");

        byte[] buffer = Serialineitor.SerializeStruct<T>(pStruct);

        byte[] lengthPrefix = BitConverter.GetBytes((ushort)buffer.Length);
        try
        {
            await PipeStream.WriteAsync(lengthPrefix.AsMemory(), pToken);
            await base.WriteBytesAsync(buffer, pToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[StreamWriteStruct>WriteAsycn] ExceptionError-> {e.Message}");
            throw;
        }
    }
}

public class StreamReadStruct : StreamBytes
{
    public StreamReadStruct(Stream pPipeStream) : base(pPipeStream) { }

    public async Task<T> ReadAsycn<T>(CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        byte[] lengthBuffer = await base.ReadBytesAsycn(2, pToken);
        if (lengthBuffer.Length != 2)
            throw new InvalidOperationException($"(StreamReadStruct>ReadAsycn): Expected 2 bytes for length header, got {lengthBuffer.Length}");

        ushort packetLength = BitConverter.ToUInt16(lengthBuffer, 0); // ToUInt16

        try
        {
            byte[] buffer = await base.ReadBytesAsycn(packetLength, pToken);
            return Serialineitor.DeserializeStruct<T>(buffer);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[StreamWriteStruct>ReadAsycn] ExceptionError-> {e.Message}");
            throw;
        }
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

    public virtual async Task<byte[]> ReadBytesAsycn(int pSize, CancellationToken pToken = default)
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

    public virtual async Task WriteBytesAsync(byte[] buffer, CancellationToken pToken = default)
    {
        if (buffer == null) throw new ArgumentNullException("(StreamBytes>WriteBytesAsync): " + nameof(buffer));

        await PipeStream.WriteAsync(buffer.AsMemory(0, buffer.Length), pToken);
        await PipeStream.FlushAsync(pToken);
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
