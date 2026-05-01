using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Resources;
using TerbinLibrary.Extension;

namespace TerbinLibrary.Data;

/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */


public enum CodeAcessJSonDel : sbyte
{
    NotExistKey = -1,
    Fail = 0,
    Succes = 1,
}
public enum CodeAcessJSonSave : sbyte
{
    ErrorSerialize = -1,
    NotExistKey = 0,
    Succes = 1,
}

public class AcessJSon
{
    private static Dictionary<string, string> _places = new Dictionary<string, string>();

    public string? this[string pKeyDir] // XD
    {
        get => Get(pKeyDir);
        set
        {
            if (value != null) Set(pKeyDir, value);
        }
    }

    public static string? Get(string pKeyDir)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKeyDir))
                return _places[pKeyDir];
        }
        return null;
    }

    public static void Set(string pKeyDir, string pPlace)
    {
        lock (_places)
        {
            // Segun copilot es suficientemente inteligente como añadirlo si no existe y sustituirlo si existe (Segun copilot).
            _places[pKeyDir] = pPlace;
        }
    }


    public static CodeAcessJSonDel Del(string pKeyDir)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKeyDir))
                return (CodeAcessJSonDel)_places.Remove(pKeyDir).ToSByte();
            return CodeAcessJSonDel.NotExistKey;
        }
    }

    public static T? Acess<T>(string pKeyDir, string pFile) where T : class
    {
        string? dir = getDir(pKeyDir);
        if (dir == null)
            return null;

        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(dir, fileName);
        if (!File.Exists(routeComplete)) return null;

        string json = File.ReadAllText(routeComplete);
        if (json == null) return null;

        return JsonConvert.DeserializeObject<T>(json);
    }


    public static CodeAcessJSonSave Save<T>(string pKeyDir, string pFile, T pContent) where T : class
    {
        string? dir = getDir(pKeyDir);
        if (dir == null)
            return CodeAcessJSonSave.NotExistKey;

        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(dir, fileName);
        if (!File.Exists(routeComplete))
            Directory.CreateDirectory(dir);

        string json = JsonConvert.SerializeObject(pContent); // Formatting.Indented
        if (json == null) return CodeAcessJSonSave.ErrorSerialize;

        File.WriteAllText(routeComplete, json);
        return CodeAcessJSonSave.Succes;
    }


    public static CodeAcessJSonSave SaveDirect<T>(string pDir, string pFile, T pContent) where T : class
    {
        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(pDir, fileName);
        if (!File.Exists(routeComplete))
            Directory.CreateDirectory(pDir);

        string json = JsonConvert.SerializeObject(pContent); // Formatting.Indented
        if (json == null) return CodeAcessJSonSave.ErrorSerialize;

        File.WriteAllText(routeComplete, json);
        return CodeAcessJSonSave.Succes;
    }

    private static string? getDir(string pKeyDir)
    {
        lock (_places)
        {
            _places.TryGetValue(pKeyDir, out var dir);
            return dir;
        }
    }

    private static string getFileName(string pFile)
    {
        return pFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? pFile : pFile + ".json";
    }
}