using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Reflection;
using TerbinLibrary;
using TerbinLibrary.Executables;
using TerbinLibrary.Id;
using TerbinLibrary.Memory;

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

- Execute es para mandar y olvidarte (ya lo recibiras en OnRecive);
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

    private event Func<PacketRequest, Task>? _onRecive;
    private event Func<PacketRequest, Task>? _onNewClientConnect;

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

    public event Func<PacketRequest, Task>? OnRecive
    {
        add => _onRecive += value;
        remove
        {
            if (value != null)
                _onRecive -= value;
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

        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
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

    public async Task<ushort> Execute(byte pActionMethod, MemoryStream pPayload)
    {
        return await Execute(pActionMethod, pPayload.ToArray());
    }
    public async Task<ushort> Execute(byte pActionMethod, byte[] pPayload)
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

    public async Task<PacketRequest?> HandleSendSigle(byte pActionMethod, byte[] pPayload, ushort pId, bool pRecuperate = true)
    {
        await AddQueue(0, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.NotAsign, pPayload, pId);

        if (!pRecuperate)
            return null;
        return await recuperateReply(pId);
    }

    public async Task<PacketRequest?> HandleSendFragment(byte pActionMethod, byte[] pPayload, ushort pId, bool pRecuperate = true)
    {
        // TODO: solicitamos memoria.

        int totalPackets = (int)Math.Ceiling(pPayload.Length * TerbinProtocol.FRAGMENT_IN_MULTIPLICATE_INVERSE);
        for (ushort i = 1; i < totalPackets; i++)
        {
            byte[] fragmentPayload = pPayload[..TerbinProtocol.FRAGMENT_IN];
            pPayload = pPayload[TerbinProtocol.FRAGMENT_IN..];
            if (pPayload.Length <= TerbinProtocol.MAX_PLD)
                await AddQueue(TerbinProtocol.FINAL_PACKET, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.New, fragmentPayload, pId);
            else
                await AddQueue(i, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.New, fragmentPayload, pId);
        }

        if (!pRecuperate)
            return null;
        return await recuperateReply(pId);
    }

    private async Task<PacketRequest> handleGetMemory()
    {

        throw new NotImplementedException("Ñe");
    }

    private async Task<PacketRequest> administreMemory()
    {

        throw new NotImplementedException("Ñe");
    }

    private async Task<PacketRequest> recuperateReply(ushort pId)
    {
        var tcs = new TaskCompletionSource<PacketRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pendingRequests.TryAdd(pId, tcs))
            throw new InvalidOperationException($"Ya se está esperando el IdRequest: {pId}");

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

        return await tcs.Task;
    }

    private async Task handleReceive(PacketRequest pCapsule)
    {
        if (_pendingRequests.TryRemove(pCapsule.Head.IdRequest, out var tcs))
            tcs.TrySetResult(pCapsule);

        if (pCapsule.ActionMethod > 9)
        {
            ExecutableDispatcher.DispatchAsync(pCapsule);
        }
    }

    // --- Reply --- //
    public async Task ReplySucces(byte pActionMethod, ushort pId)
    {
        await AddQueue(0, CodeStatus.Succes, pActionMethod, (byte)CodeTerbinMemory.NotAsign, [], pId);
    }
    public async Task ReplyError(CodeStatus pStatus, byte pActionMethod, ushort pId)
    {
        await AddQueue(0, pStatus, pActionMethod, (byte)CodeTerbinMemory.NotAsign, [], pId);
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
            pStatus: pStatus);
        PacketRequest capsule = new PacketRequest(
            pHead: head,
            pActionMethod: pActionMethod,
            pIdMemory: pIdMemory,
            pPayload: pSectionPayload);
        _queue.Enqueue(capsule);
        _signal.Release();
    }

    // --- Manages --- //
    private async Task manageReceive()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            PacketRequest r = await _reader.ReadAsycn<PacketRequest>(_stopToken);


            //bool isFinal = r.Head.OrderRequest == TerbinProtocol.FINAL_PACKET || r.Head.OrderRequest == 0;
            //TerbinMemory.Store(r.Head.IdRequest, r.Head.OrderRequest, r.Payload, isFinal);

            //if (isFinal)
            //{
            //    _ = handleReceive(r);
            //    _ = _onRecive?.Invoke(r);
            //}
        }
    }
    private async Task manageSend()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(_stopToken);

            if (!_queue.TryDequeue(out PacketRequest data))
                continue;

            try
            {
                await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
            }
            catch(Exception e)
            {
                // TODO: Logger.
            }
            finally
            {
                //_signal.Release();
            }
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

