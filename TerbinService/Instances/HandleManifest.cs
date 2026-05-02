using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Data;

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



    public static void CreatePredeterminated(string pName)
    {
        string? dirInfo = InstancesService.MakePathFolderInformation(pName);
        if (dirInfo == null)
            return;
        Directory.CreateDirectory(dirInfo);
        CreatePredeterminatedInstance(pName, dirInfo);
    }

    public static void CreatePredeterminatedInstance(string pName, string pDir)
    {
        var manifest = new InstanceManifest
        {
            Name = pName,
            Version = ManagerFarlands.GetVersion(),
            Plugins = []
        };
        JSonUtil.SaveDirect(pDir, TerbinServiceConst.NAME_OF_MANIFEST, manifest);
    }


    public static bool UpdateInstace(string pName, Action<InstanceManifest> updateAction)
    {
        var dirInstace = InstancesService.MakePathFolder(pName);
        if (dirInstace == null)
            return false;

        return UpdateInstace(pName, dirInstace, updateAction);
    }

    public static bool UpdateInstace(string pName, string pDirInstance, Action<InstanceManifest> updateAction)
    {
        var path = InstancesService.MakePathFolderInformation(pName);
        if (path is null)
            return false;

        JSonUtil.UpdateDirect<InstanceManifest>(path, TerbinServiceConst.NAME_OF_MANIFEST, updateAction);
        return true;
    }
}