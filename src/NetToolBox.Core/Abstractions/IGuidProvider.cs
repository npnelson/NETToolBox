using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Core.Abstractions
{
    public interface IGuidProvider
    {
        Guid NewGuid();
    }
}
