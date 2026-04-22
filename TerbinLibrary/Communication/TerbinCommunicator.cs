using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
// using System.IO.Pipelines;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TerbinLibrary;
using TerbinLibrary.Execution;
using TerbinLibrary.Id;
using TerbinLibrary.Memory;
using TerbinLibrary.Serialize;

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

// TODO: Revisar si una respuesta manda IdMemory en None o Undefined pero payload es mas largo, cambiar eso para mandar en tanda.
// TODO: Llamar a si mismo.

public class TerbinCommunicator : IDisposable
{
    // ****************************( Variables )**************************** //
    private PipeStream _thePipe;
    private StreamReadStruct _reader;
    private StreamWriteStruct _writer;
    private CancellationToken _stopToken;

    private readonly ConcurrentQueue<PacketRequest> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly ConcurrentDictionary<ushort, TaskCompletionSource<PacketRequest>> _pendingRequests = new();

    private event Func<PacketRequest, Task<PacketRequest?>>? _onRecive;
    private event Func<Task>? _onNewClientConnect;

    public bool IsServer
    {
        get => field;
        private set => field = value;
    } = false;

    public ushort MaximumResponseTime
    {
        get => field;
        set => field = value;
    } = TerbinProtocol.MAXIMUS_RESPONSE_TIME;

    // ****************************( Getters, Setters e Indexadores )**************************** //
    public bool IsConnect => _thePipe?.IsConnected ?? false;

    public event Func<PacketRequest, Task<PacketRequest?>>? OnRecive
    {
        add => _onRecive += value;
        remove
        {
            if (value != null)
                _onRecive -= value;
        }
    }
    public event Func<Task>? OnNewClientConnect
    {
        add => _onNewClientConnect += value;
        remove
        {
            if (value != null)
                _onNewClientConnect -= value;
        }
    }

    public static NamedPipeServerStream NewTerbinPipe
    {
        get => CreateServerPipe();
    }
    public static NamedPipeClientStream NewClientTerbinPipe
    {
        get => CreateClientPipe();
    }

    // ****************************( Construct )**************************** //
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

