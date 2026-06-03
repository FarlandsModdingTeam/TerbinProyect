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

        foreach (var type in pAssembly.GetTypes())
        {
            CheckMember(type, type.Name, ref throwExceptionAtEnd, ref exceptionDetails);

            foreach (var method in type.GetMethods(flags))
                CheckMember(method, $"{type.Name}.{method.Name}()", ref throwExceptionAtEnd, ref exceptionDetails);

            foreach (var field in type.GetFields(flags))
                CheckMember(field, $"{type.Name}.{field.Name}", ref throwExceptionAtEnd, ref exceptionDetails);

            foreach (var prop in type.GetProperties(flags))
                CheckMember(prop, $"{type.Name}.{prop.Name}", ref throwExceptionAtEnd, ref exceptionDetails);
        }

        if (throwExceptionAtEnd)
            throw new NotImplementedException($"Unresolved critical TODOs were found:\n{exceptionDetails}");
    }

    private static void CheckMember(MemberInfo pMember, string pDisplayName, ref bool pThrowException, ref string pExceptionDetails)
    {
        var attributes = pMember.GetCustomAttributes(typeof(TODOAttribute), false);

        foreach (TODOAttribute todo in attributes)
        {
            string outputText = $"<{pDisplayName}> TODO: {todo.Message}";

            if (todo.Exception)
            {
                red(outputText);
                pThrowException = true;
                pExceptionDetails += $"- {outputText}\n";
            }
            else
            {
                yellow(outputText);
            }
        }
    }

    private static void red(string pMsg)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(pMsg);
        Console.ForegroundColor = old;
    }

    private static void yellow(string pMsg)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(pMsg);
        Console.ForegroundColor = old;
    }
}