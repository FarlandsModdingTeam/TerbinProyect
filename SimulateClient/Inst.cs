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


    public static async Task GetAll(TerbinCommunicator pCommunicator)
    {
        PacketRequest r;
        r = await getAll(pCommunicator);

        await Helper.Fin();
    }

    private static Task<PacketRequest> getAll(TerbinCommunicator pCommunicator)
    {
        Task<PacketRequest> r;

        r = pCommunicator.Communicate(new(CodeServices.ReadAll, CodeServicesSection.Instances), []);
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            //if (await Helper.IsError(r)) return;
            await Helper.IsError(r);

            try
            {
                List<ReferenceInstanceDTO> dto = new();
                ReadOnlySpan<byte> reader = r.Payload;

                ThreeQuartersInt length = reader.Read<ThreeQuartersInt>();

                int offset = 0;
                Console.WriteLine("**( ReferenceInstanceDTO )**");
                for (int i = 0; i < length; i++)
                {
                    //ReferenceInstanceDTO tmp = new();
                    ReferenceInstanceDTO tmp = reader.ReadStruct<ReferenceInstanceDTO>(ref offset);
                    //tmp.ReadFrom(reader);

                    Console.WriteLine($"""
                    Name: {tmp.Name};
                    OutSide: {tmp.OutSide};
                    """);
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
        Console.Write($"-------( Create-Instance )---------\n" +
            $"[Client] \"Nombre de la Instancia\" \n");
        string name = Helper.Read("name");


        PacketRequest r;
        r = await getOne(pCommunicator, name);


        await Helper.Fin();
    }


    private static Task<PacketRequest> getOne(TerbinCommunicator pCommunicator, string pName)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Read, CodeServicesSection.Instances), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            //if (await Helper.IsError(r)) return;
            await Helper.IsError(r);

            try
            {
                ReadOnlySpan<byte> reader = r.Payload;

                Console.WriteLine("**( ReferenceInstanceDTO )**");
                //ReferenceInstanceDTO tmp = new();
                ReferenceInstanceDTO tmp = reader.ReadStruct<ReferenceInstanceDTO>();
                //tmp.ReadFrom(reader);

                Console.WriteLine($"""
                    Name: {tmp.Name};
                    OutSide: {tmp.OutSide};
                """);
            }
            catch (Exception e)
            {
                e.PrintException("getOne");
            }

        });
        return r;
    }


}
