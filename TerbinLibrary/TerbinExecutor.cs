using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Executables;

namespace TerbinLibrary;

public class TerbinExecutor
{
    TerbinCommunicator _communicator;

    public TerbinExecutor(TerbinCommunicator pCommunicator)
    {
        this._communicator = pCommunicator;


        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
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
}
