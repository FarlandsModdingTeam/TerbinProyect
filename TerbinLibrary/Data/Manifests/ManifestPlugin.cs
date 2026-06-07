using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data;

namespace TerbinLibrary.Data.Manifests;
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



public class ManifestPlugin : IManifest
{
    // Nombre del mod
    public string? Name { get; set; }
    // Creador del mod
    // public string? Owner { get; set; }
    // Id en el Storage
    public string? Id { get; set; }
    // Id generado al instalar
    public string? IdLocal { get; set; }
    // Pagina web del creador
    // public string? UrlWeb { get; set; }
    // Version del mod.
    // public string? Version { get; set; }
    // TOOD: ¿Esto que era?
    // public string? PathRoot { get; set; }
    // Si fue instalado fuera de la instancia y por tanto las rutas no son relativas.
    public bool? OutSideIntance { get; set; }
    // Contenido del plugin
    public DirectoryHandwritten? HandWritten { get; set; }

    public string? GetId()
    {
        return Id;
    }
}

