using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Exceptions;

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

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase encargada de escribir estructuras serializadas en un flujo de datos (Stream).<br />
/// Notas: Utiliza un prefijo de 2 bytes para indicar el tamaño del paquete antes del payload.<br />
/// Tips: Asegúrate de que la estructura implemente IStructSerializable.<br />
/// ___________________( English )___________________<br />
/// Class responsible for writing serialized structures to a data stream.<br />
/// Notes: Uses a 2-byte prefix to indicate the packet size before the payload.<br />
/// Tips: Ensure the struct implements IStructSerializable.<br />
/// </summary>
public class StreamWriteStruct : StreamBytes
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la clase <see cref="StreamWriteStruct"/>.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the <see cref="StreamWriteStruct"/> class.<br />
    /// </summary>
    /// <param name="pPipeStream">Es: El flujo de datos donde se escribirá. <br />En: The data stream where it will be written.</param>
    public StreamWriteStruct(Stream pPipeStream) : base(pPipeStream) { }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe una estructura de forma asíncrona en el flujo de datos.<br />
    /// Notas: Valida que el tamaño de la estructura no supere el valor máximo de ushort.<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously writes a struct to the data stream.<br />
    /// Notes: Validates that the struct size does not exceed the maximum ushort value.<br />
    /// </summary>
    /// <typeparam name="T">Es: El tipo de estructura a escribir. <br />En: The type of structure to write.</typeparam>
    /// <param name="pStruct">Es: La estructura que se va a serializar y escribir. <br />En: The structure to serialize and write.</param>
    /// <param name="pToken">Es: Token para cancelar la operación asíncrona. <br />En: Token to cancel the asynchronous operation.</param>
    public async Task WriteAsycn<T>(T pStruct, CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        if (pStruct.GetSize() > ushort.MaxValue)
            throw new ArgumentOutOfRangeException("(StreamWriteStruct>WriteAsycn): Struct large overflow ushort max");

        byte[] buffer = Serialineitor.SerializeStructRaw<T>(pStruct);

        byte[] lengthPrefix = BitConverter.GetBytes((ushort)buffer.Length);
        try
        {
            await PipeStream.WriteAsync(lengthPrefix.AsMemory(), pToken);
            await base.WriteBytesAsync(buffer, pToken);
        }
        catch (Exception e)
        {
            e.PrintException("StreamWriteStruct>WriteAsycn");
            throw;
        }
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase encargada de leer estructuras serializadas desde un flujo de datos (Stream).<br />
/// Notas: Lee un prefijo de 2 bytes para determinar el tamaño del paquete antes del payload.<br />
/// Tips: El tipo devuelto debe implementar IStructSerializable.<br />
/// ___________________( English )___________________<br />
/// Class responsible for reading serialized structures from a data stream.<br />
/// Notes: Reads a 2-byte prefix to determine the packet size before reading the payload.<br />
/// Tips: The returned type must implement IStructSerializable.<br />
/// </summary>
public class StreamReadStruct : StreamBytes
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la clase <see cref="StreamReadStruct"/>.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the <see cref="StreamReadStruct"/> class.<br />
    /// </summary>
    /// <param name="pPipeStream">Es: El flujo de datos desde donde se leerá. <br />En: The data stream to read from.</param>
    public StreamReadStruct(Stream pPipeStream) : base(pPipeStream) { }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee de forma asíncrona una estructura desde el flujo de datos.<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously reads a struct from the data stream.<br />
    /// </summary>
    /// <typeparam name="T">Es: El tipo de estructura a leer y deserializar. <br />En: The type of structure to read and deserialize.</typeparam>
    /// <param name="pToken">Es: Token para cancelar la operación asíncrona. <br />En: Token to cancel the asynchronous operation.</param>
    /// <returns>Es: La estructura deserializada leída del stream. <br />En: The deserialized struct read from the stream.</returns>
    public async Task<T> ReadAsycn<T>(CancellationToken pToken = default)
        where T : struct, IStructSerializable
    {
        byte[] lengthBuffer = await base.ReadBytesAsycn(2, pToken);
        if (lengthBuffer.Length != 2)
            throw new InvalidOperationException($"(StreamReadStruct>ReadAsycn): Expected 2 bytes for length header, got {lengthBuffer.Length}");

        try
        {
            ushort packetLength = BitConverter.ToUInt16(lengthBuffer, 0); // ToUInt16

            byte[] buffer = await base.ReadBytesAsycn(packetLength, pToken);
            return Serialineitor.DeserializeStructRaw<T>(buffer);
        }
        catch (Exception e)
        {
            e.PrintException("StreamWriteStruct>ReadAsycn");
            throw;
        }
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase base abstracta que proporciona operaciones de lectura y escritura de bytes sobre un flujo.<br />
/// Notas: Implementa de forma segura el patrón IDisposable para la liberación de recursos.<br />
/// ___________________( English )___________________<br />
/// Abstract base class providing byte read and write operations over a stream.<br />
/// Notes: Safely implements the IDisposable pattern for resource release.<br />
/// </summary>
// TODO: (Mangincian): ver si hace falta MarshalByRefObject;
public abstract class StreamBytes : /*MarshalByRefObject,*/ IDisposable
{
    private Stream _pipeStream;

    // ****************************( Getters, Setters e Indexadores )**************************** //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el flujo de datos (Stream) subyacente.<br />
    /// ___________________( English )___________________<br />
    /// Gets the underlying data stream (Stream).<br />
    /// </summary>
    public Stream PipeStream
    {
        get => _pipeStream;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de <see cref="StreamBytes"/> vinculada al stream proveído.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of <see cref="StreamBytes"/> linked to the provided stream.<br />
    /// </summary>
    /// <param name="pPipeStream">Es: El flujo a utilizar para operaciones base. <br />En: The stream to use for base operations.</param>
    public StreamBytes(Stream pPipeStream)
    {
        this._pipeStream = pPipeStream;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee un número exacto de bytes desde el stream de forma asíncrona.<br />
    /// Notas: Asegura que se lean los bytes solicitados para evitar datos incompletos.<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously reads an exact number of bytes from the stream.<br />
    /// Notes: Ensures the requested bytes are fully read to avoid incomplete data.<br />
    /// </summary>
    /// <param name="pSize">Es: Cantidad de bytes a leer. <br />En: Amount of bytes to read.</param>
    /// <param name="pToken">Es: Token para cancelar la operación asíncrona. <br />En: Token to cancel the asynchronous operation.</param>
    /// <returns>Es: Un arreglo de bytes de tamaño igual al solicitado con el contenido. <br />En: A byte array of the requested size with the contents.</returns>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe un arreglo de bytes en el stream asíncronamente y lo vacía (flush).<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously writes a byte array to the stream and flushes it.<br />
    /// </summary>
    /// <param name="buffer">Es: Arreglo de bytes a escribir. <br />En: Byte array to write.</param>
    /// <param name="pToken">Es: Token para cancelar la operación asíncrona. <br />En: Token to cancel the asynchronous operation.</param>
    public virtual async Task WriteBytesAsync(byte[] buffer, CancellationToken pToken = default)
    {
        if (buffer == null) throw new ArgumentNullException("(StreamBytes>WriteBytesAsync): " + nameof(buffer));

        await PipeStream.WriteAsync(buffer.AsMemory(0, buffer.Length), pToken);
        await PipeStream.FlushAsync(pToken);
    }


    // ****************************( Implement IDisposable )**************************** //
    private bool _disposed = false;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Libera de forma pública todos los recursos en uso o inicializados.<br />
    /// ___________________( English )___________________<br />
    /// Publicly releases all in-use or initialized resources.<br />
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Método protegido para liberación de recursos del patrón IDisposable.<br />
    /// ___________________( English )___________________<br />
    /// Protected method for IDisposable pattern resource release.<br />
    /// </summary>
    /// <param name="disposing">Es: Determina si debe liberar los recursos administrados o solo los de bajo nivel. <br />En: Determines whether to release managed resources or only low-level ones.</param>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Libera recursos administrados (por ejemplo, Streams o componentes .NET).<br />
    /// ___________________( English )___________________<br />
    /// Releases managed resources (such as .NET Streams or components).<br />
    /// </summary>
    protected virtual void liberateAdministered()
    {
        // Liberar recursos administrados.
        _pipeStream?.Dispose();

    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Libera recursos NO administrados (porcentajes de sistema, interop, buffers nativos).<br />
    /// ___________________( English )___________________<br />
    /// Releases unmanaged resources (OS handles, interop, native buffers).<br />
    /// </summary>
    protected virtual void liberateNotAdministered()
    {
        // Liberar recursos NO administrados aquí (si los hubiera).

    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Destructor subyacente llamado por el GC para asegurar que se limpie la memoria.<br />
    /// ___________________( English )___________________<br />
    /// Underlying finalizer called by the GC to assure memory is cleaned.<br />
    /// </summary>
    ~StreamBytes()
    {
        Dispose(false);
    }
}
