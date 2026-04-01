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

    Console.WriteLine($"[Client] Tamaño bytes ({Marshal.SizeOf<PacketRequest>()})");
    // StreamBytes.StructToBytes(cap).ToArray().Length

    var id = ShortId.New;
    Console.WriteLine($"[Client] Id: {id}");

    using var pipe = CommunicationThreads.CreateClientPipe();
    await pipe.ConnectAsync();
    _ = manejerSends(pipe);
    Console.WriteLine($"[Client] ¡Conectado!");

    var writer = new StreamWritesStruct(pipe);

    while (true)
    {
        Console.Write($"-------( Start )---------\n"+
            $"[Client] (byte de accion: 0 = Stop, 10 = CreateInstance, )\n"+
            $"[Client] Action -> ");
        byte input = byte.Parse(Console.ReadLine()); // alacguabar!
        Console.Write($"[Client] ({input}), {(CodeAction)input}\n"+
            $"-------(  End  )---------\n");

        var header = new Header(id);
        var cap = new PacketRequest(pHead: header, pActionMethod: input);

        await writer.WriteAsycn<PacketRequest>(cap);

        if (input == 0)
        {
            Console.WriteLine($"[Client] Desconexion elegida.");
            break;
        }
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
        if (r.ActionMethod == (byte)CodeAction.Stop && // Nunca llega esta respuesta.
            r.Head.Status == CodeStatus.Succes)
        {
            Console.WriteLine($"[Client] Desconexion recibida.");
            break;
        }
    }
}