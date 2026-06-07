using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Protocol;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using static System.Collections.Specialized.BitVector32;

namespace TerbinLibrary.Serialize;
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
/// Define un contrato para estructuras que pueden ser serializadas y deserializadas de forma eficiente.<br />
/// ___________________( English )___________________<br />
/// Defines a contract for structures that can be serialized and deserialized efficiently.<br />
/// </summary>
public interface IStructSerializable
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el tamaño de la estructura en bytes.<br />
    /// ___________________( English )___________________<br />
    /// Gets the size of the structure in bytes.<br />
    /// </summary>
    /// <returns>Es: Tamaño en bytes. <br />En: Size in bytes.</returns>
    int GetSize();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe los datos de la estructura en el búfer proporcionado.<br />
    /// ___________________( English )___________________<br />
    /// Writes the structure data to the provided buffer.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer de destino. <br />En: The destination buffer.</param>
    void WriteTo(Span<byte> pBuffer);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee los datos para llenar la estructura a partir del búfer proporcionado.<br />
    /// ___________________( English )___________________<br />
    /// Reads the data to populate the structure from the provided buffer.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer de origen de solo lectura. <br />En: The read-only source buffer.</param>
    void ReadFrom(ReadOnlySpan<byte> pBuffer);


    byte[] Read()
    {
        Span<byte> buffer = new byte[this.GetSize()];
        this.WriteTo(buffer);
        return buffer.ToArray();
    }


    void Write(byte[] pArray)
    {
        ReadOnlySpan<byte> buffer = pArray;
        this.ReadFrom(buffer);
    }
}


/// <summary>
/// ___________________( Español )___________________<br />
/// Utilidad para la serialización secuencial de datos en un búfer de bytes dinámico.<br />
/// Permite añadir diferentes tipos de datos (estructuras, arreglos, genéricos no administrados).<br />
/// Notas: El búfer interno crece automáticamente cuando hace falta.<br />
/// ___________________( English )___________________<br />
/// Utility for sequential data serialization into a dynamic byte buffer.<br />
/// Allows adding different data types (structures, arrays, unmanaged generics).<br />
/// Notes: The internal buffer grows automatically when needed.<br />
/// </summary>
[TODO("Abrir Minecraft si es mejor Raw o otra cosa")]
[TODO("La parte estatica solo deberia contener Raw y no añadir largo y Los Raw no deberian avanzar offset")]
public class Serialineitor
{
    private byte[] _content;
    private int _offset;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia del serializador con un tamaño inicial de 2 bytes.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the serializer with an initial size of 2 bytes.<br />
    /// </summary>
    public Serialineitor() : this(2) { }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia del serializador con un tamaño específico.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the serializer with a specific size.<br />
    /// </summary>
    /// <param name="pSize">Es: El tamaño inicial en bytes. <br />En: Initial size in bytes.</param>
    public Serialineitor(int pSize) : this(null, pSize) { }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia utilizando un contenido previo y tamaño base.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance using previous content and base size.<br />
    /// </summary>
    /// <param name="pInitialContent">Es: El contenido original en bytes (opcional). <br />En: Original byte content (optional).</param>
    /// <param name="pSize">Es: El tamaño inicial si el contenido está vacío. <br />En: Initial size if the content is empty.</param>
    public Serialineitor(byte[]? pInitialContent, int pSize = 2)
    {
        this._content = pInitialContent ?? new byte[pSize];
        this._offset = pInitialContent?.Length ?? 0;
    }

