using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution.Data;
using TerbinLibrary.Memory;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
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
  empieza: mayusculas = publica.
  empieza: minuscula = privada.
 */

/// <summary>
/// ___________________( Español )___________________<br />
/// Despachador de ejecutables que maneja el registro, anulación y ejecución de acciones basadas en secuencias de bytes.<br />
/// Utiliza diccionarios concurrentes para un manejo seguro en hilos.<br />
/// Notas: Implementa <see cref="IExecutableDispatcher"/>.<br />
/// Tips: Evita operaciones largas de bloqueo dentro de los delegados registrados sin el uso del CancellationToken.<br />
/// ___________________( English )___________________<br />
/// Executable dispatcher that handles registration, unregistration, and execution of byte sequence-based actions.<br />
/// Uses concurrent dictionaries for thread-safe handling.<br />
/// Notes: Implements <see cref="IExecutableDispatcher"/>.<br />
/// Tips: Avoid long blocking operations within the registered delegates without the use of the CancellationToken.<br />
/// </summary>
[TODO("Separar esto del Protocolo y hacer un envoltorio que haga estos pero contrlando excepciones, Status, etc... del protocolo")]
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Registra en el despachador una nueva acción asociada a su manejador.<br />
    /// ___________________( English )___________________<br />
    /// Registers a new action along with its handler in the dispatcher.<br />
    /// </summary>
    /// <param name="pAction">Es: Acción ejecutable que se va a registrar. <br />En: Executable action to register.</param>
    /// <param name="pHandler">Es: Delegado que manejará la ejecución de la acción. <br />En: Delegate that will handle the action's execution.</param>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Desregistra y elimina los manejadores asociados a una secuencia de acciones.<br />
    /// ___________________( English )___________________<br />
    /// Unregisters and removes the handlers associated with a sequence of actions.<br />
    /// </summary>
    /// <param name="pActions">Es: Identificador de la acción a eliminar. <br />En: Action identifier to remove.</param>
    /// <returns>Es: True si se eliminó correctamente; False en caso contrario. <br />En: True if removed successfully; False otherwise.</returns>
    public bool Unregister(IEquatable<IEnumerable<byte>> pActions) => _handlers.TryRemove(pActions, out _);

    // DispatchAsync hace demasiadas cosas.
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Procesa de manera asíncrona una petición entrante validando su estado, memoria y la lista de manejadores.<br />
    /// Notas: Maneja cancelación, respuestas sin acción o la ejecución normal de una petición.<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously processes an incoming request by validating its status, memory, and the list of handlers.<br />
    /// Notes: Handles cancellation, actionless responses, or normal request execution.<br />
    /// </summary>
    /// <param name="pHead">Es: Cabecera de la petición recibida. <br />En: Header of the received request.</param>
    /// <param name="pPayload">Es: Carga útil de datos. <br />En: Data payload.</param>
    /// <param name="pActions">Es: Clave que identifica la acción solicitada. <br />En: Key identifying the requested action.</param>
    /// <returns>Es: Respuesta con la información del resultado o null. <br />En: Response with status result info or null.</returns>
    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions)
    {
        if (!_handlers.TryGetValue(pActions, out var handlers))
        {
            if (pActions.Equals(_RESPONSE))
                throw new NotImplementedException("Response!, You must implement 'Response'");

            TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ActionNotFound);
        }

        if (pHead.Status == CodeStatus.Execute) // <- Evitamos hacer todas comprobaciones.
        { /* La gracia esque nunca se compruebe Execute para que sea la predeterminada.*/ }
        else if (pHead.Status == CodeStatus.CheckExecution)
            return InfoResponse.CreateSucces(pHead.IdRequest);

        else if (pHead.Status == CodeStatus.CancelByAction)
            return cancelCts(pActions, pHead.IdRequest);
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

            if (_activeExecutionsByAction.TryGetValue(pActions, out var cts))
                return await TerbinExecutableHelper.ExecutionList(handlers, pHead, pPayload, cts);
            else
                throw new Exception("CancellationTokenNotFound");
        }
        catch (Exception e)
        {
            e.PrintException("CompoundExecutableDispatcher>DispatchAsync");
            byte[] pld = new ExceptionDTO(e, "CompoundExecutableDispatcher>DispatchAsync").Serialize();
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionException, pld);
        }
        finally
        {
            // TerbinMemoryHelper.TryReleaseMemory(pHead.IdMemory);
            //_activeExecutionsByAction.TryRemove(pActions, out _);
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta extraer la entidad y el resto de la memoria a partir del payload dado.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to extract the entity and the rest of the memory from the given payload.<br />
    /// </summary>
    /// <param name="pPayload">Es: El arreglo de bytes original recibido. <br />En: The original byte array received.</param>
    /// <param name="pEntity">Es: La entidad extraída (el primer byte). <br />En: The extracted entity (the first byte).</param>
    /// <param name="pMemory">Es: El resto del arreglo de bytes tras la entidad. <br />En: The remaining byte array after the entity.</param>
    /// <returns>Es: True si el payload es válido y se extrajo; False en caso contrario. <br />En: True if the payload is valid and extracted; False otherwise.</returns>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Registra en lote todos los ejecutables decorados con TerbinExecutableAttribute dentro de un ensamblado.<br />
    /// ___________________( English )___________________<br />
    /// Bulk registers all executables decorated with TerbinExecutableAttribute within an assembly.<br />
    /// </summary>
    /// <param name="pAssembly">Es: Ensamblado que contiene los métodos a registrar. <br />En: Assembly containing the methods to be registered.</param>
    public void RegisterFromAssembly(Assembly pAssembly)
    {
        TerbinExecutableHelper.RegisterFromAssembly<TerbinExecutableAttribute, ExecutableDispatcher>(pAssembly, this);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Cancela el token asociado a una acción o lo reporta como no iniciado.<br />
    /// ___________________( English )___________________<br />
    /// Cancels the token associated with an action or reports it as not initiated.<br />
    /// </summary>
    /// <param name="pActions">Es: Las acciones a identificar. <br />En: The actions to identify.</param>
    /// <param name="pIdRequest">Es: Identificador de la petición. <br />En: The request identifier.</param>
    /// <returns>Es: Respuesta indicando si se procesó la cancelación o no. <br />En: Response indicating validation of cancellation.</returns>
    private InfoResponse cancelCts(IEquatable<IEnumerable<byte>> pActions, ushort pIdRequest)
    {
        if (_activeExecutionsByAction.TryRemove(pActions, out var ctsCancel))
        {
            ctsCancel.Cancel();
            return InfoResponse.CreateSucces(pIdRequest);
        }
        return InfoResponse.Create(pIdRequest, CodeStatus.ActionNotInitiated);
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Administrador estático que facilita el acceso global a un administrador de ejecutables interno (<see cref="ExecutableDispatcher"/>).<br />
/// ___________________( English )___________________<br />
/// Static manager that provides global access to an internal executable dispatcher (<see cref="ExecutableDispatcher"/>).<br />
/// </summary>
public static class TerbinExecutableManager
{
    private static ExecutableDispatcher _dispatcher = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Registra globalmente una nueva acción asociada a su manejador.<br />
    /// ___________________( English )___________________<br />
    /// Globally registers a new action and its handler.<br />
    /// </summary>
    /// <param name="pAction">Es: Acción a registrar. <br />En: Action to register.</param>
    /// <param name="pHandler">Es: Manejador de la acción. <br />En: The action's handler.</param>
    public static void Register(IExecutableAttribute pAction, TerbinExecutableDelegate pHandler) =>
           _dispatcher.Register(pAction, pHandler);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Desregistra globalmente y elimina los manejadores para una secuencia de acciones.<br />
    /// ___________________( English )___________________<br />
    /// Globally unregisters and removes handlers for an action sequence.<br />
    /// </summary>
    /// <param name="pActions">Es: Identificador de la acción a eliminar. <br />En: Identifier to unregister.</param>
    /// <returns>Es: True en caso de éxito o False en caso contrario. <br />En: True on success, False otherwise.</returns>
    public static bool Unregister(IEquatable<IEnumerable<byte>> pActions) =>
           _dispatcher.Unregister(pActions);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Procesa de manera asíncrona una petición en el despachador global.<br />
    /// ___________________( English )___________________<br />
    /// Asynchronously processes a request in the global dispatcher.<br />
    /// </summary>
    /// <param name="pHead">Es: Cabecera. <br />En: Request header.</param>
    /// <param name="pPayload">Es: Carga de datos. <br />En: Data payload.</param>
    /// <param name="pActions">Es: Identificador de la acción. <br />En: Action identifier.</param>
    /// <returns>Es: Respuesta con la información del resultado o null. <br />En: Response with status result info or null.</returns>
    public static async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions) =>
                          await _dispatcher.DispatchAsync(pHead, pPayload, pActions);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Registra globalmente todos los ejecutables decorados en un ensamblado.<br />
    /// ___________________( English )___________________<br />
    /// Globally registers all decorated executables in an assembly.<br />
    /// </summary>
    /// <param name="pAssembly">Es: El ensamblado a examinar. <br />En: The assembly to examine.</param>
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