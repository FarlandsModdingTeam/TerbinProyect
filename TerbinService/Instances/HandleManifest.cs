using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Useful;

namespace TerbinService.Instances;

public class HandleManifest
{
    private const string _INSTANCES = ".Instances.json";

    public static void UpdateCoreManifest(string pName)
    {
        var allInstaces = GetCore();
        allInstaces.Add(pName);
        JSonUtil.Save(TerbinConfiguration.RUTE_INSTANCES, _INSTANCES, allInstaces);
    }
    public static void DeleteInstanceCoreManifest(string pName)
    {
        var allInstaces = GetCore();
        allInstaces.Remove(pName);
        JSonUtil.Save(TerbinConfiguration.RUTE_INSTANCES, _INSTANCES, allInstaces);
    }

    public static List<string> GetCore()
    {
        var allInstaces = JSonUtil.Acess<List<string>>(TerbinConfiguration.RUTE_INSTANCES, _INSTANCES);
        allInstaces ??= new();
        return allInstaces;
    }
}
