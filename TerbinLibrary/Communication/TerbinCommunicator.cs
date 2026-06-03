using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
// using System.IO.Pipelines;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Id;
using TerbinLibrary.Memory;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerbinLibrary.Communication;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */

/*
- Nada debe bloquear hilo ni ejecucion;
-- TerbinProcotol:
    1 Si cabe en una se manda;
    2 Conseguir memoria;
    3 Recibir id de la memoria a mandar;
    4 Mandar tanda paquetes;

- Send es para mandar y olvidarte (ya lo recibiras en OnRecive);
- Communicate es para mandar sabiendo que quieres recibir una respuesta;
 */


// PaVerano:
// se que si utilizo las propias funciones del servicio para solicitar info faltante o preguntar algo (llamando al cliente)?
// ¿Como hago para que pasa si no exite la funcion? ¿que pasa si excede tiempo? ¿como cancelo su hay que hacerlo?
// ¡¡Como cancelo!!
// ¡¡El CheckExecution solo comprueba si existe los metodos de primer nivel!! no ahonda!
// Seria ideal que al cancelar pudieras revertirlo, al clonar farlands borrarlo, al instalar bepiex desintalarlo, etc.
// Tengo mucho sueño

// TODO: (importante) Mandar al cliente un paquete si hah abido una excepcion.
// └─Un evento que se dispare al recibir una excepcion.

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase principal para la comunicación mediante pipes nombrados de Terbin.<br />
/// Administra la conexión, envío, recepción y encolado de paquetes tanto para cliente como para servidor.<br />
/// Notas: Implementa <see cref="IDisposable"/> para la correcta liberación de recursos.<br />
/// Tips: Puede operar en modo Servidor o Cliente dependiendo de su inicialización.<br />
/// ___________________( English )___________________<br />
/// Main class for communication through Terbin named pipes.<br />
/// Manages connection, sending, receiving, and packet queuing for both client and server.<br />
/// Notes: Implements <see cref="IDisposable"/> for proper resource release.<br />
/// Tips: Can operate in Server or Client mode depending on its initialization.<br />
/// </summary>
[TODO("Hay Muchos, Revisar")]
public class TerbinCommunicator : IDisposable
{
    // ****************************( Variables )**************************** //
    private PipeStream _thePipe;
    private StreamReadStruct _reader;
    private StreamWriteStruct _writer;
    private CancellationToken _stopToken;

    private readonly ConcurrentQueue<PacketRequest> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly ConcurrentDictionary<ushort, (TaskCompletionSource<PacketRequest> Tcs, CancellationTokenSource Cts)> _pendingRequests = new();

    private event Func<PacketRequest, Task<InfoResponse?>>? _onRecive;
    private event Func<Task>? _onNewClientConnect;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Indica si la instancia actual está configurada como servidor.<br />
    /// ___________________( English )___________________<br />
    /// Indicates whether the current instance is configured as a server.<br />
    /// </summary>
    public bool IsServer
    {
        get => field;
        private set => field = value;
    } = false;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Define el tiempo máximo de espera de respuesta en segundos.<br />
    /// ___________________( English )___________________<br />
    /// Defines the maximum response wait time in seconds.<br />
    /// </summary>
    public ushort MaximumResponseTime
    {
        get => field;
        set => field = value;
    } = TerbinProtocol.MAXIMUS_RESPONSE_TIME;

