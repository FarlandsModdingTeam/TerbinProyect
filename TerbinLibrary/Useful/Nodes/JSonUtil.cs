using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using TerbinLibrary.Extension;

namespace TerbinLibrary.Useful.Nodes;
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

/*
 Recuerdo que por X razon no podia utilizar System.Text.Json pero no recuerdo.
 */


[TODO("Hacer los Acces y Saves con patrón Try.")]
public class JSonUtil
{
    private static readonly ConcurrentDictionary<string, string> _places = new();
    private static readonly ConcurrentDictionary<string, Lock> _fileLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Lock> _updateLocks = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerSettings _settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    private static Lock getFileLock(string pFilePath)
    {
        lock (_fileLocks)
        {
            if (!_fileLocks.TryGetValue(pFilePath, out var fileLock))
            {
                fileLock = new Lock();
                _fileLocks[pFilePath] = fileLock;
            }
            return fileLock;
        }
    }

    private static Lock getUpdateLock(string pPath, string pFile)
    {
        string key = pPath + "|" + pFile;
        lock (_updateLocks)
        {
            if (!_fileLocks.TryGetValue(key, out var updateLock))
            {
                updateLock = new Lock();
                _fileLocks[key] = updateLock;
            }
            return updateLock;
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
                return (CodeAcessJSonDel)_places.TryRemove(pKeyDir, out _ ).ToSByte();
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

        lock (getFileLock(routeComplete))
        {
            if (!File.Exists(routeComplete)) return null;

            string json = File.ReadAllText(routeComplete);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
    public static T? AcessDirect<T>(string pPath) where T : class
    {
        string dir = Path.GetDirectoryName(pPath) ?? throw new Exception("Not Acces to Path");
        string file = Path.GetFileName(pPath);
        return AcessDirect<T>(dir, file);
    }
    public static T? AcessDirect<T>(string pDir, string pFile) where T : class
    {
        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(pDir, fileName);

        lock (getFileLock(routeComplete))
        {
            if (!File.Exists(routeComplete)) return null;

            string json = File.ReadAllText(routeComplete);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }


    public static CodeAcessJSonSave Save<T>(string pKeyDir, string pFile, T pContent) where T : class
    {
        string? dir = getDir(pKeyDir);
        if (dir == null)
            return CodeAcessJSonSave.NotExistKey;

        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(dir, fileName);

        lock (getFileLock(routeComplete))
        {
            if (!File.Exists(routeComplete))
                Directory.CreateDirectory(dir);

            string json = JsonConvert.SerializeObject(pContent, Formatting.Indented, _settings);
            if (json == null) return CodeAcessJSonSave.ErrorSerialize;

            File.WriteAllText(routeComplete, json);
            return CodeAcessJSonSave.Succes;
        }
    }


    public static CodeAcessJSonSave SaveDirect<T>(string pDir, string pFile, T pContent) where T : class
    {
        string fileName = getFileName(pFile);

        string routeComplete = Path.Combine(pDir, fileName);

        lock (getFileLock(routeComplete))
        {
            if (!File.Exists(routeComplete))
                Directory.CreateDirectory(pDir);

            string json = JsonConvert.SerializeObject(pContent, Formatting.Indented, _settings);
            if (json == null) return CodeAcessJSonSave.ErrorSerialize;

            File.WriteAllText(routeComplete, json);
            return CodeAcessJSonSave.Succes;
        }
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



    /// <summary>
    /// Carga un JSON, ejecuta las modificaciones dadas y lo guarda automáticamente.
    /// </summary>
    public static CodeAcessJSonSave Update<T>(string pKeyDir, string pFile, Action<T> updateAction) where T : class, new()
    {
        string? dir = getDir(pKeyDir);
        if (dir == null)
            return CodeAcessJSonSave.NotExistKey;

        return UpdateDirect(dir, pFile, updateAction);
    }

    /// <summary>
    /// Carga un JSON, ejecuta las modificaciones dadas y lo guarda automáticamente.
    /// </summary>
    public static CodeAcessJSonSave UpdateDirect<T>(string pDir, string pFile, Action<T> updateAction) where T : class, new()
    {
        lock (getUpdateLock(pDir, pFile))
        {
            T data = AcessDirect<T>(pDir, pFile) ?? new T();

            updateAction(data);

            return SaveDirect(pDir, pFile, data);
        }
    }


    // ********************( Prototipos )******************** //

    public string? this[string pKeyDir] // XD
    {
        get => Get(pKeyDir);
        set
        {
            if (value != null) Set(pKeyDir, value);
        }
    }

    public static string ToJson<T>(T? pObj, Formatting? pFor = null, JsonSerializerSettings? pSettings = null)
    {
        pFor ??= Formatting.Indented;
        pSettings ??= _settings;
        return JsonConvert.SerializeObject(pObj, pFor.Value, pSettings);
    }
}