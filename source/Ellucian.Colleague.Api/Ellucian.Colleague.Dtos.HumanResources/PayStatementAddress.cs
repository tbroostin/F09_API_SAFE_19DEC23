using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// A PayStatementAddress is an object wrapper around a single line (string) of address text.
    /// It should be used as a group with other PayStatementAddress objects, such as in a mailing label
    /// </summary>
    public class PayStatementAddress
    {
        /// <summary>
        /// A single address line in a group of address lines, such as a mailing label.
        /// </summary>
        public string AddressLine { get; set; }
    }
}
