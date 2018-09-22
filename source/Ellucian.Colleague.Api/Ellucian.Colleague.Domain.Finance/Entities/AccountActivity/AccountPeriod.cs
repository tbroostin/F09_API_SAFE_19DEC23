// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class AccountPeriod
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public AccountPeriod()
        {
            AssociatedPeriods = null;
            Balance = null;
            Description = string.Empty;
            Id = string.Empty;
            StartDate = null;
            EndDate = null;
        }

        public List<string> AssociatedPeriods { get; set; }
        public decimal? Balance { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
