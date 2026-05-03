using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Configuration;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.Useful;
using TerbinService.Data;
using TerbinService.Instances;

namespace TerbinService.Manifests;

public static class HandleManifest
{
    private const string _INSTANCES = ".IndexInstances.json";

    public static void UpdateCore(string pName)
    {
        var allInstaces = GetCore();
        allInstaces.Add(pName);
        JSonUtil.Save(TerbinConfiguration.RUTE_INSTANCES, _INSTANCES, allInstaces);
    }
    public static void DeleteInstanceCore(string pName)
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
        var pathInstance = InstancesService.MakePathFolder(pName);
        if (pathInstance == null)
            return false;

        return UpdateInstace(pName, pathInstance, updateAction);
    }

    public static bool UpdateInstace(string pName, string pPathInstance, Action<InstanceManifest> updateAction)
    {
        var pathInformation = InstancesService.MakePathFolderInformation(pName);
        if (pathInformation is null)
            return false;

        JSonUtil.UpdateDirect<InstanceManifest>(pathInformation, TerbinServiceConst.NAME_OF_MANIFEST, updateAction);
        return true;
    }


    public static void HandleAddPlugin(string pNameInstace, DirectoryHandwritten? pHandwritten)
    {
        var information = InstancesService.MakePathFolderInformation(pNameInstace);
        if (information is null)
            throw new Exception("TODO: informar de que no se pudo conseguir la information en manifest");

        Guid g = Guid.NewGuid();
        string name = $"{g:N}";
        string file = $"{g:N}.json";
        string pathManifest = Path.Combine(information, $"{g:N}.json");

        var manifest = new PluginManifest
        {
            Name = name,
            Content = pHandwritten,
        };
        JSonUtil.SaveDirect(information, file, manifest);

        var reference = new ReferencePlugin
        {
            Name = name,
            GUID = name,
            Path = pathManifest,
        };

        HandleManifest.UpdateInstace(pNameInstace, m => { m.Plugins.Add(reference); });
    }
}