using System.IO.Pipes;
using System.Reflection;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary;
using TerbinLibrary.Execution;
using TerbinLibrary.Protocol;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;

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

    public static AsyncLocal<AmongInfoThreads> CurrentConst = new AsyncLocal<AmongInfoThreads>();

    public Worker(ILogger<Worker> pLogger, IHostApplicationLifetime pAppLifetime)
    {
        Worker._appLifetime = pAppLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken pStoppingToken)
    {
        Cts = CancellationTokenSource.CreateLinkedTokenSource(pStoppingToken);
        //await TerbinProtocol.InitProtocol(Cts.Token);
        await autoCreatePipe(Cts.Token);
        //ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        //TerbinExecutableCRUDManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }


    // Pruebas
    public async Task InitProtocol(CancellationToken pTokenCancellation)
    {
        await autoCreatePipe(pTokenCancellation);
    }

    // Pruebas
    private async Task autoCreatePipe(CancellationToken pTokenCancellation)
    {
        try
        {
            var communicator = new TerbinCommunicator(true, pTokenCancellation);
            TerbinExecutor.Register(Assembly.GetExecutingAssembly());
            communicator.OnRecive += async (pCapsule) =>
            {
                Console.Log($"Packet: {pCapsule}");
                CurrentConst.Value = new AmongInfoThreads
                {
                    Communicator = communicator,
                };
                return await TerbinExecutableManager.DispatchAsync(pCapsule.Head, pCapsule.Payload, pCapsule.ActionMethod);
            };
            communicator.OnNewClientConnect += async () =>
            {
                _ = Task.Run(() => autoCreatePipe(pTokenCancellation), pTokenCancellation);
            };
        }
        catch (Exception e)
        {
            e.PrintException("Worker>autoCreatePipe");
        }
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Stop)]
    public static async Task<InfoResponse?> Stop(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);

            Console.WriteLine("[Worker] Execution stoped");
            if (pToken.IsCancellationRequested) return;
            _appLifetime?.StopApplication();
            Cts?.Cancel();
        });
        Console.WriteLine("[Worker] Stopping execution...");
        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}
