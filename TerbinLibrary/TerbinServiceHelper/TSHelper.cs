using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace TerbinLibrary.TerbinServiceHelper;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */



/// <summary>
/// ___________________( Español )___________________<br />
/// Clase principal con utilidades de ayuda para los servicios de Terbin.<br />
/// ___________________( English )___________________<br />
/// Main generic helper utility class for Terbin services.<br />
/// </summary>
public class TSHelper
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene la representación en arreglo de bytes de un código de error interno.<br />
    /// ___________________( English )___________________<br />
    /// Gets the byte array representation of an internal error code.<br />
    /// </summary>
    /// <param name="pError">Es: Código de error a evaluar. <br />En: Internal error code to evaluate.</param>
    /// <returns>Es: Arreglo de bytes serializado. <br />En: Serialized byte array.</returns>
    public static byte[] GetError(InternalErrors pError)
    {
        return Serialineitor.Serialize((ushort)pError);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea y formatea un texto descriptivo a partir de una excepción capturada.<br />
    /// ___________________( English )___________________<br />
    /// Creates and formats a descriptive text out of a caught exception.<br />
    /// </summary>
    /// <param name="pE">Es: Excepción detectada. <br />En: Caught exception context.</param>
    /// <param name="pSite">Es: Referencia del sitio o contexto para el reporte. <br />En: Site reference context for logging report.</param>
    /// <returns>Es: Texto con detalles de la excepción. <br />En: String with exception details.</returns>
    public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
    {
        return Exception.CreateStringException(pE, pSite);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Subclase que comprende utilidades específicas orientadas a la depuración (Debugging).<br />
    /// ___________________( English )___________________<br />
    /// Subclass comprising specific utilities aimed at debugging sessions.<br />
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime iterativamente el tipo y los códigos de hash de un conjunto de objetos.<br />
        /// ___________________( English )___________________<br />
        /// Iteratively prints the type and hash codes of a set of objects.<br />
        /// </summary>
        /// <param name="pObjs">Es: Listado de elementos a evaluar. <br />En: Array of items to evaluate.</param>
        public static void PrintHas(params object[] pObjs)
        {
            string allHas = "";
            for (int i = 0; i < pObjs.Length; i++)
            {
                allHas += pObjs[i].GetType() + ": ";
                allHas += pObjs[i].GetHashCode() + ((i < pObjs.Length) ? "; \n" : "");
            }
            Console.Log($"{allHas}");
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime simultáneamente el tipo y código hash de varios objetos empleando LINQ.<br />
        /// ___________________( English )___________________<br />
        /// Concurrently prints the type and hash code of multiple objects using LINQ.<br />
        /// </summary>
        /// <param name="pObjs">Es: Colección de parámetros. <br />En: Parameter collection.</param>
        public static void PrintAllHas(params object[] pObjs)
        {
            string allHas = string.Join("; \n", pObjs.Select(o => $"{o.GetType()}: {o.GetHashCode()}"));
            Console.Log($"{allHas}");
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime el tipo y código hash exclusivamente de un objeto proporcionado.<br />
        /// ___________________( English )___________________<br />
        /// Prints the type and hash code exclusively of an inputted object.<br />
        /// </summary>
        /// <param name="pObj">Es: Instancia objetiva. <br />En: Objective instance.</param>
        public static void PrintHas(object pObj)
        {
            Console.Log($"{pObj.GetType()}: {pObj.GetHashCode()}");
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime explícitamente el tipo genérico y su respectivo código hash.<br />
        /// ___________________( English )___________________<br />
        /// Explicitly prints the generic type and its respective hash code.<br />
        /// </summary>
        /// <typeparam name="T">Es: Tipado estricto a solicitar. <br />En: Strict typing constraint to invoke.</typeparam>
        /// <param name="pObj">Es: Instancia no escalar/escalar dada. <br />En: Passed strictly typed instance object.</param>
        public static void PrintHasByT<T>(T pObj) where T : notnull
        {
            Console.Log($"{pObj.GetType()}: {pObj.GetHashCode()}");
        }
    }
}