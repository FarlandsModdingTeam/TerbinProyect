using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;
using TerbinLibrary.Extension;
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

/// <summary>
/// 
/// </summary>
public sealed class SimpleExecutableDispatcher : IExecutableDispatcher
{
    private readonly ConcurrentDictionary<byte, List<TerbinExecutableDelegate>> _handlers = new();

    public void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        if (pAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pAction));

        if (_handlers.TryGetValue(pAction.Action[0], out var listDelegates))
            listDelegates.Add(pHandler);
        else
        {
            listDelegates = new List<TerbinExecutableDelegate>();
            listDelegates.Add(pHandler);
            _handlers.TryAdd(pAction.Action[0], listDelegates);
        }
    }

    public bool Unregister(byte pAction) => _handlers.TryRemove(pAction, out _);


    public async Task<InfoResponse?> DispatchAsync(PacketRequest pCapsule)
    {
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handlers))
        {
            TerbinExecutableHelper.TryReleaseMemory(pCapsule.Head.IdMemory);
            return InfoResponse.Create(pCapsule.Head.IdRequest, CodeStatus.ActionNotFound);
        }

        if (TerbinExecutableHelper.TryGetMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            var error = (r == TerbinErrorCode.MemoryReleaseFailed) ? CodeStatus.ErrorReleaseMemory : CodeStatus.ErrorGetPaylaodMemory;
            return InfoResponse.Create(pCapsule.Head.IdRequest, error);
        }

        try
        {
            Console.WriteLine($"[DispatchAsync] id: {pCapsule.Head.IdMemory}, Order: {pCapsule.Head.OrderRequest}, L: {memo.Length}"); //Encoding.UTF8.GetString(memo)
            if (pCapsule.Head.Status == CodeStatus.CheckExecution)
                return InfoResponse.CreateSucces(pCapsule.Head.IdRequest);

            if (pCapsule.ActionMethod == (byte)CodeTerbinProtocol.Response)
            {
                for (int i = 0; i < handlers.Count; i++)
                {
                    _ = handlers[i](pCapsule.Head, memo);
                }
                return null; // Por si alguien hace el bruto.
            }

            return await TerbinExecutableHelper.ExecutionList(handlers, pCapsule.Head, memo);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ExecutableDispatcher>DispatchAsync] ExceptionError->  {e.Message}");
            return InfoResponse.Create(pCapsule.Head.IdRequest, CodeStatus.ExecutionError);
        }
    }

    public void RegisterFromAssembly(Assembly pAssembly)
    {
        TerbinExecutableHelper.RegisterFromAssembly<TerbinExecutableAttribute, SimpleExecutableDispatcher>(pAssembly, this);
    }
}

public static class TerbinExecutableManager
{
    private static SimpleExecutableDispatcher _dispatcher = new();

    public static void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler) =>
        _dispatcher.Register(pAction, pHandler);

    public static bool Unregister(byte pAction) =>
        _dispatcher.Unregister(pAction);

    public static async Task<InfoResponse?> DispatchAsync(PacketRequest pCapsule) =>
        await _dispatcher.DispatchAsync(pCapsule);

    public static void RegisterFromAssembly(Assembly pAssembly) =>
        _dispatcher.RegisterFromAssembly(pAssembly);
}
