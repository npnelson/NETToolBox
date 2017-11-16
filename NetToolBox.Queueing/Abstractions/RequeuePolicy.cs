using NetToolBox.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Queueing.Abstractions
{
    public abstract class RequeuePolicy
    {
        protected readonly IDateTimeProvider _dateTimeProvider;

        public RequeuePolicy(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        public abstract DateTime CalculateNextQueueTime();
       
        public bool HasExpired(DateTime initialQueueTime)
        {
            var retval = false;
            if (DateTime.UtcNow > initialQueueTime.Add(FinalExpirationTimeSpan)) retval = true;
            return retval;

        }
        protected virtual TimeSpan FinalExpirationTimeSpan => TimeSpan.FromDays(1);
    }
}
