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

    public TerbinExecutor(/*TerbinCommunicator pCommunicator*/)
    {
        //this._communicator = pCommunicator;

        // ExecutableDispatcher.RegisterAll();
    }
    // TODO: perdirle a Luis que sea mi tutor.

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
    public static async Task<PacketRequest> Load(Header pHead, MemoryStream pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemory.Store(pHead.IdMemory, pHead.OrderRequest, pParameters.ToArray());
        }


        pHead.Status = CodeStatus.NotFound;
        return new PacketRequest(pHead: pHead, pActionMethod: (byte)CodeTerbinProtocol.Info);
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<PacketRequest> Solicit(Header pHead, MemoryStream pParameters)
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
    public static async Task<PacketRequest> Create(Header pHead, MemoryStream pParameters)
    {

        throw new NotImplementedException("Ñe");
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<PacketRequest> Read(Header pHead, MemoryStream pParameters)
    {

        throw new NotImplementedException("Ñe");
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<PacketRequest> Update(Header pHead, MemoryStream pParameters)
    {

        throw new NotImplementedException("Ñe");
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<PacketRequest> Deleted(Header pHead, MemoryStream pParameters)
    {

        throw new NotImplementedException("Ñe");
    }


    // ya ni me acuerdo para que era.
    [TerbinExecutable((byte)CodeTerbinProtocol.Cancel)]
    public static async Task<PacketRequest> Cancel(Header pHead, MemoryStream pParameters)
    {

        throw new NotImplementedException("Ñe");
    }
}