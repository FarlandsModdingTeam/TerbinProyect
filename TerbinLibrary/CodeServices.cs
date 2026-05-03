using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;

namespace TerbinLibrary;

// methods:
public enum CodeServices : byte
{
    InstallBepInEx = 10,

    Plugin_Tests = 20,

    ReadAllInstances = 30,

    WIP_NewService = 255,
}

public enum CodeSubServices : byte
{
    Game = 10,

    Plugin = 20,

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
    FarlandRuteNotExist = 101,

    // Mods = 200,
    PluginNotConect = 201,

    // Instances = 300,
    InstaceGetSizeError = 301,
    InstaceNotExistOrConfigError = 302,
    InstaceNotExit = 303,

    // FCM = 400,

    // Rute = 500,
    RuteSerializeError = 501,
    RuteAccesNullOrNotExist = 502,

    // BepInEx = 600,
    BepInExNotConect = 601,
    BepInExNotInstall = 602,

    // Zip = 1000,
    ZipExtractError = 1001,
    ZipExtractException = 1002,
    ZipDeletedTempException = 1003,
}

public class TSHelper
{
    public static byte[] GetError(CodeInternalErrors pError)
    {
        return Serialineitor.Serialize((ushort)pError);
    }

    public static IProgress<TerbinInfoProgrss> CreateProgessBarr(TerbinCommunicator pCommunicator, byte pIdMemory)
    {
        if (pIdMemory <= TerbinProtocol.RESERVE_MEMORY)
            throw new OverflowException($"Id memory is reserved! {pIdMemory}");
        return new Progress<TerbinInfoProgrss>(p =>
        {
            _ = pCommunicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, p.ToArray());
        });
    }
}