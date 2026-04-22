using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;

// TODO: (Verano) Mover a NO estatico para permitir empalmes.
public static class TerbinExecutor
{
    //TerbinCommunicator _communicator;

    public static void Init()
    {
        RegisterAll();
    }

    // TODO: perdirle a Luis que sea mi tutor.

    public static void RegisterAll()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        //ExecutableDispatcher.RegisterFromAssembly(pAssembly);
        TerbinExecutableCRUDManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        //TerbinExecutableCRUDManager.RegisterFromAssembly(pAssembly);
    }

    public static async Task<PacketRequest?> Execution(PacketRequest pRequest)
    {
        var capR = await ExecutableDispatcher.DispatchAsync(pRequest);
        return capR;
    }


    //public void Prueba(byte[]? b = null, char[]? c = null)
    //{
    //    Prueba(c:['1', '2', '3']);
    //}

    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<PacketRequest?> Load(Header pHead, byte[] pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemory.Store(pHead.IdMemory, pHead.OrderRequest, pParameters);
        }


        pHead.Status = CodeStatus.Succes;
        return new PacketRequest(pHead: pHead, pActionMethod: (byte)CodeTerbinProtocol.Response);
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<PacketRequest?> Solicit(Header pHead, byte[] pParameters)
    {
        if (pHead.IdMemory == (byte)CodeTerbinMemory.New)
        {
            byte id = TerbinMemory.GetStore();
            pHead.Status = CodeStatus.Succes;
            return new PacketRequest(pHead: pHead, pIdMemory: id);
        }

        return null;
    }



    [TerbinExecutable((byte)CodeTerbinProtocol.Create)]
    public static async Task<PacketRequest?> Create(Header pHead, byte[] pParameters)
    {
        PacketRequest? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Create, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<PacketRequest?> Read(Header pHead, byte[] pParameters)
    {
        PacketRequest? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Read, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<PacketRequest?> Update(Header pHead, byte[] pParameters)
    {
        PacketRequest? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Update, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<PacketRequest?> Deleted(Header pHead, byte[] pParameters)
    {
        PacketRequest? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Deleted, pParameters);
        return r;
    }



    // ya ni me acuerdo para que era.
    [TerbinExecutable((byte)CodeTerbinProtocol.Cancel)]
    public static async Task<PacketRequest?> Cancel(Header pHead, byte[] pParameters)
    {

        throw new NotImplementedException("Ñe");
    }
}