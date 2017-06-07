using NetToolBox.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.TestHelpers.Core
{
    public sealed class TestGuidProvider : IGuidProvider
    {
        private Guid _nextGuid = Guid.NewGuid();
        public Guid NewGuid()
        {
            return _nextGuid;
        }
        public void SetNextGuid(Guid guid)
        {
            _nextGuid = guid;
        }

    }
}
