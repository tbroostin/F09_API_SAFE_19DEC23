//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// HR Person status from HR.STATUSES used in PERSTAT.
    /// </summary>
    [Serializable]
    public class PersonStatuses: GuidCodeItem
    {
        /// <summary>
        /// Category of Full Time or Part Time
        /// </summary>
        public ContractType? Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonStatuses"/> class.
        /// </summary>
        public PersonStatuses(string guid, string code, string description, string category)
            : base(guid, code, description)
        {
            if (!string.IsNullOrEmpty(category) && category.Substring(0, 1).ToUpperInvariant() == "F")
            {
                Category = ContractType.FullTime;
            }
            else
            {
                Category = ContractType.PartTime;
            }
        }
    }
}
