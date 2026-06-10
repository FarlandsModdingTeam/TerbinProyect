using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.HelperData;



public static class ObjectExtension
{
    public static bool IsNull(this object? pObj)
    {
        return pObj is null;
    }
}
