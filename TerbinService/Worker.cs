using System.Reflection;
using TerbinLibrary.Communication;
using TerbinService.Communication;
using TerbinService.Instances;

namespace TerbinService;

// Loco ¿Porque una clase tiene parametros de entrada?
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    public static CancellationTokenSource? Cts;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        CommunicationThreads.Init();
    }

    [TerbinExecutable(CodeAction.Stop)]
    public static async Task<Capsule> Stop(Header pHead, MemoryStream pPaarameters)
    {
        Console.WriteLine("(Worker): Stopping execution...");
        Cts?.Cancel();

        pHead.Status = CodeStatus.Succes;
        return new Capsule
        {
            Head = pHead,
            ActionMethod = CodeAction.Stop,
        };
    }
}
