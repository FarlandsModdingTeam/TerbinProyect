using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Protocol;

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

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase que gestiona el almacenamiento fragmentado en memoria de paquetes o secuencias de datos en tránsito.<br />
/// ___________________( English )___________________<br />
/// Class that manages the fragmented in-memory storage of packets or data sequences in transit.<br />
/// </summary>
public class TerbinMemory
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador único para este bloque de memoria en particular.<br />
    /// ___________________( English )___________________<br />
    /// Unique identifier for this particular memory block.<br />
    /// </summary>
    public byte Id
    {
        get => field;
        set => field = value;
    }
    = (byte)CodeTerbinMemory.NotAsign;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Identificador de la solicitud (request) vinculada actualmente a este bloque de memoria.<br />
    /// ___________________( English )___________________<br />
    /// Identifier of the request linked currently to this memory block.<br />
    /// </summary>
    public ushort IdRequest
    {
        get => field;
        set => field = value;
    }
    = (ushort)CodeTerbinMemory.NotAsign;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Propiedad computada que verifica si la memoria está ocupada por alguna solicitud en curso.<br />
    /// ___________________( English )___________________<br />
    /// Computed property verifying if the memory is currently occupied by an ongoing request.<br />
    /// </summary>
    public bool IsOcupated => IdRequest != (ushort)CodeTerbinMemory.NotAsign;

    private readonly Dictionary<ushort, byte[]> _fragments = new();
    private int _totalSize = 0;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Evento disparado cada vez que se añade un nuevo fragmento al diccionario interno de la memoria.<br />
    /// ___________________( English )___________________<br />
    /// Event triggered whenever a new fragment is added to the internal memory dictionary.<br />
    /// </summary>
    public event Action? OnAdd;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Evento disparado de manera asíncrona un instante después de soltar y proceder a limpiar esta memoria.<br />
    /// ___________________( English )___________________<br />
    /// Event triggered asynchronously an instant after releasing and proceeding to clear this memory.<br />
    /// </summary>
    public event Action? OnRelease;

    // TODO: comprobar nulos, vacios, etc.
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Agrega un nuevo fragmento de bytes en la memoria tomando en cuenta su orden de flujo o índice.<br />
    /// Notas: Aún se encuentra pendiente la verificación y tratamiento de datos nulos o vacíos.<br />
    /// ___________________( English )___________________<br />
    /// Adds a new byte fragment into the memory considering its flow order or index.<br />
    /// Notes: Verification and treatment for null or empty data is still pending.<br />
    /// </summary>
    /// <param name="pOrder">Es: Orden numérico correspondido o llave del fragmento. <br />En: Corresponding numerical order or fragment key index.</param>
    /// <param name="pData">Es: Arreglo de bytes del conjunto a guardar. <br />En: Array of bytes from the dataset to be saved.</param>
    public void AddFragment(ushort pOrder, byte[] pData)
    {
        lock (_fragments)
        {
            if (!_fragments.ContainsKey(pOrder))
            {
                _fragments.Add(pOrder, pData);
                _totalSize += pData.Length;
            }
        }
        OnAdd?.Invoke();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta recuperar el archivo de datos completo uniendo de forma consecutiva todos los fragmentos previamente almacenados.<br />
    /// ___________________( English )___________________<br />
    /// Tries to fetch the complete data file by sequentially merging all the previously stored fragments.<br />
    /// </summary>
    /// <param name="pData">Es: Búfer de salida referenciado con los datos concatenados resultantes si tiene éxito. <br />En: Referenced output buffer mapped with the resulting concatenated bytes upon success.</param>
    /// <returns>Es: Un valor de certeza en conjunto a un posible código de error. <br />En: A truth status flag matched to a possible error trace code.</returns>
    public (bool succes, TerbinErrorCode typeError) TryGetFullData(out byte[] pData)
    {
        pData = [];

        KeyValuePair<ushort, byte[]>[] fragmentsCopy;
        int totalSizeCopy;
        lock (_fragments)
        {
            fragmentsCopy = _fragments.ToArray();
            totalSizeCopy = _totalSize;
        }

        if (fragmentsCopy.Length == 0)
            return (false, TerbinErrorCode.InvalidLength);

        Array.Sort(fragmentsCopy, (a, b) => a.Key.CompareTo(b.Key));

        // Comprobamos si falta alguna parte de informacio intermedia.
        if (!chechMissing(fragmentsCopy))
            return (false, TerbinErrorCode.OrderMismatch);

        pData = new byte[totalSizeCopy];
        int offset = 0;
        foreach (var f in fragmentsCopy)
        {
            Buffer.BlockCopy(f.Value, 0, pData, offset, f.Value.Length);
            offset += f.Value.Length;
        }
 
        return (true, TerbinErrorCode.None);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Verifica internamente la secuencia, comprobando la integridad contra fragmentos perdidos o índices salteados.<br />
    /// ___________________( English )___________________<br />
    /// Internally verifies the sequence integrity, checking against dropped fragments or skipped key numbers.<br />
    /// </summary>
    /// <param name="pFragments">Es: Matriz de fragmentos previamente organizados. <br />En: Layout sequence array matrix consisting of ordered mapped segments.</param>
    /// <returns>Es: Verdadero en caso consecutivo continuo. <br />En: True for absolute continuous valid stream blocks.</returns>
    private bool chechMissing(KeyValuePair<ushort, byte[]>[] pFragments)
    {
        for (ushort i = 0; i < pFragments.Length; i++)
        {
            if (pFragments[i].Key != (i + 1))
                return false;
        }
        return true;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Suelta la memoria, restableciendo su identificador central originario para concluir con una profunda limpieza.<br />
    /// ___________________( English )___________________<br />
    /// Releases the mapped block memory, clearing its primary pointer ID tracing backward to deep cleaning disposal patterns.<br />
    /// </summary>
    public void Release()
    {
        IdRequest = (byte)CodeTerbinMemory.NotAsign;

        OnAdd = null;
        OnRelease?.Invoke();

        Clear();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Descarta cada fragmento anteriormente guardado y limpia el diccionario asegurando sus hilos subyacentes con candados de memoria.<br />
    /// ___________________( English )___________________<br />
    /// Discards every previously placed payload bit and empties the main dictionary, ensuring nested thread lock protections safely.<br />
    /// </summary>
    public void Clear()
    {
        lock (_fragments)
        {
            _fragments.Clear();
            _totalSize = 0;
        }
    }
}