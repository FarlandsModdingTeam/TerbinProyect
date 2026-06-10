using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Data.Transport;

public interface IToDTO<T>
{
    T ToDTO();
}
