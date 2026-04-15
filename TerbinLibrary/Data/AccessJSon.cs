using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Resources;
using System.IO;

namespace TerbinLibrary.Data;

public class AcessJSon
{
    private static Dictionary<string, string> _places = new Dictionary<string, string>();

    public static string Get(string pKey)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKey))
                return _places[pKey];
        }
        return "null";
    }

    public static void Set(string pKey, string pPlace)
    {
        lock (_places)
        {
            if (!_places.ContainsKey(pKey))
                _places.Add(pKey, pPlace);
            else
                _places[pKey] = pPlace;
        }
    }

    public static sbyte Del(string pKey)
    {
        lock (_places)
        {
            if (_places.ContainsKey(pKey))
                return _places.Remove(pKey).ToSByte();
            return -1;
        }
    }

    public static T? Acess<T>(string pKey, string pFile) where T : class
    {
        Dictionary<string, string> places;
        lock (_places)
        {
            places = new(_places);
            // places = [with(_places)]; // ¿Como que "preview?
        }

        string fileName = getFileName(pFile);

        if (places.TryGetValue(pKey, out var dir))
        {
            string routeComplete = Path.Combine(dir, fileName);
            if (!File.Exists(routeComplete)) return null;

            string json = File.ReadAllText(routeComplete);
            if (json == null) return null;

            return JsonConvert.DeserializeObject<T>(json);
        }
        return null;
    }

    public static sbyte Save<T>(string pKey, string pFile, T pContent) where T : class
    {
        Dictionary<string, string> places;
        lock (_places)
        {
            places = new (_places);
            // places = [with(_places)];
        }

        string fileName = getFileName(pFile); 

        if (places.TryGetValue(pKey, out var dir))
        {
            string routeComplete = Path.Combine(dir, fileName);
            if (!File.Exists(routeComplete)) return -1;

            string json = JsonConvert.SerializeObject(pContent);
            if (json == null) return -1;

            File.WriteAllText(routeComplete, json);
            return 1;
        }
        return 0;
    }

    private static string getFileName(string pFile)
    {
        return pFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? pFile : pFile + ".json";
    }

}
