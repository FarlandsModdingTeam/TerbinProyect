using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Protocol;

namespace TerbinLibrary.Memory;
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
/// Clase auxiliar que facilita el manejo, ensamblado y liberación de flujos de memoria en red, colaborando con los paquetes recibidos.<br />
/// ___________________( English )___________________<br />
/// Helper class that facilitates the handling, assembly, and release of network memory streams, collaborating with incoming packets.<br />
/// </summary>
public class TerbinMemoryHelper
{

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta obtener el flujo completo de memoria a partir de una cápsula (paquete). Si no es un bloque final, extrae solo su carga útil; si lo es, ensambla las partes y libera la memoria usada.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to get the full memory stream from a packet capsule. If it is not a final block, it extracts only its payload; if it is, it assembles the stored fragments and frees the used memory.<br />
    /// </summary>
    /// <param name="pCapsule">Es: Paquete de solicitud recibido. <br />En: Received request packet.</param>
    /// <param name="pMemory">Es: Búfer de salida con el ensamble de datos. <br />En: Output buffer with the final assembly.</param>
    /// <returns>Es: Código de error resultante (None si fue exitoso). <br />En: Resulting error code (None on success).</returns>
    public static TerbinErrorCode TryGetMemoryStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        // Si es paquete individual => devuelve su PLD.
        if (pCapsule.Head.OrderRequest != TerbinProtocol.FINAL_PACKET)
        {
            pMemory = pCapsule.Payload ?? Array.Empty<byte>();
            return TerbinErrorCode.None;
        }
        var codeError = TryAssembleStream(pCapsule, out pMemory);
        if (!TryReleaseMemory(pCapsule.Head.IdMemory))
            codeError = (codeError != TerbinErrorCode.None) ? TerbinErrorCode.MemoryReleaseFailed : codeError;
        return codeError;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Ensambla los fragmentos guardados en el manejador de memoria con el contenido restante del paquete actual.<br />
    /// ___________________( English )___________________<br />
    /// Assembles the stored fragments in the memory manager with the remaining content of the current packet.<br />
    /// </summary>
    /// <param name="pCapsule">Es: Paquete que contiene la parte final o complementaria. <br />En: Packet containing the final or complementary part.</param>
    /// <param name="pMemory">Es: Búfer combinado resultante de la unión. <br />En: Resulting combined buffer from the join.</param>
    /// <returns>Es: Código de error retornado por la obtención de memoria. <br />En: Error code returned from memory fetch.</returns>
    public static TerbinErrorCode TryAssembleStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        if (TerbinMemoryManager.TryGetResult(pCapsule.Head.IdMemory, out var bytes) is var r && r.succes)
        {
            pMemory = CombinePayload(pCapsule, bytes);
            return TerbinErrorCode.None;
        }
        else
        {
            pMemory = [];
            return r.typeError;
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Combina la información previamente almacenada con los datos brutos cargados en la cápsula final.<br />
    /// ___________________( English )___________________<br />
    /// Combines the previously stored information with the raw payload loaded inside the final capsule.<br />
    /// </summary>
    /// <param name="pCapsule">Es: Paquete contenedor de la carga complementaria. <br />En: Container packet of the complementary load.</param>
    /// <param name="pBytes">Es: Datos previos recuperados del almacén temporal. <br />En: Previous grouped bytes gathered from temporal local storage.</param>
    /// <returns>Es: Nuevo arreglo que comprende todos los bytes consecutivos. <br />En: New matched array comprising all consecutive bytes.</returns>
    public static byte[] CombinePayload(PacketRequest pCapsule, byte[] pBytes)
    {
        var payload = pCapsule.Payload ?? Array.Empty<byte>();
        pBytes = pBytes ?? Array.Empty<byte>();
        byte[] result = new byte[payload.Length + pBytes.Length];
        if (pBytes.Length > 0)
            Buffer.BlockCopy(pBytes, 0, result, 0, pBytes.Length);
        if (payload.Length > 0)
            Buffer.BlockCopy(payload, 0, result, pBytes.Length, payload.Length);
        return result; // [.. pBytes, .. pCapsule.Payload]
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta liberar la memoria utilizada por el flujo comprobando que su ID no pertenezca a la reserva principal del protocolo.<br />
    /// ___________________( English )___________________<br />
    /// Tries to release the memory used by the stream checking that its ID does not belong to the primary protocol reserves.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Puntero identificador de la sesión en memoria. <br />En: Identifier pointer of the memory session.</param>
    /// <returns>Es: Verdadero si la memoria se liberó y gestionó de forma lícita. <br />En: True if memory was rightfully released and managed.</returns>
    public static bool TryReleaseMemory(byte pIdMemory)
    {
        if (pIdMemory > TerbinProtocol.RESERVE_MEMORY)
            return TerbinMemoryManager.Release(pIdMemory);
        return false;
    }

}
