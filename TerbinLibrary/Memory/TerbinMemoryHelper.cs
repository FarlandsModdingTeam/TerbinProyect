using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

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


public class TerbinMemoryHelper
{

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

    public static bool TryReleaseMemory(byte pIdMemory)
    {
        if (pIdMemory > TerbinProtocol.RESERVE_MEMORY)
            return TerbinMemoryManager.Release(pIdMemory);
        return false;
    }

}
