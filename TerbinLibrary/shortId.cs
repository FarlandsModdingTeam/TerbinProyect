using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Id;

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
        int total = nextId() + (now.Second + now.Minute + now.Hour);
        return (byte)total;
    }

    private static ushort nextId()
    {
        return Incremental++;
    }
}
