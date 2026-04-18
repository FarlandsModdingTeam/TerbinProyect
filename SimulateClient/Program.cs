using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using System.Reflection;
using TerbinLibrary.Serialize;
using TerbinLibrary;


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
    byte input2 = byte.MaxValue;

    if (input >= (byte)CodeTerbinProtocol.Create &&
        input <= (byte)CodeTerbinProtocol.Deleted)
    {
        Console.Write($"[Client] Action2 ->");
        input2 = byte.Parse(Console.ReadLine()); // alacguabar!
    }
    Console.Write($"[Client] ({input}), {(CodeMethodsTerbinService)input}\n" +
        $"-------(  End  )---------\n");

    // Esto fufa :)
    byte[] menssaje;
    if (input2 != byte.MaxValue)
    {
        // Incluye input2 al inicio del mensaje
        var baseMsg = Serialineitor.SerializeArray<char>("matenme".ToCharArray());
        menssaje = new byte[baseMsg.Length + 1];
        menssaje[0] = input2;
        Array.Copy(baseMsg, 0, menssaje, 1, baseMsg.Length);
    }
    else
    {
        menssaje = Serialineitor.SerializeArray<char>("matenme".ToCharArray());
    }


    //char[] m = "matenme".ToCharArray();

    // TODO: (Solucionar error):  Este solo manda 12 bytes.
    Span<byte> spanMenssaje = new byte["matenme".ToCharArray().Length * 2];
    spanMenssaje.WriteArray<char>("matenme".ToCharArray());

    // TODO: (Solucionar error): Este solo manda 7 bytes.
    MemoryStream memo = new MemoryStream();
    StreamWriter writer = new StreamWriter(memo);

    writer.Write("matenme".ToCharArray());
    writer.Flush();

    var a = spanMenssaje.ToArray();

    //Console.WriteLine($"[Client] Mensaje: {spanMenssaje.ToArray().ToString()}");
    try
    {
        var r = await communicator.Communicate(input, menssaje);
        Console.WriteLine($"[Client] R (Action: {r.ActionMethod} Status: {r.Head.Status})");
        if (r.Payload.Length > 0)
        {
            string rute = new(Serialineitor.DeserializeArray<char>(r.Payload));
            Console.WriteLine($"[Client] R (l: {rute.Length}, m: {rute})");
        }
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