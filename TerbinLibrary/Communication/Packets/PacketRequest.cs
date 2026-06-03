using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;

namespace TerbinLibrary.Communication.Packets;
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
/// Estructura que encapsula todos los datos para una solicitud o paquete Terbin.<br />
/// Permite enviar, recibir y procesar las peticiones mediante serialización binaria.<br />
/// Notas: Esta estructura implementa IStructSerializable.<br />
/// Tips: Utilice los métodos de creación estáticos para crear respuestas fácilmente.<br />
/// ___________________( English )___________________<br />
/// Structure encapsulating all data for a Terbin request or packet.<br />
/// Allows sending, receiving, and processing requests through binary serialization.<br />
/// Notes: This structure implements IStructSerializable.<br />
/// Tips: Use the static creation methods to easily create responses.<br />
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PacketRequest : IStructSerializable
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Cabecera del paquete.<br />
    /// ___________________( English )___________________<br />
    /// Header of the packet.<br />
    /// </summary>
    public Header Head;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificadores del método de acción o comando asociado al paquete.<br />
    /// ___________________( English )___________________<br />
    /// Identifiers of the associated action method or command of the packet.<br />
    /// </summary>
    public IdArray ActionMethod;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Carga útil adjunta o información de datos del paquete.<br />
    /// ___________________( English )___________________<br />
    /// Attached payload or data information of the packet.<br />
    /// </summary>
    public byte[] Payload;

    public readonly bool IsSucces => Head.IsSucces;


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de <see cref="PacketRequest"/> con valores predeterminados.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of <see cref="PacketRequest"/> with default values.<br />
    /// </summary>
    public PacketRequest()
    {
        Head = new Header();
        ActionMethod = new IdArray((byte)CodeTerbinProtocol.Response);
        Payload = [];
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de <see cref="PacketRequest"/> aceptando una cabecera opcional.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of <see cref="PacketRequest"/> accepting an optional header.<br />
    /// </summary>
    /// <param name="pHead">Es: La cabecera para el paquete. <br />En: The header for the packet.</param>
    public PacketRequest(Header? pHead = null)
        : this (pHead, (IdArray?)null, (byte[]?)null)
    { }
    //public PacketRequest(
    //    Header? pHead = null,
    //    byte[]? pActionMethod = null,
    //    byte[]? pPayload = null)
    //    : this(pHead, new IdArray(pActionMethod ?? [(byte)CodeTerbinProtocol.Response]), pPayload)
    //{ }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Constructor principal que asigna sus propiedades de cabecera, acción y carga útil.<br />
    /// ___________________( English )___________________<br />
    /// Main constructor that assigns its header, action, and payload properties.<br />
    /// </summary>
    /// <param name="pHead">Es: Cabecera base de la petición. <br />En: Base header of the request.</param>
    /// <param name="pActionMethod">Es: Identificador del comando a procesar. <br />En: Identifier of the command to be processed.</param>
    /// <param name="pPayload">Es: Información con la carga de datos (payload). <br />En: Info containing the data payload.</param>
    public PacketRequest(
        Header? pHead = null,
        IdArray? pActionMethod = null,
        byte[]? pPayload = null)
    {
        Head = pHead ?? new Header();
        ActionMethod = pActionMethod ?? new IdArray((byte)CodeTerbinProtocol.Response);
        Payload = pPayload ?? [];
    }

    // Header + bye + byte + ThreeQuartersInt + byte[]
    // 7 + 1 + 0 + 2 + Length
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el tamaño en bytes que ocupará esta estructura al ser serializada.<br />
    /// ___________________( English )___________________<br />
    /// Gets the size in bytes this structure will occupy when serialized.<br />
    /// </summary>
    /// <returns>Es: Cantidad total de bytes. <br />En: Total amount of bytes.</returns>
    public readonly ushort GetSize() => (ushort)(8 + TerbinProtocol.LENGTH_ARRAY + (Payload?.Length ?? 0) + ActionMethod.GetSize());

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Puebla este paquete a partir de un Span de solo lectura (ReadOnlySpan).<br />
    /// ___________________( English )___________________<br />
    /// Populates this packet coming from a read-only Span.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Span de bytes que contiene la información sin tratar. <br />En: Bytes Span containing the raw info.</param>
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Head = pBuffer.Read<Header>(ref offset);
        ActionMethod = pBuffer.ReadStruct<IdArray>(ref offset);
        Payload = pBuffer.ReadArray<byte>(ref offset);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe el paquete completo en un búfer o Span (Span) de destino.<br />
    /// ___________________( English )___________________<br />
    /// Writes the complete packet into a target buffer or Span.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Span de bytes destino donde se volcará la información. <br />En: Target bytes Span where the info gets written.</param>
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<Header>(ref offset, Head);
        pBuffer.WriteStruct<IdArray>(ref offset, ActionMethod);
        pBuffer.WriteArray<byte>(ref offset, Payload);
    }



    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Vacía la carga útil y restaura la cabecera para liberar estados previos.<br />
    /// ___________________( English )___________________<br />
    /// Clears the payload and resets the header to release previous states.<br />
    /// </summary>
    public void ClearPacket()
    {
        Head.OrderRequest = TerbinProtocol.ORDER_SINGLE;
        Head.IdMemory = (byte)CodeTerbinMemory.NotAsign;
        Payload = [];
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Genera rápidamente una solicitud de respuesta con estado de error a partir de un ID de solicitud.<br />
    /// ___________________( English )___________________<br />
    /// Quickly generates a response request with error state out of a request ID.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: ID original de la petición. <br />En: Original request ID.</param>
    /// <param name="pError">Es: Status de error asociado. <br />En: Associated error status.</param>
    /// <returns>Es: Paquete creado listo para enviar como respuesta de error. <br />En: Created packet ready to be sent as an error response.</returns>
    public static PacketRequest CreateResponseError(ushort pIdRequest, CodeStatus pError)
    {
        Header h = new(pIdRequest: pIdRequest);
        return CreateResponseError(h, pError);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Genera velozmente una respuesta de éxito de acuerdo a un ID de petición determinado.<br />
    /// ___________________( English )___________________<br />
    /// Quickly issues a successful response according to a specific request ID.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: ID original al que se está respondiendo. <br />En: Original ID being replied to.</param>
    /// <returns>Es: Paquete configurado correctamente para su uso como respuesta. <br />En: Packet configured correctly for use as a response.</returns>
    public static PacketRequest CreateResponseSucces(ushort pIdRequest)
    {
        Header h = new(pIdRequest: pIdRequest);
        return CreateResponseSucces(h);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Construye una respuesta de error empleando un objeto Header original.<br />
    /// ___________________( English )___________________<br />
    /// Constructs an error response using an original Header object.<br />
    /// </summary>
    /// <param name="pHead">Es: Cabecera base original para la nueva petición de respuesta. <br />En: Original base header for the new response request.</param>
    /// <param name="pError">Es: Código identificando el error ocurrido. <br />En: Code tracing the caused error.</param>
    /// <returns>Es: Paquete configurado mediante la cabecera entrada. <br />En: Configured packet via the input header.</returns>
    public static PacketRequest CreateResponseError(Header pHead, CodeStatus pError)
    {
        pHead.Status = pError;
        return CreateResponse(pHead);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Construye una respuesta exitosa sobrescribiendo internamente la cabecera dada.<br />
    /// ___________________( English )___________________<br />
    /// Constructs a success response internally overwriting the given header.<br />
    /// </summary>
    /// <param name="pHead">Es: Referencia a la cabecera original. <br />En: Reference to the original header.</param>
    /// <returns>Es: Nuevo objeto configurado como respuesta exitosa. <br />En: New object configured as a success response.</returns>
    public static PacketRequest CreateResponseSucces(Header pHead)
    {
        pHead.Status = CodeStatus.Succes;
        return CreateResponse(pHead);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Ensambla una respuesta general asignando las marcas correspondientes de un Response.<br />
    /// ___________________( English )___________________<br />
    /// Assembles a general response assigning corresponding flags marking it as Response.<br />
    /// </summary>
    /// <param name="pHead">Es: Cabecera del paquete original. <br />En: Header of the original packet.</param>
    /// <param name="pPayload">Es: Opcional, los bytes de respuesta a retornar al emisor. <br />En: Optional, the response bytes to return to the sender.</param>
    /// <returns>Es: Instancia final lista de PacketRequest. <br />En: Ready final instance of PacketRequest.</returns>
    public static PacketRequest CreateResponse(Header pHead, byte[]? pPayload = null)
    {
        pHead.IdMemory = (byte)CodeTerbinMemory.NotAsign;
        pHead.OrderRequest = TerbinProtocol.ORDER_SINGLE;
        return new PacketRequest(pHead, new IdArray((byte)CodeTerbinProtocol.Response), pPayload);
    }

    /*
    public static explicit operator PacketRequest(Task<PacketRequest?> v)
    {
        if (v != null)
            return (PacketRequest)v;
        else
            return new PacketRequest();
    }*/


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Devuelve un texto legible para su uso en diagnósticos (Debugging).<br />
    /// ___________________( English )___________________<br />
    /// Gives back a readable text equivalent intended for diagnostics (Debugging).<br />
    /// </summary>
    /// <returns>Es: Cadena textual que representa cada valor. <br />En: Text string depicting each value.</returns>
    public override string ToString()
    {
        return $"(Head: {Head}, ActionMethod: {ActionMethod}, Payload: {Util.DebugTerbinLibrary.ArrayToString(Payload)})";
    }
}

/*
 No Byte[], MemoryStream, string, BinaryFormatter, Span<byte>, creo que solo me queda unsafe y no se como funciona.
*/
