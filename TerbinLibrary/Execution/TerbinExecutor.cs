using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;

public class TerbinExecutor
{
    TerbinCommunicator _communicator;

    public TerbinExecutor(TerbinCommunicator pCommunicator)
    {
        this._communicator = pCommunicator;

        ExecutableDispatcher.RegisterAll();
    }


    public async Task<PacketRequest> Execution(PacketRequest pRequest)
    {
        var capR = await ExecutableDispatcher.DispatchAsync(pRequest);
        _ = Reply(capR);
        return capR;
    }

    public async Task Reply(PacketRequest pRequest)
    {
        await this._communicator.Reply(pRequest);
    }



    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<PacketRequest> Load(Header pHead, MemoryStream pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemory.Store(pHead.IdMemory, pHead.OrderRequest, pParameters.ToArray());
        }


        throw new NotImplementedException("Ñe"); 
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<PacketRequest> Solicit(Header pHead, MemoryStream pParameters)
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