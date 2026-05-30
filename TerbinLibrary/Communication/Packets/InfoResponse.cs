using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Protocol;

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
/// Estructura que representa la información de una respuesta de Terbin.<br />
/// Contiene datos para identificar la solicitud original, el estado y su carga útil.<br />
/// Notas: Se utiliza para construir respuestas simples a las peticiones del usuario o sistema.<br />
/// Tips: Utiliza los métodos estáticos Create* para instanciar fácilmente la estructura.<br />
/// ___________________( English )___________________<br />
/// Structure representing the information of a Terbin response.<br />
/// It contains data to identify the original request, status, and its payload.<br />
/// Notes: It is used to build simple responses to user or system requests.<br />
/// Tips: Use the static Create* methods to easily instantiate the structure.<br />
/// </summary>
public struct InfoResponse //: IInfo
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador original de la petición a la cual se está respondiendo.<br />
    /// ___________________( English )___________________<br />
    /// Original identifier of the request being replied to.<br />
    /// </summary>
    public ushort IdRequest { get => field; set => field = value; }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Estado o resultado del procesamiento de la respuesta.<br />
    /// ___________________( English )___________________<br />
    /// Status or execution result of the response.<br />
    /// </summary>
    public CodeStatus Status { get => field; set => field = value; }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador del método de acción utilizado por el paquete de respuesta.<br />
    /// ___________________( English )___________________<br />
    /// Identifier of the action method used by the response packet.<br />
    /// </summary>
    public IdArray ActionMethod { get => field; set => field = value; }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Carga útil (datos en bytes) anexados a la respuesta.<br />
    /// ___________________( English )___________________<br />
    /// Payload (byte data) attached to the response.<br />
    /// </summary>
    public byte[] Payload { get => field; set => field = value; }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de <see cref="InfoResponse"/> con valores predeterminados.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of <see cref="InfoResponse"/> with default values.<br />
    /// </summary>
    public InfoResponse()
    {
        IdRequest = TerbinProtocol.ORDER_SINGLE;
        Status = CodeStatus.NotAsign;
        ActionMethod = new IdArray((byte)CodeTerbinProtocol.Response);
        Payload = [];
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea una nueva respuesta base configurada con un identificador y estado específico.<br />
    /// ___________________( English )___________________<br />
    /// Creates a new base response configured with a specific identifier and status.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Identificador de la solicitud. <br />En: Request identifier.</param>
    /// <param name="pStatus">Es: Estado general aplicable. <br />En: Applicable general status.</param>
    /// <returns>Es: Instancia generada de InfoResponse. <br />En: Generated InfoResponse instance.</returns>
    public static InfoResponse Create(ushort pIdRequest, CodeStatus pStatus)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = pStatus,
        };
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea velozmente un mensaje de respuesta para cuando ocurre un error interno en el Worker.<br />
    /// ___________________( English )___________________<br />
    /// Quickly creates a response message for when an internal Worker error occurs.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Id original de la petición fallida. <br />En: Original id of the failed request.</param>
    /// <param name="pPld">Es: Carga de bytes conteniendo detalles extra del error. <br />En: Bytes payload containing extra details of the error.</param>
    /// <returns>Es: Respuesta con estado de error interno. <br />En: Response with internal error status.</returns>
    public static InfoResponse CreateInteralError(ushort pIdRequest, params byte[] pPld)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.InternalWorkerError,
            Payload = pPld,
        };
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Construye una respuesta señalando el éxito de la petición inicial sin datos extra.<br />
    /// ___________________( English )___________________<br />
    /// Constructs a response signaling the success of the initial request with no extra data.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Id al que se asocia la respuesta. <br />En: Id to which the response is associated.</param>
    /// <returns>Es: Instancia de éxito lista para despachar. <br />En: Success instance ready for dispatch.</returns>
    public static InfoResponse CreateSucces(ushort pIdRequest)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.Succes,
        };
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Construye una respuesta de éxito pero incluyendo unos datos en formato de carga útil o Payload.<br />
    /// ___________________( English )___________________<br />
    /// Constructs a success response including a payload.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Id correspondiente de la petición. <br />En: Corresponding id of the request.</param>
    /// <param name="pPLD">Es: Información util o bytes generados listos para empaquetarlos en la respuesta. <br />En: Useful info or generated bytes ready to be packed on the response.</param>
    /// <returns>Es: Estructura de respuesta de éxito conteniendo datos. <br />En: Success response structure containing data.</returns>
    public static InfoResponse CreateSucces(ushort pIdRequest, byte[] pPLD)
    {
        return new InfoResponse
        {
            IdRequest = pIdRequest,
            Status = CodeStatus.Succes,
            Payload = pPLD,
        };
    }

    /*
    public void InfoSend(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public Task<PacketRequest?> InfoSendAsync(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }
    */
}
