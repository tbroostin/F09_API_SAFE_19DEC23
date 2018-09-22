using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class HighSchool : HighSchoolGpa
    {
        public string HighSchoolName { get; set; }
        public string GraduationType { get; set; }
        public decimal? SummaryCredits { get; set; }
        public List<Credential> Credentials { get; set; }
        public DateTime? CredentialsEndDate { get; set; }
        public string Comments { get; set; }

        public HighSchool(string highSchoolId, decimal? highSchoolGpa, string lastAttendedYear)
            : base(highSchoolId, highSchoolGpa, lastAttendedYear)
        {
            Credentials = new List<Credential>();
        }
        public HighSchool(string highSchoolId)
            : base(highSchoolId)
        {
            new HighSchoolGpa(highSchoolId);
            Credentials = new List<Credential>();
        }
    }
}
