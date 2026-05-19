using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution.Collection;
using TerbinLibrary.Memory;
using TerbinLibrary.Protocol;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;


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
public sealed class ExecutableDispatcher : IExecutableDispatcher
{
    // (byte action, byte subAction), ByteArrayKey
    // Es un object porque solo necesito el Has, al no poner ByteArrayKey me ahorro una conversion en TryGetValue.
    // Recuerda utilizar un objeto que Implemente un algoritmo para que el HasCode sea igual en los array como el de IdArray.
    private readonly ConcurrentDictionary<IEquatable<IEnumerable<byte>>, List<TerbinExecutableDelegate>> _handlers = new();

    private readonly ConcurrentDictionary<IEquatable<IEnumerable<byte>>, CancellationTokenSource> _activeExecutionsByAction = new();
    //private readonly ConcurrentDictionary<ushort, IEquatable<IEnumerable<byte>>> _activeExecutionsByRequest = new();

    // is a "const"
    private static readonly ByteArrayKey _RESPONSE = new((byte)CodeTerbinProtocol.Response);

    public void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler)
    {
        ArgumentNullException.ThrowIfNull(pHandler);
        if (pAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pAction));

        // Es ahorrarme una linea ya escrita pero me da rabia, ¿Como me ahorro un Add?.
        bool inHandlres = _handlers.TryGetValue((ByteArrayKey)pAction.Action, out var listDelegates);
        if (inHandlres)
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            listDelegates.Add(pHandler);
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
        else
        {
            listDelegates = new List<TerbinExecutableDelegate>();
            listDelegates.Add(pHandler);
            // no quitar conversion.
            _handlers.TryAdd((ByteArrayKey)pAction.Action, listDelegates);
            /*using*/ var cts = new CancellationTokenSource();
            _activeExecutionsByAction.TryAdd((ByteArrayKey)pAction.Action, cts);
        }
    }

    public bool Unregister(IEquatable<IEnumerable<byte>> pActions) => _handlers.TryRemove(pActions, out _);

    // DispatchAsync hace demasiadas cosas.
    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions)
    {
        if (!_handlers.TryGetValue(pActions, out var handlers))
        {
            TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ActionNotFound);
        }

        if (pHead.Status == CodeStatus.Execute) // <- Evitamos hacer todas comprobaciones.
        { /* La gracia esque nunca se compruebe Execute para que sea la predeterminada.*/ }
        else if (pHead.Status == CodeStatus.CheckExecution)
            return InfoResponse.CreateSucces(pHead.IdRequest);

        else if (pHead.Status == CodeStatus.CancelByAction)
            return CancelCts(pActions, pHead.IdRequest);
        else if (pHead.Status == CodeStatus.CancelByRequest)
            return null; // TODO_Verano: el sistema.

        try
        {
            if (pActions.Equals(_RESPONSE))
            {
                for (int i = 0; i < handlers.Count; i++)
                    _ = handlers[i](pHead, pPayload, CancellationToken.None);
                return null; // Por si alguien hace el bruto.
            }

            if (_activeExecutionsByAction.TryRemove(pActions, out var cts))
                return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload, cts);
            else
                throw new Exception("CancellationTokenNotFound");
        }
        catch (Exception e)
        {
            e.PrintException("CompoundExecutableDispatcher>DispatchAsync");
            // TODO: Crear String y mandar de vuelta.
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionException);
        }
        finally
        {
            // TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            //_activeExecutionsByAction.TryRemove(pActions, out _);
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
        TerbinExecutableHelper.RegisterFromAssembly<TerbinExecutableAttribute, ExecutableDispatcher>(pAssembly, this);
    }

    private InfoResponse CancelCts(IEquatable<IEnumerable<byte>> pActions, ushort pIdRequest)
    {
        if (_activeExecutionsByAction.TryRemove(pActions, out var ctsCancel))
        {
            ctsCancel.Cancel();
            return InfoResponse.CreateSucces(pIdRequest);
        }
        return InfoResponse.Create(pIdRequest, CodeStatus.ActionNotInitiated);
    }
}

public static class TerbinExecutableManager
{
    private static ExecutableDispatcher _dispatcher = new();

    public static void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler) =>
           _dispatcher.Register(pAction, pHandler);

    public static bool Unregister(IEquatable<IEnumerable<byte>> pActions) =>
           _dispatcher.Unregister(pActions);

    public static async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions) =>
                          await _dispatcher.DispatchAsync(pHead, pPayload, pActions);

    public static void RegisterFromAssembly(Assembly pAssembly) =>
           _dispatcher.RegisterFromAssembly(pAssembly);
}


/*-- Debug de printear has
            Console.Warn("--(Printeando Has porque handlres no encontrados)--");
            Console.Warn("Has de la accion");
            TSHelper.Debug.PrintHasByT(pActions);
            var keys = _handlers.Keys;
            Console.Warn("PrintHas");
            TSHelper.Debug.PrintHasByT(keys.ToArray());
            Console.Warn("PrintAllHas");
            TSHelper.Debug.PrintAllHas(keys.ToArray());

            Console.Warn("Exodia");
            ByteArrayKey claveBuscada = (ByteArrayKey)pActions;
            Console.WriteLine($"Buscando -> Hash: {claveBuscada.GetHashCode()} | Bytes: {string.Join("-", claveBuscada)}");

            foreach (var kvp in _handlers)
            {
                Console.WriteLine($"En Diccionario -> Hash: {kvp.Key.GetHashCode()} | Bytes: {string.Join("-", kvp.Key)}");
            }

*/