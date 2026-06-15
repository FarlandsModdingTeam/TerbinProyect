using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
/// Clase utilitaria para la lectura y deserialización secuencial de datos desde búferes de memoria de solo lectura.<br />
/// ___________________( English )___________________<br />
/// Utility class for sequential data reading and deserialization from read-only memory buffers.<br />
/// </summary>
public class BufferReader
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee y reconstruye un arreglo de tipo no administrado avanzando el offset indicado.<br />
    /// Notas: Lee la longitud como formato interno de bytes primeramente.<br />
    /// ___________________( English )___________________<br />
    /// Reads and reconstructs an unmanaged type array shifting the designated offset ahead.<br />
    /// Notes: Polls length header first on inner bytes footprint format.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer original de solo lectura. <br />En: Originating read-only span block limit bounds setup string context matrix param map pointer target.</param>
    /// <param name="pOffset">Es: Apuntador al inicio de lectura. <br />En: Starting read cursor pointer marker reference map target field setup sequence layout.</param>
    /// <returns>Es: Un array tipado reconstruido instanciado en un nuevo alocador. <br />En: Newly reconstructed instantiated typed generic unmanaged resulting pattern layout sequence.</returns>
    public static T[] GetArray<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        ThreeQuartersInt length = Get<ThreeQuartersInt>(pBuffer, ref pOffset);

        if (length == 0) return Array.Empty<T>();

        // la longitud es de BYTES (Serialineitor.GetArraySize multiplicó por el SizeOf<T>) 
        var slice = pBuffer.Slice(pOffset, length);

        T[] array = MemoryMarshal.Cast<byte, T>(slice).ToArray();
        pOffset += length;

        return array;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae el siguiente valor unmanaged actualizando al índice subsiguiente el desplazamiento referenciado.<br />
    /// ___________________( English )___________________<br />
    /// Extracts following consecutive unmanaged cast format pulling pointer layout off to its ending byte setup.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Span de memoria de lectura. <br />En: Memory block read limits form span boundary target space constraint payload element source limit parameter mapping.</param>
    /// <param name="pOffset">Es: Indice inicial donde leer. <br />En: Origin index start reading locator pointer variable string setup flag.</param>
    /// <returns>Es: Representación literal desprotegida leída (Struct u otro). <br />En: Naked polled representation payload mapping layout property cast object element target readout.</returns>
    public static T Get<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset)
       where T : unmanaged
    {
        T value = MemoryMarshal.Read<T>(pBuffer[pOffset..]);
        pOffset += Unsafe.SizeOf<T>();
        return value;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte el segmento requerido hacia una estructura de un molde estático asignable mediante iterfaz base de copia. <br />
    /// ___________________( English )___________________<br />
    /// Translates chunk limit size bounded data off towards struct factor object instantiated mold allocating target mapping explicit wrapping cast base template footprint. <br />
    /// </summary>
    /// <param name="pBuffer">Es: El arreglo fragmentado continuo objetivo. <br />En: Continuing payload chunk layout limit target map array form span string mapping sequence base source property param payload wrapper constraint parameter field factor setup.</param>
    /// <param name="pOffset">Es: Marcador base en constante modificación. <br />En: Offset mutable origin padding flag constraint size pointer map flag string mapping setup source parameter.</param>
    /// <param name="pStruct">Es: Instancia estructural previa orientada al uso. <br />En: Predetermined layout setup parameter format map layout mold format structure struct form cast wrapper matrix list target bounds array map source list instance format vector parameter setup parameter payload map layout configuration bounds map.</param>
    /// <returns>Es: Un nuevo elemento formateado estructurado pópulando desde raw limit chunk setup array limits. <br />En: Returns parsed populating array payload memory raw form chunk representation payload element variables setup factor chunk format.</returns>
    public static T GetStruct<T>(ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        //ushort lenth = pStruct.GetSize();
        T newStruct = Serialineitor.DeserializeStructRaw<T>(pBuffer[pOffset../*(pOffset+lenth)*/].ToArray());
        pOffset += newStruct.GetSize();
        return newStruct;
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase de extensión para la lectura de datos mediante Span de sólo lectura.<br />
/// Notas: Facilita el uso de la clase BufferReader directamente desde un ReadOnlySpan.<br />
/// Tips: Utiliza los métodos con 'ref ReadOnlySpan' para avanzar automáticamente el buffer, o los métodos con 'ref int pOffset' para mantener el buffer original intacto y solo avanzar el índice.<br />
/// ___________________( English )___________________<br />
/// Extension class for reading data through a read-only Span.<br />
/// Notes: Facilitates the use of the BufferReader class directly from a ReadOnlySpan.<br />
/// Tips: Use the 'ref ReadOnlySpan' methods to automatically advance the buffer, or the 'ref int pOffset' methods to keep the original buffer intact and only advance the index.<br />
/// </summary>
[TODO("Usar \"out\" para devolver el byte[] y asin funcionar directamente con arrays.")]
public static class BufferReaderExtension
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee un arreglo de elementos de tipo no administrado avanzando directamente el span de memoria proporcionado.<br />
    /// Notas: Usa internamente el encabezado de tamaño en formato bytes definido por el dato inicial.<br />
    /// Tips: Ideal para evitar el uso manual de variables de offset cuando no se necesita conservar el inicio del span.<br />
    /// ___________________( English )___________________<br />
    /// Reads an array of unmanaged elements by directly advancing the provided memory span.<br />
    /// Notes: Internally uses the size header in byte format defined by the initial data.<br />
    /// Tips: Ideal for avoiding manual use of offset variables when you do not need to keep the start of the span.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer de solo lectura pasado por referencia para modificar su inicio resultante. <br />En: The read-only buffer passed by reference to modify its resulting start slice.</param>
    /// <typeparam name="T">Es: El tipo de valor unmanaged esperado. <br />En: The expected unmanaged value type.</typeparam>
    /// <returns>Es: Un nuevo array construido con los datos leídos. <br />En: A new array built with the read data.</returns>
    public static T[] ReadArray<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : unmanaged
    {
        ThreeQuartersInt length = pBuffer.Read<ThreeQuartersInt>();

        T[] newArray = MemoryMarshal.Cast<byte, T>(pBuffer[..length]).ToArray();
        pBuffer = pBuffer[length..];

        return newArray;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae el siguiente valor unmanaged y recorta el span al primer byte no leído.<br />
    /// Notas: Este método asume que el tipo T cabe completamente en lo que resta del buffer.<br />
    /// Tips: Úselo secuencialmente para leer estructuras simples y datos primitivos.<br />
    /// ___________________( English )___________________<br />
    /// Extracts the next unmanaged value and slices the span to the first unread byte.<br />
    /// Notes: This method assumes the type T fully fits into the remaining buffer.<br />
    /// Tips: Use it sequentially to read simple structures and primitive data.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Span de memoria de lectura por referencia. <br />En: Read memory span by reference.</param>
    /// <typeparam name="T">Es: Estructura unmanaged objetivo. <br />En: Target unmanaged structure.</typeparam>
    /// <returns>Es: El valor leído des-serializado. <br />En: The deserialized read value.</returns>
    public static T Read<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : unmanaged
    {
        T newValue = MemoryMarshal.Read<T>(pBuffer);
        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
        return newValue;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte y extrae el segmento inicial a una estructura con un formato específico basado en una instancia provista.<br />
    /// Notas: Se basa en la interfaz IStructSerializable para la reconstrucción.<br />
    /// Tips: Utiliza deserialización cruda pasando el tamaño de la estructura para ajustar el span.<br />
    /// ___________________( English )___________________<br />
    /// Converts and extracts the initial slice to a specifically formatted structure based on a provided instance.<br />
    /// Notes: It relies on the IStructSerializable interface for reconstruction.<br />
    /// Tips: It uses raw deserialization passing the structure size to trim the span.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Span de memoria como fuente. <br />En: Memory span as source.</param>
    /// <param name="pStruct">Es: Instancia estructural de referencia y esquema. <br />En: Reference structural instance and schema.</param>
    /// <typeparam name="T">Es: Tipo estructural implementando IStructSerializable. <br />En: Structural type implementing IStructSerializable.</typeparam>
    /// <returns>Es: La nueva instancia serializada extraída del buffer. <br />En: The newly extracted serialized instance from the buffer.</returns>
    [TODO("Esto tiene multiples problemas")]
    [TODO("Excepciona cuando no deberia")]
    public static T ReadStruct<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : struct, IStructSerializable
    {
        T newStruct = Serialineitor.DeserializeStructRaw<T>(pBuffer/*[..length]*/.ToArray());
        var length = newStruct.GetSize();
        pBuffer = pBuffer[length..];
        return newStruct;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee y devuelve un array no administrado avanzando el offset de manera tradicional.<br />
    /// Notas: Llama estáticamente a la clase principal BufferReader.<br />
    /// Tips: Preferido si mantiene la referencia base del buffer y usa el offset como cursor.<br />
    /// ___________________( English )___________________<br />
    /// Reads and returns an unmanaged array advancing the offset in a traditional way.<br />
    /// Notes: Statically calls the main BufferReader class.<br />
    /// Tips: Preferred if you keep the base reference to the buffer and use the offset as cursor.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer original inalterado. <br />En: The unedited original buffer.</param>
    /// <param name="pOffset">Es: Puntero que simula el índice actual. <br />En: Pointer simulating the current index.</param>
    /// <typeparam name="T">Es: El tipo de valor del array. <br />En: The value type of the array.</typeparam>
    /// <returns>Es: Arreglo de tipo no administrado recién creado e inicializado. <br />En: Freshly created and initialized unmanaged type array.</returns>
    public static T[] ReadArray<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        return BufferReader.GetArray<T>(pBuffer, ref pOffset);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Lee un valor singular del tipo no administrado y suma el tamaño al marcador de posición.<br />
    /// Notas: Forma estándar dependiente de la talla nativa del tipo.<br />
    /// Tips: Forma segura de leer secuencialmente sin cambiar el apuntador original del span.<br />
    /// ___________________( English )___________________<br />
    /// Reads a singular value of an unmanaged type and adds its size to the position marker.<br />
    /// Notes: Standard way depending on the native size of the type.<br />
    /// Tips: Safe way to read sequentially without changing the span's original starting pointer.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Array de span de bytes de solo lectura. <br />En: Read-only byte span array.</param>
    /// <param name="pOffset">Es: Indice o puntero incremental. <br />En: Incremental index or pointer.</param>
    /// <typeparam name="T">Es: El tipo a deserializar de manera insegura desde su cast binario. <br />En: The type to deserialize unsafely via its binary cast.</typeparam>
    /// <returns>Es: Elemento decodificado extraído del arreglo. <br />En: Decoded element extracted from the array.</returns>
    public static T Read<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        return BufferReader.Get<T>(pBuffer, ref pOffset);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene un componente estructurado complejo usando una memoria base como molde dimensional.<br />
    /// Notas: Usa polimorfismo superficial para descifrar la estructura predefinida.<br />
    /// Tips: Útil cuando se necesita el cascarón pStruct para dictar comportamientos de copiado.<br />
    /// ___________________( English )___________________<br />
    /// Gets a complex structured component using a base memory as dimensional mold.<br />
    /// Notes: Uses shallow polymorphism to decode the predefined structure.<br />
    /// Tips: Useful when the pStruct shell is needed to dictate copying behaviors.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Origen de bytes estáticos puros. <br />En: Pure static byte source.</param>
    /// <param name="pOffset">Es: Valor a incrementar al consumir espacio. <br />En: Value to increment upon space consumption.</param>
    /// <param name="pStruct">Es: Plantilla a seguir en la deserialización. <br />En: Template to follow during deserialization.</param>
    /// <typeparam name="T">Es: Estructura conteniendo reglas de objeto serializable. <br />En: Structure containing serializable object rules.</typeparam>
    /// <returns>Es: Instancia completamente poblada e independiente. <br />En: Fully populated and independent instance.</returns>
    public static T ReadStruct<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        return BufferReader.GetStruct<T>(pBuffer, ref pOffset, pStruct);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Infiere una nueva instancia tipo T y prosigue a deserializar un segmento para llenarlo.<br />
    /// Notas: Internamente llama a su contraparte sobrecargada generando una instancia limpia previamente.<br />
    /// Tips: Método sugerido para estructuras sin estado previo requerido.<br />
    /// ___________________( English )___________________<br />
    /// Infers a new T-type instance and proceeds to deserialize a chunk to fill it.<br />
    /// Notes: Internally calls its overloaded counterpart generating a clean instance first.<br />
    /// Tips: Suggested method for structures without required prior state.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Entorno de memoria constante de lectura. <br />En: Read constant memory environment.</param>
    /// <param name="pOffset">Es: Indice que mantiene seguimiento del tamaño procesado. <br />En: Index keeping track of the processed size.</param>
    /// <typeparam name="T">Es: El tipo exacto que describe IStructSerializable. <br />En: Exact type mapping to IStructSerializable.</typeparam>
    /// <returns>Es: Elemento T resultante extraido como copia desde los bytes. <br />En: Resultant T element extracted as a copy from the bytes.</returns>
    public static T ReadStruct<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : struct, IStructSerializable
    {
        T newStruct = new T();
        return BufferReader.GetStruct<T>(pBuffer, ref pOffset, newStruct);
    }
}
