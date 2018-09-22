using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class HighSchoolGpa
    {
        private readonly string _highSchoolId;
        public string HighSchoolId { get { return _highSchoolId; } }
        public decimal? Gpa { get; set; }
        public string LastAttendedYear { get; set; }

        public HighSchoolGpa(string highSchoolId, decimal? highSchoolGpa, string lastAttendedYear)
        {
            if (string.IsNullOrEmpty(highSchoolId))
            {
                throw new ArgumentNullException("HighSchoolId may not be null or empty");
            }
            this._highSchoolId = highSchoolId;
            this.Gpa = highSchoolGpa;
            this.LastAttendedYear = lastAttendedYear;
        }
        public HighSchoolGpa(string highSchoolId)
        {
            if (string.IsNullOrEmpty(highSchoolId))
            {
                throw new ArgumentNullException("HighSchoolId may not be null or empty");
            }
            this._highSchoolId = highSchoolId;
        }
    }
}
