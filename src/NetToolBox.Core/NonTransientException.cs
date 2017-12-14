using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Core
{
    /// <summary>
    /// Use this class or one inherited from it to indicate the exception is not due to a transient condtion
    /// </summary>
    public class NonTransientException:Exception
    {
        public NonTransientException(string message):base(message)
        {

        }
        public NonTransientException(string message,Exception innerException):base(message,innerException)
        {

        }
    }
}
