using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Memory;
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

// TODO: Solicitar memoria
// ├─Solicita.
// ├─Crea o asigna memoria.
// └─Devuelve Id de memoria.

// TODO: Sobre escribe datos.
// TODO: Añade datos.
// TODO: Limpia (pero desde Containers).
public static class TerbinMemory
{
    // Diccionario concurrente para que sea Thread-Safe
    private static readonly ConcurrentDictionary<ushort, TerbinContainer2> _containers = new();

    // El Communicator llama a esto al recibir cualquier fragmento
    public static void Store(ushort pIdRequest, ushort pOrder, byte[] pData, bool pIsFinal)
    {
        var container = _containers.GetOrAdd(pIdRequest, id => new TerbinContainer2 { IdRequest = id });
        container.AddFragment(pOrder, pData, pIsFinal);
    }

    // El Usuario o el Communicator llaman a esto para obtener el resultado
    public static bool TryGetResult(ushort pIdRequest, out byte[]? pData)
    {
        if (_containers.TryGetValue(pIdRequest, out var container) && container.IsComplete)
        {
            pData = container.GetFullData();
            return true;
        }
        pData = null;
        return false;
    }

    // Importante: El usuario debe liberar la memoria cuando ya no la necesite
    public static void Release(ushort pIdRequest)
    {
        _containers.TryRemove(pIdRequest, out _);
    }
}