using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Id;
using TerbinLibrary.Protocol;

namespace TerbinLibrary.Memory;/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


/// <summary>
/// ___________________( Español )___________________<br />
/// Gestor estático para la administración de contenedores de memoria de Terbin en concurrencia.<br />
/// ___________________( English )___________________<br />
/// Static manager for concurrent administration of Terbin memory containers.<br />
/// </summary>
public static class TerbinMemoryManager
{
    private static readonly ConcurrentDictionary<byte, TerbinMemory> _containers = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Propiedad que expone la colección concurrente de contenedores de memoria activos.<br />
    /// ___________________( English )___________________<br />
    /// Property that exposes the concurrent collection of active memory containers.<br />
    /// </summary>
    public static ConcurrentDictionary<byte, TerbinMemory> Containers => _containers;



    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el identificador de un contenedor disponible. Si no hay ninguno libre, crea uno nuevo.<br />
    /// ___________________( English )___________________<br />
    /// Gets the identifier of an available container. If none is free, creates a new one.<br />
    /// </summary>
    /// <returns>Es: El identificador en formato byte del contenedor de memoria. <br />En: The byte format identifier of the memory container.</returns>
    public static byte GetFreeStore()
    {
        byte? idContainer = null;
        foreach (var item in _containers)
        {
            if (!item.Value.IsOcupated)
            {
                idContainer = item.Key;
                break;
            }
        }
        
        if (idContainer != null)
            return idContainer.Value;
            
        return createStore().id;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea internamente un nuevo contenedor de memoria y lo añade a la colección concurrente.<br />
    /// ___________________( English )___________________<br />
    /// Internally creates a new memory container and adds it to the concurrent collection.<br />
    /// </summary>
    /// <returns>Es: Tupla indicando si la operación fue exitosa y el ID generado. <br />En: Tuple indicating if the operation succeeded and the generated ID.</returns>
    private static (bool succes, byte id) createStore()
    {
        byte id = MiniID.NewB;
        return (_containers.TryAdd(id, new TerbinMemory { Id = id }), id);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Almacena un fragmento de datos en el contenedor de memoria especificado.<br />
    /// ___________________( English )___________________<br />
    /// Stores a data fragment in the specified memory container.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor de memoria. <br />En: Identifier of the memory container.</param>
    /// <param name="pOrder">Es: El orden que corresponde a este fragmento de datos. <br />En: The order corresponding to this data fragment.</param>
    /// <param name="pData">Es: El arreglo de bytes a almacenar. <br />En: The byte array to store.</param>
    public static void Store(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        var container = _containers.GetOrAdd(pIdMemory, id => new TerbinMemory { IdRequest = id });
        container.AddFragment(pOrder, pData);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Sobrescribe de manera forzada el contenido de un contenedor asignando una nueva instancia de memoria.<br />
    /// ___________________( English )___________________<br />
    /// Forcefully overwrites the content of a container by assigning a new memory instance.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor a sobrescribir. <br />En: Identifier of the container to overwrite.</param>
    /// <param name="pOrder">Es: El orden que corresponde a este nuevo fragmento de datos. <br />En: The order corresponding to this new data fragment.</param>
    /// <param name="pData">Es: El nuevo arreglo de bytes a almacenar. <br />En: The new byte array to store.</param>
    public static void ReStore(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        var newContainer = new TerbinMemory { IdRequest = pIdMemory };
        newContainer.AddFragment(pOrder, pData);
        _containers.AddOrUpdate(pIdMemory, newContainer, (_, _) => newContainer);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Sobrescribe limpiando un contenedor existente o creando uno nuevo si no existía.<br />
    /// ___________________( English )___________________<br />
    /// Overwrites by clearing an existing container or creating a new one if it didn't exist.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor a sobrescribir. <br />En: Identifier of the container to overwrite.</param>
    /// <param name="pOrder">Es: El orden que corresponde a este nuevo fragmento. <br />En: The order corresponding to this new fragment.</param>
    /// <param name="pData">Es: El nuevo arreglo de bytes a almacenar. <br />En: The new byte array to store.</param>
    public static void OverwriteStore(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        if (_containers.TryGetValue(pIdMemory, out var container))
        {
            container.Clear();
            container.AddFragment(pOrder, pData);
        }
        else
        {
            Store(pIdMemory, pOrder, pData);
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta obtener los datos completos almacenados en un contenedor de memoria.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to obtain the complete data stored in a memory container.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor de memoria. <br />En: Identifier of the memory container.</param>
    /// <param name="pData">Es: Parámetro de salida con los datos resultantes. <br />En: Output parameter with the resulting data.</param>
    /// <returns>Es: Tupla con el éxito de la operación y el posible error asociado. <br />En: Tuple with the operation's success and possible associated error.</returns>
    public static (bool succes, TerbinErrorCode typeError) TryGetResult(byte pIdMemory, out byte[] pData)
    {
        if (_containers.TryGetValue(pIdMemory, out var container))
        {
            if (container.TryGetFullData(out pData) is var r && r.succes)
                return (true, TerbinErrorCode.None);
            else
                return (false, r.typeError);
        }
        
        pData = [];
        // ¿no habia una excepcion de intentar hacceder a null?
        return (false, TerbinErrorCode.ValueOutOfRange);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta obtener la instancia del contenedor de memoria asociado al identificador proporcionado.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to get the memory container instance associated with the provided identifier.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor a buscar. <br />En: Identifier of the container to search.</param>
    /// <param name="pMemory">Es: Parámetro de salida con el contenedor de memoria encontrado. <br />En: Output parameter with the found memory container.</param>
    /// <returns>Es: True si se encontró el contenedor. <br />En: True if the container was found.</returns>
    public static bool TryGetMemory(byte pIdMemory, out TerbinMemory? pMemory)
    {
        bool success = _containers.TryGetValue(pIdMemory, out var memory);
        pMemory = memory;
        return success;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Marca el contenedor de memoria especificado como libre limpiando su estado interno.<br />
    /// ___________________( English )___________________<br />
    /// Marks the specified memory container as free by cleaning its internal state.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor a liberar. <br />En: Identifier of the container to release.</param>
    /// <returns>Es: True si el contenedor existía y se liberó correctamente. <br />En: True if the container existed and was released successfully.</returns>
    public static bool Release(byte pIdMemory)
    {
        if (_containers.TryGetValue(pIdMemory, out var value))
        {
            value.Release();
            return true;
        }
        return false;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Elimina permanentemente el contenedor de memoria del registro manejado.<br />
    /// ___________________( English )___________________<br />
    /// Permanently removes the memory container from the managed registry.<br />
    /// </summary>
    /// <param name="pIdMemory">Es: Identificador del contenedor a eliminar. <br />En: Identifier of the container to remove.</param>
    /// <returns>Es: True si fue eliminado. <br />En: True if it was removed.</returns>
    public static bool Remove(byte pIdMemory) => _containers.TryRemove(pIdMemory, out _);
}