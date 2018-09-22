/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The type of the employment frequency.
    /// </summary>
    [Serializable]
    public enum EmploymentFrequenciesType
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,


        /// <summary>
        /// daily
        /// </summary>
        Daily,

        /// <summary>
        /// weekly
        /// </summary>
        Weekly,

        /// <summary>
        /// biWeekly
        /// </summary>
        Biweekly,

        /// <summary>
        /// monthly
        /// </summary>
        Monthly,

        /// <summary>
        /// quarterly
        /// </summary>
        Quarterly,

        /// <summary>
        /// annually
        /// </summary>
        Annually,

        /// <summary>
        /// semimonthly
        /// </summary>
        Semimonthly,

        /// <summary>
        /// semiannually
        /// </summary>
        Semiannually,

        /// <summary>
        /// contractual
        /// </summary>
        Contractual

    }
}
