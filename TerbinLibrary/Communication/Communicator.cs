using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using TerbinLibrary.Communication;
using TerbinLibrary.Executables;
using TerbinLibrary.Id;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Communication;

public class Communicator : IDisposable
{
    // ****************************( Variables )**************************** //
    private PipeStream _thePipe;
    private StreamReadStruct _reader;
    private StreamWriteStruct _writer;
    private CancellationToken _stopToken;

    private readonly ConcurrentQueue<PacketRequest> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public bool IsServer
    {
        get => field;
        private set => field = value;
    } = false;

    public ushort Id
    {
        get
        {
            return field;
        }
        private set
        {
            field = value;
        }
    } = 0;

    // ****************************( Getters, Setters e Indexadores )**************************** //
    public bool IsConnect => _thePipe?.IsConnected ?? false;
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

        if (IsServer)
        {
            Id = (ushort)ShortIdReserved.Server;
            _thePipe = CreateServerPipe(pName);
        }
        else
        {
            Id = ShortId.New;
            _thePipe = CreateClientPipe(pName);
        }
        _writer = new StreamWriteStruct(_thePipe);
        _reader = new StreamReadStruct(_thePipe);

        _stopToken = pTokenCancellation;

        _ = Task.Run(handleSend, _stopToken);
    }


    // ****************************( Methods )**************************** //
    // TODO: A mi planta le falta agua.

    public async Task<bool> HandleConnect()
    {
        if (!IsConnect)
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

    public async Task Execute(byte pActionMethod, MemoryStream pPayload)
    {
        // TODO: Craer protocolo estandar.
        // TODO: Gestionar longitud.
        await AddQueue(0, CodeStatus.Execute, pActionMethod, 0, pPayload.ToArray());
    }

    // --- Reply --- //
    public async Task ReplySucces()
    {

    }
    public async Task ReplyError(CodeStatus pStatus)
    {

    }


    // --- Queue --- //
    public async Task AddQueue(ushort pOrderRequest,
                            CodeStatus pStatus,
                            byte pActionMethod,
                            byte pIdMemory,
                            byte[] pSectionPayload)
    {
        Header head = createHead(pOrderRequest, pStatus);
        PacketRequest capsule = new PacketRequest(
            pHead: head,
            pActionMethod: pActionMethod,
            pIdMemory: pIdMemory,
            pPayload: pSectionPayload);

        _queue.Enqueue(capsule);
        _signal.Release();

        //return Task.CompletedTask;
    }

    // --- Handles --- //
    private async Task handleReceive()
    {
        while (!_stopToken.IsCancellationRequested)
        {

        }
    }
    private async Task handleSend()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(_stopToken);

            if (_queue.TryDequeue(out PacketRequest data))
            {
                await _writer.WriteAsycn<PacketRequest>(data, _stopToken);
            }
        }
    }


    // ****************************( Helps )**************************** //
    private Header createHead(ushort pOrderRequest, CodeStatus pStatus)
    {
        return new Header(pIdClient: Id,
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

