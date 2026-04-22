using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;

public static class TerbinExecutableHelper
{
    public static bool IsFirmParameters(ParameterInfo[] pParameters)
    {
        return
        (
            pParameters.Length == 2 &&
            pParameters[0].ParameterType == typeof(Header) &&
            pParameters[1].ParameterType == typeof(byte[])
        );
    }

    public static bool IsFirmReturn(MethodInfo pMethod)
    {
        return
        (
            pMethod.ReturnType == typeof(Task<PacketRequest?>)
        );
    }
    public static TerbinErrorCode TryGetMemoryStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        // Si es paquete individual, recibe devuelve su PLD.
        if (pCapsule.Head.OrderRequest != TerbinProtocol.FINAL_PACKET)
        {
            pMemory = pCapsule.Payload ?? Array.Empty<byte>();
            return TerbinErrorCode.None;
        }
        return TryAssembleStream(pCapsule, out pMemory);
    }

    public static TerbinErrorCode TryAssembleStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        if (TerbinMemory.TryGetResult(pCapsule.Head.IdMemory, out var bytes) is var r && r.succes)
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
            return TerbinMemory.Release(pIdMemory);
        return false;
    }


    public static void RegisterAll()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }

}
