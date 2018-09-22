using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FacultyDeptLoad
    {
        public int? DeptPcts { get; set; }
        public string FacultyDepartment { get; set; }
    }
}
