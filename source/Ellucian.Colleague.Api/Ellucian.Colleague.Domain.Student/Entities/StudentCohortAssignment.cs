using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentCohortAssignment
    {
        public StudentCohortAssignment()
        {
        }

        public StudentCohortAssignment(string code, string guid)
        {
            if(string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException("code is required.");
            }
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException(string.Format("guid is required. Code: {0}", code));
            }
            _recordKey = code;
            _recordGuid = guid;
        }

        private string _recordKey;
        public string RecordKey { get { return _recordKey; } }
        private string _recordGuid;
        public string RecordGuid { get { return _recordGuid; } }
        public string PersonId { get; set; }
        public string CohortId { get; set; }
        public DateTime? StartOn { get; set; }
        public DateTime? EndOn { get; set; }
        public string AcadLevel { get; set; }
        public string CohortType { get; set; }
    }
}
