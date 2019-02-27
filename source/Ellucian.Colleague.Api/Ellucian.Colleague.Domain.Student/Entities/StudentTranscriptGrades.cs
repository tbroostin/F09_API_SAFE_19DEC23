// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentTranscriptGrades
    {
        // Required Ids
        private string _Id;
        /// <summary>
        /// Unique ID.
        /// </summary>
        public string Id { get { return _Id; } }

        // Required Ids
        private string _Guid;
        /// <summary>
        /// Unique ID.
        /// </summary>
        public string Guid { get { return _Guid; } }

        /// <summary>
        /// Course associated with this academic credit.
        /// </summary>
        public string Course { get; set; }
       
        /// <summary>
        /// Final grade expiration date
        /// </summary>
        public DateTime? FinalGradeExpirationDate { get; set; }
   
        /// <summary>
        /// Attempted credits, typically the same as the intended credits for the course unless the verified grade is not to be included in attempted.
        /// </summary>
        public decimal? AttemptedCredit { get; set; }

        /// <summary>
        /// Completed credits, typically the same as the original credits for the course.
        /// </summary>
        public decimal? CompletedCredit { get; set; }
    
        /// <summary>
        /// Total grade points earned, used as the numerator in the GPA calculation
        /// </summary>
        public decimal? GradePoints { get; set; }
        
        /// <summary>
        /// The list of academic credit Ids that are involved in the replacement
        /// </summary>
        public List<string> RepeatAcademicCreditIds { get; set; }

        /// <summary>
        /// ID for a student course section associated with the academic credit
        /// </summary>     
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// The ID of the person whose academic activity this is.
        /// </summary>
        public string StudentId { get; set; }

        //public string SectionId { get; set; }

        /// <summary>
        /// The credit type associated with this entry for
        //use in GPA calculation
        /// </summary>
        public string CreditType { get; set; }

        /// <summary>
        /// This is the number of continuing education units that
        /// will be counted towards CEUs attempted.
        /// </summary>
        public decimal? AttemptedCeus { get; set; }

        /// <summary>
        /// This is the number of continuing ed units completed.
        /// </summary>
        public decimal? CompletedCeus { get; set; }

        /// <summary>
        /// This is the number of grade points that this entry
        /// will contribute toward the student's running cumulative.
        /// </summary>
        public decimal? ContribGradePoints { get; set; }

        /// <summary>
        /// The number of credits completed that will be counted
        /// towards cumulative GPA credits
        /// </summary>
        public decimal? ContribGpaCredits { get; set; }

        /// <summary>
        /// The number of credits completed that will be counted
        /// towards cumulative credits completed.
        /// </summary>
        public decimal? ContribCmplCredits { get; set; }

        /// <summary>
        /// Indictes when a student repeats a course.
        /// </summary>
        public string ReplCode { get; set; }

        /// <summary>
        ///  The date the verified grade was added or changed.
        /// </summary>
        public DateTime? VerifiedGradeDate { get; set; }

        /// <summary>
        /// It is used to determine which set of
        // grades can be assigned to this student's academic credit entry.
        /// </summary>
        public string GradeSchemeCode { get; set; }
        
        /// <summary>
        ///  verified final grade of the student's academic credit.
        /// </summary>
        public string VerifiedGrade { get; set; }

        /// <summary>
        /// Name of the course when the student took it.
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// Description of this academic credit record.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Contains a flag to tell transcript display to use the cumulative or alternate 
        /// cumulative data fields for display of the totals.
        /// </summary>
        public bool StwebTranAltcumFlag { get; set; }

        /// <summary>
        /// This is the number of grade points that this entry
        /// will contribute toward the student's alternate running cumulative.
        /// </summary>
        public decimal? AltcumContribGradePts { get; set; }

        /// <summary>
        /// The number of GPA credits that will be counted
        /// towards the alternate cumulative GPA credits.
        /// </summary>
        public decimal? AltcumContribGpaCredits { get; set; }

        /// <summary>
        /// The number of credits completed that will be counted
        /// towards the alternate cumulative credits completed.
        /// </summary>
        public decimal? AltcumContribCmplCredits { get; set; }

        /// <summary>
        /// Course Section
        /// </summary>
        public string CourseSection { get; set; }

        /// <summary>
        /// StudentTranscriptGradesHistory collection
        /// </summary>
        public List<StudentTranscriptGradesHistory> StudentTranscriptGradesHistory { get; set; }

        /// <summary>
        /// Base constructor for student transcript grades. 
        /// </summary>
        /// <param name="id">ID of this student transcript grades</param>
        /// <param name="guid">GUID of this student transcript grades</param>
        public StudentTranscriptGrades(string id, string guid)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("StudentTranscriptGrades id not found.");  
            }
            _Id = id;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Format("StudentTranscriptGrades guid not found for id: '{0}'.", id)); 
            }
            _Guid = guid;
        }
  
        /// <summary>
        /// Equals method used for comparisons
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            StudentTranscriptGrades other = obj as StudentTranscriptGrades;
            if (other == null)
            {
                return false;
            }
            return Id.Equals(other.Id);

        }

        /// <summary>
        /// Needed for Equals comparisons
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }      
    }
}
