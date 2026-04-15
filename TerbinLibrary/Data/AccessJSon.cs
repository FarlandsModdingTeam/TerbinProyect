using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Resources;

namespace TerbinLibrary.Data;

public class AcessJSon
{
    private static Dictionary<string, string> _places = new Dictionary<string, string>();

    public static string? Get(string pKey)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKey))
                return _places[pKey];
        }
        return null;
    }

    public static void Set(string pKey, string pPlace)
    {
        lock (_places)
        {
            _places[pKey] = pPlace;
        }
    }


    public enum CodeAcessJSonDel : sbyte
    {
        NotExistKey = -1,
        Fail = 0,
        Succes = 1,
    }
    public static CodeAcessJSonDel Del(string pKey)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKey))
                return (CodeAcessJSonDel)_places.Remove(pKey).ToSByte();
            return CodeAcessJSonDel.NotExistKey;
        }
    }

    public static T? Acess<T>(string pKey, string pFile) where T : class
    {
        string? dir = getDir(pKey);
        if (dir == null)
            return null;

        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(dir, fileName);
        if (!File.Exists(routeComplete)) return null;

        string json = File.ReadAllText(routeComplete);
        if (json == null) return null;

        return JsonConvert.DeserializeObject<T>(json);
    }


    public enum CodeAcessJSonSave : sbyte
    {
        ErrorSerialize = -1,
        NotExistKey = 0,
        Succes = 1,
    }
    public static CodeAcessJSonSave Save<T>(string pKey, string pFile, T pContent) where T : class
    {
        string? dir = getDir(pKey);
        if (dir == null)
            return CodeAcessJSonSave.NotExistKey;

        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(dir, fileName);

        string json = JsonConvert.SerializeObject(pContent); // Formatting.Indented
        if (json == null) return CodeAcessJSonSave.ErrorSerialize;

        File.WriteAllText(routeComplete, json);
        return CodeAcessJSonSave.Succes;
    }

    private static string? getDir(string pKey)
    {
        lock (_places)
        {
            _places.TryGetValue(pKey, out var dir);
            return dir;
        }
    }

    private static string getFileName(string pFile)
    {
        return pFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? pFile : pFile + ".json";
    }
}