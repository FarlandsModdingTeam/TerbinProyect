using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;

public class TerbinExecutor
{
    //TerbinCommunicator _communicator;

    public TerbinExecutor(Assembly pAssembly)
    {
        RegisterAll(pAssembly);
    }
    public TerbinExecutor()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly()); // Terbin Library
    }

    // TODO: perdirle a Luis que sea mi tutor.

    public static void RegisterAll(Assembly pAssembly)
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly()); // Terbin Library
        ExecutableDispatcher.RegisterFromAssembly(pAssembly); // Externo

        // TerbinExecutableCRUDManager.RegisterFromAssembly(Assembly.GetExecutingAssembly()); // no hay internos
        TerbinExecutableCRUDManager.RegisterFromAssembly(pAssembly);
    }

    public async Task<PacketRequest> Execution(PacketRequest pRequest)
    {
        var capR = await ExecutableDispatcher.DispatchAsync(pRequest);
        //_ = Reply(capR);
        return capR;
    }

    //public async Task Reply(PacketRequest pRequest)
    //{
    //    await this._communicator.Reply(pRequest);
    //}


    //public void Prueba(byte[]? b = null, char[]? c = null)
    //{
    //    Prueba(c:['1', '2', '3']);
    //}

    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<PacketRequest> Load(Header pHead, byte[] pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemory.Store(pHead.IdMemory, pHead.OrderRequest, pParameters);
        }


        pHead.Status = CodeStatus.Succes;
        return new PacketRequest(pHead: pHead, pActionMethod: (byte)CodeTerbinProtocol.None);
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<PacketRequest> Solicit(Header pHead, byte[] pParameters)
    {
        if (pHead.IdMemory == (byte)CodeTerbinMemory.New)
        {
            byte id = TerbinMemory.GetStore();
            pHead.Status = CodeStatus.Succes;
            return new PacketRequest(pHead: pHead, pIdMemory: id);
        }

        pHead.Status = CodeStatus.NotFound;
        return new PacketRequest(pHead: pHead, pActionMethod: (byte)CodeTerbinProtocol.Info);
    }



    [TerbinExecutable((byte)CodeTerbinProtocol.Create)]
    public static async Task<PacketRequest> Create(Header pHead, byte[] pParameters)
    {
        PacketRequest r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Create, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<PacketRequest> Read(Header pHead, byte[] pParameters)
    {
        PacketRequest r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Read, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<PacketRequest> Update(Header pHead, byte[] pParameters)
    {
        PacketRequest r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Update, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<PacketRequest> Deleted(Header pHead, byte[] pParameters)
    {
        PacketRequest r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Deleted, pParameters);
        return r;
    }



    // ya ni me acuerdo para que era.
    [TerbinExecutable((byte)CodeTerbinProtocol.Cancel)]
    public static async Task<PacketRequest> Cancel(Header pHead, byte[] pParameters)
    {

        throw new NotImplementedException("Ñe");
    }
}