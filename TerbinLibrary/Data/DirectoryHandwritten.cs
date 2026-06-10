using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerbinLibrary.Data;


/// <summary>
/// ___________________( Español )___________________<br />
/// Representa una estructura de directorios y archivos de forma manual.<br />
/// Permite almacenar listas de rutas y serializarlas a formato JSON.<br />
/// Notas: Esta clase es útil para mapear estructuras de sistema de archivos sin depender de objetos pesados del sistema operativo.<br />
/// Tips: Puedes obtener el tamaño total sumando la cantidad de archivos y directorios con el método GetSize().<br />
/// ___________________( English )___________________<br />
/// Represents a manual directory and file structure.<br />
/// Allows storing lists of paths and serializing them to JSON format.<br />
/// Notes: This class is useful for mapping file system structures without relying on heavy OS objects.<br />
/// Tips: You can get the total size by adding the number of files and directories using the GetSize() method.<br />
/// </summary>
public class DirectoryHandwritten
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene o establece la lista de directorios representados como cadenas de texto.<br />
    /// Notas: Por defecto se inicializa como una lista vacía.<br />
    /// Tips: Almacena rutas relativas o absolutas según tus necesidades.<br />
    /// ___________________( English )___________________<br />
    /// Gets or sets the list of directories represented as strings.<br />
    /// Notes: It is initialized as an empty list by default.<br />
    /// Tips: Store relative or absolute paths according to your needs.<br />
    /// </summary>
    public List<string> Directories { get; set; } = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene o establece la lista de archivos representados como cadenas de texto.<br />
    /// Notas: Por defecto se inicializa como una lista vacía.<br />
    /// Tips: Puedes almacenar opcionalmente solo los nombres de archivo o las rutas completas.<br />
    /// ___________________( English )___________________<br />
    /// Gets or sets the list of files represented as strings.<br />
    /// Notes: It is initialized as an empty list by default.<br />
    /// Tips: You can optionally store just the file names or the full paths.<br />
    /// </summary>
    public List<string> Files { get; set; } = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene o establece la ruta raíz a la cual pertenecen estos directorios y archivos.<br />
    /// Notas: Esta propiedad es ignorada durante la serialización JSON.<br />
    /// Tips: Utilízala como referencia principal para resolver rutas relativas.<br />
    /// ___________________( English )___________________<br />
    /// Gets or sets the root path to which these directories and files belong.<br />
    /// Notes: This property is ignored during JSON serialization.<br />
    /// Tips: Use it as a primary reference to resolve relative paths.<br />
    /// </summary>
    [JsonIgnore]
    public string? Root { get; set; }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa la instancia actual a una cadena en formato JSON utilizando las opciones especificadas.<br />
    /// Notas: Es ideal cuando necesitas formatos específicos de serialización, como ignorar datos nulos.<br />
    /// Tips: Reutiliza instancias de JsonSerializerOptions en toda la app para mejorar el rendimiento.<br />
    /// ___________________( English )___________________<br />
    /// Serializes the current instance to a JSON formatted string using the specified options.<br />
    /// Notes: Ideal when you need specific serialization formats, such as ignoring null data.<br />
    /// Tips: Reuse JsonSerializerOptions instances throughout the app to improve performance.<br />
    /// </summary>
    /// <param name="options">
    /// Es: Las opciones de serialización JSON a utilizar. <br />En: The JSON serialization options to use.
    /// </param>
    /// <returns>
    /// Es: Una cadena de texto con la representación en JSON del objeto. <br />En: A string containing the JSON representation of the object.
    /// </returns>
    public string ToJson(JsonSerializerOptions options) => JsonSerializer.Serialize(this, options);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa la instancia actual a una cadena en formato JSON utilizando las opciones por defecto.<br />
    /// Notas: Las opciones por defecto incluyen la indentación activada para mejor lectura.<br />
    /// Tips: Usa este método para volcar rápidamente el contenido a un registro o la consola.<br />
    /// ___________________( English )___________________<br />
    /// Serializes the current instance to a JSON formatted string using default options.<br />
    /// Notes: Default options include indentation enabled for better readability.<br />
    /// Tips: Use this method to quickly dump the content to a log or console.<br />
    /// </summary>
    /// <returns>
    /// Es: Una cadena de texto con formato JSON indentado. <br />En: A formatted, indented JSON string.
    /// </returns>
    public string ToJson() => JsonSerializer.Serialize(this, _options);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el tamaño total sumando la cantidad de directorios y de archivos.<br />
    /// Notas: El valor devuelto es de tipo long para evitar desbordamientos de datos.<br />
    /// Tips: Es útil para validar si la estructura tiene datos antes de iterar.<br />
    /// ___________________( English )___________________<br />
    /// Gets the total size by summing the number of directories and files.<br />
    /// Notes: The returned value is a long type to prevent data overflow.<br />
    /// Tips: Useful for validating if the structure has data before iterating.<br />
    /// </summary>
    /// <returns>
    /// Es: La suma total de los elementos contenidos en Directories y Files. <br />En: The total sum of elements contained in Directories and Files.
    /// </returns>
    public long GetSize() => (long)Directories.Count + (long)Files.Count;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Opciones de serialización JSON estáticas para su configuración por defecto.<br />
    /// Notas: Se ha habilitado la escritura de objetos JSON indentados.<br />
    /// Tips: Al ser estático, es compartido por las instancias, resultando en un mejor rendimiento.<br />
    /// ___________________( English )___________________<br />
    /// Static JSON serialization options for default configuration.<br />
    /// Notes: Indented JSON object writing has been enabled.<br />
    /// Tips: Being static, it is shared across instances, resulting in better performance.<br />
    /// </summary>
    [JsonIgnore]
    private static JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };
}
