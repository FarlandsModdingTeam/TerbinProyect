using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;

namespace SimulateClient;

internal class Game : ITests
{
    public static Task LittleByLittle(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public static Task Yolo(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }


    // C:\Program Files (x86)\Steam\steamapps\common\Farlands
    public static async Task Dup(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Game )---------\n" +
            $"[Client] \"1. Name Instance | 2. Path Game\"");
        string name = Helper.Read("1. Name");
        string path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Farlands";

        PacketRequest r;
        r = await duplicate(pCommunicator, name, path);


        await Helper.Fin();
    }


    private static Task<PacketRequest> duplicate(TerbinCommunicator pCommunicator, string pName, string pPath)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray())
                    .AddArray<char>(pPath.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Duplicate, CodeServicesSection.Game), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            Console.Succes("Duplicado Correctamente");

        });
        return r;
    }

    public static async Task Rm(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Game )---------\n" +
            $"[Client] \"Name Instance\"");
        string name = Helper.Read("1. Name");

        PacketRequest r;
        r = await deleted(pCommunicator, name);

        await Helper.Fin();
    }


    private static Task<PacketRequest> deleted(TerbinCommunicator pCommunicator, string pName)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Deleted, CodeServicesSection.Game), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            Console.Succes("Eliminado Correctamente");
        });
        return r;
    }
}
