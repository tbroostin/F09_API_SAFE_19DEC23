// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// A DegreePlanArchive is a "snap-shot" of a student's degree plan at a point in time
    /// </summary>
    [Serializable]
    public class DegreePlanArchive
    {
        private int _DegreePlanId;
        private int _Id;
        private string _StudentId;
        private int _Version;
        private List<DegreePlanNote> _Notes = new List<DegreePlanNote>();
        private List<ArchivedCourse> _ArchivedCourses = new List<ArchivedCourse>();
        private List<StudentProgram> _StudentPrograms = new List<StudentProgram>();

        /// <summary>
        /// A unique identifier for the plan archive. 0 if a new plan.
        /// </summary>
        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id == 0)
                {
                    _Id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// The record key of the degree plan from which this archive was generated
        /// </summary>
        public int DegreePlanId
        {
            get
            {
                return _DegreePlanId;
            }
        }

        /// <summary>
        /// The student Id for the person associated to the plan archive (required).
        /// </summary>
        public string StudentId
        {
            get
            {
                return _StudentId;
            }
        }

        /// <summary>
        /// The version number of the degree plan that was archived.
        /// </summary>
        public int Version
        {
            get
            {
                return _Version;
            }
        }

        /// <summary>
        /// The date/time the archive was created
        /// </summary>
        public DateTimeOffset? CreatedDate { get; set; }

        /// <summary>
        /// The ID of the user who created the archive
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Record key for the last person to complete a review of the plan
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// Timestamp for the last review of the plan
        /// </summary>
        public DateTimeOffset? ReviewedDate { get; set; }

        /// <summary>
        /// Comments added to the archive by student or advisors
        /// </summary>
        public List<DegreePlanNote> Notes
        {
            get { return _Notes; }
            set
            {
                if (value != null)
                {
                    _Notes = value;
                }
            }
        }

        /// <summary>
        /// Record of each planned course on the archive - along with all associated data
        /// </summary>
        public List<ArchivedCourse> ArchivedCourses
        {
            get { return _ArchivedCourses; }
            set
            {
                if (value != null)
                {
                    _ArchivedCourses = value;
                }
            }
        }

        /// <summary>
        /// Record of each planned course on the archive - along with all associated data
        /// </summary>
        public List<StudentProgram> StudentPrograms
        {
            get { return _StudentPrograms; }
            set
            {
                if (value != null)
                {
                    _StudentPrograms = value;
                }
            }
        }

        /// <summary>
        /// Construct a degree plan archive, based on a provided degree plan and current user's ID
        /// </summary>
        /// <param name="degreePlan">a degree plan</param>
        /// <param name="currentUserId">the current user's id</param>
        public DegreePlanArchive(int id, int degreePlanId, string studentId, int version)
        {
            if (id < 0)
            {
                throw new ArgumentException("Id must be >= 0");
            }
            if (degreePlanId <= 0)
            {
                throw new ArgumentNullException("degreePlanId", "Degree Plan Archive must have a Degree Plan Id indicating the plan it originated from.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Degree Plan Archive must have a student Id");
            }
            Id = id;
            _DegreePlanId = degreePlanId;
            _StudentId = studentId;
            _Version = version;
        }

        public static DegreePlanArchive CreateDegreePlanArchive(DegreePlan degreePlan, string currentUserId, IEnumerable<StudentProgram> studentPrograms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> academicCredits, IEnumerable<Grade> grades)
        {
            DegreePlanArchive degreePlanArchive = new DegreePlanArchive(0, degreePlan.Id, degreePlan.PersonId, degreePlan.Version);
            // These created by fields are going to be reset when the record is created in Colleague.
            degreePlanArchive.CreatedBy = currentUserId;
            degreePlanArchive.CreatedDate = DateTime.Now;
            degreePlanArchive.ReviewedBy = degreePlan.LastReviewedAdvisorId;
            degreePlanArchive.ReviewedDate = degreePlan.LastReviewedDate;
            degreePlanArchive.StudentPrograms = studentPrograms.ToList();
            degreePlanArchive.Notes = degreePlan.Notes;
            var archiveCourses = new List<ArchivedCourse>();
            List<string> academicCreditIdsUsed = new List<string>();

            // We have all "active" academic credits, but we don't want to include transfer or noncourse work in the archive
            var academicCreditsToArchive = academicCredits.Where(a => a != null && !(a.Status==CreditStatus.TransferOrNonCourse));

            foreach (var plannedCourse in degreePlan.NonTermPlannedCourses)
            {
                var course = courses.Where(crs => crs.Id == plannedCourse.CourseId).FirstOrDefault();
                var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
                // You cannot add a nonterm course - only a nonterm section. So in this case a section will be specified.
                // However, an advisor can only approve "courses" on specific terms.  
                var approval = degreePlan.Approvals.Where(a => a.CourseId == plannedCourse.CourseId && string.IsNullOrEmpty(a.TermCode)).OrderByDescending(a => a.Date).FirstOrDefault();
                // Find the academic credit that matches up with this course, if any
                var academicCredit = FindAcademicCredit(plannedCourse.CourseId, plannedCourse.SectionId, "", academicCreditsToArchive, ref academicCreditIdsUsed);
                try
                {
                    var archiveCourse = CreateArchiveCourse(plannedCourse, null, course, section, approval, academicCredit, grades);
                    archiveCourses.Add(archiveCourse);
                }
                catch
                {
                    // Something (probably course Id is missing. Just skip the course
                } 
            }
            foreach (var termCode in degreePlan.TermIds)
            {
                var plannedCourses = degreePlan.GetPlannedCourses(termCode);
                foreach (var plannedCourse in plannedCourses)
                {
                    var course = courses.Where(crs => crs.Id == plannedCourse.CourseId).FirstOrDefault();
                    var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
                    // Now, see if this course has been approved or denied for the specified term.
                    var approval = degreePlan.Approvals.Where(a => a.CourseId == plannedCourse.CourseId && a.TermCode == termCode).OrderByDescending(a => a.Date).FirstOrDefault();
                    // Find the academic credit that matches up with this course, if any
                    var academicCredit = FindAcademicCredit(plannedCourse.CourseId, plannedCourse.SectionId, termCode, academicCreditsToArchive, ref academicCreditIdsUsed);
                    try
                    {
                        var archiveCourse = CreateArchiveCourse(plannedCourse, termCode, course, section, approval, academicCredit, grades);
                        archiveCourses.Add(archiveCourse);
                    }
                    catch
                    {
                        // Something (probably course Id is missing. Just skip the course
                    }
                }
            }
            // Also add any academic credits that were not planned
            foreach (var academicCredit in academicCreditsToArchive)
            {
                if (academicCredit != null && !academicCreditIdsUsed.Contains(academicCredit.Id) && !(academicCredit.Status == CreditStatus.TransferOrNonCourse))
                {
                    // Get the course and section associated with this academic credit
                    Course course = null;
                    Section section = null;
                    DegreePlanApproval approval = null;
                    if (academicCredit.Course != null)
                    {
                        course = courses.Where(crs => crs.Id == academicCredit.Course.Id).FirstOrDefault();
                        // Even though it isn't currently on the plan, there might be a past approval
                        approval = degreePlan.Approvals.Where(a => a.CourseId == academicCredit.Course.Id && a.TermCode == academicCredit.TermCode).OrderByDescending(a => a.Date).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(academicCredit.SectionId))
                    {
                        section = sections.Where(s => s.Id == academicCredit.SectionId).FirstOrDefault();
                    }
                    try
                    {
                        var archiveCourse = CreateArchiveCourse(null, academicCredit.TermCode, course, section, approval, academicCredit, grades);
                        archiveCourses.Add(archiveCourse);
                    }
                    catch
                    {
                        // Something (probably course Id is missing. Just skip the course
                    }
                }
            }
            degreePlanArchive.ArchivedCourses = archiveCourses;
            return degreePlanArchive;
        }

        /// <summary>
        /// Archive course may be created for a planned course or an academic credit.
        /// </summary>
        /// <param name="plannedCourse"></param>
        /// <param name="termCode"></param>
        /// <param name="course"></param>
        /// <param name="section"></param>
        /// <param name="approval"></param>
        /// <param name="academicCredit"></param>
        /// <returns></returns>
        private static ArchivedCourse CreateArchiveCourse(PlannedCourse plannedCourse, string termCode, Course course, Section section, DegreePlanApproval approval, AcademicCredit academicCredit, IEnumerable<Grade> grades)
        {
            if (plannedCourse == null && academicCredit == null)
            {
                throw new ArgumentException("Either planned course or academic credit must be provided");
            }

            var courseId = string.Empty;
            if (course != null)
            {
                courseId = course.Id;
            }
            else if (plannedCourse != null)
            {
                courseId = plannedCourse.CourseId;
            }
            else if (academicCredit != null && academicCredit.Course != null)
            {
                courseId = academicCredit.Course.Id;
            }
            else
            {
                throw new ArgumentException("Cannot create an archive course, no course Id available");
            }

            var archiveCourse = new ArchivedCourse(courseId);

            if (course != null)
            {
                archiveCourse.Name = course.Name;
                archiveCourse.Title = course.Title;
                archiveCourse.Credits = course.MinimumCredits;
                archiveCourse.ContinuingEducationUnits = course.Ceus.GetValueOrDefault();
            }
            else
            {
                archiveCourse.Name = courseId;
                archiveCourse.Title = "Not Available";
            }

            archiveCourse.TermCode = termCode;

            if (plannedCourse != null)
            {
                archiveCourse.SectionId = plannedCourse.SectionId;
                if (plannedCourse.Credits.HasValue)
                {
                    archiveCourse.Credits = plannedCourse.Credits.GetValueOrDefault();
                }
                archiveCourse.AddedBy = plannedCourse.AddedBy;
                archiveCourse.AddedOn = plannedCourse.AddedOn;
                archiveCourse.IsPlanned = true;
            }

            if (section != null)
            {
                if (!string.IsNullOrEmpty(section.Title))
                {
                    archiveCourse.Title = section.Title;
                }
                if (!string.IsNullOrEmpty(section.Number))
                {
                    // No way to know what the delimter is so use a space here.
                    if (!string.IsNullOrEmpty(section.CourseName))
                    {
                        archiveCourse.Name = section.CourseName + " " + section.Number;
                    }
                    else
                    {
                        archiveCourse.Name = archiveCourse.Name + " " + section.Number;
                    }
                }
                // While we always save the credits on a planned course when a section is planned, 
                // we don't store CEUs with the planned section. So we should check the section and 
                // save the CEUs for the planned section. Could be different than the CEUS on the course.
                if (section.Ceus.HasValue)
                {
                    archiveCourse.ContinuingEducationUnits = section.Ceus;
                }
            }

            if (approval != null)
            {
                archiveCourse.ApprovalStatus = approval.Status.ToString();
                archiveCourse.ApprovedBy = approval.PersonId;
                archiveCourse.ApprovalDate = approval.Date;
            }

            // if academic credit is not null, set boolean registered flag and other
            // attributes that may be overridden by the academic credit.
            if (academicCredit != null)
            {
                archiveCourse.RegistrationStatus = academicCredit.Status.ToString();
                archiveCourse.Credits = academicCredit.Credit;
                if (academicCredit.HasVerifiedGrade)
                {
                    archiveCourse.Credits = academicCredit.AdjustedCredit;
                    var grade = grades.Where(g => g.Id == academicCredit.VerifiedGrade.Id && g.GradeSchemeCode == academicCredit.GradeSchemeCode).FirstOrDefault();
                    if (grade != null && grade.IsWithdraw)
                    {
                        archiveCourse.HasWithdrawGrade = true;
                    }
                }
                archiveCourse.ContinuingEducationUnits = academicCredit.ContinuingEducationUnits;
            }

            return archiveCourse;
        }

        /// <summary>
        /// Find an academic credit item that matches up with a planned course
        /// </summary>
        /// <param name="courseId">Id of the course that is planned</param>
        /// <param name="sectionId">Id of the section that is planned, if any</param>
        /// <param name="termId">Term of the planned course, if any</param>
        /// <param name="academicCredits">List of all academic credits</param>
        /// <param name="academicCreditIdsUsed">List of academic credits already associated with a planned course</param>
        /// <returns>AcademicCredit object</returns>
        private static AcademicCredit FindAcademicCredit(string courseId, string sectionId, string termId, IEnumerable<AcademicCredit> academicCredits, ref List<string> academicCreditIdsUsed)
        {
            var academicCreditsFound = academicCredits.Where(a => (a.Course != null && a.Course.Id == courseId) && (string.IsNullOrEmpty(sectionId) || a.SectionId == sectionId) && (string.IsNullOrEmpty(termId) || a.TermCode == termId));
            if (academicCreditsFound != null)
            {
                foreach (var academicCreditItem in academicCreditsFound)
                {
                    // If this academic credit has already been paired up with a planned course, or is a transfer/noncourse credit, keep looping
                    if (!(academicCreditIdsUsed.Contains(academicCreditItem.Id)))
                    {
                        // If this academic credit has not already been associated with another section, add it to the list of 
                        // academic credits used so it won't be used again, and return the academic credit object.
                        academicCreditIdsUsed.Add(academicCreditItem.Id);
                        return academicCreditItem;
                    }
                }
            }
            return null;
        }
    }
}
