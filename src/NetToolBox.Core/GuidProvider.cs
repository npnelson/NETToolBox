using NetToolBox.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Core
{
    public sealed class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();        
    }
}
