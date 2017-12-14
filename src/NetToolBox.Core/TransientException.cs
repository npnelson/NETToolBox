using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Core
{
    /// <summary>
    /// Use this exception class or one inherited from it to indicate the exception is likely transient (ie network down,etc)
    /// </summary>
    public class TransientException:Exception
    {
        public TransientException(string message) : base(message)
        {

        }
        public TransientException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
