using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using System.Reflection;
using TerbinLibrary.Serialize;


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

await Task.Delay(1000);

while (true)
{
    Console.Write($"-------( Start )---------\n" +
        $"[Client] (byte de accion: 0 = Stop, 10 = CreateInstance, )\n" +
        $"[Client] Action -> ");
    byte input = byte.Parse(Console.ReadLine()); // alacguabar!
    Console.Write($"[Client] ({input}), {(CodeMethodsTerbinService)input}\n" +
        $"-------(  End  )---------\n");

    byte[] menssaje = new byte["matenme".ToCharArray().Length * 2];
    menssaje = Serialineitor.SerializeArray<char>("matenme".ToCharArray());

    //MemoryStream memo = new MemoryStream();
    //StreamWriter writer = new StreamWriter(memo);

    //writer.Write("matenme".ToCharArray());
    //writer.Flush();


    //Console.WriteLine($"[Client] Mensaje: {memo.ToString}");
    try
    {
        var r = await communicator.Communicate(input, menssaje);
        Console.WriteLine($"[Client] R (Action: {r.ActionMethod} Status: {r.Head.Status})");
    }
    catch (Exception e)
    {
        Console.WriteLine($"[Client] Error-> {e.Message}");
    }

    if (input == 0)
    {
        Console.WriteLine($"[Client] Desconexion elegida, esperando confirmación del servidor...");
        break; // <-- Ahora sí, nos largamos!.
    }
}