using System.IO.Pipes;
using System.Reflection;
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

    Console.WriteLine("Creando Encapsulamiento...");
    var id = Guid.NewGuid();
    var cap = new Capsule
    {
        Head = new Header
        {
            IdClient = id,
            Status = CodeStatus.NotAsign
        },
        ActionMethod = CodeAction.CreateInstance,
        //Parameters = "" // Convert.ToBase64String(Convert.FromBase64String("f"))
    };

    await Task.Delay(1000);

    using var pipe = new NamedPipeClientStream(".", "TerbinPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync();
    var writer = new StreamWritesStruct(pipe);

    Console.WriteLine("Enviando Peticion...");
    await writer.WriteAsycn<Capsule>(cap).ContinueWith(r =>
    {

    });

    // Esto es mentira no esperan.

    var capClose = new Capsule
    {
        Head = new Header
        {
            IdClient = id,
            Status = CodeStatus.NotAsign
        },
        ActionMethod = CodeAction.Stop,
        //Parameters = "" // Convert.ToBase64String(Convert.FromBase64String("f"))
    };
    await writer.WriteAsycn<Capsule>(capClose).ContinueWith(rr =>
    {
        Console.WriteLine("(Deteniendo ejecucion...)");
    });
}

