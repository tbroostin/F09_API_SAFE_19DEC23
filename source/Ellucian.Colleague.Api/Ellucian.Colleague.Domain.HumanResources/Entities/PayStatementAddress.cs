using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayStatementAddress
    {
        public string AddressLine { get; set; }

        public PayStatementAddress(string addressLine)
        {
            AddressLine = addressLine;
        }
    }
}