    // ****************************( Getters, Setters e Indexadores )**************************** //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene un valor que indica si el pipe está conectado actualmente.<br />
    /// ___________________( English )___________________<br />
    /// Gets a value indicating whether the pipe is currently connected.<br />
    /// </summary>
    public bool IsConnect => _thePipe?.IsConnected ?? false;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Evento que se dispara cuando se recibe un nuevo paquete válido.<br />
    /// ___________________( English )___________________<br />
    /// Event triggered when a new valid packet is received.<br />
    /// </summary>
    public event Func<PacketRequest, Task<InfoResponse?>>? OnRecive
    {
        add => _onRecive += value;
        remove
        {
            if (value != null)
                _onRecive -= value;
        }
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Evento que se dispara cuando un nuevo cliente se conecta al servidor.<br />
    /// ___________________( English )___________________<br />
    /// Event triggered when a new client connects to the server.<br />
    /// </summary>
    public event Func<Task>? OnNewClientConnect
    {
        add => _onNewClientConnect += value;
        remove
        {
            if (value != null)
                _onNewClientConnect -= value;
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea y devuelve un nuevo pipe de tipo servidor configurado por defecto.<br />
    /// ___________________( English )___________________<br />
    /// Creates and returns a new default-configured server named pipe.<br />
    /// </summary>
    public static NamedPipeServerStream NewTerbinPipe
    {
        get => CreateServerPipe();
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea y devuelve un nuevo pipe de tipo cliente configurado por defecto.<br />
    /// ___________________( English )___________________<br />
    /// Creates and returns a new default-configured client named pipe.<br />
    /// </summary>
    public static NamedPipeClientStream NewClientTerbinPipe
    {
        get => CreateClientPipe();
    }

    // ****************************( Construct )**************************** //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la clase <see cref="TerbinCommunicator"/>.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the <see cref="TerbinCommunicator"/> class.<br />
    /// </summary>
    /// <param name="pIsServer">Es: Indica si debe inicializarse como servidor. <br />En: Indicates whether it should initialize as a server.</param>
    /// <param name="pTokenCancellation">Es: Token de cancelación para detener las tareas en segundo plano. <br />En: Cancellation token to stop background tasks.</param>
    /// <param name="pName">Es: El nombre del pipe. <br />En: The name of the pipe.</param>
    public TerbinCommunicator(bool pIsServer = false, CancellationToken pTokenCancellation = default, string pName = "TerbinPipe")
    {
        IsServer = pIsServer;
        _stopToken = pTokenCancellation;

        if (IsServer)
        {
            _thePipe = CreateServerPipe(pName);
        }
        else
        {
            _thePipe = CreateClientPipe(pName);
        }
        _writer = new StreamWriteStruct(_thePipe);
        _reader = new StreamReadStruct(_thePipe);

        TerbinExecutor.Init(this);
        if (pIsServer)
            _ = manageConnectClient();
    }


    // ****************************( Methods )**************************** //
    // TODO: A mi planta le falta agua.

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta conectarse si no está conectado y no es un servidor.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to connect if not connected and not a server.<br />
    /// </summary>
    /// <returns>Es: Verdadero si logró conectarse, falso en caso contrario. <br />En: True if connected successfully, false otherwise.</returns>
    public async Task<bool> TryConnect()
    {
        if (!IsConnect && !IsServer)
        {
            return await Connect();
        }
        return false;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza la conexión del cliente e inicia las tareas de fondo para envío y recepción.<br />
    /// ___________________( English )___________________<br />
    /// Connects the client and starts background tasks for sending and receiving.<br />
    /// </summary>
    /// <returns>Es: Verdadero si la conexión es exitosa. <br />En: True if connection is successful.</returns>
    public async Task<bool> Connect()
    {
        if (_thePipe is NamedPipeClientStream pipe)
        {
            await pipe.ConnectAsync();
            StartBackgroundTasks();
            return true;
        }
        return false;
    }

    private void StartBackgroundTasks()
    {
        _ = Task.Run(manageReceive, _stopToken);
        _ = Task.Run(manageSend, _stopToken);
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Se comunica usando un InfoPacket estructurado.<br />
    /// Notas: Aún no implementado.<br />
    /// ___________________( English )___________________<br />
    /// Communicates using a structured InfoPacket.<br />
    /// Notes: Not yet implemented.<br />
    /// </summary>
    /// <param name="pInfo">Es: La información encapsulada a enviar. <br />En: The encapsulated info to send.</param>
    public async Task<PacketRequest> CommunicateByInfoPacket(InfoPacket pInfo)
    {
        // Puede.
        throw new NotImplementedException("=> CommunicateByInfoPacket <=");
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Envía un paquete esperando activamente una respuesta.<br />
    /// Se recomienda usar esta función si precisas una confirmación o datos devueltos.<br />
    /// ___________________( English )___________________<br />
    /// Sends a packet actively awaiting a response.<br />
    /// Recommended to use this function if you need confirmation or returned data.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: El método de acción representado por IdArray. <br />En: The action method represented by IdArray.</param>
    /// <param name="pPayload">Es: El contenido de datos a enviar. <br />En: The data payload to send.</param>
    /// <param name="pStatus">Es: El estado de la ejecución a mandar. <br />En: The execution status to send.</param>
    /// <param name="pId">Es: El identificador del paquete, o nulo para autogenerarlo. <br />En: The packet ID, or null to auto-generate.</param>
    /// <returns>Es: El paquete respuesta recibido. <br />En: The response packet received.</returns>
    public async Task<PacketRequest> Communicate(IdArray pActionMethod, byte[] pPayload, CodeStatus pStatus = CodeStatus.Execute, ushort? pId = null)
    {
        ushort id = pId ?? MiniID.NewS;
        PacketRequest? p = await send(pActionMethod, pPayload, pStatus, id);

        if (p != null)
            return p.Value;

        var reply = await recuperateReply(id);
        return reply;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Envía un paquete sin esperar respuesta.<br />
    /// Si ocurre un error de fragmentación devolverá un paquete de error, de lo contrario devuelve null.<br />
    /// ___________________( English )___________________<br />
    /// Sends a packet without waiting for a response (fire and forget).<br />
    /// If a fragmentation error occurs it will return an error packet, otherwise returns null.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Métodos o acciones en el sistema. <br />En: Methods or actions in the system.</param>
    /// <param name="pPayload">Es: El cuerpo de los datos que quieres mandar. <br />En: The data body you want to send.</param>
    /// <param name="pStatus">Es: Estado opcional del código a mandar. <br />En: Optional code status to send.</param>
    /// <param name="pId">Es: ID de paquete explícito. <br />En: Explicit packet ID.</param>
    public async Task<PacketRequest?> Send(IdArray pActionMethod, byte[] pPayload, CodeStatus pStatus = CodeStatus.Execute, ushort? pId = null)
    {
        ushort id = pId ?? MiniID.NewS;
        return await send(pActionMethod, pPayload, pStatus, id);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Forma sencilla de enviar bytes individuales con una acción específica.<br />
    /// ___________________( English )___________________<br />
    /// Simple way to send individual bytes with a specific action.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Array de acciones. <br />En: Array of actions.</param>
    /// <param name="pStatus">Es: Codigo de estado enviando. <br />En: Status code being sent.</param>
    /// <param name="pId">Es: Id del paquete. <br />En: Packet ID.</param>
    /// <param name="pPayload">Es: Bytes de datos pasados como params. <br />En: Data bytes passed as params.</param>
    public async Task<PacketRequest?> SendBytes(IdArray pActionMethod,CodeStatus pStatus = CodeStatus.Execute, ushort? pId = null, params byte[] pPayload)
    {
        ushort id = pId ?? MiniID.NewS;
        return await send(pActionMethod, pPayload, pStatus, id);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lógica interna compartida para el envío general de paquetes.<br />
    /// Administra si se enviará como un solo paquete o fragmentado.<br />
    /// ___________________( English )___________________<br />
    /// Internal shared logic for general packet sending.<br />
    /// Manages whether to send it as a single packet or fragmented.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Los identificadores de la acción. <br />En: The action identifiers.</param>
    /// <param name="pPayload">Es: El contenido del mensaje (payload). <br />En: The message content (payload).</param>
    /// <param name="pStatus">Es: Estado a configurar. <br />En: Status to configure.</param>
    /// <param name="pId">Es: Identificador forzado del requerimiento. <br />En: Forced request identifier.</param>
    public async Task<PacketRequest?> send(IdArray pActionMethod, byte[] pPayload, CodeStatus pStatus, ushort pId)
    {
        PacketRequest? error = null;
        if (pPayload.Length <= TerbinProtocol.MAX_PLD)
            _ = HandleSendSigle(pActionMethod, pPayload, pId, pStatus);
        else
            error = await HandleSendFragment(pActionMethod, pPayload, pId, pStatus);
        return error; // Devuelve null si todo esta correcto.
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Maneja el envío de paquetes que no requieren fragmentación porque su tamaño es adecuado.<br />
    /// ___________________( English )___________________<br />
    /// Handles sending packets that do not require fragmentation because their size is suitable.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Acción a ejecutar. <br />En: Action to execute.</param>
    /// <param name="pPayload">Es: Los datos en bytes. <br />En: The data in bytes.</param>
    /// <param name="pIdRequest">Es: Identificación de la petición. <br />En: Request identifier.</param>
    /// <param name="pStatus">Es: Estado de la petición a enviar. <br />En: Request status to send.</param>
    public async Task/*<TerbinErrorCode>*/ HandleSendSigle(IdArray pActionMethod, byte[] pPayload, ushort pIdRequest, CodeStatus pStatus)
    {
        await addQueue(
            TerbinProtocol.ORDER_SINGLE,
            pStatus,
            pActionMethod,
            (byte)CodeTerbinMemory.NotAsign,
            pPayload,
            pIdRequest);
        //return TerbinErrorCode.None;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Maneja el envío de paquetes demasiado grandes que requieren ser fragmentados en porciones usando memoria.<br />
    /// ___________________( English )___________________<br />
    /// Handles sending packets that are too large and require fragmentation using memory portions.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Acción o método a procesar. <br />En: Action or method to process.</param>
    /// <param name="pPayload">Es: Bytes extensos a mandar. <br />En: Extensive bytes to send.</param>
    /// <param name="pIdRequest">Es: Identificación base de la petición. <br />En: Base identifier for the request.</param>
    /// <param name="pStatus">Es: Estado a proporcionar en la petición. <br />En: Status to provide in the request.</param>
    public async Task<PacketRequest?> HandleSendFragment(IdArray pActionMethod, byte[] pPayload, ushort pIdRequest, CodeStatus pStatus)
    {
        var check = await Communicate(pActionMethod, [], CodeStatus.CheckExecution);
        if (check.Head.Status != CodeStatus.Succes)
            return check;

        var request = await SoliciteRequestMemory();
        if (request.Head.Status != CodeStatus.Succes || request.Payload.Length <= 0)
            return request;

        byte idMemory = request.Payload[0];
        ushort currentPacketIndex = 1;

        while (pPayload.Length > TerbinProtocol.MAX_PLD)
        {
            if (currentPacketIndex >= TerbinProtocol.FINAL_PACKET - 1)
            {
                return PacketRequest.CreateResponseError(pIdRequest, CodeStatus.OverMaximunPacket);
            }

            byte[] fragmentPayload = pPayload[..TerbinProtocol.FRAGMENT_IN];
            pPayload = pPayload[TerbinProtocol.FRAGMENT_IN..];

            await Load(currentPacketIndex, idMemory, fragmentPayload, pIdRequest);
            currentPacketIndex++;
        }
        await addQueue(TerbinProtocol.FINAL_PACKET, pStatus, pActionMethod, idMemory, pPayload, pIdRequest);
        return null;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Solicita de manera remota un ID de memoria para preparar envíos fragmentados.<br />
    /// ___________________( English )___________________<br />
    /// Remotely requests a memory ID to prepare fragmented sends.<br />
    /// </summary>
    public async Task<PacketRequest> SoliciteRequestMemory()
    {
        ushort idR = MiniID.NewS;
        await addQueue(TerbinProtocol.ORDER_SINGLE, CodeStatus.Execute, new IdArray((byte)CodeTerbinProtocol.Solicit), (byte)CodeTerbinMemory.New, [], idR);

        var r = await recuperateReply(idR);
        return r;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Carga una sección de bytes fragmentados a la memoria previamente solicitada.<br />
    /// ___________________( English )___________________<br />
    /// Loads a section of fragmented bytes into previously requested memory.<br />
    /// </summary>
    /// <param name="pOrderRequest">Es: Orden del paquete actual para su ensamblado. <br />En: Current packet order for assembly.</param>
    /// <param name="pIdMemory">Es: ID de la memoria destino en el otro extremo. <br />En: Destination memory ID on the other end.</param>
    /// <param name="pPayload">Es: El fragmento de payload a subir. <br />En: The payload fragment to upload.</param>
    /// <param name="pIdRequest">Es: El identificador general de la solicitud original. <br />En: General identifier of the original request.</param>
    [TODO("Returna false cuando alomejor deneria meter una excepcion")]
    public async Task<bool> Load(
                ushort pOrderRequest,
                byte pIdMemory,
                byte[] pPayload,
                ushort? pIdRequest = null)
    {
        pIdRequest ??= MiniID.NewS;
        if (pPayload.Length >= TerbinProtocol.MAX_PLD)
            return false; // TODO: que false ni ostia, metele una excepcion.

        await addQueue(pOrderRequest, CodeStatus.Execute, new IdArray((byte)CodeTerbinProtocol.Load), pIdMemory, pPayload, pIdRequest.Value);
        return true;
    }

    private async Task<PacketRequest> recuperateReply(ushort pId)
    {
        var tcs = new TaskCompletionSource<PacketRequest>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(MaximumResponseTime));

        if (!_pendingRequests.TryAdd(pId, (tcs, cts)))
        {
            cts.Dispose();
            return PacketRequest.CreateResponseError(pId, CodeStatus.AlreadyExistsPetition);
        }


        cts.Token.Register(() =>
        {
            // Intentamos sacar la petición del diccionario
            if (_pendingRequests.TryRemove(pId, out var removedTcs))
            {
                var timeoutHeader = new Header(pIdRequest: pId, pOrderRequest: TerbinProtocol.ORDER_SINGLE, pStatus: CodeStatus.OverMaximumTime);
                var timeoutPacket = new PacketRequest(pHead: timeoutHeader, (IdArray?)null, (byte[]?)null);
                removedTcs.Tcs.TrySetResult(timeoutPacket);
                removedTcs.Cts.Dispose();
            }
        });

        return await tcs.Task;
    }

    private async Task handleReceive(PacketRequest pCapsule)
    {
        if (_onRecive == null)
            return;

        Console.Warn($"Packet: {pCapsule}");
        if (TerbinMemoryHelper.TryGetMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            var error = (r == TerbinErrorCode.MemoryReleaseFailed) ? CodeStatus.ErrorReleaseMemory : CodeStatus.ErrorGetPaylaodMemory;
            await Reply(InfoResponse.Create(pCapsule.Head.IdRequest, error));
            return;
        }
        pCapsule.Payload = memo;

        InfoResponse? rCap = await _onRecive.Invoke(pCapsule);
        if (rCap != null)
            await Reply(rCap.Value);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Entrega la respuesta recibida a la tarea que la estaba esperando.<br />
    /// ___________________( English )___________________<br />
    /// Delivers the received response to the task that was waiting for it.<br />
    /// </summary>
    /// <param name="pCapsule">Es: Paquete devuelto que cumple con la solicitud. <br />En: Returned packet resolving the request.</param>
    public void GiveResponse(PacketRequest pCapsule)
    {
        if (_pendingRequests.TryRemove(pCapsule.Head.IdRequest, out var entry))
        {
            entry.Tcs.TrySetResult(pCapsule);
            entry.Cts.Dispose();
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Prolonga o cancela el contador de tiempo de espera para una petición pendiente.<br />
    /// ___________________( English )___________________<br />
    /// Prolongs or resets the timeout counter for a pending request.<br />
    /// </summary>
    /// <param name="pIdRequest">Es: Identificador de la petición. <br />En: Identifier for the request.</param>
    public void GiveProlong(ushort pIdRequest)
    {
        if (_pendingRequests.TryGetValue(pIdRequest, out var entry))
        {
            entry.Cts.CancelAfter(TimeSpan.FromSeconds(MaximumResponseTime));
        }
    }

    // --- Reply --- //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Responde formalmente a una petición utilizando la estructura de formato InfoResponse.<br />
    /// ___________________( English )___________________<br />
    /// Formally replies to a request using the InfoResponse format structure.<br />
    /// </summary>
    /// <param name="pInfo">Es: Objeto estructurado de respuesta. <br />En: Structured response object.</param>
    public async Task Reply(InfoResponse pInfo)
    {
        await send(pInfo.ActionMethod, pInfo.Payload, pInfo.Status, pInfo.IdRequest);
    }


    // --- Queue --- //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Agrega un paquete directamente a la cola de envíos tras configurarlo utilizando parámetros base.<br />
    /// ___________________( English )___________________<br />
    /// Adds a packet directly to the send queue after configuring it from base parameters.<br />
    /// </summary>
    /// <param name="pOrderRequest">Es: Orden del comando. <br />En: Command order.</param>
    /// <param name="pStatus">Es: Codigo de estado aplicable.. <br />En: Applicable status code.</param>
    /// <param name="pActionMethod">Es: Metodo o acción destino. <br />En: Destined method or action.</param>
    /// <param name="pIdMemory">Es: Identificador de memoria. <br />En: Memory identifier.</param>
    /// <param name="pSectionPayload">Es: Bytes a escribir. <br />En: Bytes to write.</param>
    /// <param name="pIdRequest">Es: Identificador del paquete. <br />En: Packet identifier.</param>
    public async Task addQueue(
                ushort pOrderRequest,
                CodeStatus pStatus,
                IdArray pActionMethod,
                byte pIdMemory,
                byte[] pSectionPayload,
                ushort pIdRequest)
    {
        Header head = new Header(
            pIdRequest: pIdRequest,
            pOrderRequest: pOrderRequest,
            pIdMemory: pIdMemory,
            pStatus: pStatus);
        PacketRequest capsule = new PacketRequest(
            pHead: head,
            pActionMethod: pActionMethod,
            pPayload: pSectionPayload);
        await addQueue(capsule);
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Ingresa un paquete formateado en la cola para su posterior envío de forma asíncrona.<br />
    /// ___________________( English )___________________<br />
    /// Queues a formatted packet for later sending asynchronously.<br />
    /// </summary>
    /// <param name="pCapsule">Es: Paquete ya formado a guardar. <br />En: Formed packet to store.</param>
    public async Task addQueue(PacketRequest pCapsule)
    {
        Console.Log($"Packet: {pCapsule}");
        _queue.Enqueue(pCapsule);
        _signal.Release();
    }

    // --- Manages --- //
    private async Task manageReceive()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            try
            {
                PacketRequest r = await _reader.ReadAsycn<PacketRequest>(_stopToken);
                Console.Log($"Packet: {r}");
                if (_stopToken.IsCancellationRequested)
                    break;
                _ = handleReceive(r);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                e.PrintException("TerbinCommunicator>manageReceive");
                break;
            }
        }
    }
    private async Task manageSend()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(_stopToken);

                if (!_queue.TryDequeue(out PacketRequest data))
                    continue;

                if (_stopToken.IsCancellationRequested)
                    break;
                Console.Log($"Packet: {data}");
                await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine($"[Communicator] El cliente se ha desconectado limpiamente.");
                break;
            }
            catch (Exception e)
            {
                e.PrintException("TerbinCommunicator>manageSend");
                break;
            }
            finally
            {
                //_signal.Release();
            }
        }
    }

    private async Task manageConnectClient()
    {
        if (_thePipe is NamedPipeServerStream pipe)
        {
            await pipe.WaitForConnectionAsync(_stopToken);
            //_ = Task.Run(_onNewClientConnect?.Invoke(), _stopToken);
            StartBackgroundTasks();
            _onNewClientConnect?.Invoke();
        }
    }


    // ****************************( Helps )**************************** //
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Metodo de ayuda para facilitar la creación sencilla de un Pipe Servidor.<br />
    /// ___________________( English )___________________<br />
    /// Helper method to easily create a Server Named Pipe.<br />
    /// </summary>
    /// <param name="pName">Es: El nombre asigando al pipe. <br />En: The name assigned to the pipe.</param>
    public static NamedPipeServerStream CreateServerPipe(string pName = "TerbinPipe")
    {
        return new NamedPipeServerStream(
                pName,
                PipeDirection.In | PipeDirection.Out,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Metodo de ayuda para facilitar la creación sencilla de un Pipe Cliente.<br />
    /// ___________________( English )___________________<br />
    /// Helper method to easily create a Client Named Pipe.<br />
    /// </summary>
    /// <param name="pName">Es: El nombre asigando al pipe. <br />En: The name assigned to the pipe.</param>
    public static NamedPipeClientStream CreateClientPipe(string pName = "TerbinPipe")
    {
        return new NamedPipeClientStream(
                ".",
                pName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
    }

    // ****************************( Implement IDisposable )**************************** //
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool pDisposing)
    {
        if (_disposed)
            return;

        if (pDisposing)
        {
            liberateAdministered();
        }
        liberateNotAdministered();

        _disposed = true;
    }

    protected virtual void liberateAdministered()
    {
        // Liberar recursos administrados.
        _thePipe?.Dispose();
        _reader.Dispose();
        _writer.Dispose();

    }
    protected virtual void liberateNotAdministered()
    {
        // Liberar recursos NO administrados aquí (si los hubiera).

    }

    ~TerbinCommunicator()
    {
        Dispose(false);
    }
}

