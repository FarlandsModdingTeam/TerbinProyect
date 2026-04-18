using System;
using System.Collections.Concurrent;
using System.Reflection;
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

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TerbinCRUDAttribute : Attribute
{
    /// <summary>
    /// Create, Read, Update, Deleted
    /// </summary>
    public CodeTerbinProtocol Action { get; }
    public byte Entity { get; }

    public TerbinCRUDAttribute(CodeTerbinProtocol pAction, byte pEntity)
    {
        Action = pAction;
        Entity = pEntity;
    }
}

/// <summary>
/// Dispatcher instanciable para una acción CRUD concreta.
/// La clave interna es Entity (primer byte del payload).
/// </summary>
public sealed class TerbinExecutableCRUDDispatcher
{
    private readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();

    public CodeTerbinProtocol Action { get; }

    public TerbinExecutableCRUDDispatcher(CodeTerbinProtocol pAction)
    {
        if (!isCrudAction(pAction))
            throw new ArgumentOutOfRangeException(nameof(pAction), "Action must be CRUD.");
        Action = pAction;
    }

    public void Register(byte pEntity, ExecutableHandler pHandler)
    {
        if (pHandler is null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pEntity] = pHandler;
    }

    public bool Unregister(byte pEntity) => _handlers.TryRemove(pEntity, out _);

    public async Task<PacketRequest> DispatchAsync(PacketRequest pCapsule)
    {
        if (!tryGetEntityAndMemoryStream(pCapsule, out var entity, out var memo))
        {
            pCapsule.Head.Status = CodeStatus.BadRequest;
            return pCapsule;
        }

        if (!_handlers.TryGetValue(entity, out var handler))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            return pCapsule;
        }

        try
        {
            return await handler(pCapsule.Head, memo).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TerbinExecutableCRUDDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            pCapsule.Head.Status = CodeStatus.ExecutionError;
            return pCapsule;
        }
    }

    private static bool tryGetEntityAndMemoryStream(PacketRequest pCapsule, out byte pEntity, out MemoryStream pMemory)
    {
        pEntity = byte.MinValue;
        pMemory = (MemoryStream)MemoryStream.Null;

        if (!tryGetPayloadBytes(pCapsule, out var bytes))
            return false;

        if (bytes.Length == 0)
            return false;

        pEntity = bytes[0];
        var bodyLength = bytes.Length - 1;
        pMemory = new MemoryStream(bytes, 1, bodyLength, writable: false, publiclyVisible: true);
        return true;
    }

    private static bool tryGetPayloadBytes(PacketRequest pCapsule, out byte[] pBytes)
    {
        pBytes = [];

        if (pCapsule.Head.OrderRequest != TerbinProtocol.FINAL_PACKET)
        {
            pBytes = pCapsule.Payload ?? [];
            tryReleaseMemory(pCapsule.Head.IdMemory);
            return true;
        }

        if (TerbinMemory.TryGetResult(pCapsule.Head.IdMemory, out var buffered) is var r && r.succes)
        {
            var last = pCapsule.Payload ?? [];
            pBytes = combinePayload(buffered, last); // buffered primero, last al final
            tryReleaseMemory(pCapsule.Head.IdMemory);
            return true;
        }

        tryReleaseMemory(pCapsule.Head.IdMemory);
        return false;
    }

    private static byte[] combinePayload(byte[] pBuffered, byte[] pLast)
    {
        var result = new byte[pBuffered.Length + pLast.Length];
        Buffer.BlockCopy(pBuffered, 0, result, 0, pBuffered.Length);
        Buffer.BlockCopy(pLast, 0, result, pBuffered.Length, pLast.Length);
        return result;
    }

    private static void tryReleaseMemory(byte pIdMemory)
    {
        if (pIdMemory > TerbinProtocol.RESERVE_MEMORY)
            _ = TerbinMemory.Release(pIdMemory);
    }

    private static bool isCrudAction(CodeTerbinProtocol pAction) =>
        pAction is CodeTerbinProtocol.Create
            or CodeTerbinProtocol.Read
            or CodeTerbinProtocol.Update
            or CodeTerbinProtocol.Deleted;
}

/// <summary>
/// Manager estático que enruta las 4 acciones CRUD.
/// </summary>
public static class TerbinExecutableCRUDManager
{
    private static readonly IReadOnlyDictionary<CodeTerbinProtocol, TerbinExecutableCRUDDispatcher> _dispatchers =
        new Dictionary<CodeTerbinProtocol, TerbinExecutableCRUDDispatcher>
        {
            [CodeTerbinProtocol.Create] = new(CodeTerbinProtocol.Create),
            [CodeTerbinProtocol.Read] = new(CodeTerbinProtocol.Read),
            [CodeTerbinProtocol.Update] = new(CodeTerbinProtocol.Update),
            [CodeTerbinProtocol.Deleted] = new(CodeTerbinProtocol.Deleted),
        };

    public static void Register(CodeTerbinProtocol pAction, byte pEntity, ExecutableHandler pHandler)
    {
        if (!_dispatchers.TryGetValue(pAction, out var dispatcher))
            throw new ArgumentOutOfRangeException(nameof(pAction), "Action must be CRUD.");

        dispatcher.Register(pEntity, pHandler);
    }

    public static bool Unregister(CodeTerbinProtocol pAction, byte pEntity)
    {
        if (!_dispatchers.TryGetValue(pAction, out var dispatcher))
            return false;

        return dispatcher.Unregister(pEntity);
    }

    public static Task<PacketRequest> DispatchAsync(PacketRequest pCapsule)
    {
        var action = (CodeTerbinProtocol)pCapsule.ActionMethod;
        if (!_dispatchers.TryGetValue(action, out var dispatcher))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            return Task.FromResult(pCapsule);
        }

        return dispatcher.DispatchAsync(pCapsule);
    }

    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<TerbinCRUDAttribute>(inherit: false);
                if (attr is null) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(Header) ||
                    parameters[1].ParameterType != typeof(MemoryStream))
                    continue;

                if (method.ReturnType != typeof(Task<PacketRequest>))
                    continue;

                var del = (Func<Header, MemoryStream, Task<PacketRequest>>)Delegate.CreateDelegate(
                    typeof(Func<Header, MemoryStream, Task<PacketRequest>>), method);

                Register(attr.Action, attr.Entity, (h, b) => del(h, b));
            }
        }
    }
}