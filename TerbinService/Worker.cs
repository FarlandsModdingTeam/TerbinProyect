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
        //ExecutableDispatcher.RegisterFromAssembly(typeof(HandleInstances).Assembly);

        CommunicationThreads.Init();

        // Tu lógica aquí, usando _cts.Token en lugar de stoppingToken
        while (!Cts.Token.IsCancellationRequested)
        {
            // Trabajo del worker...
            await Task.Delay(1000, Cts.Token);
        }
    }

    [TerbinExecutable(CodeAction.Stop)]
    public static async Task Stop(Header pHead, MemoryStream pPaarameters)
    {
        Console.WriteLine("(Stopping execution...)");
        Cts?.Cancel();
    }
}
