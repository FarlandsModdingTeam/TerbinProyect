using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication;

// methods:
public enum CodeMethodsTerbinService : byte
{
    CreateInstance = 10,
    DeleteInstance = 11,

    //ChangeFarladsRute = 100,
    //ChangeInstancesRute = 101,
}

public enum CodeServices : byte
{
    Farland = 10,

    Mods = 20,

    Instances = 30,

    FCM = 40,


    Rute = 110,
}