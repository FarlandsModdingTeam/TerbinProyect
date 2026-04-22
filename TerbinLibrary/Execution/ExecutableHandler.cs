using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinLibrary.Execution;

public delegate Task<InfoResponse?> ExecutableHandler(Header pHead, byte[] pParameters);
