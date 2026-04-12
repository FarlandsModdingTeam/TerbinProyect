using System.IO.Pipes;
using System.Reflection;
using TerbinLibrary.Communication;
using TerbinLibrary;
using TerbinLibrary.Execution;

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
        await TerbinProtocol.InitProtocol(Cts.Token);
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }





    [TerbinExecutable((byte)CodeTerbinProtocol.Stop)]
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
        return new PacketRequest(pHead, (byte)CodeTerbinProtocol.Stop, 1);
    }
}
