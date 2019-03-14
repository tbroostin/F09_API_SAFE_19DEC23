using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class ScholarshipApplicationAwards
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Desc { get; set; }

        public string MinMax { get; set; }

        public string AddnlRequ { get; set; }

        public bool Checked { get; set; }
    }
}
