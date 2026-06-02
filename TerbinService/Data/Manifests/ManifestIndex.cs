using System;
using System.Collections.Generic;
using System.Text;
using TerbinService.Data.References;

namespace TerbinService.Data.Manifests;

public class ManifestIndex
{
    public List<ReferenceInstance> Instances = new();
}
