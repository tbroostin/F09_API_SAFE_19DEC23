using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class HousingAssignmentStatus
    {
        public string Status { get; set; }
        public DateTime? StatusDate { get; set; }
    }
}
