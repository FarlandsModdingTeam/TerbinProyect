using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;

// methods:
public enum CodeServices : byte
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

public enum TypeService : byte
{
    Service = 1,
    SubService = 2,
}

public enum CodeInternalErrors : ushort
{
    IdSoliciteError = 11,
    TODO_WIP = 12,
    TODO_SoliciteInfo = 13,

    // Farland = 100,

    // Mods = 200,

    // Instances = 300,

    // FCM = 400,

    // Rute = 500,
    RuteSerializeError = 501,
    RuteAccesNullOrNotExist = 502,

    // BepInEx = 600,
    BepInExNotConect = 601,

    // Zip = 1000,
    ZipExtractError = 1001,
    ZipExtractException = 1002,
    ZipDeletedTempException = 1003,
}