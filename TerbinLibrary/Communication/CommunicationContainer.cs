using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TerbinLibrary.Communication;
public enum CodeAction : byte
{
    Stop = 0,
    PlaceHolder = 1,
}
public enum CodeStatus : short
{
    NotAsign = -1,

    Succes = 200,

    NotFound = 404,

    //etc...
}

[StructLayout(LayoutKind.Sequential)]
public struct Header
{
    public Guid IdClient;
    public CodeAction Action;
    public CodeStatus Status;
}

[StructLayout(LayoutKind.Sequential)]
public struct Capsule
{
    public Header Head;
    public string Method;
    public byte[] Parameters;
}


public class HeaderHelper // nombre provisional, necesito sugerencias.
{ 

}