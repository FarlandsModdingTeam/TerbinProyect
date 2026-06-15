using Newtonsoft.Json.Linq;
using SimulateClient;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Memory;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;

// TODO: No cambiarle el nombre al descargar.
// TODO: Antes de instalar ver si dentro tiene una carpeta llamada Plugins.
// TODO: Los Manifest esten protegidos anti Multihilo.
// TODO: Manifest de Plugin guarde si requiere BepInEx.
// TODO: InstallByInstace y InstallByPath

// Console.Write($"\Trabajando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual | Finalizado: {p.Finish}");

#if false


//ManagerFarlands.LaunchGame("C:\\Users\\PC\\Documents\\TerbinInstances\\_cosas_old\\mqm1\\Farlands.exe");
Console.WriteLine($"SimulateClient esta desactivado.");
Console.WriteLine($"Ponga en false el if para activarlo.");
await pressAnyKeyToContinue();

#else
var communicator = new TerbinCommunicator(false);
TerbinExecutor.Register(Assembly.GetExecutingAssembly());
communicator.OnRecive += async p =>
{
    //CurrentConst.Value = new AmongInfoThreads
    //{
    //    Communicator = communicator,
    //};
    return await TerbinExecutableManager.DispatchAsync(p.Head, p.Payload, p.ActionMethod);
};

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
    string @class;
    string select;

    Console.Write($"-------( Start )---------\n" +
        $"[Client] \"1. Nombre-Clase | 2. Yolo(1) o Poco-A-Poco(2)\" \n");

    @class = Helper.Read("1. name");
    if (@class is "exit" or "ex" or "sa" or "salir")
        break;

    select = Helper.Read("2. tipe"); //(int.Parse(Helper.Read("2. tipe")) == 1) ? "Yolo" : "LittleByLittle";


    string? meth = select switch
    {
        "1" => "Yolo",
        "2" => "LittleByLittle",

        "n" => null,
        "null" => null,
        string msg when string.IsNullOrEmpty(msg) => null,

        _ => select,
    };


    Type? classType;
    MethodInfo? method;
    classType = Type.GetType($"SimulateClient.{@class}");
    method = (string.IsNullOrEmpty(meth)) ? null : classType?.GetMethod(meth, BindingFlags.Static | BindingFlags.Public);


    if (method != null)
    {
        var result = method.Invoke(null, new object[] { communicator });

        if (result is Task task)
        {
            await task;
        }
    }
    else
    {
        Console.WriteLine($"[Client] El method es null");
    }
}


//public class ProgramStoped
//{

//    [TerbinExecutable((byte)CodeTerbinProtocol.Stop)]
//    public static async Task<InfoResponse?> Stop(Header pHead, byte[] pParameters)
//    {
//        _ = Task.Run(async () =>
//        {
//            await Task.Delay(100);
//            Console.WriteLine("[Worker] Execution stoped");
//            _appLifetime?.StopApplication();
//            Cts?.Cancel();
//        });
//        Console.WriteLine("[Worker] Stopping execution...");
//        return InfoResponse.CreateSucces(pHead.IdRequest);
//    }
//}

#endif

Console.Error("Fin del Programa del todo");
await Helper.Fin();