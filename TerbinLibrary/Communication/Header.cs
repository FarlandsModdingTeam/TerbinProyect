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
public struct Capsule
{
    public Guid IdClient;
    public CodeAction Action;
    public CodeStatus Status;
}
