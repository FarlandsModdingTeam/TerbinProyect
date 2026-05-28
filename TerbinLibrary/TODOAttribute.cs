using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TerbinLibrary;

public class TODOAttribute : Attribute
{
    public string? Message { get; }
    public bool? Exception { get; }

    public TODOAttribute(string pMsg) : this(pMsg, false)
    {
    }

    public TODOAttribute(string pMsg, bool pException)
    {
        this.Message = pMsg;
        this.Exception = pException;

        if (pException)
        {
            red($"TODO: {pMsg}");
            throw new NotImplementedException($"TODO: {pMsg}");
        }
        else
        {
            yellow($"TODO: {pMsg}");
        }
    }


    private void red(string pMsg)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(pMsg);
        Console.ForegroundColor = old;
    }
    private void yellow(string pMsg)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(pMsg);
        Console.ForegroundColor = old;
    }
}
