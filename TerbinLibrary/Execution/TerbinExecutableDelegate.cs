using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinLibrary.Execution;

public delegate Task<InfoResponse?> TerbinExecutableDelegate(Header pHead, byte[] pParameters);
