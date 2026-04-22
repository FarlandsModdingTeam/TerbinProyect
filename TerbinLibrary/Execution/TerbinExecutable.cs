using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Extension;
using TerbinLibrary.Memory;
using TerbinLibrary.Serialize;

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
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handler))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            tryReleaseMemory(pCapsule.Head.IdMemory);
            pCapsule.ClearPLD();
            return pCapsule;
        }

        if (getMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            pCapsule.Head.Status = CodeStatus.ErrorGetPaylaodMemory;
            pCapsule.ClearPLD();
            return pCapsule;
        }
        
        try
        {
            Console.WriteLine($"[DispatchAsync] id: {pCapsule.Head.IdMemory}, Order: {pCapsule.Head.OrderRequest}, L: {memo.Length}"); //Encoding.UTF8.GetString(memo)
            return await handler(pCapsule.Head, memo)
                .ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ExecutableDispatcher>DispatchAsync] ExceptionError->  {e.Message}");
            pCapsule.Head.Status = CodeStatus.ExecutionError;
            return pCapsule;
        }
    }

    [Obsolete]
    private static TerbinErrorCode tryGetMemoryStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        var error = getMemoryStream(pCapsule, out pMemory);
        if (error != TerbinErrorCode.None)
            tryReleaseMemory(pCapsule.Head.IdMemory);
        return error;
    }

    private static TerbinErrorCode getMemoryStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        // Si es paquete individual, recibe devuelve su PLD.
        if (pCapsule.Head.OrderRequest != TerbinProtocol.FINAL_PACKET)
        {
            pMemory = pCapsule.Payload ?? Array.Empty<byte>();
            return TerbinErrorCode.None;
        }
        return tryAssembleStream(pCapsule, out pMemory);
    }

    private static TerbinErrorCode tryAssembleStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        if (TerbinMemory.TryGetResult(pCapsule.Head.IdMemory, out var bytes) is var r && r.succes)
        {
            pMemory = combinePayload(pCapsule, bytes);
            return TerbinErrorCode.None;
        }
        else
        {
            pMemory = [];
            return r.typeError;
        }
    }

    public static byte[] combinePayload(PacketRequest pCapsule, byte[] pBytes)
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

    private static bool tryReleaseMemory(byte pIdMemory)
    {
        if (pIdMemory > TerbinProtocol.RESERVE_MEMORY)
            return TerbinMemory.Release(pIdMemory);
        return false;
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
                    parameters[1].ParameterType != typeof(byte[]))
                    continue;

                if (method.ReturnType != typeof(Task<PacketRequest>))
                    continue;

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, byte[], Task<PacketRequest>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<PacketRequest>>), method);

                Register(attr.Action, (h, b) => del(h, b));
            }
        }
    }

}
