using System.IO.Pipes;
using System.Runtime.InteropServices;
using TerbinLibrary.Communication;
using TerbinLibrary.Id;
using TerbinLibrary.Serialize;
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

    // Console.WriteLine($"[Client] Tamaño bytes ({Marshal.SizeOf<PacketRequest>()})");
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
        while (!SimulateClient.Write)
        {
            await Task.Delay(500);
        }

        Console.Write($"-------( Start )---------\n"+
            $"[Client] (byte de accion: 0 = Stop, 10 = CreateInstance, )\n"+
            $"[Client] Action -> ");
        byte input = byte.Parse(Console.ReadLine()); // alacguabar!
        Console.Write($"[Client] ({input}), {(CodeAction)input}\n"+
            $"-------(  End  )---------\n");

        byte[] menssaje = new byte["matenme".ToCharArray().Length * 2];
        try
        {
            menssaje = Serialineitor.SerializeArray<char>("matenme".ToCharArray());
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Cliente] Excption => {e.Message}");
        }

        var header = new Header(id);
        var cap = new PacketRequest(pHead: header, pActionMethod: input, pPayload: menssaje/**/);

        await writer.WriteAsycn<PacketRequest>(cap);

        if (input == 0)
        {
            Console.WriteLine($"[Client] Desconexion elegida, esperando confirmación del servidor...");

            // Esperamos a que manejerSends reciba el paquete
            while (!SimulateClient.Disconnect)
            {
                await Task.Delay(100);
            }

            Console.WriteLine($"[Client] Confirmación recibida, cerrando pipe de forma segura.");
            break; // <-- Ahora sí nos vamos.
        }
        SimulateClient.Write = false;
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

        if (r.ActionMethod == (byte)CodeAction.Stop &&
            r.Head.Status == CodeStatus.Succes)
        {
            Console.WriteLine($"[Client] Desconexion recibida.");
            SimulateClient.Disconnect = true; // <-- Avisamos al hilo principal
            break;
        }
        SimulateClient.Write = true;
    }
}

class SimulateClient
{
    public static bool Write = true;
    public static bool Disconnect = false;
}
