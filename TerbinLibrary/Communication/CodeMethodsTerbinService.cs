using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication;

// methods:
public enum CodeMethodsTerbinService : byte
{
    CreateInstance = 10,
    DeleteInstance = 11,

    ChangeFarladsRute = 100,
    ChangeInstancesRute = 101,
}
