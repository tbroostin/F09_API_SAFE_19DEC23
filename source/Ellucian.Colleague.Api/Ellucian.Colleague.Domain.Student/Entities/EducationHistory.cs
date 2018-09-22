using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class EducationHistory
    {
        private readonly string _id;

        public string Id { get { return _id; } }
        public List<HighSchool> HighSchools { get; set; }
        public List<College> Colleges { get; set; }

        public EducationHistory(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Id cannot be null or empty");
            }
            _id = id;
            HighSchools = new List<HighSchool>();
            Colleges = new List<College>();
        }
    }
}
