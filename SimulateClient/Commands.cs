using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimulateClient;

internal static partial class Commands
{
    private const string _patternGetMethod = @"\-(?:[a-zA-Z0-9.-]+)?";
    private const string _patternGetClass = @"^\S+";

    [GeneratedRegex(_patternGetMethod)]
    public static partial Regex GetMethod();


    [GeneratedRegex(_patternGetClass)]
    public static partial Regex GetClass();
}
