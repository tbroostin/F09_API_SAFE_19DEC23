/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class having stipend information associated with employee compensation
    /// </summary>
    [Serializable]
    public class EmployeeStipend
    {
        /// <summary>
        /// Description of the stipend received by employee
        /// </summary>
        public string StipendDescription { get; set; }
        /// <summary>
        /// Stipend Amount received by employee
        /// </summary>
        public decimal? StipendAmount { get; set; }

        public EmployeeStipend(string stipendDescription, decimal? stipendAmount)
        {
            StipendDescription = stipendDescription;
            StipendAmount = stipendAmount;
        }
    }
}
