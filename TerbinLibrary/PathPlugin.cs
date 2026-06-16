using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary;

public static class PathPlugin
{
    public const string ROOT = /*Instance*/"/";
    public static readonly string BEPINEX_PLUGINS = Path.Combine(ROOT, "BepInEx", "plugins");
    public static readonly string MELONLOADER_MODS = Path.Combine(ROOT, "Mods");

    //public static string Root
    //{
    //    get => ROOT;
    //}
    //public static string BepInExPlugin
    //{
    //    get => Path.Combine(ROOT);
    //}

}
