using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Id;


public enum ShortIdReserved : ushort
{
    Solicited = 0,
    DoNotAssign = 1,
    Server = 2,
}

/// <summary>
/// 0 = solicitar, 1 = no asignar, 2 = server,  
/// </summary>
public static class ShortId
{
    private static ushort _incremental = 10;

    public static ushort Incremental
    {
        get => _incremental;
        private set
        {
            if (value < 10)
                value = 10;

            _incremental = value;
        }
    }

    public static ushort New
    {
        get => NewShortId();
    }

    public static ushort NewShortId()
    {
        var now = DateTime.Now;
        var timeId = now.Second + now.Minute;
        int total = nextId() + (timeId < 0 ? 0 : timeId);
        return (byte)total;
    }

    private static ushort nextId()
    {
        return Incremental++;
    }
}
