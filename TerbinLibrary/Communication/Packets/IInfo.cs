using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication.Packets;

public interface IInfo
{
    void InfoSend(TerbinCommunicator pCommunicator);
    Task<PacketRequest?> InfoSendAsync(TerbinCommunicator pCommunicator);
}