        TerbinExecutor.Init();
        if (pIsServer)
            _ = manageConnectClient();
    }


    // ****************************( Methods )**************************** //
    // TODO: A mi planta le falta agua.

    public async Task<bool> TryConnect()
    {
        if (!IsConnect && !IsServer)
        {
            return await Connect();
        }
        return false;
    }

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

    public async Task<PacketRequest> Communicate(byte pActionMethod, MemoryStream pPayload)
    {
        return await Communicate(pActionMethod, pPayload.ToArray());
    }
    public async Task<PacketRequest> Communicate(byte pActionMethod, byte[] pPayload)
    {
        ushort id = MiniID.NewS;
        PacketRequest reply;
        if (pPayload.Length <= TerbinProtocol.MAX_PLD)
            reply = (PacketRequest) await HandleSendSigle(pActionMethod, pPayload, id, CodeStatus.Execute, true);
        else
            reply = (PacketRequest) await HandleSendFragment(pActionMethod, pPayload, id, CodeStatus.Execute, true);
        return reply;
    }

    public async Task<ushort> Send(byte pActionMethod, MemoryStream pPayload)
    {
        return await Send(pActionMethod, pPayload.ToArray());
    }
    public async Task<ushort> Send(byte pActionMethod, byte[] pPayload)
    {
        ushort id = MiniID.NewS;
        if (pPayload.Length <= TerbinProtocol.MAX_PLD)
            _ = HandleSendSigle(pActionMethod, pPayload, id, CodeStatus.Execute, false);
        else
            _ = HandleSendFragment(pActionMethod, pPayload, id, CodeStatus.Execute, false);
        return id;
    }

    public async Task<PacketRequest?> HandleSendSigle(byte pActionMethod, byte[] pPayload, ushort pIdRequest, CodeStatus pStatus, bool pRecuperate = true)
    {
        await AddQueue(
            TerbinProtocol.ORDER_SINGLE,
            pStatus,
            pActionMethod,
            (byte)CodeTerbinMemory.NotAsign,
            pPayload,
            pIdRequest);

        if (!pRecuperate)
            return null;

        // TODO: gestionar error.
        var reply = await recuperateReply(pIdRequest);
        if (reply.typeError != TerbinErrorCode.None) Console.WriteLine($"[HandleSendSigle] recuperate {reply.typeError}");
        return reply.packet;
    }

    // TODO: Controlar respuesta de guardar datos.
    public async Task<PacketRequest?> HandleSendFragment(byte pActionMethod, byte[] pPayload, ushort pIdRequest, CodeStatus pStatus, bool pRecuperate = true)
    {
        var request = await soliciteRequestMemory();
        if (request.typeError != TerbinErrorCode.None)
        {
            request.packet.Payload = Serialineitor.Serialize<ushort>((ushort)request.typeError);
            return request.packet;
        }

        byte idMemory = request.packet.Head.IdMemory;
        ushort currentPacketIndex = 1;

        while (pPayload.Length > TerbinProtocol.MAX_PLD)
        {
            if (currentPacketIndex >= TerbinProtocol.FINAL_PACKET - 1)
            {
                // TODO: mandar respuesta en vez de excepcion.
                throw new Exception("¡currentPacketIndex >= TerbinProtocol.FINAL_PACKET - 1!");
            }

            byte[] fragmentPayload = pPayload[..TerbinProtocol.FRAGMENT_IN];
            pPayload = pPayload[TerbinProtocol.FRAGMENT_IN..];

            await AddQueue(currentPacketIndex, CodeStatus.Execute, (byte)CodeTerbinProtocol.Load, idMemory, fragmentPayload, pIdRequest);
            currentPacketIndex++;
        }


        await AddQueue(TerbinProtocol.FINAL_PACKET, pStatus, pActionMethod, idMemory, pPayload, pIdRequest);

        if (!pRecuperate)
            return null;

        var reply = await recuperateReply(pIdRequest);
        if (reply.typeError != TerbinErrorCode.None) Console.WriteLine($"[HandleSendFragment] recuperate {reply.typeError}");
        return reply.packet;
    }

    private async Task<(PacketRequest packet, TerbinErrorCode typeError)> soliciteRequestMemory()
    {
        ushort idR = MiniID.NewS;
        await AddQueue(0, CodeStatus.Execute, (byte)CodeTerbinProtocol.Solicit, (byte)CodeTerbinMemory.New, [], idR);

        var r = await recuperateReply(idR);
        return r;
    }

    [Obsolete]
    private async Task<byte> soliciteMemory()
    {
        ushort idR = MiniID.NewS;
        await AddQueue(0, CodeStatus.Execute, (byte)CodeTerbinProtocol.Solicit, (byte)CodeTerbinMemory.New, [], idR);

        var r = await recuperateReply(idR);
        if (r.typeError != TerbinErrorCode.None)
        {
            return (r.packet.Head.Status == CodeStatus.Succes)
                    ? r.packet.Head.IdMemory
                    : (byte)CodeTerbinMemory.ErrorRecuperate;
        }
        else
        {
            return (byte)CodeTerbinMemory.Undefined;
        }
    }

    private async Task<(PacketRequest packet, TerbinErrorCode typeError)> recuperateReply(ushort pId)
    {
        var tcs = new TaskCompletionSource<PacketRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pendingRequests.TryAdd(pId, tcs))
            return (new PacketRequest(), TerbinErrorCode.AlreadyExists);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(MaximumResponseTime));

        cts.Token.Register(() =>
        {
            // Intentamos sacar la petición del diccionario
            if (_pendingRequests.TryRemove(pId, out TaskCompletionSource<PacketRequest>? removedTcs))
            {
                var timeoutHeader = new Header(pIdRequest: pId, pOrderRequest: 0, pStatus: CodeStatus.OverMaximumTime);
                var timeoutPacket = new PacketRequest(pHead: timeoutHeader);
                removedTcs.TrySetResult(timeoutPacket);
            }
        });

        // TODO: TerbinErrorCode excede tiempo.
        return (await tcs.Task, TerbinErrorCode.None);
    }

    [Obsolete]
    private async Task handleReceive(PacketRequest pCapsule)
    {
        if (_pendingRequests.TryRemove(pCapsule.Head.IdRequest, out var tcs))
            tcs.TrySetResult(pCapsule);
        if (pCapsule.Head.Status == CodeStatus.Execute &&
            _onRecive != null)
        {
            PacketRequest? rCap = await _onRecive.Invoke(pCapsule);
            if (rCap != null)
                await Reply(rCap.Value);
        }
    }

    // --- Reply --- //
    public async Task Reply(PacketRequest pRequest)
    {
        await AddQueue(pRequest);
    }


    private ushort handleReplySize(byte pActionMethod, byte[] pPayload)
    {
        ushort id = MiniID.NewS;
        if (pPayload.Length <= TerbinProtocol.MAX_PLD)
            _ = HandleSendSigle(pActionMethod, pPayload, id, CodeStatus.InternalWorkerError, false);
        else
            _ = HandleSendFragment(pActionMethod, pPayload, id, CodeStatus.InternalWorkerError, false);
        return id;
    }

    // --- Queue --- //
    public async Task AddQueue(
                ushort pOrderRequest,
                CodeStatus pStatus,
                byte pActionMethod,
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
        await AddQueue(capsule);
    }
    public async Task AddQueue(PacketRequest pCapsule)
    {
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
                _ = handleReceive(r);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[TerbinCommunicator>manageReceive] ExceptionError-> {e.Message}");
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

                await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine($"[Communicator] El cliente se ha desconectado limpiamente.");
                break;
            }
            catch (Exception e)
            {
                // TODO: Logger.
                Console.WriteLine($"[TerbinCommunicator>manageSend] ExceptionError-> {e.Message}");
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
    public static NamedPipeServerStream CreateServerPipe(string pName = "TerbinPipe")
    {
        return new NamedPipeServerStream(
                pName,
                PipeDirection.In | PipeDirection.Out,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
    }
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

