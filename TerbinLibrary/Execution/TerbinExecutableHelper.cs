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
            pMethod.ReturnType == typeof(Task<InfoResponse?>)
        );
    }
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




    public static void RegisterFromAssembly<T, E>(Assembly pAssembly, E pExecutor)
        where T : Attribute, IExecutableAttribute
        where E : IExecutableDispatcher 
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attrs = method.GetCustomAttributes<T>(inherit: false);
                if (!attrs.Any()) continue;

                var parameters = method.GetParameters();
                if (!IsFirmParameters(parameters))
                    continue;

                if (!IsFirmReturn(method))
                    continue;

                var del = (Func<Header, byte[], Task<InfoResponse?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<InfoResponse?>>), method);

                foreach (var attr in attrs)
                {
                    pExecutor.Register(attr, (h, b) => del(h, b));
                }
            }
        }
    }



    public static async Task<InfoResponse?> ExecutionList(List<TerbinExecutableDelegate> pHandlers, Header pHead, byte[] pPayload)
    {
        var pendignTask = new List<Task<InfoResponse?>>(pHandlers.Count);
        for (int i = 0; i < pHandlers.Count; i++)
        {
            pendignTask.Add(pHandlers[i](pHead, pPayload));
        }

        while (pendignTask.Count > 0)
        {
            var completeTask = await Task.WhenAny(pendignTask).ConfigureAwait(false);
            pendignTask.Remove(completeTask);

            var result = await completeTask.ConfigureAwait(false);
            if (result != null)
                return result;
        }
        return null;
        //.ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
    }
}
