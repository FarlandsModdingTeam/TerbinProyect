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
        int c = 1;


        {
            Console.WriteLine($"[Client] ==> BepInEx");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.BepInEx.ToCharArray())
                        .Add(false);
            r = await pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            await Helper.IsError(r.Head.Status);
        }


        {
            Console.WriteLine($"[Client] ==> Exploratior");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_EXPLORER.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            await Helper.IsError(r.Head.Status);
        }


        {
            Console.WriteLine($"[Client] ==> FCM");
            await Helper.PressAnyKeyToContinue();

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_FCM.ToCharArray());
            r = await pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize());

            Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            await Helper.IsError(r.Head.Status);
        }


        await Helper.Fin();
    }
    
    public static async Task Yolo(TerbinCommunicator pCommunicator)
    {
        Serialineitor s;
        int c = 1;


        {
            Console.WriteLine($"[Client] ==> BepInEx");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.BepInEx.ToCharArray())
                        .Add(false);
            _ = pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize()).ContinueWith(async p =>
            {
                PacketRequest r = await p;
                Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
                Helper.PrintMethod(r.ActionMethod);
                await Helper.IsError(r.Head.Status);
            });

        }


        {
            Console.WriteLine($"[Client] ==> Exploratior");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_EXPLORER.ToCharArray());
            _ = pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize()).ContinueWith(async p =>
            {
                PacketRequest r = await p;
                Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
                Helper.PrintMethod(r.ActionMethod);
                await Helper.IsError(r.Head.Status);
            });
        }


        {
            Console.WriteLine($"[Client] ==> FCM");

            s = new Serialineitor()
                        .AddArray(TerbinURLs.MOD_FCM.ToCharArray());
            _ = pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize()).ContinueWith(async p =>
            {
                PacketRequest r = await p;
                Console.Log($"[Client] {c++} R (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
                if (await Helper.IsError(r.Head.Status)) return;
            });
        }


        await Helper.Fin();
    }


    private static Task<PacketRequest> download(TerbinCommunicator pCommunicator, Serialineitor s)
    {
        var r = pCommunicator.Communicate(new(CodeServices.Dowload, CodeServicesSection.Plugin), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r.Head.Status)) return;
        });
        return r;
    }
}
