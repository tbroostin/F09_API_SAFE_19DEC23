using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Instructor
    {
        public Instructor(string guid, string id)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required for instructor.");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id is required for instructor.");
            }

            this.RecordGuid = guid;
            this.RecordKey = id;
        }

        public string RecordGuid { get; private set; }
        public string RecordKey { get; private set; }
        public IEnumerable<int?> DepartmentPoints { get; set; }
        public string HomeLocation { get; set; }
        public string SpecialStatus { get; set; }
        public string ContractType { get; set; }
        public string TentureType { get; set; }
        public DateTime? TenureTypeDate { get; set; }
        public IEnumerable<FacultyDeptLoad> Departments { get; set; }
    }
}
