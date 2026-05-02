using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;

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
public sealed class CompoundExecutableDispatcher : IExecutableDispatcher
{
    // TODO: Que la key sea un array de byte.
    private readonly ConcurrentDictionary<(byte action, byte subAction), List<TerbinExecutableDelegate>> _handlers = new();

    public void Register(IExecutableAttribute pSubAction, TerbinExecutableDelegate pHandler)
    {
        if (pHandler is null) throw new ArgumentNullException(nameof(pHandler));
        if (pSubAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pSubAction));

        if (_handlers.TryGetValue((pSubAction.Action[0], pSubAction.Action[1]), out var listDelegates))
            listDelegates.Add(pHandler);
        else
        {
            listDelegates = new List<TerbinExecutableDelegate>();
            listDelegates.Add(pHandler);
            _handlers.TryAdd((pSubAction.Action[0], pSubAction.Action[1]), listDelegates);
        }
    }

    public bool Unregister(byte pAction, byte pSubAction) => _handlers.TryRemove((pAction, pSubAction), out _);

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte pAction, byte pSubAction, byte[] pPayload)
    {
        if (!_handlers.TryGetValue((pAction, pSubAction), out var handlers))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.SubActionNotFound);
        }

        try
        {
            return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SubActionExecutableDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionException);
        }
    }

    public static bool TryGetEntity(byte[] pPayload, out byte pEntity, out byte[] pMemory)
    {
        if (pPayload == null || pPayload.Length == 0)
        {
            pEntity = 0;
            pMemory = Array.Empty<byte>();
            return false;
        }

        pEntity = pPayload[0];
        int bodyLength = pPayload.Length - 1;
        pMemory = new byte[bodyLength];

        if (bodyLength > 0)
            Array.Copy(pPayload, 1, pMemory, 0, bodyLength);

        return true;
    }

    public void RegisterFromAssembly(Assembly pAssembly)
    {
        TerbinExecutableHelper.RegisterFromAssembly<TerbinExecutableCompoundAttribute, CompoundExecutableDispatcher>(pAssembly, this);
    }
}

public static class TerbinExecutableManagerCompound
{
    private static CompoundExecutableDispatcher _dispatcher = new();

    public static void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler) =>
        _dispatcher.Register(pAction, pHandler);

    public static bool Unregister(byte pAction, byte pSubAction) =>
        _dispatcher.Unregister(pAction, pSubAction);

    public static async Task<InfoResponse?> DispatchAsync(Header pHead, byte pAction, byte pSubAction, byte[] pPayload) =>
        await _dispatcher.DispatchAsync(pHead, pAction, pSubAction, pPayload);

    public static void RegisterFromAssembly(Assembly pAssembly) =>
        _dispatcher.RegisterFromAssembly(pAssembly);
}