using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TerbinLibrary.Useful;

public static partial class PluginUtil
{
    private const string _pattern = @"\b\d+\.\d+(?:\.\d+)*\b(?:-[a-zA-Z0-9.-]+)?";

    [GeneratedRegex(_pattern)]
    private static partial Regex versionRegex();

    public static string ExtratVersion(string pFileName)
    {
        string finalVersion;
        string? cleanName;

        cleanName = Path.GetFileNameWithoutExtension(pFileName);
        if (string.IsNullOrEmpty(cleanName))
            cleanName = pFileName.Replace(".zip", null);


        MatchCollection coincidences = versionRegex().Matches(cleanName);

        if (coincidences.Count == 0)
            return string.Empty;

        finalVersion = new("");
        for (int i = 0; i < coincidences.Count; i++)
        {
            finalVersion += coincidences[i];
            if (i + 1 < coincidences.Count)
                finalVersion += "-";
        }

        //if (coincidences.Count == 1)
        //    return coincidences[0].Value;
        // Esto me lo a puesto VisualStudio, No se si cogera el ultimo.
        // return coincidences[^1].Value;

        return finalVersion;
    }
}
