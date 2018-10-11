using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class GetActiveRestrictionsResponseDto
    {
        public List<string> ActiveRestrictions { get; set; }

        // Constructors
        public GetActiveRestrictionsResponseDto()
        { }

        public GetActiveRestrictionsResponseDto
        (
            List<string> activeRestrictions
        )
        {
            this.ActiveRestrictions = activeRestrictions;
        }
    }
}
