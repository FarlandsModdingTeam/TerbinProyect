using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Id;
using TerbinLibrary.Memory;
using TerbinLibrary.Serialize;
using TerbinService;
// TODO: Instalar BepiEx.
// ├─Crear Tuberia y Encabezado.
// ├─Usar Reflexión para a las clases y metodos sin tener un swich enorme.
// └─Intentar instalar BepiEx.

_ = Task.Run(() => simulateClient());

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

async Task simulateClient()
{
    await Task.Delay(1000);

    var communicator = new TerbinCommunicator(false);
    var executor = new TerbinExecutor();
    ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());

    if (await communicator.Connect())
    {
        Console.WriteLine($"[Client] ¡Conectado!");
    }
    else
    {
        Console.WriteLine($"[Client] ¡Error de Conexion!");
        return;
    }


    while (true)
    {
        Console.Write($"-------( Start )---------\n"+
            $"[Client] (byte de accion: 0 = Stop, 10 = CreateInstance, )\n"+
            $"[Client] Action -> ");
        byte input = byte.Parse(Console.ReadLine()); // alacguabar!
        Console.Write($"[Client] ({input}), {(CodeMethodsTerbinService)input}\n"+
            $"-------(  End  )---------\n");

        //byte[] menssaje = new byte["matenme".ToCharArray().Length * 2];
        //try
        //{
        //    menssaje = Serialineitor.SerializeArray<char>("matenme".ToCharArray());
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine($"[Cliente] Excption => {e.Message}");
        //}

        MemoryStream memo = new MemoryStream();
        StreamWriter writer = new StreamWriter(memo);
        writer.Write("matenme".ToCharArray());

        var r = await communicator.Communicate(input, memo);

        Console.WriteLine($"[Client] R (Action: {r.ActionMethod} Status: {r.Head.Status})");

        if (input == 0)
        {
            Console.WriteLine($"[Client] Desconexion elegida, esperando confirmación del servidor...");

            break; // <-- Ahora sí nos vamos.
        }
    }
}

async Task recibe()
{

}