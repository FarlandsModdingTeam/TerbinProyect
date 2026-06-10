using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Configuration;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace SimulateClient;


static class Yolo
{
    [Obsolete("Falta cambiar cosas antiguas", true)]
    static async Task yolo_old(TerbinCommunicator pCommunicator)
    {
        string? nameInstace, pathInstaces, pathInstace, pathFarlands;
        char[] nameArray;
        PacketRequest r;
        Serialineitor s;


        Console.Write($"-------( Start )---------\n" +
            $"[Client] (Pon el nombre de la tripodetica instancia cara alpargata)\n" +
            $"[Client] Action -> ");
        nameInstace = Console.ReadLine();
        if (nameInstace == null)
            nameInstace = "Eñe";
        Console.Write($"[Client] Nombre Instancia -> ({nameInstace})\n" +
            $"-------(  End  )---------\n");

        nameArray = nameInstace.ToCharArray();

        Console.WriteLine($"[Client] Get Ruta Instancias");
        s = new Serialineitor()
                    .AddArray(TerbinConfiguration.RUTE_INSTANCES.ToCharArray());
        r = await pCommunicator.Communicate(new IdArray(TerbinCRUD.Read, CodeSubServices.Rute), s.Serialize());
        Console.WriteLine($"[Client] 1 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;

        ReadOnlySpan<byte> reader = r.Payload;
        pathInstaces = reader.ReadArray<char>().CrString();
        pathInstace = Path.Combine(pathInstaces, nameInstace);


        Console.WriteLine($"[Client] Get Ruta Farlsnds");
        s = new Serialineitor()
                    .AddArray(TerbinConfiguration.RUTE_FARLANDS.ToCharArray());
        r = await pCommunicator.Communicate(new(TerbinCRUD.Read, CodeSubServices.Rute), s.Serialize());
        Console.WriteLine($"[Client] 2 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;

        ReadOnlySpan<byte> reader1 = r.Payload;
        pathFarlands = reader1.ReadArray<char>().CrString();


        await Helper.PressAnyKeyToContinue();
        Console.WriteLine($"[Client] Creamos instancia");
        s = new Serialineitor()
                    .AddArray(nameInstace.ToCharArray());
        r = await pCommunicator.Communicate(new(TerbinCRUD.Create, CodeSubServices.Instances), s.Serialize());
        Console.WriteLine($"[Client] 3 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;


        await Helper.PressAnyKeyToContinue();
        Console.WriteLine($"[Client] Clonamos Farlands");
        s = new Serialineitor()
                    .AddArray(nameInstace.ToCharArray())
                    .AddArray(pathFarlands.ToCharArray());
        r = await pCommunicator.Communicate(new(TerbinCRUD.Duplicate, CodeSubServices.Game), s.Serialize());
        Console.WriteLine($"[Client] 4 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;


        await Helper.PressAnyKeyToContinue();
        Console.WriteLine($"[Client] Instalamos BepInEx");
        s = new Serialineitor()
                    .AddArray(nameInstace.ToCharArray())
                    .AddArray(TerbinURLs.BepInEx.ToCharArray())
                    .Add(false);
        r = await pCommunicator.Communicate(new(CodeServices.Install, CodeSubServices.Plugin), s.Serialize());
        Console.WriteLine($"[Client] 5 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;


        await Helper.PressAnyKeyToContinue();
        Console.WriteLine($"[Client] Instalamos Exploratior");
        s = new Serialineitor()
                    .AddArray(nameInstace.ToCharArray())
                    .AddArray(TerbinURLs.MOD_EXPLORER.ToCharArray())
                    .Add(true);
        r = await pCommunicator.Communicate(new(CodeServices.Install, CodeSubServices.Plugin), s.Serialize());
        Console.WriteLine($"[Client] 6 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;


        await Helper.PressAnyKeyToContinue();
        Console.WriteLine($"[Client] Instalamos FCM");
        s = new Serialineitor()
                    .AddArray(nameInstace.ToCharArray())
                    .AddArray(TerbinURLs.MOD_FCM.ToCharArray())
                    .Add(true);
        r = await pCommunicator.Communicate(new(CodeServices.Install, CodeSubServices.Plugin), s.Serialize());
        Console.WriteLine($"[Client] 7 R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
        if (await Helper.IsError(r.Head.Status)) return;


        Console.WriteLine($"[Client] 8 Final -----------------------------------------------------------");
        await Helper.PressAnyKeyToContinue();
    }



}