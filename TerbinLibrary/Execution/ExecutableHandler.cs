using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinLibrary.Execution;

public delegate Task<PacketRequest> ExecutableHandler(Header pHead, byte[] pParameters);
