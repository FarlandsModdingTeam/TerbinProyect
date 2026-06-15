using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Configuration;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful.Nodes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace TerbinService.Managers;
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



[TODO("Que el json no sea un Dictionary<string, string>, sino un objeto que dentro tenga el Diccionario.")]
public static partial class Manager
{
    public static class Configuration
    {
        private const string FOLDER = "config/";
        private const string JSON = "config.json";
        private const string KEY = "Config";

        public static event Action<string, string>? OnChangeConfig;

        private static readonly Lock _lockPredeterminated = new();
        private static readonly Lock _lockSetGet = new();

        public static string? GetConfg(string pKey)
        {
            lock (_lockSetGet)
                if (JSonUtil.Get(KEY) == null)
                    JSonUtil.Set(KEY, FOLDER);

            var r = JSonUtil.Acess<Dictionary<string, string>>(KEY, JSON);
            if (r == null)
            {
                setPredeterminatedConfig();
                r = JSonUtil.Acess<Dictionary<string, string>>(KEY, JSON);
                if (r == null)
                    return null;
            }

            if (r.TryGetValue(pKey, out string? value))
                return value;
            else
                return getPredeterminatedAndSave(pKey, r);
        }

        public static CodeAcessJSonSave SetConfig(string pKey, string pData)
        {
            Dictionary<string, string> data;
            if (JSonUtil.Acess<Dictionary<string, string>>(KEY, JSON) is var r && r != null)
                data = r;
            else
            {
                JSonUtil.Set(KEY, FOLDER);
                data = new();
            }

            data[pKey] = pData;

            CodeAcessJSonSave result;
            result = JSonUtil.Save(KEY, JSON, data);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                OnChangeConfig?.Invoke(pKey, pData);
            });
            return result;
        }


        private static void setPredeterminatedConfig()
        {
            lock (_lockPredeterminated)
            {
                var data = new Dictionary<string, string>();
                string? dirFarlands = ManagerFarlands.GetRuteSteamFarlands();
                if (dirFarlands != null)
                    data.Add(TerbinConfiguration.RUTE_FARLANDS, dirFarlands);

                data.Add(TerbinConfiguration.RUTE_INSTANCES, MakePathInstances());

                data.Add(TerbinConfiguration.RUTE_STORAGE_PLUGINS, MakePathStorage());

                lock (_lockSetGet)
                    JSonUtil.Set(KEY, FOLDER);
                JSonUtil.Save(KEY, JSON, data);
            }
        }

        public static string? GetPredeterminatedAndSave(string pKey)
        {
            var r = JSonUtil.Acess<Dictionary<string, string>>(KEY, JSON);
            if (r == null) return null;
            return getPredeterminatedAndSave(pKey, r);
        }
        private static string? getPredeterminatedAndSave(string pKey, Dictionary<string, string> pContent)
        {
            string? pre = GetPredeterminated(pKey);
            if (string.IsNullOrEmpty(pre)) return null;
            pContent.Add(pKey, pre);
            JSonUtil.Save(KEY, JSON, pContent);
            return pre;
        }

        public static string? GetPredeterminated(string pKey)
        {
            return pKey switch
            {
                TerbinConfiguration.RUTE_FARLANDS => ManagerFarlands.GetRuteSteamFarlands(),
                TerbinConfiguration.RUTE_INSTANCES => MakePathInstances(),
                TerbinConfiguration.RUTE_STORAGE_PLUGINS => MakePathStorage(),

                _ => null
            };
        }

        public static string MakePathInstances()
        {
            string d = GetPathDocument();
            return Path.Combine(d, "TerbinInstances");
        }

        public static string MakePathStorage()
        {
            string d = GetPathDocument();
            return Path.Combine(d, "TerbinStorage");
        }


        public static string GetPathDocument()
        {
            string d;
            d = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(d))
                d = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return d;
        }
    }
}
