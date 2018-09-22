using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    [Serializable]
    public class VendorType : GuidCodeItem
    {
        public VendorType(string guid, string code, string description)
            : base(guid, code, description) { }
    }
}
