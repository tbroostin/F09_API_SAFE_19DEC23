// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcadCredCourseSecInfo
    {
        public string RecordGuid { get; private set; }
        public string RecordKey { get; private set; }
        public string SectionId { get; private set; }
        public string StatusCode { get; set; }
        public string GradeScheme { get; set; }
        public string Term { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VerifiedGrade { get; set; }
        public string FinalGrade { get; set; }

        public StudentAcadCredCourseSecInfo(string sectionId)
        {
            SectionId = sectionId;
        }

        public StudentAcadCredCourseSecInfo(string guid, string id, string sectionId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Format("GUID is required. Record key: '{0}'.", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required.");
            }
            RecordGuid = guid;
            RecordKey = id;
            SectionId = sectionId;
        }
    }
}