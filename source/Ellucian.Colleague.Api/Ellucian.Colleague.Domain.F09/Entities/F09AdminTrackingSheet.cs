using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class F09AdminTrackingSheet
    {
        public string StuId { get; set; }
        public string StuName { get; set; }
        public string StadType { get; set; }
        public string Prog { get; set; }
        public string ReviewTerms { get; set; }
    }
}
