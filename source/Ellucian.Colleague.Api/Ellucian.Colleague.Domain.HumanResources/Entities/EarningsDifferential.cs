/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// An EarningsDifferential describes how to modify a person's earnings when this differential is applied to any of that person's earnings. 
    /// The criteria for when to apply this modification
    /// is determined by the Payroll Office or by the person's work schedule.
    /// </summary>
    [Serializable]
    public class EarningsDifferential : CodeItem
    {
        /// <summary>
        /// Create an EarningsDifferential with a code and a description
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public EarningsDifferential(string code, string description)
            : base (code, description)
        {

        }
    }
}
