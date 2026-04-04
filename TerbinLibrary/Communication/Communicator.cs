using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using TerbinLibrary.Executables;
using TerbinLibrary.Id;
using TerbinLibrary;

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

public class Communicator : IDisposable
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

    [Obsolete]
    public ushort Id
    {
        get => field;
        private set => field = value;
    } = 0;

    public ushort MaximumResponseTime
    {
        get => field;
        set => field = value;
    } = 180;

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
    public Communicator(bool pIsServer = false, CancellationToken pTokenCancellation = default, string pName = "TerbinPipe")
    {
        IsServer = pIsServer;
        _stopToken = pTokenCancellation;

        if (IsServer)
        {
            //Id = (ushort)MiniID.Server;
            _thePipe = CreateServerPipe(pName);
        }
        else
        {
            //Id = MiniID.NewS;
            _thePipe = CreateClientPipe(pName);
        }
        _writer = new StreamWriteStruct(_thePipe);
        _reader = new StreamReadStruct(_thePipe);

        _ = Task.Run(manageReceive, _stopToken);
        _ = Task.Run(manageSend, _stopToken);
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

        throw new NotImplementedException("Ñe");
    }

    public async Task<ushort> Execute(byte pActionMethod, MemoryStream pPayload)
    {
        return await Execute(pActionMethod, pPayload.ToArray());
    }
    public async Task<ushort> Execute(byte pActionMethod, byte[] pPayload)
    {
        ushort id = MiniID.NewS;
        if (pPayload.Length <= TerbinProtocol.MAX_PLD)
            _ = handleExecuteSigle(pActionMethod, pPayload, id);
        else
            _ = handleExecuteFragment(pActionMethod, pPayload, id);
        return id;
    }

    private async Task<PacketRequest?> handleExecuteSigle(byte pActionMethod, byte[] pPayload, ushort pId, bool pRecuperate = true)
    {
        await AddQueue(0, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.NotAsign, pPayload, pId);

        if (!pRecuperate)
            return null;
        return await recuperateReply(pId);
    }

    private async Task<PacketRequest?> handleExecuteFragment(byte pActionMethod, byte[] pPayload, ushort pId, bool pRecuperate = true)
    {
        // TODO: solicitamos memoria.

        for (ushort i = 1; i < 10; i++)
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
            _ = handleReceive(r);
            _ = _onRecive?.Invoke(r);
        }
    }
    private async Task manageSend()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(_stopToken);

            if (_queue.TryDequeue(out PacketRequest data))
            {
                try
                {
                    await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
                }
                catch(Exception e)
                {

                }
                finally
                {
                    //_signal.Release();
                }
            }
        }
    }


    // ****************************( Helps )**************************** //
    [Obsolete]
    private Header createHead(ushort pOrderRequest, CodeStatus pStatus)
    {
        return new Header(pIdRequest: Id,
                          pOrderRequest: pOrderRequest,
                          pStatus: pStatus);
    }

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

    ~Communicator()
    {
        Dispose(false);
    }
}

