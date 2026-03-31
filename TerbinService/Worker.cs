using System.IO.Pipes;
using System.Reflection;
using TerbinLibrary.Communication;
using TerbinService.Communication;
using TerbinService.Instances;

namespace TerbinService;


public class Worker(ILogger<Worker> logger) : BackgroundService
{
    public static CancellationTokenSource? Cts;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        //CommunicationThreads.Init();
        _ = autoCreatePipe();
    }

    private async Task autoCreatePipe()
    {
        var pipe = CommunicationThreads.NewTerbinPipe;
        await pipe.WaitForConnectionAsync(Cts.Token);

        _ = Task.Run(() => autoCreatePipe(), Cts.Token);
        _ = handleClient(pipe, Cts.Token);
    }

    private async Task handleClient(NamedPipeServerStream pPipe, CancellationToken pTokenCancellation)
    {
        StreamReadStruct reader = new(pPipe);
        StreamWritesStruct writer = new(pPipe);

        while (!pTokenCancellation.IsCancellationRequested)
        {
            PacketRequest cap = await reader.ReadAsycn<PacketRequest>(pTokenCancellation);
            //Console.WriteLine($"[Worker] Client {cap.Head.IdRequest:N}");

            var capR = await ExecutableDispatcher.DispatchAsync(cap);

            _ = writer.WriteAsycn<PacketRequest>(capR);
        }
    }


    [TerbinExecutable(CodeAction.Stop)]
    public static async Task<PacketRequest> Stop(Header pHead, MemoryStream pParameters)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            Console.WriteLine("(Worker): Execution stoped");
            Cts?.Cancel();
        });
        Console.WriteLine("(Worker): Stopping execution...");
        pHead.Status = CodeStatus.Succes;
        return new PacketRequest(pHead, CodeAction.Stop, 1);
    }
}
