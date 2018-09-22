using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Records the types of pay period states. The pay period status is derived based on the pay control.
    /// </summary>
    [Serializable]
    public enum PayPeriodStatus
    {
        /// <summary>
        /// Pay period has been created for the paycycle, but is not yet prepared
        /// </summary>
        New,
        /// <summary>
        /// Pay period has been prepared, but not yet generated
        /// </summary>
        Prepared,
        /// <summary>
        /// Pay period has or is in the process of being generated
        /// </summary>
        Generated,
        /// <summary>
        /// Pay period has completed payroll calculations (benefits/taxes)
        /// </summary>
        Calculation,
        /// <summary>
        /// Pay period has completed check/advice printing
        /// </summary>
        Print,
        /// <summary>
        /// Pay period has completed summary registers/GL posting
        /// </summary>
        Posting,
        /// <summary>
        /// Pay period has completed employee history update
        /// </summary>
        History,
        /// <summary>
        /// Pay period is in manual check or reversal
        /// </summary>
        Reversal
    }
}
