using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class UpdateStudentRestrictionRequest
    {
        public string Id { get; set; }

        public string Restriction { get; set; }

        public string Action { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public List<string> Comments { get; set; }

        public List<string> Options { get; set; }
    }
}
