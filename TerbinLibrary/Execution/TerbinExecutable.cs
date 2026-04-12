using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */

// TODO: AllowMultiple in true.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TerbinExecutableAttribute : Attribute
{
    public byte Action { get; } // CodeAction

    // TODO: guardar la lista de types de la funcion.
    // TODO: funcion estatica que serialice/deserialice a los tipos y cantidades guardadas.

    public TerbinExecutableAttribute(byte pAction) => Action = pAction;
}


public delegate Task<PacketRequest> ExecutableHandler(Header pHead, MemoryStream pParameters);


public sealed class ExecutableDispatcher
{
    private static readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();


    public static void Register(byte pAction, ExecutableHandler pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pAction] = pHandler;
    }

    public static bool Unregister(byte pAction) => _handlers.TryRemove(pAction, out _);


    public static async Task<PacketRequest> DispatchAsync(PacketRequest pCapsule) // Aqui es donde se ejecuta, manda cojones.
    {
        Console.WriteLine($"[Worker] Llegamos al despacahdor");
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handler))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            if (pCapsule.Head.IdMemory > TerbinProtocol.RESERVE_MEMORY)
                TerbinMemory.Release(pCapsule.Head.IdMemory);
            return pCapsule;
        }

        if (!tryGetMemoryStream(pCapsule, out var memo))
        {
            // TODO: Controlar.
        }
        try
        {
            return await handler(pCapsule.Head, memo)
                .ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
        }
        catch
        {
            pCapsule.Head.Status = CodeStatus.ExecutionError;
            return pCapsule;
        }
    }

    private static bool tryGetMemoryStream(PacketRequest pCapsule, out MemoryStream pMemory)
    {
        pMemory = getMemoryStream(pCapsule);
        return TerbinMemory.Release(pCapsule.Head.IdMemory);
    }

    private static MemoryStream getMemoryStream(PacketRequest pCapsule)
    {
        if (pCapsule.Head.OrderRequest != TerbinProtocol.FINAL_PACKET)
            return new MemoryStream(pCapsule.Payload ?? []);

        if (TerbinMemory.TryGetResult(pCapsule.Head.IdMemory, out var bytes) is var r && r.succes)
        {
            return addToMemoryStream(pCapsule, bytes);
        }
        else
        {
            // TODO: Logger.
        }

        return new MemoryStream();
    }

    public static MemoryStream addToMemoryStream(PacketRequest pCapsule, byte[] pBytes)
    {
        byte[] result = new byte[pCapsule.Payload.Length + pBytes.Length];
        Buffer.BlockCopy(pCapsule.Payload, 0, result, 0, pCapsule.Payload.Length);
        Buffer.BlockCopy(pBytes, 0, result, pCapsule.Payload.Length, pBytes.Length);
        return new MemoryStream(result); // [.. pBytes, .. pCapsule.Payload]
    }

    // Esto no funcionara porque registrara TerbinLibrary XD
    [Obsolete]
    public static void RegisterAll()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }

    // Escanea métodos estáticos con [TerbinCommand(1)] y firma Task<Capsule>(Header, byte[])
    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<TerbinExecutableAttribute>(inherit: false);
                if (attr == null) continue;

                // Requiere: Task<Capsule> M(Header, byte[])
                var parameters = method.GetParameters();
                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(Header) ||
                    parameters[1].ParameterType != typeof(MemoryStream))
                    continue;

                if (method.ReturnType != typeof(Task<PacketRequest>))
                    continue;

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, MemoryStream, Task<PacketRequest>>)Delegate.CreateDelegate(
                    typeof(Func<Header, MemoryStream, Task<PacketRequest>>), method);

                Register(attr.Action, (h, b) => del(h, b));
            }
        }
    }

}
