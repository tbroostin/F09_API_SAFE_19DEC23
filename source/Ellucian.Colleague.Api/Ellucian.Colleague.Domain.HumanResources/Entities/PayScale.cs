/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PayScale
    /// </summary>
    [Serializable]
    public class PayScale : GuidCodeItem
    {
        /// <summary>
        /// The start date of the pay scale.
        /// </summary>
        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("Start Date cannot be after the EndDate");
                }
                startDate = value;
            }
        }

        private DateTime? startDate;

        /// <summary>
        /// The end date of the pay scale
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("End Date cannot be before Start Date");
                }
                endDate = value;
            }
        }

        private DateTime? endDate;

        /// <summary>
        /// Key to SWTABLES, wage table id.
        /// </summary>
        public string WageTableId { get; set; }

        /// <summary>
        /// Key to SWTABLES, wage table guid.
        /// </summary>
        public string WageTableGuid { get; set; }

        /// <summary>
        /// The pay structure based on grades with step levels.
        /// </summary>
        public List<PayScalesScales> Scales { get; set; }

        /// <summary>
        /// Create a PayScale
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="code">A code that may be used to identify the pay scale.</param>
        /// <param name="description">The full name of the pay scale.</param>
        /// <param name="startDate">The start date of the pay scale.</param>
        /// <param name="endDate">The end date of the pay scale.</param>
        public PayScale(string guid, string code, string description, DateTime? startDate, DateTime? endDate)
           : base (guid, code, description)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.Scales = new List<PayScalesScales>();
        }
    }
}