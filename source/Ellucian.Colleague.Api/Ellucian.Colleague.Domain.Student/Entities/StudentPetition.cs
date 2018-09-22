// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentPetition
    {
        /// <summary>
        /// Petition Id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string _id;

        public string StudentId { get { return _studentId; } }
        private string _studentId;
        
        /// <summary>
        /// Course id
        /// </summary>
        public string CourseId
        {
            get { return _courseId; }
        }
        private string _courseId;

        /// <summary>
        /// Section Id that student belongs to
        /// </summary>
        public string SectionId
        {
            get { return _sectionId; }
        }
        private string _sectionId;

        /// <summary>
        /// Status Code - approved, Denied
        /// </summary>
        public string StatusCode 
        { 
            get { return _statusCode; } 
        }
        private string _statusCode;

        /// <summary>
        /// Reason Code for this petition
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Free-form comment for this petition
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Term code associated to the petition. Used if the petition is associated to a course and not a section. 
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Start Date associated to the petition. 
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date associated to the petition.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Date/time this petition was last changed
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }

        /// <summary>
        /// The name of the person who last updated the petition
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Indicates the type of student petition (faculty consent or regular)
        /// </summary>
        public StudentPetitionType Type { get { return _type; } }
        private StudentPetitionType _type;

        /// <summary>
        /// Constructor to create a new student petition. While the current forms in Colleague now require either a reason code or comment and either dates or terms these
        /// rules were not always in affect so we are making the domain accept items even when these rules are not true.
        /// </summary>
        /// <param name="id">Id of the student petition</param>
        /// <param name="courseId">Course Id associated to the petition. A petition must have a course or a section Id.</param>
        /// <param name="sectionId">Section Id associated to the petition. A petition must have either a course or section Id.</param>
        /// <param name="studentId">Student associated to the petition. Required.</param>
        /// <param name="type">Type of petition - either StudentPetition or FacultyConsent. Defaults to StudentPetition.</param>
        /// <param name="statusCode">Status code of the petition. Required.</param>
        public StudentPetition(string id, string courseId, string sectionId, string studentId, StudentPetitionType type, string statusCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required");
            }
            if (string.IsNullOrEmpty(statusCode))
            {
                throw new ArgumentNullException("statusCode", "Status Code is required");
            }
            if (string.IsNullOrEmpty(courseId) && string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("Either Course ID or Section ID is required");
            }
            this._id = id;
            this._studentId = studentId;
            this._courseId = courseId;
            this._sectionId = sectionId;
            this._statusCode = statusCode;
            this._type = type;
        }
      
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var item = obj as StudentPetition;
            if (item.StudentId == this.StudentId)
            {
                if (item.SectionId == this.SectionId && !string.IsNullOrEmpty(item.SectionId))
                {
                    // same student and section
                    return true;
                }
                if ((!string.IsNullOrEmpty(item.CourseId) || !string.IsNullOrEmpty(this.CourseId)) && item.CourseId == this.CourseId)
                {
                    if (item.TermCode == this.TermCode && !string.IsNullOrEmpty(item.TermCode))
                    {
                        // same student and course and term
                        return true;
                    }
                    if (string.IsNullOrEmpty(this.TermCode) && string.IsNullOrEmpty(item.TermCode))
                    {
                        if (item.EndDate.HasValue && this.EndDate.HasValue && item.StartDate <= this.EndDate && item.EndDate >= this.StartDate)
                        {
                            // same student and course and dates overlap
                            return true;
                        }
                    }
                    // In the case where one uses dates and the other uses a term, we cannot compare here because we don't have term dates
                    // throw an exception 
                    if ((!string.IsNullOrEmpty(this.CourseId) && string.IsNullOrEmpty(this.TermCode)) || ((!string.IsNullOrEmpty(item.CourseId) && string.IsNullOrEmpty(item.TermCode))))
                    {
                        throw new Exception("Cannot compare Petitions. Cannot compare Term against dates");
                    }
                }
                else
                {
                    // courses are specified and are not equal
                    return false;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
