using System.IO.Pipes;
using System.Reflection;
using TerbinLibrary.Communication;
using TerbinLibrary.Executables;

namespace TerbinService;
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


public class Worker : BackgroundService
{
    public static CancellationTokenSource? Cts;
    private static IHostApplicationLifetime? _appLifetime;

    public Worker(ILogger<Worker> pLogger, IHostApplicationLifetime pAppLifetime)
    {
        Worker._appLifetime = pAppLifetime;
    }



    protected override async Task ExecuteAsync(CancellationToken pStoppingToken)
    {
        Cts = CancellationTokenSource.CreateLinkedTokenSource(pStoppingToken);

        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        _ = autoCreatePipe();
    }

    private async Task autoCreatePipe()
    {
        var pipe = Communicator.NewTerbinPipe;
        await pipe.WaitForConnectionAsync(Cts.Token);

        _ = Task.Run(() => autoCreatePipe(), Cts.Token);
        _ = handleClient(pipe, Cts.Token);
    }

    private async Task handleClient(NamedPipeServerStream pPipe, CancellationToken pTokenCancellation)
    {
        StreamReadStruct reader = new(pPipe);
        StreamWriteStruct writer = new(pPipe);

        while (!pTokenCancellation.IsCancellationRequested)
        {
            PacketRequest cap = await reader.ReadAsycn<PacketRequest>(pTokenCancellation);
            //Console.WriteLine($"[Worker] Client {cap.Head.OrderRequest}");

            PacketRequest capR = default;
            try
            {
                capR = await ExecutableDispatcher.DispatchAsync(cap);
                await writer.WriteAsycn<PacketRequest>(capR);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
        pPipe.Disconnect(); // porque nunca pasa aqui.
    }


    [TerbinExecutable((byte)CodeAction.Stop)]
    public static async Task<PacketRequest> Stop(Header pHead, MemoryStream pParameters)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            Console.WriteLine("(Worker): Execution stoped");
            _appLifetime?.StopApplication();
            Cts?.Cancel();
        });
        Console.WriteLine("(Worker): Stopping execution...");
        pHead.Status = CodeStatus.Succes;
        return new PacketRequest(pHead, (byte)CodeAction.Stop, 1);
    }
}
