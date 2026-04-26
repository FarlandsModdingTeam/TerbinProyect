using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

namespace TerbinLibrary.Execution;

public static class TerbinExecutableHelper
{
    public static bool IsFirmParameters(ParameterInfo[] pParameters)
    {
        return
        (
            pParameters.Length == 2 &&
            pParameters[0].ParameterType == typeof(Header) &&
            pParameters[1].ParameterType == typeof(byte[])
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

                var parameters = method.GetParameters();
                if (!IsFirmParameters(parameters))
                    continue;

                if (!IsFirmReturn(method))
                    continue;

                var del = (Func<Header, byte[], Task<InfoResponse?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<InfoResponse?>>), method);

                foreach (var attr in attrs)
                {
                    pExecutor.Register(attr, (h, b) => del(h, b));
                }
            }
        }
    }



    public static async Task<InfoResponse?> ExecutionList(List<TerbinExecutableDelegate> pHandlers, Header pHead, byte[] pPayload)
    {
        var pendignTask = new List<Task<InfoResponse?>>(pHandlers.Count);
        for (int i = 0; i < pHandlers.Count; i++)
        {
            pendignTask.Add(pHandlers[i](pHead, pPayload));
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
        //.ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
    }
}
