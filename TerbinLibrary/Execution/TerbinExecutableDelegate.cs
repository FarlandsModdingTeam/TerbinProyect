using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinLibrary.Execution;

public interface ITerbinExecutableDelegateStatic
{

}
public interface ITerbinExecutableDelegateNonStatic
{

}

public delegate Task<InfoResponse?> TerbinExecutableDelegate(Header pHead, byte[] pParameters);
