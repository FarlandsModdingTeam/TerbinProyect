using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace TerbinLibrary;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */



[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class TODOAttribute : Attribute
{
    public string Message { get; }
    public bool Exception { get; }

    public TODOAttribute(string pMsg) : this(pMsg, false)
    {
    }

    public TODOAttribute(string pMsg, bool pException)
    {
        this.Message = pMsg;
        this.Exception = pException;
    }

    public static void ChekAndPrint(Assembly pAssembly)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static | BindingFlags.Instance |
                    BindingFlags.DeclaredOnly;

        bool throwExceptionAtEnd = false;
        string exceptionDetails = "";
        int count = 1;

        foreach (var type in pAssembly.GetTypes())
        {
            CheckMember(type, type.Name, ref throwExceptionAtEnd, ref exceptionDetails, ref count);

            foreach (var method in type.GetMethods(flags))
                CheckMember(method, $"{type.Name}.{method.Name}()", ref throwExceptionAtEnd, ref exceptionDetails, ref count);

            foreach (var field in type.GetFields(flags))
                CheckMember(field, $"{type.Name}.{field.Name}", ref throwExceptionAtEnd, ref exceptionDetails, ref count);

            foreach (var prop in type.GetProperties(flags))
                CheckMember(prop, $"{type.Name}.{prop.Name}", ref throwExceptionAtEnd, ref exceptionDetails, ref count);
        }

        if (throwExceptionAtEnd)
            throw new NotImplementedException($"Unresolved critical TODOs were found:\n{exceptionDetails}");
    }

    private static void CheckMember(MemberInfo pMember, string pDisplayName, ref bool pThrowException, ref string pExceptionDetails, ref int pCount)
    {
        var attributes = pMember.GetCustomAttributes(typeof(TODOAttribute), false);

        foreach (TODOAttribute todo in attributes)
        {
            string outputText = $"<{pDisplayName}> TODO: {todo.Message}";

            if (todo.Exception)
            {
                var c = getColor(true, ref pCount);
                print(outputText, c);
                pThrowException = true;
                pExceptionDetails += $"- {outputText}\n";
            }
            else
            {
                var c = getColor(false, ref pCount);
                print(outputText, c);
            }
        }
    }

    private static ConsoleColor getColor(bool pException, ref int pCount)
    {
        ConsoleColor c;
        if (pException)
            c = (pCount % 2 == 1) ? ConsoleColor.Red : ConsoleColor.DarkRed;
        else
            c = (pCount % 2 == 1) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        pCount++;
        return c;
    }

    private static void print(string pMsg, ConsoleColor pColor)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = pColor;
        Console.WriteLine(pMsg);
        Console.ForegroundColor = old;
    }
}