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

    Console.WriteLine("[Client] Creando Encapsulamiento...");
    var id = Guid.NewGuid();
    var header = new Header(id);
    var cap = new PacketRequest(pHead: header, pActionMethod: CodeAction.CreateInstance);


    using var pipe = new NamedPipeClientStream(".", "TerbinPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync();

    _ = manejerSends(pipe);

    await Task.Delay(1000);
    var writer = new StreamWritesStruct(pipe);


    await writer.WriteAsycn<PacketRequest>(cap);

    await Task.Delay(1000);

    var capClose = new PacketRequest(pHead: header, pActionMethod: CodeAction.Stop);
    await writer.WriteAsycn<PacketRequest>(capClose);
}

// TODO: Adaptar y meter en Library.
async Task manejerSends(NamedPipeClientStream pPipe)
{
    var reader = new StreamReadStruct(pPipe);
    while (true)
    {
        var r = await reader.ReadAsycn<PacketRequest>();
        Console.WriteLine($"[Client] R (Action: {r.ActionMethod} Status: {r.Head.Status})");
        if (r.ActionMethod == CodeAction.Stop && // Nunca llega esta respuesta.
            r.Head.Status == CodeStatus.Succes)
        {
            break;
        }
    }
}