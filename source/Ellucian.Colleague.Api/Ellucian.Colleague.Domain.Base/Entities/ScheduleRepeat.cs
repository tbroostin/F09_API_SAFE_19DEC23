// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A code that identifies how often a scheduled event repeats
    /// </summary>
    [Serializable]
    public class ScheduleRepeat : CodeItem
    {
        /// <summary>
        /// Repeat interval
        /// </summary>
        private readonly int? _interval;
        public int? Interval { get { return _interval; } }

        /// <summary>
        /// Frequency type
        /// </summary>
        private readonly FrequencyType _frequencyType;
        public FrequencyType FrequencyType { get { return _frequencyType; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">ScheduleRepeat code</param>
        /// <param name="description">ScheduleRepeat description</param>
        /// <param name="interval">Repeat interval</param>
        /// <param name="frequencyType">Frequency type</param>
        public ScheduleRepeat(string code, string description, string interval, FrequencyType? frequencyType)
            : base(code, description)
        {
            // Only populate the interval and frequency type if they're both filled in
            int intervalValue;
            if (!string.IsNullOrEmpty(interval) && int.TryParse(interval, out intervalValue) && frequencyType.HasValue)
            {
                _interval = intervalValue;
                _frequencyType = frequencyType.Value;
            }
        }
    }
}
