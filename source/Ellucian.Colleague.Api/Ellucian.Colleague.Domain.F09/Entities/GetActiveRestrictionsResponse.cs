using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class GetActiveRestrictionsResponse
    {
        public List<string> ActiveRestrictions { get; set; }
    }
}
