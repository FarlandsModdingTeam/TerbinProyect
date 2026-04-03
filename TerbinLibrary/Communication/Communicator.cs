using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using TerbinLibrary.Executables;
using TerbinLibrary.Id;
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

    private event Func<PacketRequest, Task>? _onRecive;
    private event Func<PacketRequest, Task>? _onNewClientConnect;

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

    public async Task Execute(byte pActionMethod, MemoryStream pPayload)
    {
        await Execute(pActionMethod, pPayload.ToArray());
    }
    public async Task Execute(byte pActionMethod, byte[] pPayload)
    {
        if (pPayload.Length <= TerbinProtocol.MAXPLD)
        {
            AddQueue(0, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.NotAsign, pPayload);
        }



        // TODO: Craer protocolo estandar.
    }

    private async Task handleExecuteFragment(byte pActionMethod, byte[] pPayload)
    {
        AddQueue(0, CodeStatus.Execute, pActionMethod, (byte)CodeTerbinMemory.New, pPayload);
    }

    private async Task<PacketRequest> handleGetMemory()
    {

        throw new NotImplementedException("Ñe");
    }

    private async Task<PacketRequest> administreMemory()
    {

        throw new NotImplementedException("Ñe");
    }


    // --- Reply --- //
    public async Task ReplySucces(byte pActionMethod)
    {
        AddQueue(0, CodeStatus.Succes, pActionMethod, (byte)CodeTerbinMemory.NotAsign, []);
    }
    public async Task ReplyError(CodeStatus pStatus, byte pActionMethod)
    {
        AddQueue(0, pStatus, pActionMethod, (byte)CodeTerbinMemory.NotAsign, []);
    }


    // --- Queue --- //
    public void AddQueue(ushort pOrderRequest,
                            CodeStatus pStatus,
                            byte pActionMethod,
                            byte pIdMemory,
                            byte[] pSectionPayload)
    {
        PacketRequest capsule = new PacketRequest(
            pHead: createHead(pOrderRequest, pStatus),
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

