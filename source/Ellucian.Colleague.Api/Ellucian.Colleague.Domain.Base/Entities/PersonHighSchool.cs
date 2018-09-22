using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonHighSchool
    {
        public string PersonId { get; set; }
        public string HighSchoolId { get; set; }
        public decimal GradePointAverage { get; set; }

        public PersonHighSchool(string personId, string highSchoolId, decimal gpa)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(highSchoolId))
            {
                throw new ArgumentNullException("highSchoolId");
            }

            PersonId = personId;
            HighSchoolId = highSchoolId;
            GradePointAverage = gpa;
        }
    }
}
