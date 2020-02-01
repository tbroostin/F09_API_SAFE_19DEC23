using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    public class ArchiveCourse
    {
        private string _CourseId;
        public string CourseId { get { return _CourseId; } }

        public string TermCode { get; set; }
        public string SectionId { get; set; }
        public decimal? Credits { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public ArchiveCourse(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId", "Archive Course must have a Course Id.");
            }
            _CourseId = courseId;
        }
    }
}
