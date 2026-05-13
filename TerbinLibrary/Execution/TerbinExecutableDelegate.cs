using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication.Packets;

namespace TerbinLibrary.Execution;

public interface ITerbinExecutableDelegateStatic
{

}
public interface ITerbinExecutableDelegateNonStatic
{

}

public delegate Task<InfoResponse?> TerbinExecutableDelegate(Header pHead, byte[] pParameters);
