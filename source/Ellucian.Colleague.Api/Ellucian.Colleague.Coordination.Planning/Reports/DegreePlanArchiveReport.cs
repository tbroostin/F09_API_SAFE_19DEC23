﻿// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.using System;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using slf4net;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Coordination.Planning.Reports
{
    /// <summary>
    /// Model containing the parameters and datasets required to generate the degree plan archive pdf report
    /// </summary>
    public class DegreePlanArchiveReport
    {
        ILogger logger;
        /// <summary>
        /// A dataset that contains information about each archived course on the course plan archive.
        /// </summary>
        public DataSet ArchivedCoursesDataSet
        {
            get
            {
                DataSet dataSet = new DataSet();
                Type type = ArchivedCourses.GetType();
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                StringWriter writer = new StringWriter();
                List<Ellucian.Colleague.Coordination.Planning.ArchivedCourse> archivedCoursesToSerialize = new List<Ellucian.Colleague.Coordination.Planning.ArchivedCourse>();
                if (ArchivedCourses.Count() > 0)
                {
                    archivedCoursesToSerialize = ArchivedCourses;
                }
                else
                {
                    archivedCoursesToSerialize.Add(new ArchivedCourse() { Title = "No courses or course placeholders on this plan", CoursePlaceholderId = string.Empty });
                }
                xmlSerializer.Serialize(writer, archivedCoursesToSerialize);
                StringReader reader = new StringReader(writer.ToString());
                dataSet.ReadXml(reader);

                return dataSet;
            }
        }

        /// <summary>
        /// A dataset that contains information about the archived notes for the course plan archive.
        /// </summary>
        public DataSet ArchivedNotesDataSet
        {
            get
            {
                DataSet dataSet = new DataSet();
                Type type = ArchivedNotes.GetType();
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                StringWriter writer = new StringWriter();
                List<ArchivedDegreePlanNote> archivedNotesToSerialize = new List<ArchivedDegreePlanNote>();
                if (ArchivedNotes.Count() > 0)
                {
                    archivedNotesToSerialize = ArchivedNotes;
                }
                else
                {
                    archivedNotesToSerialize.Add(new ArchivedDegreePlanNote() { Text = "There are no notes on this archived plan.", Id = 0, PersonName = "", Date = "" });
                }
                xmlSerializer.Serialize(writer, archivedNotesToSerialize);
                StringReader reader = new StringReader(writer.ToString());
                dataSet.ReadXml(reader);

                return dataSet;
            }
        }
        public string ReportTitle { get; set; }
        /// <summary>
        /// Full name of the student of the course plan archive
        /// </summary>
        public string StudentName { get; set; }
        /// <summary>
        /// Last name (surname) of the student of the course plan archive
        /// </summary>
        public string StudentLastName { get; set; }
        /// <summary>
        /// First name of the student of the course plan archive
        /// </summary>
        public string StudentFirstName { get; set; }
        /// <summary>
        /// Id number of the student of the course plan archive
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Dictionary that contains the student's programs. Key is the program code and the value is the program name.
        /// </summary>
        public Dictionary<string, string> StudentPrograms { get; set; }
        /// <summary>
        /// Name of the person who last reviewed this course plan before it was archived.
        /// </summary>
        public string ReviewedBy { get; set; }
        /// <summary>
        /// Date and time the course plan was last reviewed prior to being archived.
        /// </summary>
        public DateTimeOffset ReviewedOn { get; set; }
        /// <summary>
        /// Name of person who archived (created) this course plan archive.
        /// </summary>
        public string ArchivedBy { get; set; }
        /// <summary>
        /// Date and time when the course plan archive was created.
        /// </summary>
        public DateTimeOffset ArchivedOn { get; set; }
        /// <summary>
        /// List of archived courses on this course plan archive  
        /// </summary>
        public List<ArchivedCourse> ArchivedCourses { get; set; }
        /// <summary>
        /// List of archived degree plan comments for this course plan archive
        /// </summary>
        public List<ArchivedDegreePlanNote> ArchivedNotes { get; set; }

        /// <summary>
        /// Constructor for the course plan archive
        /// </summary>
        public DegreePlanArchiveReport()
        {
            StudentPrograms = new Dictionary<string, string>();
            ArchivedCourses = new List<ArchivedCourse>();
            ArchivedNotes = new List<ArchivedDegreePlanNote>();
        }

        /// <summary>
        /// Build a degree plan archive report model from a degree plan archive.
        /// </summary>
        /// <param name="degreePlanArchive">The degree plan archive for which the model is to be built</param>
        /// <param name="student">Student of the degree plan archive</param>
        /// <param name="programs">List of program domain objects - needed for program descriptions</param>
        /// <param name="advisors">List of advisor domain objects - needed for advisor name translations</param>
        /// <param name="termDtos">List of Term DTOs - used for the term descriptions and ordering</param>
        public DegreePlanArchiveReport(Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive degreePlanArchive, Ellucian.Colleague.Domain.Student.Entities.Student student, IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program> programs, IEnumerable<Ellucian.Colleague.Domain.Planning.Entities.Advisor> advisors, IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> terms, IEnumerable<StudentProgram> studentPrograms, ILogger logger)
        {
            if (degreePlanArchive == null)
            {
                throw new ArgumentNullException("degreePlanArchive", "Degree Plan Archive is required.");
            }
            if (student == null)
            {
                throw new ArgumentNullException("student", "Student is required.");
            }
            if (programs == null)
            {
                throw new ArgumentNullException("programs", "Programs are required.");
            }
            if (advisors == null)
            {
                throw new ArgumentNullException("advisors", "Advisors are required.");
            }
            if (terms == null || terms.Count() == 0)
            {
                throw new ArgumentNullException("terms", "At least one term is required.");
            }
            if (studentPrograms == null || studentPrograms.Count() == 0)
            {
                throw new ArgumentNullException("studentPrograms", "At least one student program is required.");
            }
            this.logger = logger;
            StudentPrograms = new Dictionary<string, string>();
            ArchivedCourses = new List<ArchivedCourse>();
            ArchivedNotes = new List<ArchivedDegreePlanNote>();

            // Set up report title
            ReportTitle = "Course Plan";

            // Add name and Id to the report
            if (student.PersonDisplayName != null)
            {
                StudentName = student.PersonDisplayName.FullName;
                StudentLastName = student.PersonDisplayName.LastName;
                StudentFirstName = student.PersonDisplayName.FirstName;
            }
            else
            {
                StudentName = student.PreferredName;
                StudentLastName = student.LastName;
                StudentFirstName = student.FirstName;
            }
            StudentId = student.Id;


            // Get the student's program informtation and add it to the report
            List<string> programStrings = new List<string>();
            foreach (var program in degreePlanArchive.StudentPrograms)
            {
                try
                {
                    var programEntity = programs.Where(p => p.Code == program.ProgramCode).FirstOrDefault();
                    if (programEntity != null)
                    {
                        string programName = programEntity.Title;
                        StudentProgram spr = studentPrograms.Where(s => s != null && s.ProgramCode == program.ProgramCode).FirstOrDefault();
                        if (spr != null)
                        {
                            if (!string.IsNullOrEmpty(spr.ProgramName))
                            {
                                programName = spr.ProgramName;
                            }
                        }
                        StudentPrograms.Add(programName, program.CatalogCode);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Cannot add student program");
                }
            }

            // Get the last reviewed information and add it to the report parameters
            if (!string.IsNullOrEmpty(degreePlanArchive.ReviewedBy))
            {
                Ellucian.Colleague.Domain.Planning.Entities.Advisor reviewedBy = advisors.Where(a => a.Id == degreePlanArchive.ReviewedBy).FirstOrDefault();
                if (reviewedBy != null && reviewedBy.PersonDisplayName != null && !string.IsNullOrEmpty(reviewedBy.PersonDisplayName.FullName))
                {
                    ReviewedBy = reviewedBy.PersonDisplayName.FullName;
                }
                else
                {
                    ReviewedBy = GetAdvisorStaffName(degreePlanArchive.ReviewedBy, reviewedBy);
                }
            }

            if (degreePlanArchive.ReviewedDate.HasValue)
            {
                ReviewedOn = degreePlanArchive.ReviewedDate.Value;
            }

            // Get the created by information and add it to the report parameters
            if (!string.IsNullOrEmpty(degreePlanArchive.CreatedBy))
            {
                Ellucian.Colleague.Domain.Planning.Entities.Advisor createdBy = advisors.Where(a => a.Id == degreePlanArchive.CreatedBy).FirstOrDefault();
                if (createdBy != null && createdBy.PersonDisplayName != null && !string.IsNullOrEmpty(createdBy.PersonDisplayName.FullName))
                {
                    ArchivedBy = createdBy.PersonDisplayName.FullName;
                }
                else
                {
                    ArchivedBy = GetAdvisorStaffName(degreePlanArchive.CreatedBy, createdBy);
                }
            }

            if (degreePlanArchive.CreatedDate.HasValue)
            {
                ArchivedOn = degreePlanArchive.CreatedDate.Value;
                ReportTitle = ReportTitle + " as of " + degreePlanArchive.CreatedDate.Value.Date.ToShortDateString();
            }

            // Sort the archived courses
            var sortedArchivedCourses = (from t in terms
                                         join c in degreePlanArchive.ArchivedCourses on t.Code equals c.TermCode into joinArchiveCourses
                                         orderby t.ReportingYear, t.Sequence
                                         from ac in joinArchiveCourses
                                         select ac).ToList();
            var nontermArchivedCourses = degreePlanArchive.ArchivedCourses.Where(a => string.IsNullOrEmpty(a.TermCode));
            try
            {
                sortedArchivedCourses.AddRange(nontermArchivedCourses);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Cannot sort archived courses.");
            }

            foreach (var archivedCourse in sortedArchivedCourses)
            {
                var archivedCourseModel = new Ellucian.Colleague.Coordination.Planning.ArchivedCourse();
                archivedCourseModel.CourseId = archivedCourse.CourseId;
                archivedCourseModel.SectionId = archivedCourse.SectionId;
                archivedCourseModel.CoursePlaceholderId = string.Empty;
                archivedCourseModel.Credits = archivedCourse.Credits;
                // Rounding credits to 2 decimals as agreed
                archivedCourseModel.FormattedCredits = (archivedCourse.Credits.HasValue) ? archivedCourse.Credits.Value.ToString(".##") : string.Empty;
                archivedCourseModel.ContinuingEducationUnits = archivedCourse.ContinuingEducationUnits;
                archivedCourseModel.FormattedCeus = (archivedCourse.ContinuingEducationUnits.HasValue) ? archivedCourse.ContinuingEducationUnits.Value.ToString(".##") : string.Empty;
                archivedCourseModel.Name = archivedCourse.Name;
                archivedCourseModel.Title = archivedCourse.Title;
                archivedCourseModel.ApprovalStatus = archivedCourse.ApprovalStatus;
                if (archivedCourse.HasWithdrawGrade)
                {
                    archivedCourseModel.RegistrationStatus = "Withdrawn";
                }
                else if (archivedCourse.IsRegistered)
                {
                    archivedCourseModel.RegistrationStatus = "Yes";
                }

                // For courses that have no approval status but are planned, update the approval status.
                if (string.IsNullOrEmpty(archivedCourseModel.ApprovalStatus) && archivedCourse.IsPlanned)
                {
                    archivedCourseModel.ApprovalStatus = "Planned";
                }
                var termDescription = "Non-term Courses";
                int termReportingYear = 0;
                int termSequence = 0;
                if (!string.IsNullOrEmpty(archivedCourse.TermCode))
                {
                    var term = terms.First(t => t.Code == archivedCourse.TermCode);
                    if (term != null)
                    {
                        termDescription = term.Description;
                        termReportingYear = term.ReportingYear;
                        termSequence = term.Sequence;
                    }
                }
                archivedCourseModel.TermCode = archivedCourse.TermCode;
                archivedCourseModel.TermDescription = termDescription;
                archivedCourseModel.TermReportingYear = termReportingYear;
                archivedCourseModel.TermSequence = termSequence;

                // If there is approval information, get advisor name and format timestamps
                if (!string.IsNullOrEmpty(archivedCourse.ApprovedBy))
                {
                    // The archive course has IDs, not advisor names, so translate them
                    Ellucian.Colleague.Domain.Planning.Entities.Advisor approvedBy = advisors.Where(a => a.Id == archivedCourse.ApprovedBy).FirstOrDefault();

                    if (approvedBy != null && approvedBy.PersonDisplayName != null && !string.IsNullOrEmpty(approvedBy.PersonDisplayName.FullName))
                    {
                        archivedCourseModel.ApprovedBy = approvedBy.PersonDisplayName.FullName;
                    }
                    else
                    {
                        archivedCourseModel.ApprovedBy = GetAdvisorStaffName(archivedCourse.ApprovedBy, approvedBy, "Initial");
                    }

                    // Format the approval timestamp
                    archivedCourseModel.ApprovalDate = archivedCourse.ApprovalDate.HasValue ?
                        archivedCourse.ApprovalDate.Value.Date.ToShortDateString() : null;
                }

                // If there is added by information, get person's name and format timestamps
                if (!string.IsNullOrEmpty(archivedCourse.AddedBy))
                {
                    // The archive course has IDs, not advisor names, so translate them
                    // If the person who added the note is the student, get the name for that student, otherwise, get an advisor name
                    if (StudentId == archivedCourse.AddedBy)
                    {
                        archivedCourseModel.AddedBy = "Student";
                    }
                    else
                    {
                        Ellucian.Colleague.Domain.Planning.Entities.Advisor addedBy = advisors.Where(a => a.Id == archivedCourse.AddedBy).FirstOrDefault();

                        if (addedBy != null && addedBy.PersonDisplayName !=null && !string.IsNullOrEmpty(addedBy.PersonDisplayName.FullName))
                        {
                            archivedCourseModel.AddedBy = addedBy.PersonDisplayName.FullName;
                        }
                        else
                        {
                            archivedCourseModel.AddedBy = GetAdvisorStaffName(archivedCourse.AddedBy, addedBy, "Initial");
                        }                       
                    }
                    // Format the approval timestamp
                    archivedCourseModel.AddedDate = archivedCourse.AddedOn.HasValue ?
                        archivedCourse.AddedOn.Value.Date.ToShortDateString() : null;
                }
                try
                {
                    ArchivedCourses.Add(archivedCourseModel);
                }
                catch (Exception ex)
                {
                    // If it cannot add the course continue.
                    logger.Error(ex, "Cannot add course to archive.");
                }

            }

            // Sort the archived course placeholders
            degreePlanArchive.ArchivedCoursePlaceholders = degreePlanArchive.ArchivedCoursePlaceholders.Where(acp => acp != null).ToList();
            var sortedArchivedCoursePlaceholders = (from t in terms
                                                    join c in degreePlanArchive.ArchivedCoursePlaceholders on t.Code equals c.TermCode into joinArchiveCourses
                                                    orderby t.ReportingYear, t.Sequence
                                                    from ac in joinArchiveCourses
                                                    select ac).ToList();
            var nontermArchivedCoursePlaceholders = degreePlanArchive.ArchivedCoursePlaceholders.Where(a => string.IsNullOrEmpty(a.TermCode));
            try
            {
                sortedArchivedCoursePlaceholders.AddRange(nontermArchivedCoursePlaceholders);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Cannot sort archived course placeholders.");
            }
            foreach (var archivedCoursePlaceholder in sortedArchivedCoursePlaceholders)
            {
                var archivedCourseModel = new Ellucian.Colleague.Coordination.Planning.ArchivedCourse();
                archivedCourseModel.CourseId = string.Empty;
                archivedCourseModel.SectionId = string.Empty;
                archivedCourseModel.CoursePlaceholderId = archivedCoursePlaceholder.Id;
                archivedCourseModel.Credits = 0m;
                // Rounding credits to 2 decimals as agreed
                archivedCourseModel.FormattedCredits = archivedCoursePlaceholder.CreditInformation;
                archivedCourseModel.ContinuingEducationUnits = 0m;
                archivedCourseModel.FormattedCeus = string.Empty;
                archivedCourseModel.Name = "(Placeholder)";
                archivedCourseModel.Title = archivedCoursePlaceholder.Title;
                archivedCourseModel.ApprovalStatus = string.Empty;
                archivedCourseModel.RegistrationStatus = string.Empty;

                var termDescription = "Non-term Course Placeholders";
                int termReportingYear = 0;
                int termSequence = 0;
                if (!string.IsNullOrEmpty(archivedCoursePlaceholder.TermCode))
                {
                    var term = terms.First(t => t.Code == archivedCoursePlaceholder.TermCode);
                    if (term != null)
                    {
                        termDescription = term.Description;
                        termReportingYear = term.ReportingYear;
                        termSequence = term.Sequence;
                    }
                }
                archivedCourseModel.TermCode = archivedCoursePlaceholder.TermCode;
                archivedCourseModel.TermDescription = termDescription;
                archivedCourseModel.TermReportingYear = termReportingYear;
                archivedCourseModel.TermSequence = termSequence;

                archivedCourseModel.ApprovedBy = string.Empty;
                archivedCourseModel.ApprovalDate = string.Empty;

                // If there is added by information, get person's name and format timestamps
                if (!string.IsNullOrEmpty(archivedCoursePlaceholder.AddedBy))
                {
                    // The archive course has IDs, not advisor names, so translate them
                    // If the person who added the note is the student, get the name for that student, otherwise, get an advisor name
                    if (StudentId == archivedCoursePlaceholder.AddedBy)
                    {
                        archivedCourseModel.AddedBy = "Student";
                    }
                    else
                    {
                        Ellucian.Colleague.Domain.Planning.Entities.Advisor addedBy = advisors.Where(a => a.Id == archivedCoursePlaceholder.AddedBy).FirstOrDefault();

                        if (addedBy != null && addedBy.PersonDisplayName != null && !string.IsNullOrEmpty(addedBy.PersonDisplayName.FullName))
                        {
                            archivedCourseModel.AddedBy = addedBy.PersonDisplayName.FullName;
                        }
                        else
                        {
                            archivedCourseModel.AddedBy = GetAdvisorStaffName(archivedCoursePlaceholder.AddedBy, addedBy, "Initial");
                        }
                    }
                    // Format the approval timestamp
                    archivedCourseModel.AddedDate = archivedCoursePlaceholder.AddedOn.HasValue ?
                        archivedCoursePlaceholder.AddedOn.Value.Date.ToShortDateString() : null;
                }
                try
                {
                    ArchivedCourses.Add(archivedCourseModel);
                }
                catch (Exception ex)
                {
                    // If it cannot add the course placeholder, continue.
                    logger.Error(ex, "Cannot add course placeholder");
                }
            }

            // Notes
            var sortedNotes = degreePlanArchive.Notes.OrderByDescending(n => n.Date);
            foreach (var note in sortedNotes)
            {
                var archivedNoteDto = new ArchivedDegreePlanNote();
                archivedNoteDto.Id = note.Id;
                archivedNoteDto.Text = note.Text;
                archivedNoteDto.PersonType = (StudentId == note.PersonId ? Ellucian.Colleague.Dtos.Student.PersonType.Student.ToString() : Ellucian.Colleague.Dtos.Student.PersonType.Advisor.ToString());
                if (!string.IsNullOrEmpty(note.PersonId))
                {
                    // If the person who added the note is the student, get the name for that student, otherwise, get an advisor name
                    if (StudentId == note.PersonId)
                    {
                        archivedNoteDto.PersonName = "Student";
                    }
                    else
                    {
                        Ellucian.Colleague.Domain.Planning.Entities.Advisor noteWriter = advisors.Where(a => a.Id == note.PersonId).FirstOrDefault();

                        if (noteWriter !=null && noteWriter.PersonDisplayName != null && !string.IsNullOrEmpty(noteWriter.PersonDisplayName.FullName))
                        {
                            archivedNoteDto.PersonName = noteWriter.PersonDisplayName.FullName;
                        }
                        else
                        {
                            archivedNoteDto.PersonName = GetAdvisorStaffName(note.PersonId, noteWriter, "Initial");
                        }
                    }
                }
                // Format the note timestamp
                archivedNoteDto.Date = "on " + note.Date.Value.Date.ToShortDateString() + " at " + note.Date.Value.DateTime.ToShortTimeString();
                try
                {
                    ArchivedNotes.Add(archivedNoteDto);
                }
                catch (Exception ex)
                {
                    // If it can't add a note we will continue.
                    logger.Error(ex, "Cannot add archived note.");
                }

            }
        }

        /// <summary>
        /// Returns the formatted name of the advisor provided using the format type provided.
        /// </summary>
        /// <param name="id">advisor or staff Id</param>
        /// <param name="type">If you only want the first initial of first name, supply a type of "Initial"</param>
        /// <returns>Name of advisor. If an ID is provided but no advisor is found it will return a message saying no name found for Id.</returns>
        private string GetAdvisorStaffName(string advisorId, Ellucian.Colleague.Domain.Planning.Entities.Advisor advisor, string type = "")
        {
            if (!string.IsNullOrEmpty(advisorId) && advisor == null)
            {
                return "No name found for Id " + advisorId;
            }
            if (advisor == null)
            {
                return string.Empty;
            }

            if (type == "Initial")
            {
                // a person's first name is optional
                return !string.IsNullOrEmpty(advisor.FirstName) ? advisor.LastName + ", " + advisor.FirstName.ElementAt(0) + "." : advisor.LastName;
            }
            else
            {
                return advisor.Name;
            }
        }
    }
}
