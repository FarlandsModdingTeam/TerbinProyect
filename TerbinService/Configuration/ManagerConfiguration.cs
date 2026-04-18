using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Data;

namespace TerbinService.Configuration;

internal class ManagerConfiguration
{
    private const string FOLDER = "config/";
    private const string JSON = "config.json";
    private const string KEY = "Config";

    public static string? GetConfg(string pKey)
    {
        if (AcessJSon.Acess<Dictionary<string, string>>(KEY, JSON) is var r && r == null)
            return null;

        if (r.TryGetValue(pKey, out string? value))
            return value;
        else
            return null;
    }

    public static bool SetConfig(string pKey, string pData)
    {
        Dictionary<string, string> data;
        if (AcessJSon.Acess<Dictionary<string, string>>(KEY, JSON) is var r && r != null)
            data = r;
        else
        {
            AcessJSon.Set(KEY, FOLDER);
            data = new();
        }

        // Segun copilot es suficientemente inteligente como añadirlo si no existe y sustituirlo si existe (Segun copilot).
        data[pKey] = pData;

        return ((sbyte)AcessJSon.Save(KEY, JSON, data)).ToBool();
    }
}


/*
 La cosa esque ManagerConfiguration debe manejar config.json con #file:'C:\Repositorio\TerbinProyect\TerbinLibrary\Data\AccessJSon.cs'  que config.json no es nada mas que un archivo con diccionario donde tiene la clave de la configuracion guardada y el dato de la configuracion, por ahora solo quiero guardar la ruta del vidiojuego "Farlands" y la ruta donde se deben crear instancias de ese mismo juego pero en un futuro quiero guardar mas cosas, la cosa esque lo llames a GetConfig con la clave del diccionario del json de la ruta de farlands y gestione si al hacer un "Acess" no devuelve null que en ese caso 
 */