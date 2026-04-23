using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinLibrary.Execution;

public delegate Task<InfoResponse?> TerbinExecutableHandler(Header pHead, byte[] pParameters);
