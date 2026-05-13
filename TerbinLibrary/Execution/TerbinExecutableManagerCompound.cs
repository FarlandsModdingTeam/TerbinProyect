using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution.Collection;


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
    // (byte action, byte subAction), ByteArrayKey
    // Es un object porque solo necesito el Has, al no poner ByteArrayKey me ahorro una conversion en TryGetValue.
    private readonly ConcurrentDictionary<object, List<TerbinExecutableDelegate>> _handlers = new();

    public void Register(IExecutableAttribute pSubAction, TerbinExecutableDelegate pHandler)
    {
        ArgumentNullException.ThrowIfNull(pHandler);
        if (pSubAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pSubAction));

        // Es ahorrarme una linea ya escrita pero me da rabia, ¿Como me ahorro un Add?.
        bool inHandlres = _handlers.TryGetValue(pSubAction.Action, out var listDelegates);
        if (inHandlres)
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            listDelegates.Add(pHandler);
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
        else
        {
            listDelegates = new List<TerbinExecutableDelegate>();
            listDelegates.Add(pHandler);
            // no quitar conversion.
            _handlers.TryAdd((ByteArrayKey)pSubAction.Action, listDelegates);
        }
    }

    public bool Unregister(params byte[] pActions) => _handlers.TryRemove(pActions, out _);

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, params byte[] pActions)
    {
        if (!_handlers.TryGetValue(pActions, out var handlers))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.SubActionNotFound);
        }

        try
        {
            return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload);
        }
        catch (Exception e)
        {
            string exceptionString = $$"""
                [CompoundExecutableDispatcher>DispatchAsync] ExceptionError->
                {
                    Message: {{e.Message}};
                    Source: {{e.Source}};
                    Inner: {{e.InnerException?.Message ?? "N/A"}};
                    Trace: {{e.StackTrace}};
                    String: {{e.ToString()}}
                }
                """;
            Console.WriteLine(exceptionString);
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

    public static bool Unregister(params byte[] pActions) =>
        _dispatcher.Unregister(pActions);

    public static async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, params byte[] pActions) =>
        await _dispatcher.DispatchAsync(pHead, pPayload, pActions);

    public static void RegisterFromAssembly(Assembly pAssembly) =>
        _dispatcher.RegisterFromAssembly(pAssembly);
}