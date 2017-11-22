using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetToolBox.Queueing.Abstractions
{
    public interface IMessageHandler<T>
    {
        Task<bool> HandleMessageAsync(T message);
    }
}
