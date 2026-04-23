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
    private readonly ConcurrentDictionary<byte, TerbinExecutableDelegate> _handlers = new();

    public void Register(IExecutableAttribute pSubAction, TerbinExecutableDelegate pHandler)
    {
        if (pHandler is null) throw new ArgumentNullException(nameof(pHandler));
        if (pSubAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pSubAction));
        _handlers[pSubAction.Action[0]] = pHandler;
    }

    public bool Unregister(byte pSubAction) => _handlers.TryRemove(pSubAction, out _);

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload)
    {
        if (!tryGetEntity(pPayload, out var entity, out var memo))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetPaylaodMemory);
        }

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
            Console.WriteLine($"[SubActionExecutableDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionError);
        }
    }

    private static bool tryGetEntity(byte[] pPayload, out byte pEntity, out byte[] pMemory)
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

    public static bool Unregister(byte pAction) =>
        _dispatcher.Unregister(pAction);

    public static async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload) =>
        await _dispatcher.DispatchAsync(pHead, pPayload);

    public static void RegisterFromAssembly(Assembly pAssembly) =>
        _dispatcher.RegisterFromAssembly(pAssembly);
}