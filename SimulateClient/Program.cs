using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Configuration;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Memory;
using TerbinLibrary.Serialize;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Protocol;

// TODO: No cambiarle el nombre al descargar.
// TODO: Antes de instalar ver si dentro tiene una carpeta llamada Plugins.
// TODO: Los Manifest esten protegidos anti Multihilo.
// TODO: Manifest de Plugin guarde si requiere BepInEx.
// TODO: InstallByInstace y InstallByPath

// Console.Write($"\Trabajando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual | Finalizado: {p.Finish}");

#if false


//ManagerFarlands.LaunchGame("C:\\Users\\PC\\Documents\\TerbinInstaces\\_cosas_old\\mqm1\\Farlands.exe");
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

