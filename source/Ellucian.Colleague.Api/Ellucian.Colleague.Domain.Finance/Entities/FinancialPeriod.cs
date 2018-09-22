// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Defines a financial period for Student Finance processing
    /// </summary>
    [Serializable]
    public class FinancialPeriod
    {
        // Private members
        private readonly PeriodType _Type;
        private readonly DateTime _Start;
        private readonly DateTime _End;

        /// <summary>
        /// Type of financial period
        /// </summary>
        public PeriodType Type { get { return _Type; } }

        /// <summary>
        /// Start date of financial period
        /// </summary>
        public DateTime Start { get { return _Start; } }

        /// <summary>
        /// End date of financial period
        /// </summary>
        public DateTime End { get { return _End; } }

        /// <summary>
        /// Constructor for a financial period
        /// </summary>
        /// <param name="period">Period type</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        public FinancialPeriod(PeriodType period, DateTime? startDate, DateTime? endDate)
        {
            switch (period)
            {
                case PeriodType.Past:
                    // End date must be specified
                    if (!endDate.HasValue)
                    {
                        throw new ArgumentNullException("endDate");
                    }
                    // End date cannot be max date
                    if (endDate.Value.Date == DateTime.MaxValue.Date)
                    {
                        throw new ArgumentOutOfRangeException("endDate");
                    }
                    _Type = PeriodType.Past;
                    _Start = DateTime.MinValue.Date;
                    _End = endDate.Value;
                    break;

                case PeriodType.Current:
                    // Start and End dates must be specified
                    if (!startDate.HasValue)
                    {
                        throw new ArgumentNullException("startDate");
                    }
                    if (!endDate.HasValue)
                    {
                        throw new ArgumentNullException("endDate");
                    }
                    // Start date must be before end date
                    if (startDate.Value.Date > endDate.Value.Date)
                    {
                        throw new ArgumentException("Start date cannot be after the end date");
                    }
                    _Type = PeriodType.Current;
                    _Start = startDate.Value;
                    _End = endDate.Value;
                    break;

                case PeriodType.Future:
                    // Start date must be specified
                    if (!startDate.HasValue)
                    {
                        throw new ArgumentNullException("startDate");
                    }
                    // Start date cannot be min date
                    if (startDate.Value.Date == DateTime.MinValue.Date)
                    {
                        throw new ArgumentOutOfRangeException("startDate");
                    }
                    _Type = PeriodType.Future;
                    _Start = startDate.Value;
                    _End = DateTime.MaxValue.Date;
                    break;
            }
        }
    }
}
