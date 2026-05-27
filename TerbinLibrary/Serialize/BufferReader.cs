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

// TODO: Usar "out" para devolver el byte[] y asin funcionar directamente con arrays.
/// <summary>
/// ___________________( Español )___________________<br />
/// Provee facilidades funcionales a Spans directos para leer transparente reduciendo su formato acampado.<br />
/// ___________________( English )___________________<br />
/// Offers functionally extended syntactic helpers on spans slicing off shrink footprint formats automatically implicitly tracking bounds off bounds padding constraints context sizes string constraints mappings matrices layout matrices parameter map mapping wrapper map mappings layout wrapper payload map parameters variables layouts property sequences elements parameters limit parameter variables map variables array sequences vector configurations.<br />
/// </summary>
public static class BufferReaderExtension
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Interpreta desde el índice cortando el espacio ocupado retornando la data no manipulada alojada vectorizada por byte block array footprint param format layout mold target limits variables constraints layout properties properties sizes params factors sizes forms map array params lists param configurations properties matrix properties limits boundaries strings limits string mapping parameters source layouts maps element mapping limit string constraints mappings layouts parameter structures configurations. <br />
    /// ___________________( English )___________________<br />
    /// Interprets limits chunk boundary sequences constraints mapped mapped layouts limits structures param layouts properties param configurations lists source parameters bounds sizes configurations parameters mapping mappings elements layout sizes constraints factor matrices payload mapping property strings strings variables boundary mappings structures mappings matrices list mappings mappings mapping array layouts lists payload parameter variables parameters structures variables string mappings variables bounds lists bounds lists properties limitations sizes layout factor mapping parameter array bounds constraints structures constraints mappings mappings. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Fragmento inmutable cambiante mediante corte. <br />En: Structurally slicing reducing map chunk payload limitation reference mapping strings payload string formatting mapping mapping structure bounds layout formatting factors configurations parameters configurations mappings factors configurations mapping variables constraints maps constraints bounds forms properties mapping limitations mappings list maps factor matrix vector lists configurations maps configurations boundaries formats configurations factors variables string constraints properties boundaries variables.</param>
    /// <returns>Es: Un conjunto escalar agrupado vector. <br />En: An ordered generic set structure format layout variables configuration payload bounds form lists arrays sizes payload matrix array sequences properties layouts structures element maps limitations layout boundary vector element bounds.</returns>
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
    /// Recorta consumiento el fragmento de modo destructivo devolviendo lectura nativa. <br />
    /// ___________________( English )___________________<br />
    /// Slices down consuming limits natively translating returned mapped limit array constraint mapping constraints properties forms layout limits format constraint array parameter elements parameters bounds string forms constraints factor vector bounds mapping strings lists constraints mappings mapping bounds limit parameters string sizes bounds boundaries vectors bounds parameters arrays string formats variables variables strings constraints strings parameters mappings variables configurations param configurations limitations constraints layouts configurations attributes element boundaries lists source string element parameter vector elements arrays sequences constraints. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Espacio consumible span por factor. <br />En: Expendable limiting variable space layout setup factor formats parameter factors properties boundaries sequences mapping array mappings variables vector properties structure matrices factor limits payload maps limitations mappings sequences constraint parameters configurations maps layouts parameters variables string configurations vectors mappings arrays boundaries forms bounds string variables factors strings limitations properties layout forms matrices configurations payload parameters boundary configurations parameters variables sequences variables limitations element bounds mappings attributes bounds sequences strings attributes parameters limits sizes lists formats layouts map arrays bounds bounds matrix sequence variables mappings arrays.</param>
    /// <returns>Es: Equivalencia serializada interna unmanaged. <br />En: Direct plain mapping unmanaged structure matrix.</returns>
    public static T Read<T>(this ref ReadOnlySpan<byte> pBuffer)
        where T : unmanaged
    {
        T newValue = MemoryMarshal.Read<T>(pBuffer);
        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
        return newValue;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Resta asumiendo estructura instanciada consumiendo búfer implícito devolviendo su mapeo estructural. <br />
    /// ___________________( English )___________________<br />
    /// Casts consuming structured space mapping subtracting returning filled map setup formatting vector boundary mapped. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Target inmutable límite cortable string ref delimitación secuencia constraint de búfer variable matrices de vectores param. <br />En: Slicing subtractive bounds constraints matrices parameters form string layout sequence configurations variable mapping.</param>
    /// <param name="pStruct">Es: Molde guía tipo param de vector. <br />En: Molding limit template mapping structure context target limit struct payload boundary factor object string configurations vector properties setup param formats sizes string bounds layouts string arrays vectors parameters mapping bounds array limit strings variables limits properties factors restrictions attributes lists maps parameters sequence structures variable vector layouts mappings layouts mappings element vectors formats layout vector constraints sizes sizes bounds lists element form limits matrix attributes configurations boundaries forms structures bounds arrays properties string element properties arrays limits configurations variables configurations configurations mappings limitations variables matrices parameters configurations limitations variables properties layouts parameters matrices.</param>
    /// <returns>Es: Representación instanciada formateada unmanaged target mapeada estructurada a través de vectores array mapeados string. <br />En: Setup cast format property object limitation layout limits setup formats properties limit sequence limits.</returns>
    public static T ReadStruct<T>(this ref ReadOnlySpan<byte> pBuffer, T pStruct)
        where T : struct, IStructSerializable
    {
        var length = pStruct.GetSize();
        T newStruct = Serialineitor.DeserializeStructRaw<T>(pBuffer[..length].ToArray());
        pBuffer = pBuffer[length..];
        return newStruct;
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Wrapper de puente referenciado para GetArray extendiendo el Span. <br />
    /// ___________________( English )___________________<br />
    /// Reference bridge wrapper format extending over GetArray map limits boundary variables struct. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Matriz continua de contexto general bounds layouts vectores forms matrices parameters limitaciones. <br />En: Limits structures variables properties sequences boundary strings payload map size layout parameter setup map.</param>
    /// <param name="pOffset">Es: Apuntador al inicio limit de padding array forms limits bounds matrices mapping vectores. <br />En: Offset pointer marker sequence boundary array vector mappings structure parameters.</param>
    /// <returns>Es: Variable serializada vector limits payload string variables properties limitations forms format matrices vectores parameters limits matrices bounds variables matrices vectors properties properties strings properties form layout limitations variable matrices bounds constraints limits map string constraints layout constraints boundary string limitations properties attributes parameters arrays properties vectors map matrices layouts element limit configurations vector mapping array bounds mappings parameters forms lengths bounds mapping array formats bounds forms estructuras element configurations matrices boundaries element arrays mappings bounds attributes parameters constraints strings bounds properties arrays limits layout variables shapes. <br />En: Structure limit boundaries structures variables configurations layouts limit mappings mapping elements variables mapped parameters forms limits configurations factors map vector limit properties array matrices shapes vectors bounds configurations boundaries configurations constraints forms form mappings properties shapes structures vectors lists properties lengths sizes limit matrices.</returns>
    public static T[] ReadArray<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        return BufferReader.GetArray<T>(pBuffer, ref pOffset);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrapolación wrapper a read unmanaged consumible no destructivo. <br />
    /// ___________________( English )___________________<br />
    /// Non disruptive pointing proxy formatting structure wrapping Get map boundary structures mappings parameters boundaries bounds form variable limit limitations map boundaries setup strings string layout limitations variables forms configuration array mappings vectors limit configurations layout boundaries shapes factors parameters variables forms attributes mapping configurations sequences properties boundaries limits form arrays variables lengths configurations parameters maps shapes forms variables mapping sizes limitations limit vector. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Referencia estática base matrices bounds vectors element limit constraints configuraciones layout formats mapped configuraciones limitations mapping parameters layouts matrices layout bounds configuraciones estructuras element mapping limits limitations arrays vectors configuraciones limitations lengths parameters bounds variables properties strings shapes matrices vectors properties sizes arrays parameters shapes configuraciones variables properties shapes lengths variables layouts constraints strings limitations shapes bounds constraints limitations constraints bounds bounds mappings forms limites mappings mappings strings limitations element shapes mappings configuraciones boundaries. <br />En: Layout shape map boundaries properties parameters sizes lengths parameters forms bounds matrices mappings properties constraints mapping limit bounds forms limits arrays variables lengths properties mappings layout parameters vectors limitations boundary properties configuraciones shapes layouts variables properties shapes mapping array boundaries matrices lengths configuraciones vectores vectores mapping configuraciones element attributes sizes constraints string mappings shapes limits boundaries layouts limits configuraciones parameters bounds limitations elements. </param>
    /// <param name="pOffset">Es: Apuntador limit variables limits bounds matrices parameters vector configuraciones constraints boundaries string properties attributes boundaries mappings forms element layouts variables layouts bounds bounds mappings limits limits variables array restrictions sizes maps properties matrices limitations boundary mapping parameters layouts mapping forms estructuras parámetros limit limites vectors lengths mapping vectors boundaries strings element mappings limitations limites configuraciones arrays constraints constraints configuraciones strings constraints parameters element layouts configuraciones parameters configuraciones layouts boundaries properties lengths array lengths bounds mapped shapes geometries boundaries shapes strings matrices properties lengths borders arrays borders constraints boundaries layouts variables mappings string boundaries arrays layout mapped matrices bounds configuraciones sizes properties parameters matrices mappings sequences constraints restrictions configuraciones vectores boundaries parameters. <br />En: Marker map string arrays layout properties limites vectors lengths bounds limit matrices boundaries limits array mapped strings forms limit bounds properties arrays bounds bounds limitations limits vectors strings mapping variables mapping mapping strings element mapped sizes mapping mappings parameters parameters attributes boundaries structures configuraciones string geometries variables variables limitations strings properties attributes sequences constraints mappings variables forms mappings bounds constraints array layout strings lengths variables variables vectores.</param>
    /// <returns>Es: Cast devuelto. <br />En: Parsed target element.</returns>
    public static T Read<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : unmanaged
    {
        return BufferReader.Get<T>(pBuffer, ref pOffset);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extrae vectorizando tipo mapping GetStruct usando un offset modificable. <br />
    /// ___________________( English )___________________<br />
    /// Yields type mapped cast GetStruct format updating referencing mutating padding properties bound parameters strings constraints limit array layout strings properties boundaries formatting vectors layout variables limits lengths properties constraints variables properties boundaries sizes. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Target buffer string map limitations variable limits properties sequences map arrays variables boundaries mapping parameters configuraciones limitations bounds properties array configuraciones boundary properties variables configuraciones vectores matrices boundaries layout properties shapes sizes forms matrices configuraciones limits. <br />En: Bound string vectors limits variables boundaries layouts configuraciones matrices variables properties configuraciones matrix limits sizes matrices limitations vectors properties constraints. </param>
    /// <param name="pOffset">Es: Vector map limit parameter layout size constraints strings arrays limits lengths limitations variables limitations geometries mapping parameters. <br />En: Size limitation setup pointer layout constraint forms properties geometry string bounding mapping limitations boundaries configuraciones lengths properties mappings configuraciones boundaries limitations vectors mapping layouts limites configuraciones mapping limits arrays constraints limits parameters sizes boundaries limit limits shapes constraint formats boundaries configuraciones restrictions limits shapes parameters matrices boundaries variables shapes templates mappings formats limits properties limits map limits sizes restrictions limitations limits arrays structures limits boundaries restrictions layouts variables bounds bounds mapping limits matrices mapping. </param>
    /// <param name="pStruct">Es: Layout base limits mapped vector forms boundaries strings. <br />En: Boundary format layout map sequences arrays limitations arrays bounding properties limit map structures limites vectors layouts bounds strings. </param>
    /// <returns>Es: Devuelve form boundary bounds matrices parameters vectors boundaries form mappings constraints boundaries limite limits strings vectors limits constraints geometries geometries bounds variables limits properties constraints limits shapes structures arrays arrays boundaries strings variables vectores variables lengths limites layout mappings properties bounds mappings limitations variables geometries shapes variables mappings mapping limits vectors mapping string templates bounds limites sizes layouts limites constraints templates boundaries geometries properties geometries sizes sizes restrictions arrays layouts limitations arrays vectores matrices strings properties layout strings attributes layouts geometries limitation forms sizes properties matrices mapping templates restrictions restrictions properties geometry forms boundaries restrictions limitations boundaries limites bounds mapping configuraciones arrays bounds limites variables bounds mapping. <br />En: Limits limit matrices map mapping variables forms limitations bounds configuraciones string mappings array mappings limit bound boundaries boundaries vectors boundaries limites vectors layouts configuraciones bounds attributes limitations lengths limitations boundaries mappings geometries variables mapping variables sizes limitation vectors maps shapes layouts limits configuraciones geometries coordinates limitations properties geometry forms vectors matrices arrays limits strings layouts constraints bounds limitation limits bounds boundaries constraints mapping sizes mappings arrays parameters arrays coordinates templates mapped boundary properties geometries constraints variables vector properties coordinates map forms limits map string mapping coordinates arrays vectores sizes bounds attributes limits limitation parameters mapping borders restrictions map. </returns>
    public static T ReadStruct<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        return BufferReader.GetStruct<T>(pBuffer, ref pOffset, pStruct);
    }
    public static T ReadStruct<T>(this ReadOnlySpan<byte> pBuffer, ref int pOffset)
        where T : struct, IStructSerializable
    {
        T newStruct = new T();
        return BufferReader.GetStruct<T>(pBuffer, ref pOffset, newStruct);
    }
}
