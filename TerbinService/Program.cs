using System.IO.Pipes;
using System.Runtime.InteropServices;
using TerbinLibrary.Communication;
using TerbinService;
using TerbinService.Communication;

_ = Task.Run(() => simulateClient());

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();


async Task simulateClient()
{
    await Task.Delay(1000);

    Console.WriteLine("[Cliente] Creando Encapsulamiento...");
    var id = Guid.NewGuid();
    var cap = new Capsule
    {
        Head = new Header
        {
            IdRequest = id,
            Status = CodeStatus.NotAsign
        },
        ActionMethod = CodeAction.CreateInstance,
    };


    using var pipe = new NamedPipeClientStream(".", "TerbinPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync();

    _ = manejerSends(pipe);

    await Task.Delay(1000);
    var writer = new StreamWritesStruct(pipe);


    await writer.WriteAsycn<Capsule>(cap);
    //Console.WriteLine("[Cliente] Peticion Ejecutada...");

    await Task.Delay(1000);
    var capClose = new Capsule
    {
        Head = new Header
        {
            IdRequest = id,
            Status = CodeStatus.NotAsign
        },
        ActionMethod = CodeAction.Stop,
        //Parameters = "" // Convert.ToBase64String(Convert.FromBase64String("f"))
    };
    await writer.WriteAsycn<Capsule>(capClose);
    //Console.WriteLine("[Cliente] Deteniendo ejecucion...");
}

async Task manejerSends(NamedPipeClientStream pPipe)
{
    var reader = new StreamReadStruct(pPipe);
    while (true)
    {
        var r = await reader.ReadAsycn<Capsule>();
        Console.WriteLine($"[Client] R (Action: {r.ActionMethod} Status: {r.Head.Status})");
    }
}