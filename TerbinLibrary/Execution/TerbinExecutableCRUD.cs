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


// TODO: CRUD no deberia manejar memoria, eso lo hace TerbinExecutable normal.
// TODO: Que no esta jarcodeado el CRUD y que pueda ser cualquier byte.

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

    // TODO: ya tengo el pAction, como tal no hay que pasarlo, (areglar).
    public async Task<PacketRequest> DispatchAsync(Header pHead, CodeTerbinProtocol pAction, byte[] pPayload)
    {
        tryGetEntity(pPayload, out var entity, out var memo);

        if (!_handlers.TryGetValue(entity, out var handler))
        {
            pHead.Status = CodeStatus.ActionNotFound;
            return new PacketRequest(pHead, (byte)pAction, (byte)CodeTerbinProtocol.None, pPayload);
        }

        try
        {
            return await handler(pHead, memo).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TerbinExecutableCRUDDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            pHead.Status = CodeStatus.ExecutionError;
            return new PacketRequest(pHead, (byte)pAction, (byte)CodeTerbinProtocol.None, pPayload);
        }
    }

    private static bool tryGetEntity(byte[] pPayload, out byte pEntity, out byte[] pMemory)
    {
        pEntity = pPayload[0];
        int bodyLength = pPayload.Length - 1;
        pMemory = new byte[bodyLength];
        if (bodyLength > 0)
            Array.Copy(pPayload, 1, pMemory, 0, bodyLength);
        return true;
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

    public static async Task<PacketRequest> DispatchAsync(Header pHead, CodeTerbinProtocol pAction, byte[] pPayload)
    {
        if (!_dispatchers.TryGetValue(pAction, out var dispatcher))
        {
            pHead.Status = CodeStatus.ActionNotFound;
            var capsule = new PacketRequest(pHead, (byte)pAction, (byte)CodeTerbinProtocol.None, pPayload);
            return capsule;
        }
        return await dispatcher.DispatchAsync(pHead, pAction, pPayload);
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
                    parameters[1].ParameterType != typeof(byte[]))
                    continue;

                if (method.ReturnType != typeof(Task<PacketRequest>))
                    continue;

                var del = (Func<Header, byte[], Task<PacketRequest>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<PacketRequest>>), method);

                Register(attr.Action, attr.Entity, (h, b) => del(h, b));
            }
        }
    }
}