using System.IO.Pipes;
using System.Runtime.InteropServices;
using TerbinLibrary.Communication;
using TerbinLibrary.Id;
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

    var id = ShortId.New;
    Console.WriteLine($"[Client] Id: {id}");


    using var pipe = new NamedPipeClientStream(".", "TerbinPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync();
    _ = manejerSends(pipe);
    Console.WriteLine($"[Client] ¡Conectado!");

    var writer = new StreamWritesStruct(pipe);

    while (true)
    {
        Console.WriteLine($"--------------------");
        Console.WriteLine($"[Client] (byte de accion: 0 = Stop, 10 = CreateInstance, )");
        byte input = (byte)/*Console.ReadLine()*/"0".ToArray()[0]; // alacguabar!


        var header = new Header(id);
        var cap = new PacketRequest(pHead: header, pActionMethod: (CodeAction)input);

        await writer.WriteAsycn<PacketRequest>(cap);

        if (input == 0)
            break;
    }
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