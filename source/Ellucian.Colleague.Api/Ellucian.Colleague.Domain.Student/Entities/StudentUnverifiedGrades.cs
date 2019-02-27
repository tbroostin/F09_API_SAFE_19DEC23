//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentUnverifiedGrades
    {
        public StudentUnverifiedGrades(string guid, string studentCourseSecId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Student unverified grades guid is required.  Missing for student course sec '" + studentCourseSecId + "'");
            }
            if (string.IsNullOrEmpty(studentCourseSecId))
            {
                throw new ArgumentNullException("Student unverified grades key is required.  Missing for guid '" + guid + "'");
            }

            Guid = guid;
            StudentCourseSecId = studentCourseSecId;
        }

        public string Guid { get; set; }

        public string StudentCourseSecId { get; private set; }

        public string StudentId { get; set; }

        public string StudentAcadaCredId { get; set; }
        
        public string GradeScheme { get; set; }

        public string GradeType { get; set; }

        public string GradeId { get; set; }

        public string FinalGrade { get; set; }

        public string MidtermGrade1 { get; set; }

        public string MidtermGrade2 { get; set; }

        public string MidtermGrade3 { get; set; }

        public string MidtermGrade4 { get; set; }

        public string MidtermGrade5 { get; set; }

        public string MidtermGrade6 { get; set; }
        
        public DateTime? MidtermGradeDate1 { get; set; }

        public DateTime? MidtermGradeDate2 { get; set; }

        public DateTime? MidtermGradeDate3 { get; set; }

        public DateTime? MidtermGradeDate4 { get; set; }

        public DateTime? MidtermGradeDate5 { get; set; }

        public DateTime? MidtermGradeDate6 { get; set; }

        public DateTime? FinalGradeDate { get; set; }

        public DateTime? IncompleteGradeExtensionDate { get; set; }

        public DateTime? LastAttendDate { get; set; }

        public bool HasNeverAttended { get; set; }
    }

}
