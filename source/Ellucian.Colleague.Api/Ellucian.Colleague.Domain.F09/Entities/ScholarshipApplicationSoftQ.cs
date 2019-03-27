using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class ScholarshipApplicationSoftQ
    {
        public string Code { get; set; }

        public string Desc { get; set; }

        public bool Checked { get; set; }
    }
}
