using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;
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


public static class TerbinExecutableHelper
{
    public static bool IsFirmParameters(ParameterInfo[] pParameters)
    {
        return
        (
            pParameters.Length == 2 &&
            pParameters[0].ParameterType == typeof(Header) &&
            pParameters[1].ParameterType == typeof(byte[]) &&
            pParameters[2].ParameterType == typeof(CancellationToken)
        );
    }

    public static bool IsFirmReturn(MethodInfo pMethod)
    {
        return
        (
            pMethod.ReturnType == typeof(Task<InfoResponse?>)
        );
    }


    public static void RegisterFromAssembly<T, E>(Assembly pAssembly, E pExecutor)
        where T : Attribute, IExecutableAttribute
        where E : IExecutableDispatcher 
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static /*| BindingFlags.Instance*/))
            {
                var attrs = method.GetCustomAttributes<T>(inherit: false);
                if (!attrs.Any()) continue;

                if (method.GetCustomAttribute<ObsoleteAttribute>(inherit: false) != null)
                {
                    var old = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Warning: {method.Name} is Obsolete");
                    Console.ForegroundColor = old;
                }

                var parameters = method.GetParameters();
                if (!IsFirmParameters(parameters))
                    continue;

                if (!IsFirmReturn(method))
                    continue;

                //var del = (Func<Header, byte[], Task<InfoResponse?>>)Delegate.CreateDelegate(
                //    typeof(Func<Header, byte[], Task<InfoResponse?>>), method);
                var del = (Func<Header, byte[], CancellationToken, Task<InfoResponse?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], CancellationToken, Task<InfoResponse?>>), method);

                foreach (var attr in attrs)
                {
                    pExecutor.Register(attr, (h, b, ct) => del(h, b, ct));
                }
            }
        }
    }


    public static async Task<InfoResponse?> ExecutionList(List<TerbinExecutableDelegate> pHandlers, Header pHead, byte[] pPayload, CancellationTokenSource pToken)
    {
        var pendignTask = new List<Task<InfoResponse?>>(pHandlers.Count);
        for (int i = 0; i < pHandlers.Count; i++)
        {
            pendignTask.Add(pHandlers[i](pHead, pPayload, pToken.Token));
        }

        while (pendignTask.Count > 0)
        {
            var completeTask = await Task.WhenAny(pendignTask).ConfigureAwait(false);
            pendignTask.Remove(completeTask);

            var result = await completeTask.ConfigureAwait(false);
            if (result != null)
                return result;
        }
        return null;
    }
    //.ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.

}
