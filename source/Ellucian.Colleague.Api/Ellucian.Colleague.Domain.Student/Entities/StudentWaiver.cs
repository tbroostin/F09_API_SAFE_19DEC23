// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Registration waiver information for a student, in regard to a particular course or section.
    /// </summary>
    [Serializable]
    public class StudentWaiver
    {
        /// <summary>
        /// A unique identifier for the waiver. 0 if a new waiver.
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
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

        /// <summary>
        /// The Student this waiver belongs to
        /// </summary>
        public string StudentId { get { return _studentId; } }
        private string _studentId;

        /// <summary>
        /// The Id of the course for which waiver is created -- required if section Id not provided.
        /// </summary>
        public string CourseId { get { return _courseId; } }
        private string _courseId;

        /// <summary>
        /// The ID of the section for which waiver is created -- required if course not provided
        /// </summary>
        public string SectionId { get { return _sectionId; } }
        private string _sectionId;

        /// <summary>
        /// Reason Code for this waiver -- required if comment not provided
        /// </summary>
        public string ReasonCode { get { return _reasonCode; } }
        private string _reasonCode;

        /// <summary>
        /// Free-form comment for this waiver -- required if ReasonCode not provided
        /// </summary>
        public string Comment { get {return _comment;} }
        private string _comment;

        /// <summary>
        /// The effective term of this waiver. Used to apply the waiver to all sections of
        /// a course for a given term. Required if start/end date not identified.
        /// </summary>
        public string TermCode { get { return _termCode; } }
        private string _termCode;

        /// <summary>
        /// The effective start date of this waiver. Used to apply the waiver to all sections over
        /// a given date range. Required only if term not identified.
        /// </summary>
        public DateTime? StartDate { get { return _startDate; } }
        private DateTime? _startDate;

        /// <summary>
        /// The effective end date of this waiver. May be null to indicate waiver is open-ended.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The Id of the person who authorized the waiver
        /// </summary>
        public string AuthorizedBy { get; set; }

        /// <summary>
        /// Date/time this waiver was last changed
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }

        /// <summary>
        /// Person who last changed this waiver
        /// </summary>
        public string ChangedBy { get; set; }

        /// <summary>
        /// Indicate whether the waiver has been revoked and is no longer in effect. 
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// List of requisite waivers. Includes only requirement-based requisites.
        /// </summary>
        public ReadOnlyCollection<RequisiteWaiver> RequisiteWaivers { get; private set; }
        private readonly List<RequisiteWaiver> _requisiteWaivers = new List<RequisiteWaiver>();

        /// <summary>
        /// Method to enable individual requisite waivers to be added for the course/section
        /// </summary>
        /// <param name="requisiteWaiver">requisite waiver object to be added</param>
        public void AddRequisiteWaiver(RequisiteWaiver requisiteWaiver)
        {
            if (requisiteWaiver != null)
            {
                var reqWaiver = RequisiteWaivers.Where(rw => rw.RequisiteId == requisiteWaiver.RequisiteId).FirstOrDefault();
                if (reqWaiver != null)
                {
                    throw new ArgumentException("Cannot add waiver for requisite with Id " + requisiteWaiver.RequisiteId + ". RequisiteWaiver already exists for student in waiver " + Id);
                }
                else
                {
                    _requisiteWaivers.Add(requisiteWaiver);
                }
            }
        }

        /// <summary>
        /// Construct a waiver for a student and a given course or section.
        /// </summary>
        /// <param name="id">Unique Id of the waiver. Empty string if a new item.</param>
        /// <param name="studentId">Id of the student</param>
        /// <param name="courseId">Id of the course</param>
        /// <param name="sectionId">Id of the section, optional</param>
        /// <param name="reasonCode">Coded reason for the waiver, required if comment not provided</param>
        /// <param name="comment">Comment, required if reason not provided</param>
        /// <param name="termCode">Term Code, required if start date not provided</param>
        /// <param name="startDate">Start effective date of the waiver, required if term not provided</param>
        /// <param name="endDate">End date of the waiver, required if term and start date not provided</param>
        public StudentWaiver(string id, string studentId, string courseId, string sectionId, string reasonCode = null, string comment = null, string termCode = null, DateTime? startDate = null, DateTime? endDate=null)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required");
            }
            if (!string.IsNullOrEmpty(courseId) && !string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("Can specify either course Id or sectionId, but not both");
            }
            if (string.IsNullOrEmpty(courseId) && string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("Either Course ID or Section ID is required");
            }
            if (string.IsNullOrEmpty(reasonCode) && string.IsNullOrEmpty(comment))
            {
                throw new ArgumentException("Either reason or comment is required");
            }
            if (!string.IsNullOrEmpty(courseId) && string.IsNullOrEmpty(termCode) && startDate == null && endDate==null)
            {
                throw new ArgumentException("Either Term or Start Date or End Date is required if Course is provided");
            }
            if (!string.IsNullOrEmpty(termCode) && startDate != null)
            {
                throw new ArgumentException("Cannot specify both TermCode and Start Date");
            }
            
            _id = id;
            _studentId = studentId;
            _courseId = courseId;
            _sectionId = sectionId;
            _reasonCode = reasonCode;
            _comment = comment;
            _termCode = termCode;
            _startDate = startDate;
            EndDate = endDate;
            IsRevoked = false;

            RequisiteWaivers = _requisiteWaivers.AsReadOnly();
        }

        public void ValidateRequisiteWaivers(Section section, Course course)
        {
            // There must be at least one item in the list of requisites for this to be a valid section waiver.
            if (RequisiteWaivers == null || RequisiteWaivers.Count() == 0)
            {
                throw new ColleagueWebApiException("A waiver must contain at least one requisite waiver.");
            }

            // Determine the list of requisites that are currently in effect for the section, and that all of the
            // requisite IDs in the waiver are found in that list.
            var waiverableRequistes = SectionProcessor.DetermineWaiverableRequisites(section, course);

            // Find the requisites in the waiver that have been actioned (waived/denied)
            var actionedRequisites = RequisiteWaivers.Where(rw => rw.Status != Domain.Student.Entities.WaiverStatus.NotSelected);
            // If any of these are not found in the list of requisites that are waiverable, throw an exception
            var invalidRequisites = actionedRequisites.Where(rw => !waiverableRequistes.Any(er => er.RequirementCode == rw.RequisiteId));
            if (invalidRequisites.Count() > 0)
            {
                throw new ColleagueWebApiException("Requisites are not eligible for waiver: " + string.Join(", ", invalidRequisites));
            }
        }

        /// <summary>
        /// Determines if two Waiver objects are equal.
        /// Throws an exception in the case where items cannot be compared (term vs dates)
        /// </summary>
        /// <param name="obj">Waiver object to compare to</param>
        /// <returns>boolean indicating if the items are equal or not</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var item = obj as StudentWaiver;
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
                        throw new ColleagueWebApiException("Cannot compare Waivers. Cannot compare Term against dates");
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
    }
}