    private void ensureCapacity(int pNeededBytes)
    {
        if (_content.Length - _offset < pNeededBytes)
        {
            int newCapacity = Math.Max(_content.Length * 2, _content.Length + pNeededBytes);
            Array.Resize(ref _content, newCapacity);
        }
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Añade un valor de tipo no administrado al búfer interno.<br />
    /// ___________________( English )___________________<br />
    /// Adds an unmanaged type value to the internal buffer.<br />
    /// </summary>
    /// <param name="pValue">Es: El pValor a añadir. <br />En: The value to add.</param>
    /// <returns>Es: Instancia actual de Serialineitor. <br />En: Current Serialineitor instance.</returns>
    public Serialineitor Add<T>(T pValue) where T : unmanaged
    {
        ensureCapacity(Unsafe.SizeOf<T>());

        BufferWriter.Add<T>(_content, ref _offset, pValue);

        return this; 
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Añade un arreglo de valores (unmanaged) a la secuencia y su prefijo de longitud.<br />
    /// ___________________( English )___________________<br />
    /// Adds an array of unmanaged values to the sequence and its length prefix.<br />
    /// </summary>
    /// <param name="pArray">Es: El arreglo de orígen. <br />En: Source array.</param>
    /// <returns>Es: Instancia actual de Serialineitor. <br />En: Current Serialineitor instance.</returns>
    public Serialineitor AddArray<T>(T[] pArray) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(pArray);
        
        int elementsBytes = pArray.Length * Unsafe.SizeOf<T>();
        ensureCapacity(TerbinProtocol.LENGTH_ARRAY + elementsBytes);

        BufferWriter.AddArray<T>(_content, ref _offset, pArray);

        return this;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Añade un objeto de tipo IStructSerializable al búfer.<br />
    /// ___________________( English )___________________<br />
    /// Adds an object of type IStructSerializable to the buffer.<br />
    /// </summary>
    /// <param name="pStruct">Es: La estructura a añadir. <br />En: The structure to add.</param>
    /// <returns>Es: Instancia actual de Serialineitor. <br />En: Current Serialineitor instance.</returns>
    public Serialineitor AddStruct<T>(T pStruct) where T : struct, IStructSerializable
    {
        int structSize = (int)pStruct.GetSize();
        ensureCapacity(structSize);

        BufferWriter.AddStruct<T>(_content, ref _offset, pStruct);

        return this;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa y devuelve el contenido acumulado como un arreglo de bytes.<br />
    /// ___________________( English )___________________<br />
    /// Serializes and returns the accumulated content as a byte array.<br />
    /// </summary>
    /// <returns>Es: El subconjunto de bytes útiles. <br />En: The subset of useful bytes.</returns>
    public byte[] Serialize()
    {
        if (_content != null)
            return _content.AsSpan(0, _offset).ToArray();
        else
            return [];
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae todos los datos serializados en un arreglo de bytes nuevo.<br />
    /// ___________________( English )___________________<br />
    /// Extracts all serialized data into a new byte array.<br />
    /// </summary>
    /// <returns>Es: El formato array serializado. <br />En: Serialized array format.</returns>
    public byte[] ToArray()
    {
        return Serialize();
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Limpia el búfer y reinicia la instancia preparándola para más asignaciones.<br />
    /// ___________________( English )___________________<br />
    /// Clears the buffer and resets the instance making it ready for more inputs.<br />
    /// </summary>
    public void Clear()
    {
        if (_content != null)
        {
            Array.Clear(_content, 0, _content.Length);
        }
        _offset = 0;
    }


    // ******************************( Parte Estatic )****************************** //
    // TODO: Que no dependa de los Buffers sino al reve.
    // TODO: Solo deberia contener Raw y no añadir largo.

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa una estructura base usando Marshall.<br />
    /// Notas: Ineficiente en escenarios con constantes lecturas. Preferir Raw.<br />
    /// ___________________( English )___________________<br />
    /// Serializes a base structure using Marshalling.<br />
    /// Notes: Inefficient on contant-read scenarios. Prefer Raw.<br />
    /// </summary>
    /// <param name="pStruct">Es: La estructura a serializar. <br />En: The struct to serialize.</param>
    /// <returns>Es: Array de bytes del serializado. <br />En: Serialization byte array.</returns>
    public static byte[] SerializeStructConst<T>(T pStruct) where T : struct
    {
        int size = Marshal.SizeOf(pStruct);
        byte[] arr = new byte[size];

        nint ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(pStruct, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Deserializa desde bytes una estructura pre-establecida.<br />
    /// ___________________( English )___________________<br />
    /// Deserializes a preset structure from bytes.<br />
    /// </summary>
    /// <param name="pBytes">Es: Orígen de bytes contiguos. <br />En: Contiguous bytes source.</param>
    /// <returns>Es: Instancia de la estructura. <br />En: Struct instance.</returns>
    public static T DeserializeStructConst<T>(byte[] pBytes) where T : struct
    {
        T newStruct = default;

        nint ptr = Marshal.AllocHGlobal(pBytes.Length);
        Marshal.Copy(pBytes, 0, ptr, pBytes.Length);

        newStruct = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return newStruct;
    }

    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa utilizando la interface explícita IStructSerializable sin generar sobrecargas tipo Marshall.<br />
    /// ___________________( English )___________________<br />
    /// Serializes using the explicit IStructSerializable interface without generating Marshall overhead.<br />
    /// </summary>
    /// <param name="pStruct">Es: La estructura eficiente a serializar. <br />En: Efficient structure to serialize.</param>
    /// <returns>Es: Byte array resultate. <br />En: Resulting byte array.</returns>
    public static byte[] SerializeStructRaw<T>(T pStruct) where T : struct, IStructSerializable
    {
        byte[] buffer = new byte[pStruct.GetSize()]; // sizeof(T) // unsafe
        pStruct.WriteTo(buffer);
        return buffer;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Genera la lectura de estructuras implementadas por el programador con asignación optimizada.<br />
    /// ___________________( English )___________________<br />
    /// Provides reading on structs implemented by the programmer with optimized allocation.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Bloque de bytes inicial. <br />En: Initial bytes block.</param>
    /// <returns>Es: Objeto reconstituido en la estructura. <br />En: Reconstituted structure object.</returns>
    public static T DeserializeStructRaw<T>(byte[] pBuffer) where T : struct, IStructSerializable
    {
        T newStruct = new();
        newStruct.ReadFrom(pBuffer);
        return newStruct;
    }

    [TODO("Crear los 'Fast' para sustituir estos metodos")]
    [Obsolete("utilice Raw o Buffer")]
    public static byte[] SerializeArray<T>(T[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        byte[] newArray = new byte[pArray.Length * Unsafe.SizeOf<T>() + TerbinProtocol.LENGTH_ARRAY];
        BufferWriter.AddArray<T>(newArray, ref offset, pArray);
        return newArray;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Toma un arreglo genérico y lo devuelve uniendo los bytes como uno plano simple.<br />
    /// ___________________( English )___________________<br />
    /// Takes a generic array and returns it joining its bytes as a simple flat buffer.<br />
    /// </summary>
    /// <param name="pArray">Es: El array de valores desprotegido. <br />En: Unprotected values array.</param>
    /// <returns>Es: Bytes de memoria planos. <br />En: Flat memory bytes.</returns>
    public static byte[] SerializeArrayRaw<T>(T[] pArray)
        where T : unmanaged
    {
        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());
        return bytes.ToArray();
    }
    [TODO("Crear los 'Fast' para sustituir estos metodos")]
    [Obsolete("utilice Raw o Buffer")]
    public static T[] DeserializeArray<T>(byte[] pArray)
        where T : unmanaged
    {
        int offset = 0;
        return BufferReader.GetArray<T>(pArray, ref offset);
    }
    [TODO("Crear los 'Fast' para sustituir estos metodos")]
    [Obsolete("utilice Raw o Buffer")]
    public static T[] DeserializeArray<T>(ref byte[] pArray)
        where T : unmanaged
    {
        ReadOnlySpan<byte> buffer = pArray;
        return buffer.ReadArray<T>();
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Deserializa a un Array de tipo mediante un cast seguro directo en memoria utilizando fragmentos delimitados.<br />
    /// ___________________( English )___________________<br />
    /// Deserializes to a Typed Array using direct safe memory casts slicing within boundaries.<br />
    /// </summary>
    /// <param name="pArray">Es: Span de solo lectura. <br />En: Readonly span buffer.</param>
    /// <param name="pLenght">Es: Longitud opcional del búfer a considerar. <br />En: Optional buffer length to consider.</param>
    /// <returns>Es: Arreglo parseado a memoria. <br />En: Parsed memory array.</returns>
    public static T[] DeserializeArrayRaw<T>(ReadOnlySpan<byte> pArray, int? pLenght = null)
        where T : unmanaged
    {
        pLenght ??= pArray.Length;
        T[] newArray = MemoryMarshal.Cast<byte, T>(pArray[..pLenght.Value]).ToArray();
        return newArray;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Computa la longitud necesaria total para un arreglo dentro de un búfer medido mediante ThreeQuartersInt.<br />
    /// ___________________( English )___________________<br />
    /// Computes the total required length of an array inside a buffer using ThreeQuartersInt formatting.<br />
    /// </summary>
    /// <param name="pLength">Es: Cantidad de elementos del array original. <br />En: Amount of elements from original array.</param>
    /// <returns>Es: Retorna total de bytes escalado a ThreeQuartersInt. <br />En: Returns total bytes scaled to ThreeQuartersInt.</returns>
    public static ThreeQuartersInt GetArraySize<T>(ThreeQuartersInt pLength) where T : unmanaged
    {
        return (ThreeQuartersInt)(pLength * Unsafe.SizeOf<T>());
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Computa la longitud necesaria total para un arreglo genérico pasando su tamaño entero original.<br />
    /// ___________________( English )___________________<br />
    /// Computes the total required length of a generic array by passing its initial integer size.<br />
    /// </summary>
    /// <param name="pLength">Es: Cantidad de campos pre calculados. <br />En: Pre-counted array fields.</param>
    /// <returns>Es: Total de tamaño a requerir para Buffer. <br />En: Total space required for Buffer.</returns>
    public static ThreeQuartersInt GetArraySize<T>(int pLength) where T : unmanaged
    {
        return (ThreeQuartersInt)(pLength * Unsafe.SizeOf<T>());
    }



    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte de forma directa y llana un tipo por valor pValue a Bytes representativos.<br />
    /// ___________________( English )___________________<br />
    /// Converts a value-type directly into its representative bytes payload.<br />
    /// </summary>
    /// <param name="pValue">Es: El valor puro (struc) <br />En: Plaint struct value</param>
    /// <returns>Es: Arreglo en espacio Heap de memoria. <br />En: Memory heap array byte result.</returns>
    public static byte[] Serialize<T>(T pValue) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        byte[] buffer = new byte[size];

        MemoryMarshal.Write(buffer.AsSpan(), in pValue);
        
        return buffer;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte de forma directa un contenido en bytes al valor original preestablecido pBuffer.<br />
    /// ___________________( English )___________________<br />
    /// Parses directly byte payload payload space backwards into the starting target pBuffer shape.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El array del buffer <br />En: Memory buffer payload source.</param>
    /// <returns>Es: Variable T deserializada <br />En: Deserialized returned target T field</returns>
    public static T Deserialize<T>(byte[] pBuffer) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(pBuffer.AsSpan());
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae y convierte desde un desfasaje un tipo especifico del array preestablecido original pBuffer.<br />
    /// ___________________( English )___________________<br />
    /// Polls out from given array by advancing towards its targeted pointer size memory address over pBuffer.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Target byte buffer a leer. <br />En: Readout array chunk.</param>
    /// <param name="pOffset">Es: Indice o padding dentro de arreglo sobre donde leer. <br />En: Index array offset marker.</param>
    /// <returns>Es: Valor deserializado <br />En: Converted cast representation</returns>
    public static T Deserialize<T>(byte[] pBuffer, int pOffset) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(pBuffer[pOffset..]);
    }




    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Permite acoplar y sumar arreglos uniéndolos.<br />
    /// ___________________( English )___________________<br />
    /// Slices and glues several arrays tying their total fields context together.<br />
    /// </summary>
    /// <param name="pFirst">Es: Origen primario de cadena array <br />En: Start chunk head chain</param>
    /// <param name="pSecond">Es: Continuidad o cola secundaria array <br />En: Final secondary trailing block chain</param>
    /// <returns>Es: Bytes sumados. <br />En: Appended total chunk block byte format</returns>
    public static byte[] Splice(byte[] pFirst, byte[] pSecond)
    {
        byte[] buffer = new byte[pFirst.Length + pSecond.Length];
        Array.Copy(pFirst, 0, buffer, 0, pFirst.Length);
        Array.Copy(pSecond, 0, buffer, pFirst.Length, pSecond.Length);
        return buffer;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Une N arreglos para hacer compoñible un gran bloque precalculado dinámicamente.<br />
    /// ___________________( English )___________________<br />
    /// Unions up to N arrays enabling big generated dynamical preallocated contiguous field chunk format.<br />
    /// </summary>
    /// <param name="pArrays">Es: Múltiples argumentos o secuencias arrays. <br />En: Various ordered sequential arrays</param>
    /// <returns>Es: Un arreglo compilativo extenso. <br />En: Conjoined layout layout matrix</returns>
    public static byte[] Splice(params byte[][] pArrays)
    {
        byte[] buffer;
        int offset = 0;
        int size = 0;

        for (int i = 0; i < pArrays.Length; i++)
        {
            checked { size += pArrays[i].Length; }
        }

        buffer = new byte[size];

        for (int i = 0; i < pArrays.Length; i++)
        {
            Array.Copy(pArrays[i], 0, buffer, offset, pArrays[i].Length);
            offset += pArrays[i].Length;
        }

        return buffer;
    }


    public static byte[] CastToByte(params object[] pData)
    {
        ArgumentNullException.ThrowIfNull(pData);

        byte[] tmp = new byte[pData.Length];
        for (int i = 0; i < pData.Length; i++)
        {
            object item = pData[i];
            if (item is byte b)
            {
                tmp[i] = b;
            }
            else if (item is null)
            {
                tmp[i] = 0;
            }
            else
            {
                try
                {
                    tmp[i] = Convert.ToByte(item);
                }
                catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
                {
                    throw new ArgumentException($"The element in the index {i} ({item.GetType()}) it cannot be converted to byte.", nameof(pData), ex);
                }
            }
        }
        return tmp;
    }
}




/// <summary>
/// ___________________( Español )___________________<br />
/// Detalla de que forma un contexto Buffer pudo errar y fracasar localizándolo mediante este registro.<br />
/// ___________________( English )___________________<br />
/// Outlines errors and status tracking logs resulting mostly out from failing generic Buffer attempts.<br />
/// </summary>
public enum BufferErrorCode : sbyte
{
    Succes = 1,

    SurpassesMax = 2,
    BufferSmall = 3,
}