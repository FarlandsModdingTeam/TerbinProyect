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

/// <summary>
/// Dispatcher instanciable para una acción CRUD concreta.
/// La clave interna es Entity (primer byte del payload).
/// </summary>
/*
public sealed class TerbinExecutableCRUDDispatcher : IExecutableDispatcher
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
    public async Task<InfoResponse?> DispatchAsync(Header pHead, CodeTerbinProtocol pAction, byte[] pPayload)
    {
        tryGetEntity(pPayload, out var entity, out var memo);

        if (!_handlers.TryGetValue(entity, out var handler))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.SubActionNotFound);
        }

        try
        {
            return await handler(pHead, memo).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[TerbinExecutableCRUDDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionError);
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

    private static bool isCrudAction(CodeTerbinProtocol pAction) =>
        pAction is CodeTerbinProtocol.Create
            or CodeTerbinProtocol.Read
            or CodeTerbinProtocol.Update
            or CodeTerbinProtocol.Deleted;
}

// TODO: Renombrar a TerbinExecutableExtraManager.
// TODO: Que admita cuaquier tipo de metodo no solo CRUD.
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

    public static async Task<InfoResponse?> DispatchAsync(Header pHead, CodeTerbinProtocol pAction, byte[] pPayload)
    {
        if (!_dispatchers.TryGetValue(pAction, out var dispatcher))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.SubActionNotFound);
        }
        return await dispatcher.DispatchAsync(pHead, pAction, pPayload);
    }

    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attrs = method.GetCustomAttributes<TerbinCRUDAttribute>(inherit: false);
                if (!attrs.Any()) continue;

                var parameters = method.GetParameters();
                if (!TerbinExecutableHelper.IsFirmParameters(parameters))
                    continue;

                if (!TerbinExecutableHelper.IsFirmReturn(method))
                    continue;

                var del = (Func<Header, byte[], Task<InfoResponse?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<InfoResponse?>>), method);

                foreach (var attr in attrs)
                {
                    Register(attr.Action, attr.Entity, (h, b) => del(h, b));
                }
            }
        }
    }
}*/