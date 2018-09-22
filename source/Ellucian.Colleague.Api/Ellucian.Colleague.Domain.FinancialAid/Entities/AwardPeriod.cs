//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Award Period - Will contain the Code and Description
    /// </summary>
    [Serializable]
    public class AwardPeriod : CodeItem
    {
        private readonly DateTime _StartDate;

        public DateTime StartDate { get { return _StartDate; } }

        /// <summary>
        /// Award Period Constructor
        /// </summary>
        /// <param name="code">The Award Period code</param>
        /// <param name="desc">The Description of the Award Period</param>
        public AwardPeriod(string code, string desc, DateTime StartDate)
            : base(code, desc)
        {
            this._StartDate = StartDate;
        }
    }
}
