using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
/// Estructura que representa la cabecera de un paquete Terbin.<br />
/// Contiene el identificador de la solicitud, el estado, el orden y la configuración de memoria.<br />
/// Notas: Esta estructura se maneja en memoria de código no administrado (unmanaged) mediante Pack = 1.<br />
/// Tips: Úsela para definir y leer la información básica de enrutamiento al transmitir información.<br />
/// ___________________( English )___________________<br />
/// Structure representing the header of a Terbin packet.<br />
/// It contains the request identifier, state, sequence order, and memory configuration.<br />
/// Notes: This is an unmanaged memory structure layout built with Pack = 1.<br />
/// Tips: Use it to define and parse basic routing information when transmitting data.<br />
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Header // la memoria es constante es unmanaged.
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador numérico de la solicitud a la que pertenece esta cabecera.<br />
    /// ___________________( English )___________________<br />
    /// Numeric identifier of the request to which this header belongs.<br />
    /// </summary>
    public ushort IdRequest;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Valor indicando el orden en secuencias de paquetes divididos.<br />
    /// Notas: 0 = paquete único, 1 = primer fragmento, ushort.MaxValue = último fragmento.<br />
    /// ___________________( English )___________________<br />
    /// Value stating the order inside a sequence of split packets.<br />
    /// Notes: 0 = single packet, 1 = first piece, ushort.MaxValue = last piece.<br />
    /// </summary>
    public ushort OrderRequest; // 0 = solo uno, 1 = es el primero, ushort.MaxValue = es el ultimo.

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Estado devuelto por la ejecución o validación de esta solicitud.<br />
    /// ___________________( English )___________________<br />
    /// Status returned by the execution or validation of this request.<br />
    /// </summary>
    public CodeStatus Status;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador de la zona de memoria asociada cuando proceda.<br />
    /// ___________________( English )___________________<br />
    /// Identifier characterizing an assigned memory sector when applicable.<br />
    /// </summary>
    public byte IdMemory;

    public readonly bool IsSucces => Status == CodeStatus.Succes;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de <see cref="Header"/> con sus valores internos por defecto.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of <see cref="Header"/> using its internal default values.<br />
    /// </summary>
    public Header()
    {
        IdRequest = 0;
        OrderRequest = 0;
        Status = CodeStatus.NotAsign;
        IdMemory = (byte)CodeTerbinMemory.Undefined;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea una nueva cabecera (Header) proporcionando los datos necesarios opcionalmente.<br />
    /// Notas: Si el parámetro del ID es 0, internamente se registrará como 1.<br />
    /// ___________________( English )___________________<br />
    /// Creates a new Header object optionally providing the needed underlying data.<br />
    /// Notes: If the passed ID is 0, it internally enforces a value of 1.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Cifra del Id primario de petición. <br />En: Primary request ID figure.</param>
    /// <param name="pOrderRequest">Es: Orden en tráfico fraccionado (Por defecto único). <br />En: Splitted traffic order (Defaults to single).</param>
    /// <param name="pStatus">Es: Estatus general a registrar inicial. <br />En: Overall initial status to log.</param>
    /// <param name="pIdMemory">Es: Referencia a la zona de memoria a interactuar. <br />En: Reference indicating memory slot interacting with.</param>
    public Header(
        ushort pIdRequest = 0,
        ushort pOrderRequest = TerbinProtocol.ORDER_SINGLE,
        CodeStatus pStatus = CodeStatus.NotAsign,
        byte pIdMemory = (byte)CodeTerbinMemory.Undefined)
    {
        IdRequest = (pIdRequest == 0) ? (ushort)1 : pIdRequest;
        OrderRequest = pOrderRequest;
        Status = pStatus;
        IdMemory = pIdMemory;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Representa de forma literal (String) los valores incluidos en esta sesión para tareas de depuración.<br />
    /// ___________________( English )___________________<br />
    /// Expresses a literal string representation of the properties covered, suited for debugging.<br />
    /// </summary>
    /// <returns>Es: Texto con forma amigable de la estructura base. <br />En: Friendly textual layout of the underlying structure.</returns>
    public override string ToString()
    {
        return $"(IdRequest: {IdRequest}, OrderRequest: {OrderRequest}, Status: {Status}, IdMemory: {IdMemory})";
    }
}