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

internal class Store : ITests
{
    public static Task LittleByLittle(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public static Task Yolo(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }


    public static async Task GetAll(TerbinCommunicator pCommunicator)
    {
        PacketRequest r;
        r = await getAll(pCommunicator);

        await Helper.Fin();
    }

    private static Task<PacketRequest> getAll(TerbinCommunicator pCommunicator)
    {
        Task<PacketRequest> r;

        r = pCommunicator.Communicate(new(CodeServices.ReadAll, CodeServicesSection.PluginStorage), []);
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            try
            {
                //List<ReferenceInstanceDTO> dto = new();
                ReadOnlySpan<byte> reader = r.Payload;

                ThreeQuartersInt length = reader.Read<ThreeQuartersInt>();

                int offset = 0;
                Console.WriteLine("**( ReferenceInstanceDTO )**");
                for (int i = 0; i < length; i++)
                {
                    //ReferenceInstanceDTO tmp = new();
                    ReferencePluginStoreDTO tmp = reader.ReadStruct<ReferencePluginStoreDTO>(ref offset);
                    //tmp.ReadFrom(reader);

                    tmp.Print();
                    Console.WriteLine("----------");
                }
            }
            catch (Exception e)
            {
                e.PrintException("getAll");
            }

        });
        return r;
    }

    public static async Task GetOne(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Store )---------\n" +
            $"[Client] \"GUID del plugin\" \n");
        string pId = Helper.Read("ID");


        PacketRequest r;
        r = await getOne(pCommunicator, pId);


        await Helper.Fin();
    }


    private static Task<PacketRequest> getOne(TerbinCommunicator pCommunicator, string pId)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pId.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Read, CodeServicesSection.PluginStorage), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            int offset = 0;
            try
            {
                ReadOnlySpan<byte> reader = r.Payload;

                Console.WriteLine("**( ReferenceInstanceDTO )**");
                //ReferenceInstanceDTO tmp = new();
                ReferencePluginStoreDTO tmp = reader.ReadStruct<ReferencePluginStoreDTO>(ref offset);
                //tmp.ReadFrom(reader);

                tmp.Print();
            }
            catch (Exception e)
            {
                e.PrintException("getOne");
            }

        });
        return r;
    }

}
