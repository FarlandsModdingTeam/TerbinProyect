using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Extension;

public static class CharArrayExtensions
{
    public static string CrString(this char[] array)
    {
        return new string(array);
    }
}
