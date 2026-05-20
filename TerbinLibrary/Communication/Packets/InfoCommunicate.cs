using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Id;
using TerbinLibrary.Protocol;

namespace TerbinLibrary.Communication.Packets;


public ref struct InfoCommunicateResponse
{
    public Task<PacketRequest> GetResult()
    {
        throw new NotImplementedException();
    }
}

// No se puede utilizar ref en asincrono, (los ref struc solo viven en Stack y no en Heap).
public struct InfoCommunicate : IInfo
{
    //private ref InfoCommunicateResponse _response;

    public InfoCommunicate()
    {
    }

    public void InfoSend(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }

    public Task<PacketRequest?> InfoSendAsync(TerbinCommunicator pCommunicator)
    {
        throw new NotImplementedException();
    }


    public InfoCommunicate SoliciteRequestMemory(ref InfoCommunicateResponse pC)
    {
        // TODO: Rellenar de todos los datos necesarios para Solicitar el id.
        // TODO: Guardarme el ref.
        return new();
    }
}
