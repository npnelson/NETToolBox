using Microsoft.Extensions.Logging;
using NetToolBox.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Dapper
{
    public sealed class DapperHelperTracer:BaseTracer
    {
        public DapperHelperTracer(ILogger<BaseTracer> logger):base(logger)
        {

        }
    }
}
