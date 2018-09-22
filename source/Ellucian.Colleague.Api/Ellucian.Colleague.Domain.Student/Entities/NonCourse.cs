using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class NonCourse
    {
        public NonCourse(string guid, string code)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Aptitude assessment guid is required.");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("Aptitude assessment id is required.");
            }
            this.Guid = guid;
            this.Code = code;
        }

        public string Guid { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string ParentAssessmentId { get; set; }
        public int? ScoreMin { get; set; }
        public int? ScoreMax { get; set; }
        public int? ScoreIncrement { get; set; }
        public string CalculationMethod { get; set; }
        public string AssessmentTypeId { get; set; }
    }
}
