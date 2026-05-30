using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.TerbinServiceHelper.Consoles;
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
/// Clase estática que provee nuevos métodos de extensión enfocados en la manipulación y escritura de la consola.<br />
/// ___________________( English )___________________<br />
/// Static class that provides new extension methods focused on console manipulation and writing.<br />
/// </summary>
public static class ConsoleExtension
{
    private static readonly Lock _lock = new();
    extension(Console)
    {
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime un mensaje por consola formateado para ser registrado a nivel informativo (color cian).<br />
        /// ___________________( English )___________________<br />
        /// Prints a formatted console message intended for informational logging (cyan color).<br />
        /// </summary>
        /// <param name="pMsg">Es: Mensaje de información a enviar. <br />En: Informational message to send.</param>
        public static void Log(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Cyan);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime una advertencia de seguridad o de estado a través de la consola visualizándolo en color amarillo.<br />
        /// ___________________( English )___________________<br />
        /// Prints a safety or status warning through the console, displaying it in yellow color.<br />
        /// </summary>
        /// <param name="pMsg">Es: Mensaje de advertencia a enviar. <br />En: Warning message to send.</param>
        public static void Warn(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Yellow);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime una alerta fatal o error, mostrando el texto de consola en color rojo.<br />
        /// ___________________( English )___________________<br />
        /// Prints a fatal alert or error, showing the console text in red color.<br />
        /// </summary>
        /// <param name="pMsg">Es: Mensaje de error a enviar. <br />En: Error message to send.</param>
        public static void Error(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Red);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Informa el éxito de un proceso imprimido en la terminal usando el color verde.<br />
        /// ___________________( English )___________________<br />
        /// Reports the success of a process printed on the terminal using green color.<br />
        /// </summary>
        /// <param name="pMsg">Es: Mensaje exitoso a enviar. <br />En: Success message to send.</param>
        public static void Succes(string pMsg)
        {
            Console.PrintLn(pMsg, ConsoleColor.Green);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime un mensaje por pantalla con un salto de línea adicional (WriteLine), asegurando un hilo seguro y aplicando un color temporal.<br />
        /// ___________________( English )___________________<br />
        /// Prints an on-screen message with an extra line break (WriteLine), ensuring thread safety while applying a temporal color.<br />
        /// </summary>
        /// <param name="pMsg">Es: El texto o variable textual a publicar. <br />En: Text or textual variable to attach.</param>
        /// <param name="pColor">Es: Color empleado al imprimir. <br />En: Color set upon printing process.</param>
        public static void PrintLn(string pMsg, ConsoleColor pColor = ConsoleColor.White)
        {
            lock (_lock)
            {
                var old = Console.ForegroundColor;
                Console.ForegroundColor = pColor;
                Console.WriteLine(pMsg);
                Console.ForegroundColor = old;
            }
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Escribe caracteres o texto en la consola de manera segura sin saltos de línea (Write) y aplicando un color asignado previamente temporal.<br />
        /// ___________________( English )___________________<br />
        /// Writes characters or plain text to the console safely without line breaks (Write) and applying an assigned temporary color.<br />
        /// </summary>
        /// <param name="pMsg">Es: Contenido en formato cadena estricto. <br />En: Text content strictly bound string format.</param>
        /// <param name="pColor">Es: Variación cromática de las letras sobre el mensaje terminal. <br />En: Displayed message text visual color scale terminal variant.</param>
        public static void Print(string pMsg, ConsoleColor pColor = ConsoleColor.White)
        {
            lock (_lock)
            {
                var old = Console.ForegroundColor;
                Console.ForegroundColor = pColor;
                Console.Write(pMsg);
                Console.ForegroundColor = old;
            }
        }
    }
}

// Ñe ñe ñe
/// <summary>
/// ___________________( Español )___________________<br />
/// Clase base abstracta de impresión por defecto de Terbin.<br />
/// ___________________( English )___________________<br />
/// Default abstract basic printing handler class of Terbin.<br />
/// </summary>
public abstract class APrint
{
    private static readonly Lock _lock = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Imprime un mensaje bloqueando temporalmente el hilo para no colisionar mensajes y modificando su color.<br />
    /// ___________________( English )___________________<br />
    /// Prints a message explicitly locking its thread as to avoid colliding events while swapping its current textual color.<br />
    /// </summary>
    /// <param name="pMsg">Es: Elemento a escribir al frente. <br />En: Targeted foreground element to print.</param>
    /// <param name="pColor">Es: Color específico implementado durante la acción local. <br />En: Defined local isolated scope operation console color.</param>
    public static void Print(string pMsg, ConsoleColor pColor)
    {
        lock (_lock)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = pColor;
            Console.Print(pMsg);
            Console.ForegroundColor = old;
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Firma de función asertiva abstracta donde el desarrollador define un modo de escritura personal.<br />
    /// ___________________( English )___________________<br />
    /// Explicit abstract footprint pointing on customized printing scenarios per application needs.<br />
    /// </summary>
    /// <param name="pMsg">Es: Componente crudo en cadena simple. <br />En: Input plain standalone simple string layout variable content.</param>
    public abstract void Print(string pMsg);
}
