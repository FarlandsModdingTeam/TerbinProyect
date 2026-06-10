using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace SimulateClient;

internal class Down : ITests
{
    // Poco a Poco.
    public static async Task LittleByLittle(TerbinCommunicator pCommunicator)
    {
        PacketRequest r;
        Serialineitor s;
        int c = 0;


        {
            Console.WriteLine($"[Client] ==> BepInEx");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.BepInEx.ToCharArray())
                        .Add(false);
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        {
            Console.WriteLine($"[Client] ==> Exploratior");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_EXPLORER.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        {
            Console.WriteLine($"[Client] ==> FCM");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_FCM.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        await Helper.Fin();
    }
    
    public static async Task Yolo(TerbinCommunicator pCommunicator)
    {

        PacketRequest r;
        Serialineitor s;
        int c = 0;


        {
            Console.WriteLine($"[Client] ==> BepInEx");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.BepInEx.ToCharArray())
                        .Add(false);
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        {
            Console.WriteLine($"[Client] ==> Exploratior");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_EXPLORER.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        {
            Console.WriteLine($"[Client] ==> FCM");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_FCM.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            if (await Helper.IsError(r.Head.Status)) return;
        }


        await Helper.Fin();
    }
}
