using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Extension;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;

namespace TerbinService.Configuration;

internal static class ManagerConfiguration
{
    private const string FOLDER = "config/";
    private const string JSON = "config.json";
    private const string KEY = "Config";

    public static event Action<string, string>? OnChangeConfig;

    private static object _lockPredeterminated = new();

    public static string? GetConfg(string pKey)
    {
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
            return null;
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

        var result = JSonUtil.Save(KEY, JSON, data);
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            OnChangeConfig?.Invoke(pKey, pData);
        });
        return result;
    }

    // TODO: Tener ruta predeterminada para ambos.
    private static void setPredeterminatedConfig()
    {
        lock (_lockPredeterminated)
        {
            var data = new Dictionary<string, string>();
            string? dirFarlands = ManagerFarlands.GetRuteSteamFarlands();
            if (dirFarlands != null)
                data.Add(TerbinConfiguration.RUTE_FARLANDS, dirFarlands);
            // TODO: Conseguir ruta instancias predeterminadas.

            JSonUtil.Set(KEY, FOLDER);
            JSonUtil.Save(KEY, JSON, data);
        }
    }
}
