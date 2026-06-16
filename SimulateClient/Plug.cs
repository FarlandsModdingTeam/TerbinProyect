using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;

namespace SimulateClient;

internal class Plug : ITests
{
    public static Task LittleByLittle(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public static Task Yolo(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }


    public static async Task Add(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Plugin )---------\n" +
            $"[Client] \"1. Nombre de la Instancia | 2. GUID del plugin\"\n");
        string name = Helper.Read("1. name");
        string id = Helper.Read("2. GUID");

        PacketRequest r;
        r = await install(pCommunicator, name, id);


        await Helper.Fin();
    }
    private static Task<PacketRequest> install(TerbinCommunicator pCommunicator, string pName, string pId)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray())
                    .AddArray<char>(pId.ToCharArray())
                    .AddArray<char>(PathPlugin.ROOT.ToCharArray());// PathPlugin.BEPINEX_PLUGINS.ToCharArray()

        r = pCommunicator.Communicate(new(CodeServices.Install, CodeServicesSection.Plugin), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            Console.Succes("Instalado Correctamente");
        });
        return r;
    }

    public static async Task Rm(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Plugin )---------\n" +
            $"[Client] \"1. Nombre de la Instancia | 2. GUID del plugin\"\n");
        string name = Helper.Read("1. name");
        string id = Helper.Read("2. GUID");

        PacketRequest r;
        r = await remove(pCommunicator, name, id);


        await Helper.Fin();
    }
    private static Task<PacketRequest> remove(TerbinCommunicator pCommunicator, string pName, string pId)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray())
                    .AddArray<char>(pId.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Deleted, CodeServicesSection.Plugin), s.Serialize());
        r.ContinueWith(async p =>
        {
            PacketRequest r = await p;
            Console.Log($"[Client] Result (Action: {r.ActionMethod} | Status: {r.Head.Status} | Memory: {r.Head.IdMemory})");
            Helper.PrintMethod(r.ActionMethod);
            if (await Helper.IsError(r)) return;

            Console.Succes("Desinstalado Correctamente");
        });
        return r;
    }



    public static async Task GetAll(TerbinCommunicator pCommunicator)
    {
        Console.Write($"-------( Plugin )---------\n" +
            $"[Client] \"Nombre de la Instancia\"\n");
        string name = Helper.Read("Name");

        PacketRequest r;
        r = await getAll(pCommunicator, name);

        await Helper.Fin();
    }

    private static Task<PacketRequest> getAll(TerbinCommunicator pCommunicator, string pName)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray());
        r = pCommunicator.Communicate(new(CodeServices.ReadAll, CodeServicesSection.Plugin), s.Serialize());
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

                if (reader.IsEmpty)
                {
                    Console.Warn("[Client] reader.IsEmpty");
                    return;
                }

                ThreeQuartersInt length = reader.Read<ThreeQuartersInt>();
                Console.Warn($"Lenght: {length}");

                int offset = 0;
                Console.WriteLine("**( ManifestPluginDTO )**");
                for (int i = 0; i < length; i++)
                {
                    //ReferenceInstanceDTO tmp = new();
                    ManifestPluginDTO tmp = reader.ReadStruct<ManifestPluginDTO>(ref offset);
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
        Console.Write($"-------( Plugin )---------\n" +
            $"[Client] \"1. Nombre de la Instancia | 2. GUID del plugin\"\n");
        string name = Helper.Read("1. name");
        string id = Helper.Read("2. GUID");

        PacketRequest r;
        r = await getOne(pCommunicator, name, id);


        await Helper.Fin();
    }


    private static Task<PacketRequest> getOne(TerbinCommunicator pCommunicator, string pName, string pId)
    {
        Task<PacketRequest> r;
        Serialineitor s;

        s = new Serialineitor()
                    .AddArray<char>(pName.ToCharArray())
                    .AddArray<char>(pId.ToCharArray());

        r = pCommunicator.Communicate(new(CodeServices.Read, CodeServicesSection.Plugin), s.Serialize());
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
                if (reader.IsEmpty)
                    Console.Warn("[Client] reader.IsEmpty");

                Console.WriteLine("**( ManifestPluginDTO )**");
                //ReferenceInstanceDTO tmp = new();
                ManifestPluginDTO tmp = reader.ReadStruct<ManifestPluginDTO>(ref offset);
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
