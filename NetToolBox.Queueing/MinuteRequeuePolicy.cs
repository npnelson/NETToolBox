using NetToolBox.Core.Abstractions;
using NetToolBox.Queueing.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.Queueing
{
    public sealed class MinuteRequeuePolicy : RequeuePolicy
    {

        // public static RequeuePolicy MinuteRequeuePolicyInstance = new MinuteRequeuePolicy();
        public MinuteRequeuePolicy(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
        {

        }
       
        public override DateTime CalculateNextQueueTime()
        {
            return _dateTimeProvider.CurrentDateTimeUTC.AddMinutes(1);
        }
    }
}
