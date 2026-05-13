using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;

namespace TerbinLibrary;

// methods:
public enum CodeServices : byte
{
    Info = 10,
    Alert = 11,

    InstallBepInEx = 12,

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

    public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
    {
        return ExceptionExtension.CreateStringException(pE, pSite);
    }

    public static class Debug
    {
        public static void PrintHas(params object[] pObjs)
        {
            string allHas = "";
            for (int i = 0; i < pObjs.Length; i++)
            {
                allHas += pObjs[i].GetType() + ": ";
                allHas += pObjs[i].GetHashCode() + ((i < pObjs.Length) ? "; \n" : "");
            }
            Console.Log($"{allHas}");
        }
        public static void PrintAllHas(params object[] pObjs)
        {
            string allHas = string.Join("; \n", pObjs.Select(o => $"{o.GetType()}: {o.GetHashCode()}"));
            Console.Log($"{allHas}");
        }
        public static void PrintHas(object pObj)
        {
            Console.Log($"{pObj.GetType()}: {pObj.GetHashCode()}");
        }


    }
}

public static class ConsoleExtension
{
    private static readonly object _lock = new();
    extension (Console)
    {
        public static void Log(string pMsg)
        {
            print(pMsg, ConsoleColor.Cyan);
        }

        public static void Warn(string pMsg)
        {
            print(pMsg, ConsoleColor.Yellow);
        }

        public static void Error(string pMsg)
        {
            print(pMsg, ConsoleColor.Red);
        }

        private static void print(string pMsg, ConsoleColor pColor)
        {
            var old = Console.ForegroundColor;

            Console.ForegroundColor = pColor;

            lock (_lock)
                Console.WriteLine(pMsg);

            Console.ForegroundColor = old;
        }
    }
}

public static class ExceptionExtension
{
    extension (Exception)
    {
        public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
        {
            return
            $$"""
            [{{pSite}}] =>
            {
                Message: {{pE.Message}};
                Source: {{pE.Source ?? "N/A"}};
                Inner: {{pE.InnerException?.Message ?? "N/A"}};
                Trace: {{pE.StackTrace ?? "N/A"}};
                String: {{pE.ToString()}}
            }
            """;
        }
    }

    public static string CrString(this Exception pE, string pSite = "ExceptionError")
    {
        return Exception.CreateStringException(pE, pSite);
    }
    public static void PrintException(this Exception pE, string pSite = "ExceptionError")
    {
        string e = Exception.CreateStringException(pE, pSite);
        Console.Error(e);
    }
}