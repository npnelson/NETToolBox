using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.TestHarness
{
    public class TableKey
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public override string ToString()
        {
            return PartitionKey + ":" + RowKey;
        }
    }
}
