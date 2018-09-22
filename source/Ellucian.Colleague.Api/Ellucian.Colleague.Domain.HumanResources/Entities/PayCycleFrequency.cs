// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayCycleFrequency : CodeItem
    {
        /// <summary>
        /// Number of times a pay cycle occurs in a year
        /// </summary>
        public int AnnualPayFrequency { get; private set; }

        /// <summary>
        /// Representation of validation code pay cycle frequency
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="annualPayFrequency"></param>
        public PayCycleFrequency(string code, string description, int annualPayFrequency)
            : base (code, description)
        {
            this.AnnualPayFrequency = annualPayFrequency;
        }

    }
}
