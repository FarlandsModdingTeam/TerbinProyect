using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Serialize;
using TerbinLibrary.Protocol;

namespace TerbinLibrary;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


// methods:
public enum CodeServices : byte
{
    Info = 10,
    Alert = 11,

    Execute = 12,
    Dowload = 13,
    Install = 14,

    //InstallBepInEx = 12,
    //Plugin_Tests = 20,
    //ReadAllInstances = 30,
    //WIP_NewService = 255,

    ReadAll = TerbinCRUD.ReadAll,
    Duplicate = TerbinCRUD.Duplicate,

    Create = TerbinCRUD.Create,
    Read = TerbinCRUD.Read,
    Update = TerbinCRUD.Update,
    Deleted = TerbinCRUD.Deleted,
}

public enum CodeServicesSection : byte
{
    Game = 10,

    Plugin = 20,
    PluginStorage = 21,

    Instances = 30,

    FCM = 40,

    Rute = 50,
}

[Obsolete("User CodeServicesSection instead")]
public enum CodeSubServices : byte
{
    Game = 10,

    Plugin = 20,
    PluginStorage = 21,

    Instances = 30,

    FCM = 40,

    Rute = 50,
    Rute_Antiguo_Obsoleto_MagincianPuto = 110,
}

public enum CodeServicesClient : byte
{
    SetMaxProgress = 10,
    SetBarProgress = 11,
}

public enum CodeManifestError : byte
{
    NotAccesName = 2,
    NotAccesId = 3,
    NotAccesIdLocal = 4,
    NotAccesPath = 5,
}


[Obsolete]
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
    PluginOnDowload = 202,
    PluginNotSuchSpace = 203,
    PluginInvalidURL = 204,
    PluginNotExist = 205,
    PluginGet = 206,
    PluginGetPath = 207,
    PluginOnSave = 208,
    PluginGetManifest = 209,

    // Instances = 300,
    InstaceGetSizeError = 301,
    InstaceNotExist = 302,

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

