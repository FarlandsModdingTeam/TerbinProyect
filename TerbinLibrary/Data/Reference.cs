using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Data;

/// <summary>
/// Es la referencia del mod en el Json (¿que json?, El del Index). <br />
/// Esto es de cuando accediamos al repositorio como BD, pinche Magincian.
/// </summary>
[Obsolete]
public class Reference
{
    public string? Name;
    public string? GUID;
    public string? manifestUrl;
}
