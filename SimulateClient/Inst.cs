using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace SimulateClient;

/// <summary>
/// Instance
/// </summary>
internal class Inst : ITests
{
    public static async Task LittleByLittle(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public static async Task Yolo(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }






    public static async Task Create(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Create-Instance )---------\n" +
            $"[Client] \"Nombre de la Instancia\" \n");
        string name = Helper.Read("name");


        PacketRequest r;
        r = await create(pCommunicator, name);


        await Helper.Fin();
    }
    private static Task<PacketRequest> create(TerbinCommunicator pCommunicator, string pName)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray());
        r = pCommunicator.Communicate(new(CodeServices.Create, CodeServicesSection.Instances), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            await Helper.IsError(r);
        });
        return r;
    }





}
