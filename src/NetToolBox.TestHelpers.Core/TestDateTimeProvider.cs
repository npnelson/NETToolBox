using NetToolBox.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetToolBox.TestHelpers.Core
{
    public sealed class TestDateTimeProvider : IDateTimeProvider
    {


        private DateTime? _currentDateTime;

        /// <summary>
        /// Sets the CurrentDateTime for the IDateTimeProvider for testing purposes
        /// </summary>
        /// <param name="dateTime"></param>
        public void SetCurrentDateTimeUTC(DateTime dateTime)
        {
            _currentDateTime = dateTime;
        }

        /// <summary>
        /// Gets the CurrentDateTime.  Initializes upon first call to then current datetime or can be set explicitly through SetCurrentDateTime
        /// Once CurrentDateTime is set, you must call SetCurrentDateTime to change it, it will not automatically update on its own.
        /// </summary>
        /// <returns></returns>
        public DateTime CurrentDateTimeUTC
        {
            get
            {
                if (_currentDateTime == null) //if it hasn't been set, set it to current date time
                {
                    //to eliminate some precision problems when using datetime vs datetime2 in SQL, we will take the current time and chop the milliseconds off
                    var currentDate = DateTime.Now;
                    _currentDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour,
                    currentDate.Minute, currentDate.Second);
                }
                return _currentDateTime.Value;
            }
        }

        /// <summary>
        /// Adds seconds to the current DateTime value. 
        /// If CurrentDateTime is null it sets it to the DateTime.Now and then adds the seconds
        /// </summary>
        /// <param name="seconds"></param>
        public void AdvanceSeconds(int seconds)
        {
            if (_currentDateTime == null) //if it hasn't been set, set it to current date time
            {
                _currentDateTime = CurrentDateTimeUTC;
            }
            _currentDateTime = _currentDateTime.Value.AddSeconds(seconds);
        }

        /// <summary>
        /// Adds seconds to the current DateTime value. 
        /// If CurrentDateTime is null it sets it to the DateTime.Now and then adds the minutes
        /// </summary>
        /// <param name="minutes"></param>
        public void AdvanceMinutes(int minutes)
        {
            if (_currentDateTime == null) //if it hasn't been set, set it to current date time
            {
                _currentDateTime = CurrentDateTimeUTC;
            }
            _currentDateTime = _currentDateTime.Value.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds seconds to the current DateTime value. 
        /// If CurrentDateTime is null it sets it to the DateTime.Now and then adds the days
        /// </summary>
        /// <param name="days"></param>
        public void AdvanceDays(int days)
        {
            if (_currentDateTime == null) //if it hasn't been set, set it to current date time
            {
                _currentDateTime = CurrentDateTimeUTC;
            }
            _currentDateTime = _currentDateTime.Value.AddDays(days);
        }

    }

}
