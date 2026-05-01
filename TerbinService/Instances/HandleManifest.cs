using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;

namespace TerbinService.Instances;

public class HandleManifest
{
    public static void UpdateCoreManifest(string pName)
    {
        var allInstaces = getCore();
        allInstaces.Add(pName);
        AcessJSon.Save(TerbinConfiguration.RUTE_INSTANCES, "Instances.json", allInstaces);
    }
    public static void DeleteInstanceCoreManifest(string pName)
    {
        var allInstaces = getCore();
        allInstaces.Remove(pName);
        AcessJSon.Save(TerbinConfiguration.RUTE_INSTANCES, "Instances.json", allInstaces);
    }


    private static List<string> getCore()
    {
        var allInstaces = AcessJSon.Acess<List<string>>(TerbinConfiguration.RUTE_INSTANCES, "Instances.json");
        allInstaces ??= new();
        return allInstaces;
    }
}
