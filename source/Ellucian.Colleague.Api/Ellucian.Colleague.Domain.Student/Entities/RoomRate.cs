//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// RoomRate
    /// </summary>
    [Serializable]
    public class RoomRate : GuidCodeItem
    {
        // Start date
        public DateTime? StartDate { get; set; }

        // End date
        public DateTime? EndDate { get; set; }

        // Accounting code
        public string AccountingCode { get; set; }

        // Cancel accounting code
        public string CancelAccountingCode { get; set; }

        // Day rate
        public decimal? DayRate { get; set; }

        // Weekly rate
        public decimal? WeeklyRate { get; set; }

        // Monthly rate
        public decimal? MonthlyRate { get; set; }

        // Term rate
        public decimal? TermRate { get; set; }

        // Annual rate
        public decimal? AnnualRate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomRate"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public RoomRate(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}