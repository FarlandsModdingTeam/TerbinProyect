using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;

// methods:
public enum CodeService : byte
{
    InstallBepInEx = 10,
}

public enum CodeSubServices : byte
{
    Farland = 10,

    Mods = 20,

    Instances = 30,

    FCM = 40,

    Rute = 50,
    Rute_Antiguo_Obsoleto_MagincianPuto = 110,
}


public enum CodeInternalErrors : ushort
{
    // Farland = 100,

    // Mods = 200,

    // Instances = 300,

    // FCM = 400,

    // Rute = 500,
    RuteErrorSerialize = 501,
    RuteAccesNullOrNotExist = 502,

    // BepInEx = 600,
    NotConectToBepInEx = 601,
}