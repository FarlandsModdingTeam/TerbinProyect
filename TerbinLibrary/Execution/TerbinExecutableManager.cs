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
    private readonly ConcurrentDictionary<object, List<TerbinExecutableDelegate>> _handlers = new();
    private readonly ConcurrentDictionary<object, CancellationTokenSource> _activeExecutions = new();

    // is a "const"
    private static readonly ByteArrayKey _RESPONSE = new((byte)CodeTerbinProtocol.Response);

    public void Register(IExecutableAttribute pSubAction, TerbinExecutableDelegate pHandler)
    {
        ArgumentNullException.ThrowIfNull(pHandler);
        if (pSubAction.Action.Length <= 0) throw new ArgumentException("No action.", nameof(pSubAction));

        // Es ahorrarme una linea ya escrita pero me da rabia, ¿Como me ahorro un Add?.
        bool inHandlres = _handlers.TryGetValue((ByteArrayKey)pSubAction.Action, out var listDelegates);
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

    public bool Unregister(IEquatable<IEnumerable<byte>> pActions) => _handlers.TryRemove(pActions, out _);

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions)
    {
        if (!_handlers.TryGetValue(pActions, out var handlers))
        {
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

            TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ActionNotFound);
        }

        try
        {
            if (pHead.Status == CodeStatus.Execute) // <- Evitamos hacer todas comprobaciones.
            { /* La gracia esque nunca se compruebe Execute para que sea la predeterminada.*/ }
            else if (pHead.Status == CodeStatus.CheckExecution)
                return InfoResponse.CreateSucces(pHead.IdRequest);
            else if (pHead.Status == CodeStatus.Cancel)
                return null; // TODO: el sistema.

            if (pActions.Equals(_RESPONSE))
            {
                for (int i = 0; i < handlers.Count; i++)
                    _ = handlers[i](pHead, pPayload);
                return null; // Por si alguien hace el bruto.
            }

            return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload);
        }
        catch (Exception e)
        {
            e.PrintException("CompoundExecutableDispatcher>DispatchAsync");
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
        TerbinExecutableHelper.RegisterFromAssembly<TerbinExecutableAttribute, ExecutableDispatcher>(pAssembly, this);
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

public sealed class ExecutableDispatcher_23 : IExecutableDispatcher
{
    private readonly ConcurrentDictionary<object, List<TerbinExecutableDelegate>> _handlers = new();

    // NUEVO: Diccionario para guardar los tokens de cancelación en base al IdRequest.
    // Cambia 'int' por el tipo real de pHead.IdRequest (ej. uint, long, string)
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeExecutions = new();

    private static readonly ByteArrayKey _RESPONSE = new((byte)CodeTerbinProtocol.Response);

    // ... (Register y Unregister se quedan igual) ...

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions)
    {
        // === LÓGICA DE CANCELACIÓN ===
        if (pHead.Status == CodeStatus.Cancel)
        {
            // Intentamos sacar el Source asociado a esta petición y cancelarlo
            if (_activeExecutions.TryRemove(pHead.IdRequest, out var cts))
            {
                cts.Cancel();
                return InfoResponse.CreateSucces(pHead.IdRequest); // O el estado que designes para "Cancelado OK"
            }
            return null; // O un InfoResponse indicando que no se encontró la tarea
        }

        if (!_handlers.TryGetValue(pActions, out var handlers))
        {
            // ... (Tu código actual de advertencias y ActionNotFound) ...
            TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ActionNotFound);
        }

        try
        {
            if (pHead.Status == CodeStatus.CheckExecution)
                return InfoResponse.CreateSucces(pHead.IdRequest);

            if (pActions.Equals(_RESPONSE))
            {
                for (int i = 0; i < handlers.Count; i++)
                    _ = handlers[i](pHead, pPayload, CancellationToken.None);
                return null;
            }

            // === LÓGICA DE EJECUCIÓN CON TOKEN ===
            using var cts = new CancellationTokenSource();

            // Registramos la tarea activa
            _activeExecutions.TryAdd(pHead.IdRequest, cts);

            try
            {
                // Pasamos el token hacia ExecutionList
                return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload, cts.Token);
            }
            finally
            {
                // Asegurarnos de limpiar el diccionario cuando la tarea termine (éxito, error o cancelación)
                _activeExecutions.TryRemove(pHead.IdRequest, out _);
            }
        }
        catch (OperationCanceledException)
        {
            // Opcional: Manejar específicamente si la tarea lanza que fue cancelada internamente
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.Cancel);
        }
        catch (Exception e)
        {
            e.PrintException("CompoundExecutableDispatcher>DispatchAsync");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionException);
        }
    }

    // ... (El resto de la clase se queda igual) ...
}