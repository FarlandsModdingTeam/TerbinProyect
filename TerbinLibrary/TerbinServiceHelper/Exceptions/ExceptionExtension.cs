using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace TerbinLibrary.TerbinServiceHelper.Exceptions;
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
/// Clase estática que provee métodos de extensión para el manejo y formateo de excepciones.<br />
/// ___________________( English )___________________<br />
/// Static class providing extension methods for handling and formatting exceptions.<br />
/// </summary>
public static class ExceptionExtension
{
    extension(Exception)
    {
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Crea y formatea una cadena de texto estructurada con los detalles completos de la excepción.<br />
        /// ___________________( English )___________________<br />
        /// Creates and formats a structured text string containing the full details of the exception.<br />
        /// </summary>
        /// <param name="pE">Es: Excepción base detectada. <br />En: Caught base exception.</param>
        /// <param name="pSite">Es: Ubicación, método o contexto del error. <br />En: Location, method, or context of the error.</param>
        /// <returns>Es: Cadena multi-línea con la información del error. <br />En: Multi-line string with the error information.</returns>
        public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
        {
            return
            $$"""
            [{{pSite}}] =>
            {
                Message: {{pE.Message}};
                Source: {{pE.Source ?? "N/A"}};
                Inner: {{pE.InnerException?.Message ?? "N/A"}};
                Trace: {{pE.StackTrace ?? "N/A"}};
                String: {{pE.ToString()}}
            }
            """;
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Método de extensión que actúa como alias para acortar la invocación al formateo de la excepción.<br />
    /// ___________________( English )___________________<br />
    /// Extension method acting as an alias to shorten the invocation for exception formatting.<br />
    /// </summary>
    /// <param name="pE">Es: Instancia extendida de la excepción. <br />En: Extended exception instance.</param>
    /// <param name="pSite">Es: Contexto o sitio desde donde surgió el problema. <br />En: Context or site where the issue arose.</param>
    /// <returns>Es: Texto detallando el fallo. <br />En: Text detailing the failure.</returns>
    public static string CrString(this Exception pE, string pSite = "ExceptionError")
    {
        return Exception.CreateStringException(pE, pSite);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Formatea la excepción capturada y la imprime directamente a la salida estándar de errores (Console.Error).<br />
    /// ___________________( English )___________________<br />
    /// Formats the caught exception and prints it straight to the standard error output (Console.Error).<br />
    /// </summary>
    /// <param name="pE">Es: Instancia de la excepción a registrar. <br />En: Exception instance to be logged.</param>
    /// <param name="pSite">Es: Origen textual del error. <br />En: Textual origin of the error.</param>
    public static void PrintException(this Exception pE, string pSite = "ExceptionError")
    {
        string e = Exception.CreateStringException(pE, pSite);
        Console.Error(e);
    }
}