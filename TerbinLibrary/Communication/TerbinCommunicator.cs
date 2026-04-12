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

    private event Func<PacketRequest, Task<PacketRequest>>? _onRecive;
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

    public event Func<PacketRequest, Task<PacketRequest>>? OnRecive
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

        _ = Task.Run(manageReceive, _stopToken);
        _ = Task.Run(manageSend, _stopToken);

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
            return true;
        }
        return false;
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
            reply = (PacketRequest) await HandleSendSigle(pActionMethod, pPayload, id, true);
        else
            reply = (PacketRequest) await HandleSendFragment(pActionMethod, pPayload, id, true);
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
            _ = HandleSendSigle(pActionMethod, pPayload, id, false);
        else
            _ = HandleSendFragment(pActionMethod, pPayload, id, false);
        return id;
    }

    //private ushort handleSendSize(byte pActionMethod, byte[] pPayload)
    //{
    //    ushort id = MiniID.NewS;
    //    if (pPayload.Length <= TerbinProtocol.MAX_PLD)
    //        _ = handleExecuteSigle(pActionMethod, pPayload, id, false);
    //    else
    //        _ = handleExecuteFragment(pActionMethod, pPayload, id, false);
    //    return id;
    //}

    public async Task<PacketRequest?> HandleSendSigle(byte pActionMethod, byte[] pPayload, ushort pIdRequest, bool pRecuperate = true)
    {
        Console.WriteLine($"[X] ¡HandleSendSigle!");
        await AddQueue(0, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.NotAsign, pPayload, pIdRequest);

        Console.WriteLine($"[X] ¡HandleSendSigle! 2");

        if (!pRecuperate)
            return null;
        Console.WriteLine($"[X] ¡HandleSendSigle! 3");
        // TODO: gestionar error.
        var r = await recuperateReply(pIdRequest);
        Console.WriteLine($"[X] r: {r.typeError}");
        return r.packet;
    }

    public async Task<PacketRequest?> HandleSendFragment(byte pActionMethod, byte[] pPayload, ushort pIdRequest, bool pRecuperate = true)
    {
        byte idMemory = await soliciteMemory();
        if (idMemory == (byte)CodeTerbinMemory.None)
        {
            // TODO: Controlar y logger.
            throw new Exception("¡idMemory es None!");
        }

        int totalPackets = (int)Math.Ceiling(pPayload.Length * TerbinProtocol.FRAGMENT_IN__MULTIPLICATE_INVERSE);
        if (totalPackets >= TerbinProtocol.FINAL_PACKET - 1)
        {
            // TODO: mandar respuesta en vez de excepcion.
            throw new Exception("¡QUE COJÓNES!");
        }

        for (ushort i = 1; i < totalPackets; i++)
        {
            byte[] fragmentPayload = pPayload[..TerbinProtocol.FRAGMENT_IN];
            pPayload = pPayload[TerbinProtocol.FRAGMENT_IN..];
            if (pPayload.Length <= TerbinProtocol.MAX_PLD)
                await AddQueue(TerbinProtocol.FINAL_PACKET, CodeStatus.Execute, pActionMethod, idMemory, fragmentPayload, pIdRequest);
            else
                await AddQueue(i, CodeStatus.Execute, pActionMethod, idMemory, fragmentPayload, pIdRequest);
        }

        if (!pRecuperate)
            return null;
        // TODO: gestionar error.
        return (await recuperateReply(pIdRequest)).packet;
    }

    private async Task<byte> soliciteMemory()
    {
        ushort idR = MiniID.NewS;
        await AddQueue(0, CodeStatus.Execute, (byte)CodeTerbinProtocol.Solicit, (byte)CodeTerbinMemory.New, [], idR);

        var r = await recuperateReply(idR);
        if (r.typeError != TerbinErrorCode.None)
        {
            return (r.packet.Head.Status == CodeStatus.Succes)
                    ? r.packet.Head.IdMemory
                    : (byte)CodeTerbinMemory.None;
        }
        else
        {
            // TODO: Logger y gestionar.
            return (byte)CodeTerbinMemory.None;
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

    private async Task handleReceive(PacketRequest pCapsule)
    {
        if (_pendingRequests.TryRemove(pCapsule.Head.IdRequest, out var tcs))
            tcs.TrySetResult(pCapsule);
        if (pCapsule.Head.Status == CodeStatus.Execute &&
            _onRecive != null)
        {
            var rCap = await _onRecive.Invoke(pCapsule);
            await Reply(rCap);
        }
    }

    // --- Reply --- //
    // Soy estupido.
    public async Task ReplySucces(byte pActionMethod, ushort pId)
    {
        await AddQueue(0, CodeStatus.Succes, pActionMethod, (byte)CodeTerbinMemory.NotAsign, [], pId);
    }
    public async Task ReplyError(CodeStatus pStatus, byte pActionMethod, ushort pId)
    {
        await AddQueue(0, pStatus, pActionMethod, (byte)CodeTerbinMemory.NotAsign, [], pId);
    }

    public async Task Reply(PacketRequest pRequest)
    {
        await AddQueue(pRequest);
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
        Console.WriteLine($"[X] AddQueue 1");
        await AddQueue(capsule);
    }
    public async Task AddQueue(PacketRequest pCapsule)
    {
        Console.WriteLine($"[X] AddQueue 2");
        _queue.Enqueue(pCapsule);
        _signal.Release();
    }

    // --- Manages --- //
    private async Task manageReceive()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            PacketRequest r = await _reader.ReadAsycn<PacketRequest>(_stopToken);

            Console.WriteLine($"[X] ¡Nuevo Paquete recivido!");

            _ = handleReceive(r);
        }
    }
    private async Task manageSend()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            Console.WriteLine($"[X] Esperando enviar:");
            await _signal.WaitAsync(_stopToken);
            Console.WriteLine($"[X] Enviado...");

            if (!_queue.TryDequeue(out PacketRequest data))
                continue;

            Console.WriteLine($"[X] Quitado y obtenido...");

            try
            {
                await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
                Console.WriteLine($"[X] Enviado");
            }
            catch(Exception e)
            {
                // TODO: Logger.
                Console.WriteLine($"[X] Error-> {e.Message}");
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
            Console.WriteLine($"[Worker] ¡Nuevo Cliente conectado! ");
            //_ = Task.Run(_onNewClientConnect?.Invoke(), _stopToken);
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

