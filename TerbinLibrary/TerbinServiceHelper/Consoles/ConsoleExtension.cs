using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.TerbinServiceHelper.Consoles;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */


public static class ConsoleExtension
{
    private static readonly object _lock = new();
    extension(Console)
    {
        public static void Log(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Cyan);
        }

        public static void Warn(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Yellow);
        }

        public static void Error(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Red);
        }
        public static void Succes(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Green);
        }

        public static void PrintLn(string pMsg, ConsoleColor pColor = ConsoleColor.White)
        {
            lock (_lock)
            {
                var old = Console.ForegroundColor;
                Console.ForegroundColor = pColor;
                Console.WriteLine(pMsg);
                Console.ForegroundColor = old;
            }
        }
        public static void Print(string pMsg, ConsoleColor pColor = ConsoleColor.White)
        {
            lock (_lock)
            {
                var old = Console.ForegroundColor;
                Console.ForegroundColor = pColor;
                Console.Write(pMsg);
                Console.ForegroundColor = old;
            }
        }
    }
}

// Ñe ñe ñe
public abstract class APrint
{
    private static readonly object _lock = new();
    public static void Print(string pMsg, ConsoleColor pColor)
    {
        lock (_lock)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = pColor;
            Console.Print(pMsg);
            Console.ForegroundColor = old;
        }
    }
    public abstract void Print(string pMsg);
}
