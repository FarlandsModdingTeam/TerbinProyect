using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace SimulateClient;

internal interface ITests
{
    static abstract Task Yolo(TerbinCommunicator pCommunicator);
    static abstract Task LittleByLittle(TerbinCommunicator pCommunicator);
}
