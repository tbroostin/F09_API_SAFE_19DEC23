// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Domain.Student.Services;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// A degree plan is a mapping of courses into terms.
    /// </summary>
    [Serializable]
    public class DegreePlan
    {
        private int _Id;
        /// <summary>
        /// A unique identifier for the plan. 0 if a new plan.
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
        /// The person Id for the person associated to the plan (required).
        /// </summary>
        private string _PersonId;
        public string PersonId
        {
            get
            {
                return _PersonId;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {

                    throw new ArgumentException("Degree Plan must have a Person Id");
                }
                else
                {
                    _PersonId = value;
                }
            }
        }

        //user id of the person doing the operation
        public string CurrentUserId { get; set; }

        /// <summary>
        /// The version number of the plan that represents the updates to the database. 
        /// </summary>
        private readonly int _Version;
        public int Version { get { return _Version; } }

        private readonly bool _ReviewRequested;
        public bool ReviewRequested { get { return _ReviewRequested; } }

        public DateTime? LastReviewedDate { get; set; }

        public string LastReviewedAdvisorId { get; set; }

        public DateTime? ReviewRequestedDate { get; set; }
        public DateTime? ReviewRequestedTime { get; set; }

        public DateTime? ArchiveNotificationDate { get; set; }
        /// <summary>
        /// Term Courses contains a dictionary that has its keys as term ids, and its values a list of PlannedCourses,
        /// each of which contains a course id and an optional section id.
        /// </summary>
        private readonly IDictionary<string, List<PlannedCourse>> TermCourses = new Dictionary<string, List<PlannedCourse>>();

        /// <summary>
        /// The course and section information related to sections that have no term that have been added to the plan.
        /// </summary>
        private List<PlannedCourse> _NonTermPlannedCourses = new List<PlannedCourse>();
        public List<PlannedCourse> NonTermPlannedCourses { get { return _NonTermPlannedCourses; } }

        /// <summary>
        /// The terms represented by this plan.
        /// </summary>
        public List<string> TermIds
        {
            get
            {
                return TermCourses.Keys.ToList();
            }
        }


        /// <summary>
        /// When a degree plan is retrieved from the database, this list of approvals represents the most recent approval status (approved or denied) given
        /// to a course/term combination - regardless whether the course/term combination still exists on the plan. 
        /// Use AddApproval to Update the approval status as a result of actions taken in the UI.
        /// </summary>
        private List<DegreePlanApproval> _Approvals = new List<DegreePlanApproval>();
        public List<DegreePlanApproval> Approvals
        {
            get
            {
                return _Approvals;
            }
            set
            {
                if (value != null)
                {
                    _Approvals = value;
                }
            }
        }

        private List<DegreePlanNote> _Notes = new List<DegreePlanNote>();
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

        private List<DegreePlanNote> _RestrictedNotes = new List<DegreePlanNote>();
        public List<DegreePlanNote> RestrictedNotes
        {
            get { return _RestrictedNotes; }
            set
            {
                if (value != null)
                {
                    _RestrictedNotes = value;
                }
            }
        }

        /// <summary>
        /// Returns list of all the Ids of the planned sections.
        /// </summary>
        public List<string> SectionsInPlan
        {
            get
            {
                var sectionIds = this.TermCourses.SelectMany(t => t.Value).Select(pc => pc.SectionId).ToList();
                sectionIds.AddRange(this.NonTermPlannedCourses.Where(c => (!string.IsNullOrEmpty(c.SectionId))).Select(c => c.SectionId).ToList());
                return sectionIds;
            }
        }

        /// <summary>
        /// Constructor for new degree plan
        /// </summary>
        /// <param name="personId"></param>
        public DegreePlan(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            _PersonId = personId;
            _Version = 0;
            _ReviewRequested = false;
        }

        /// <summary>
        /// Constructor for Degree Plan
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        public DegreePlan(int id, string personId, int version, bool reviewRequested = false)
            : this(personId)
        {
            if (id < 0)
            {
                throw new ArgumentException("Id must be >= 0");
            }
            _Id = id;
            _Version = version;
            _ReviewRequested = reviewRequested;
        }

        /// <summary>
        /// Adds a term to the plan.
        /// </summary>
        /// <param name="termId">the termId, must not be null</param>
        public void AddTerm(string termId)
        {
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId");
            }
            if (TermCourses.ContainsKey(termId))
            {
                throw new ArgumentException("TermId already in plan");
            }
            else
            {
                TermCourses[termId] = new List<PlannedCourse>();
            }
        }

        /// <summary>
        /// Returns the all planned courses (including course placeholders) that are planned for the specified term.
        /// </summary>
        /// <param name="term">the termId, must not be null</param>
        /// <returns></returns>
        /// 
        public IEnumerable<PlannedCourse> GetPlannedCourses(string termId)
        {
            if (String.IsNullOrEmpty(termId))
            {
                return NonTermPlannedCourses;
            }
            if (!TermCourses.ContainsKey(termId))
            {
                return Enumerable.Empty<PlannedCourse>();
            }
            return TermCourses[termId].AsEnumerable();
        }

        /// <summary>
        /// Allows addition of PlannedCourses in the DegreePlan. 
        /// </summary>
        public void AddCourse(PlannedCourse plannedCourse, string termId)
        {
            // Must have a planned course object
            if (plannedCourse != null)
            {
                // If the termId is null or empty and a section is provided then 
                // add the PlannedCourse to the NonTermPlannedCourse list.
                if (String.IsNullOrEmpty(termId))
                {
                    // Section is required for nonterm courses
                    if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                    {
                        // Make sure section isn't already in plan before adding it again.
                        if (_NonTermPlannedCourses.Where(n => n.SectionId == plannedCourse.SectionId).FirstOrDefault() == null)
                        {
                            var storedPlannedCourses = _NonTermPlannedCourses.ToList();
                            storedPlannedCourses.Add(plannedCourse);
                            _NonTermPlannedCourses = storedPlannedCourses;
                        }
                    }
                }
                else
                {
                    // Add term if not already in the plan
                    if (!(TermCourses.ContainsKey(termId)))
                    {
                        this.AddTerm(termId);
                    }

                    // add the course placeholder to the term if it doesn't alreay exists in the term
                    if (!string.IsNullOrWhiteSpace(plannedCourse.CoursePlaceholderId) &&
                       (TermCourses[termId].Where(t => t.CoursePlaceholderId == plannedCourse.CoursePlaceholderId).Count() == 0))
                    {
                        TermCourses[termId].Add(plannedCourse);
                    }
                    // Allow the add if there is a course and the sectionId is empty OR the section is not already on the plan
                    else if (!string.IsNullOrWhiteSpace(plannedCourse.CourseId) &&
                        (string.IsNullOrEmpty(plannedCourse.SectionId) || (TermCourses[termId].Where(c => c.SectionId == plannedCourse.SectionId).Count() == 0)))
                    {
                        TermCourses[termId].Add(plannedCourse);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="DegreePlan"/> object
        /// </summary>
        /// <param name="student">Student for whom degree plan will be created</param>
        /// <param name="studentPrograms">List of student academic programs</param>
        /// <param name="planningTerms">List of academic terms</param>
        /// <param name="programs">List of academic programs</param>
        /// <param name="createWithDefaultTerms">Flag indicating whether or not to create the degree plan with default academic terms on it; default behavior is true (include terms)</param>
        /// <returns>A new <see cref="DegreePlan"/> object</returns>
        public static DegreePlan CreateDegreePlan(Ellucian.Colleague.Domain.Student.Entities.Student student, IEnumerable<StudentProgram> studentPrograms, IEnumerable<Term> planningTerms, IEnumerable<Program> programs, bool createWithDefaultTerms = true)
        {

            if (student.DegreePlanId.HasValue == true)
            {
                throw new ExistingDegreePlanException("Student already has a plan. Cannot create a new one.", student.DegreePlanId.GetValueOrDefault(0));
            }
            else
            {
                DegreePlan newDegreePlan = new DegreePlan(student.Id);

                // Loop thru the student's programs and determine the latest program anticpated completion date.  
                // For planning purposes we will always assume this is at least a year out.
                DateTime planEndDate = DateTime.Today.AddYears(1);
                if (studentPrograms != null)
                {
                    foreach (var studentProgram in studentPrograms)
                    {
                        DateTime studentCompletionDate = studentProgram.AnticipatedCompletionDate.GetValueOrDefault();
                        DateTime studentStartDate = studentProgram.StartDate.GetValueOrDefault();
                        if (studentProgram.AnticipatedCompletionDate == null && programs.Count() > 0)
                        {
                            // If the student program doesn't have an anticipated completion date but they do have a start date, see if we can
                            // calculate their anticipated completion based on the program's months to completion.
                            Program program = null;
                            try
                            {
                                program = programs.Where(p => p.Code == studentProgram.ProgramCode).FirstOrDefault();
                            }
                            catch
                            {

                            }
                            if (program != null)
                            {
                                if (program.MonthsToComplete > 0 && studentStartDate != DateTime.MinValue)
                                {
                                    var addYears = (decimal)(program.MonthsToComplete / 12);
                                    addYears = Math.Truncate(addYears);
                                    var addMonths = (decimal)(program.MonthsToComplete - (addYears * 12));

                                    var newMonth = studentStartDate.Month + addMonths;
                                    if (newMonth > 12)
                                    {
                                        newMonth -= 12;
                                        addYears += 1;
                                    }
                                    var newYear = studentStartDate.Year + addYears;
                                    studentCompletionDate = new DateTime(Convert.ToInt32(newYear), Convert.ToInt32(newMonth), 1);
                                }
                            }
                        }
                        if (studentCompletionDate > planEndDate)
                        {
                            planEndDate = studentCompletionDate;
                        }
                    }
                }

                // If the degree plan term is being created with default terms, limit the list of terms to add to plan to those that should default on the plan and are between today and the plan end date.
                if (createWithDefaultTerms && planningTerms != null)
                {
                    IEnumerable<Term> defaultTerms = planningTerms.Where(t => t.DefaultOnPlan == true);
                    if (defaultTerms.Count() > 0)
                    {
                        foreach (var term in defaultTerms)
                        {
                            if (term.StartDate < planEndDate && term.EndDate > DateTime.Now)
                            {
                                newDegreePlan.AddTerm(term.Code);
                            }
                        }
                    }
                }
                return newDegreePlan;
            }
        }

        /// <summary>
        /// Adds an Approval entry to the DegreePlan approval history
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="status"></param>
        /// <param name="date"></param>
        /// <param name="courseId"></param>
        /// <param name="termCode"></param>
        public void AddApproval(string personId, DegreePlanApprovalStatus status, DateTime date, string courseId, string termCode)
        {
            var approvals = _Approvals.ToList();
            approvals.Add(new DegreePlanApproval(personId, status, date, courseId, termCode));
            _Approvals = approvals;
        }

        /// <summary>
        /// Converts the list of terms/planned courses returned by GetPlannedCoursesForValidation to a flat list of course objects excluding course placeholders.
        /// Since this is used for program evaluation, CompletedByDate is not specified and sections are not needed.
        /// </summary>
        /// <param name="allTerms">A list of all terms, for reference</param>
        /// <param name="registrationTerms">List of current registration terms, for reference</param>
        /// <param name="credits">List of student's credits</param>
        /// <param name="courses">List of all courses</param>
        /// <returns>IEnumerable list of courses</returns>
        public IEnumerable<PlannedCredit> GetCoursesForValidation(IEnumerable<Term> allTerms, IEnumerable<Term> registrationTerms, IEnumerable<AcademicCredit> credits, IEnumerable<Course> courses)
        {
            // Get a list of plannedCourse objects by term. Ignore registration terms and count things in the future from today as planned
            // and ignore planned courses in the past.

            var plannedTerms = GetPlannedCoursesForValidation(null, allTerms, new List<Term>(), credits, new List<Section>());

            var evaluationPlannedCourses = ConvertPlannedTermToEvalPlannedCourses(plannedTerms, courses);

            return evaluationPlannedCourses;
        }

        public IEnumerable<PlannedCredit> ConvertPlannedTermToEvalPlannedCourses(Dictionary<string, IEnumerable<PlannedCourse>> plannedTerms, IEnumerable<Course> courses)
        {
            // Return the list of planned courses in a flat list of objects. Similar to PlannedCourse, but with a subset of
            // properties and including the course object, needed by the evaluator.
            // Course Placeholders are not included result.
            var evaluationPlannedCourses = new List<PlannedCredit>();

            foreach (var plannedTerm in plannedTerms)
            {
                foreach (var plannedCourse in plannedTerm.Value)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(plannedCourse.CourseId))
                        {
                            var course = courses.Where(c => c.Id == plannedCourse.CourseId).FirstOrDefault();
                            var evalPlannedCourse = new PlannedCredit(course, plannedTerm.Key, plannedCourse.SectionId);
                            if (plannedCourse.Credits.HasValue) { evalPlannedCourse.Credits = plannedCourse.Credits; }
                            evaluationPlannedCourses.Add(evalPlannedCourse);
                        }
                    }
                    catch
                    {
                        // not a viable planned course without a valid course Id
                    }
                }
            }
            return evaluationPlannedCourses;
        }

        private const string _NonTermKey = "NONTERM";

        /// <summary>
        /// Return the plannedCourses that should be included during validation, based on the given
        /// completion date and the credits for a student to-date. The degree plan planned courses 
        /// must be filtered by the credits (registered/taken courses that match up) and the given
        /// date, that causes planned courses to be excluded that have not been completed by that date.
        /// Course Placeholders are not included in result.
        /// </summary>
        /// <param name="completedByDate"></param>
        /// <param name="startByDate"></param>
        /// <param name="allTerms"></param>
        /// <param name="registrationTerms"></param>
        /// <param name="credits"></param>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<PlannedCourse>> GetPlannedCoursesForValidation(DateTime? completedByDate, IEnumerable<Term> allTerms, IEnumerable<Term> registrationTerms, IEnumerable<AcademicCredit> credits, IEnumerable<Section> sections)
        {
            // Initialize returned list of courses
            var plannedCoursesDict = new Dictionary<string, IEnumerable<PlannedCourse>>();

            // If no date specified, set to max value to get all future planned courses
            if (completedByDate == null)
            {
                completedByDate = DateTime.MaxValue;
            }

            // Set minimum start date from which we will draw planned courses.  
            // NOTE: Planned courses that exist in past terms (terms no longer open for registration) are always disregarded,
            // because only academic credits completed should be considered in past terms. This routine's primary purpose is to collect pertinent
            // current and future planned courses (that do not match up to any existing (in progress) credits). 
            DateTime minStartDate = DateTime.Today;
            if ((registrationTerms != null) && (registrationTerms.Count() > 0))
            {
                minStartDate = registrationTerms.Select(t => t.StartDate).Min();
            }

            // This list used to track the academic credits as they are matched up with planned courses on the plan
            var creditsUsed = new List<string>();

            // First loop through nonterm planned courses, return items that fall within the date
            // range and do not correspond with an academic credit.
            var nonTermPlannedCourses = new List<PlannedCourse>();
            foreach (var plannedCourse in NonTermPlannedCourses)
            {
                // NonTerm courses must have an associated section (if not, guess we'll just skip them)
                if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                {
                    // Consider this item only if it will start on or after minStartDate and 
                    // be complete by the completedByDate (a null end date is ok if completed by is not specified)
                    var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
                    if (section != null && section.StartDate >= minStartDate
                        && ((section.EndDate <= completedByDate)
                        || (section.EndDate == null && completedByDate == DateTime.MaxValue)))
                    {
                        // Find an academic credit for this section and see if it's already been used
                        var credit = credits.Where(c => c.SectionId == plannedCourse.SectionId).FirstOrDefault();
                        if ((credit != null) && (!creditsUsed.Contains(credit.Id)))
                        {
                            // acad credit that hasn't been used yet was found for this planned course so don't 
                            // add the planned course to the return list, and track the credit used so it's not used again
                            creditsUsed.Add(credit.Id);
                        }
                        else
                        {
                            // acad credit not found for this planned course, so add it to the list of returned planned courses
                            nonTermPlannedCourses.Add(plannedCourse);
                        }
                    }
                }
            }
            if (nonTermPlannedCourses.Count() > 0)
            {
                plannedCoursesDict[_NonTermKey] = nonTermPlannedCourses.Distinct();
            }

            // Get planned courses from each of the terms, if term dates fall within minimum and completedBy dates AND planned course not already
            // included in credits (in the case of registration terms)
            foreach (var termCode in this.TermIds)
            {
                var plannedCourses = new List<PlannedCourse>();

                // Get all but alternate courses for each term on the degree plan. 
                // Sequence items so that those with section Id are examined first.
                //IEnumerable<PlannedCourse> plannedTermCourses = this.GetPlannedCourses(termCode).Where(p => p.IsAlternate == false).OrderByDescending(p => p.SectionId);
                IEnumerable<PlannedCourse> plannedTermCourses = this.GetPlannedCourses(termCode).Where(c => !string.IsNullOrEmpty(c.CourseId)).OrderByDescending(p => p.SectionId);
                if (plannedTermCourses.Count() > 0)
                {
                    // Get the term and continue if the term dates conform to the set minimum and completion date
                    // Changed check from term start to term end to account for minimesters within a
                    // term, so a course can be 'planned' until a term ends.

                    var term = allTerms.Where(t => t.Code == termCode).FirstOrDefault();
                    if ((term != null) && ((term.EndDate <= completedByDate) && (term.EndDate >= minStartDate)))
                    {
                        // All planned courses need to be verified, regardless of term. None will be included in terms before the specified minimum date.
                        // Examine each course and only return the ones not included in the academic credits found for that term
                        foreach (var plannedCourse in plannedTermCourses)
                        {
                            var matched = false;
                            if (credits != null && credits.Count() > 0)
                            {
                                // If this course can be found in the list of acad credit for this term then don't include in the returned list.
                                var crsCredits = credits.Where(c => c.Course != null && c.Course.Id == plannedCourse.CourseId && c.TermCode == term.Code);

                                // In case there is more than one instance of credit for the same course on the plan, loop through and match individually.
                                foreach (var credit in crsCredits)
                                {
                                    if ((credit != null) && (!creditsUsed.Contains(credit.Id)) && (!matched) &&
                                        (string.IsNullOrEmpty(plannedCourse.SectionId) || plannedCourse.SectionId == credit.SectionId) &&
                                        CreditShouldBeValidated(credit))
                                    {
                                        // acad credit that hasn't been used yet was found for this planned course so don't 
                                        // add it to the return list; track the one found so it's not used again
                                        creditsUsed.Add(credit.Id);
                                        matched = true;
                                    }
                                }
                            }
                            if (!matched)
                            {
                                // acad credit not found for this planned course, so add it to the list of returned planned courses
                                plannedCourses.Add(plannedCourse);

                            }
                        }
                    }
                }
                if (plannedCourses.Count() > 0)
                {
                    plannedCoursesDict[termCode] = plannedCourses;
                }
            }
            return plannedCoursesDict;
        }

        private bool CreditShouldBeValidated(AcademicCredit credit)
        {
            return (credit.Status != CreditStatus.Dropped && credit.Status != CreditStatus.Withdrawn) ||
                   ((credit.Status == CreditStatus.Dropped || credit.Status == CreditStatus.Withdrawn) && credit.IsCompletedCredit);
        }

        /// <summary>
        /// Returns the list of all requirement codes referenced by requisites in the planned courses that need to be validated.
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="registrationTerms"></param>
        /// <param name="courses"></param>
        /// <param name="credits"></param>
        /// <param name="sections"></param>
        /// <returns>List of Requirement Codes</returns>
        public IEnumerable<string> GetRequirementCodes(IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Course> courses, IEnumerable<AcademicCredit> credits, IEnumerable<Section> sections)
        {
            var requirementCodes = new List<string>();

            //First find the planned courses that need to be evaluated based on the given completion date and the credits for a student to-date.
            var plannedTerms = GetPlannedCoursesForValidation(null, terms, registrationTerms, credits, sections);
            foreach (var plannedTerm in plannedTerms)
            {
                // Select the course requisites
                var crsRequisites = (from plannedCourse in plannedTerm.Value
                                     join course in courses
                                     on plannedCourse.CourseId equals course.Id into joinCourse
                                     from crs in joinCourse
                                     select crs.Requisites).SelectMany(s => s);
                // Select the section requisites
                var secRequisites = (from plannedCourse in plannedTerm.Value
                                     where (!string.IsNullOrEmpty(plannedCourse.SectionId))
                                     join section in sections
                                     on plannedCourse.SectionId equals section.Id into joinSections
                                     from sec in joinSections
                                     select sec.Requisites).SelectMany(rq => rq);
                // Combine the lists
                var requisites = crsRequisites.Union(secRequisites);
                crsRequisites = requisites;

                if (crsRequisites != null && crsRequisites.Count() > 0)
                {
                    // Create list of requirement codes from the list of requisites
                    var requirementCodeIds = crsRequisites.Where(r => !string.IsNullOrEmpty(r.RequirementCode)).Select(c => c.RequirementCode).Distinct().ToList();
                    if (requirementCodeIds != null && requirementCodeIds.Count() > 0)
                    {
                        requirementCodes.AddRange(requirementCodeIds);
                    }
                }
            }
            return requirementCodes.Distinct();
        }


        /// <summary>
        /// Used internally to clear all degree plan messages before conflict checking
        /// </summary>
        private void ClearMessages()
        {
            foreach (var termCode in TermIds)
            {
                var plannedCourses = GetPlannedCourses(termCode);
                foreach (var plannedCourse in plannedCourses)
                {
                    plannedCourse.ClearWarnings();
                }
            }
        }

        /// <summary>
        /// Returns a degree plan with integrated messages generated as a result of the process of validating the degree plan.
        /// Validation consists of:
        ///     Checking for missing corequisites
        ///     Checking for overlap in scheduled sections
        ///     Checking for invalid credit values
        ///     Checking for unsatisfied prerequisites
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="registrationTerms"></param>
        /// <param name="courses"></param>
        /// <param name="sections"></param>
        /// <param name="credits"></param>
        /// <param name="requirements"></param>
        public void CheckForConflicts(IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits, IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement> requirements, IEnumerable<RuleResult> ruleResults, string studentLocation = null)
        {
            // Clear all messages for all planned courses
            ClearMessages();

            // Add messages for planned courses with missing corequisites or unsatisfied prerequisites or where the planned course may not be offered in the respective term.
            CheckRequisitesAndOfferings(terms, registrationTerms, courses, sections, credits, requirements, ruleResults, studentLocation);

            // Add messages for planned sections with time conflicts
            CheckTimeConflicts(terms, registrationTerms, sections, credits);

            // Add messages for planned sections with invalid credit values
            ValidateCredits(registrationTerms, sections);
        }

        /// <summary>
        /// Find corequisites and prerequisites on current planning terms that are not satisfied by the planned courses and sections. Also verify course offerings in respective terms.
        /// </summary>
        /// <param name="terms"></param>
        /// <param name="registrationTerms"></param>
        /// <param name="courses"></param>
        /// <param name="sections"></param>
        private void CheckRequisitesAndOfferings(IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits, IEnumerable<Requirement> requirements, IEnumerable<RuleResult> ruleResults, string studentLocation)
        {
            var completedByDate = DateTime.MaxValue;
            // Get a list of items in the degree plan that it makes sense to validate. Only want
            // the items that are planned but not registered (in a registration term) and items in 
            // upcoming terms. Don't want to validate items from past terms or terms no longer in registration.
            var plannedTerms = GetPlannedCoursesForValidation(completedByDate, terms, registrationTerms, credits, sections);
            // Loop through terms to check requisites
            foreach (var plannedTerm in plannedTerms)
            {
                if (plannedTerm.Key == _NonTermKey)
                {
                    // This is a list of nonterm items. All others keyed by term Id
                    var nonTermPlannedCourses = plannedTerm.Value;
                    // Check corequisites of each planned non-term course/section (by definition, added to non-term
                    // list ONLY if section does not have a term.
                    foreach (var plannedCourse in NonTermPlannedCourses)
                    {
                        var course = courses.Where(c => c.Id == plannedCourse.CourseId).FirstOrDefault();
                        if (course != null)
                        {
                            // Process only if a section has been planned (should ALWAYS be planned if nonterm, therefore no ELSE for this IF)
                            if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                            {
                                CheckSectionRequisites(plannedCourse, null, terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                            }
                        }
                    }
                }
                else
                {
                    // Loop through list of planned courses returned for this term.
                    var termCode = plannedTerm.Key;
                    var plannedCourses = plannedTerm.Value;
                    foreach (var plannedCourse in plannedCourses)
                    {
                        var course = courses.Where(c => !string.IsNullOrEmpty(plannedCourse.CourseId) && c.Id == plannedCourse.CourseId).FirstOrDefault();
                        if (course != null)
                        {
                            // If a section has been planned, check the section corequisites
                            if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                            {
                                CheckSectionRequisites(plannedCourse, termCode, terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                            }
                            else
                            {
                                // Get planned courses to include in the evaluation. 
                                // They must be completed by the start date of the planned course's term.
                                var term = terms.Where(t => t.Code == plannedTerm.Key).FirstOrDefault();

                                if (term != null)
                                {
                                    // For the planned courses with a term but without an associated section we need to validate whether the
                                    // course is being offered in this term
                                    ValidateCourseOfferingInTerm(plannedCourse, course, term, studentLocation);

                                    // Next continue with the course requisite checking
                                    foreach (var requisite in course.Requisites)
                                    {
                                        if (!string.IsNullOrEmpty(requisite.RequirementCode))
                                        {
                                            DateTime plannedCourseStart = term.StartDate;
                                            DateTime plannedCourseEnd = term.EndDate;
                                            string plannedCourseTermCode = plannedTerm.Key;
                                            EvaluateRequisite(plannedCourse, plannedCourseTermCode, requisite, plannedCourseStart, plannedCourseEnd, terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                                        }
                                        else
                                        {
                                            if (requisite.CorequisiteCourseId != null)
                                            {
                                                // In this is the case - i.e. the corequisite course is filled in - we will just do it the old way. 
                                                CheckCourseCoreq(plannedCourse, termCode, requisite, terms, courses, sections, credits);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the planned course is normally offered in the term in which it has been planned.
        /// If there is a mismatch between the course's offering codes and the term's session or yearly cycles, a warning will be added to the planned course.
        /// This verification only applies to planned courses with a term and without an associated section. Once there is a section planned it means the client has actually created a section in that term so it is immaterial.
        /// And warnings are not shown for academic credit items for a similar reason. 
        /// </summary>
        /// <param name="plannedCourse">Planned course being verified</param>
        /// <param name="course">Associated course for the planned course</param>
        /// <param name="term">Associated term for the planned course</param>
        private void ValidateCourseOfferingInTerm(PlannedCourse plannedCourse, Course course, Term term, string studentLocation)
        {
            // This verification only applies to planned courses with a term and without an associated section. Once there is a section planned this doesn't matter.
            if (course != null && plannedCourse != null && term != null && string.IsNullOrEmpty(plannedCourse.SectionId))
            {
                string courseSessionCycle = null;
                string courseYearlyCycle = null;
                // Determine if the course has any restrictions by location based on the student's location.  If so those take precedence
                if (!string.IsNullOrEmpty(studentLocation) && course.LocationCycleRestrictions != null && course.LocationCycleRestrictions.Any())
                {
                    var cycleByLocations = course.LocationCycleRestrictions.Where(l => l.Location == studentLocation).ToList();
                    if (cycleByLocations != null && cycleByLocations.Any())
                    {
                        bool courseOfferingMatches = false;
                        // Loop through each possible set of restrictions and see if any of them match.  If so we are good to go.  If not include a warning.
                        foreach (var cycleByLocation in cycleByLocations)
                        {
                            courseSessionCycle = cycleByLocation.SessionCycle;
                            courseYearlyCycle = cycleByLocation.YearlyCycle;
                            // There is a match if the course's term has no restrictions or if the location restriction doesn't have a corresponding value OR it has a value and matches the term's value.
                            if ((string.IsNullOrEmpty(courseSessionCycle) || term.SessionCycles == null || !term.SessionCycles.Any() || term.SessionCycles.Contains(courseSessionCycle))
                                && (string.IsNullOrEmpty(courseYearlyCycle) || term.YearlyCycles == null || !term.YearlyCycles.Any() || term.YearlyCycles.Contains(courseYearlyCycle)))
                            {
                                courseOfferingMatches = true;
                                break;
                            }
                        }
                        // If the offering restriction doesn't match the term that the course is placed on add a warning.
                        if (!courseOfferingMatches)
                        {
                            plannedCourse.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.CourseOfferingConflict));
                        }
                    }
                }
                else
                {
                    // There were no specific restrictions to review based on this student's location so add warnings only based on the overall course restrictions.
                    courseSessionCycle = course.TermSessionCycle;
                    courseYearlyCycle = course.TermYearlyCycle;
                    if ((!string.IsNullOrEmpty(courseSessionCycle) && term.SessionCycles != null && term.SessionCycles.Any() && !term.SessionCycles.Contains(courseSessionCycle)) || (!string.IsNullOrEmpty(courseYearlyCycle) && term.YearlyCycles != null && term.YearlyCycles.Any() && !term.YearlyCycles.Contains(courseYearlyCycle)))
                    {
                        plannedCourse.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.CourseOfferingConflict));
                    }
                }
            }
        }

        private void CheckSectionRequisites(PlannedCourse plannedCourse, string plannedTerm, IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits, IEnumerable<Requirement> requirements, IEnumerable<RuleResult> ruleResults)
        {
            var course = courses.Where(c => !string.IsNullOrEmpty(plannedCourse.CourseId) && c.Id == plannedCourse.CourseId).FirstOrDefault();
            var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
            if (section != null && course != null)
            {
                // process requirement-based requisites. Process section requisites if flagged to override course
                if (section.Requisites != null && section.OverridesCourseRequisites)
                {
                    foreach (var requisite in section.Requisites.Where(r => !string.IsNullOrEmpty(r.RequirementCode)))
                    {
                        EvaluateRequisite(plannedCourse, null, requisite, section.StartDate, ((section.EndDate == null) ? DateTime.MaxValue : section.EndDate.GetValueOrDefault()), terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                    }
                    // Now check for protected course requisites because even if the section is overriding the course requisites these
                    // must still be observed.
                    var protectedCourseRequisites = course.Requisites.Where(r => r.IsProtected);
                    if (protectedCourseRequisites.Count() > 0)
                    {
                        foreach (var requisite in protectedCourseRequisites)
                        {
                            if (!string.IsNullOrEmpty(requisite.RequirementCode))
                            {
                                DateTime plannedCourseEnd = section.EndDate == null ? DateTime.MaxValue : section.EndDate.GetValueOrDefault();
                                DateTime plannedCourseStart = section.StartDate;
                                string plannedCourseTerm = plannedTerm;
                                EvaluateRequisite(plannedCourse, null, requisite, plannedCourseStart, plannedCourseEnd, terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                            }
                        }
                    }
                }
                else
                {
                    if (course.Requisites.Count() > 0)
                    {
                        foreach (var requisite in course.Requisites)
                        {
                            if (!string.IsNullOrEmpty(requisite.RequirementCode))
                            {
                                DateTime plannedCourseEnd = section.EndDate == null ? DateTime.MaxValue : section.EndDate.GetValueOrDefault();
                                DateTime plannedCourseStart = section.StartDate;
                                string plannedCourseTerm = plannedTerm;
                                EvaluateRequisite(plannedCourse, null, requisite, plannedCourseStart, plannedCourseEnd, terms, registrationTerms, courses, sections, credits, requirements, ruleResults);
                            }
                        }
                    }
                }

                // process requisite sections
                if (section.SectionRequisites != null & section.SectionRequisites.Count() > 0)
                {
                    foreach (var requisite in section.SectionRequisites)
                    {
                        CheckRequisiteSections(plannedCourse, requisite, credits);
                    }
                }

                // process requisite courses (specified on section, obsolete after conversion)
                if (section.Requisites != null && section.Requisites.Where(r => !string.IsNullOrEmpty(r.CorequisiteCourseId)).Count() > 0)
                {
                    foreach (var requisite in section.Requisites.Where(r => !string.IsNullOrEmpty(r.CorequisiteCourseId)))
                    {
                        CheckCourseCoreq(plannedCourse, null, requisite, terms, courses, sections, credits);
                    }
                }

            }
        }

        /// <summary>
        /// Checks whether the section corequisite is satisfied on this degree plan
        /// </summary>
        /// <param name="plannedCourse">The planned course/section that requests the corequisite</param>
        /// <param name="plannedTermCode">The term of the planned course/section, blank if non-term</param>
        /// <param name="req">Object containing the section ID and whether required</param>
        /// <param name="terms">Reference list of term objects</param>
        /// <param name="courses">Reference list of course objects</param>
        /// <param name="sections">Reference list of section objects</param>
        private void CheckRequisiteSections(PlannedCourse plannedCourse, SectionRequisite req, IEnumerable<AcademicCredit> credits)
        {
            // loop through all sections cited in the requisite and determine if on the plan or not.
            var requisiteSectionsDict = new Dictionary<string, bool>();
            foreach (var sectionId in req.CorequisiteSectionIds)
            {
                // If section is valid and it's in the plan or in credits, check all the possible term-nonterm scenarios to determine if timing is ok.
                if (sectionId != null && (SectionsInPlan.Contains(sectionId) || (credits.Where(c => c.SectionId == sectionId).FirstOrDefault() != null)))
                {
                    requisiteSectionsDict[sectionId] = true;
                }
                else
                {
                    requisiteSectionsDict[sectionId] = false;
                }
            }
            // If required and number needed is less than number of sections specified, create group warning
            if (req.NumberNeeded > 0 && req.NumberNeeded < req.CorequisiteSectionIds.Count())
            {
                if (requisiteSectionsDict.Where(rsf => rsf.Value == true).Count() < req.NumberNeeded)
                {
                    plannedCourse.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = req });
                }
            }
            else
            {
                foreach (var notFoundSection in requisiteSectionsDict)
                {
                    if (notFoundSection.Value == false)
                    {
                        // Create a single warning for each missing item, identifying section Id in all cases.
                        var warning = new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite);
                        warning.SectionRequisite = req;
                        warning.SectionId = notFoundSection.Key.ToString();
                        plannedCourse.AddWarning(warning);
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the course co-requisite is satisfied on this degree plan - this assumes co-requisites are in the old format
        /// and requires that the CorequisiteCourseId is included in the requisite
        /// </summary>
        /// <param name="plannedCourse">The planned course/section that requests the co-requisite</param>
        /// <param name="plannedTermCode">The term of the planned course/section, blank if non-term</param>
        /// <param name="coreq">Object containing the course ID and whether required</param>
        /// <param name="terms">Reference list of term objects</param>
        /// <param name="courses">Reference list of course objects</param>
        /// <param name="sections">Reference list of section objects</param>
        private void CheckCourseCoreq(PlannedCourse plannedCourse, string plannedCourseTermCode, Requisite coreq, IEnumerable<Term> terms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits)
        {
            if (string.IsNullOrEmpty(coreq.CorequisiteCourseId))
            {
                throw new ArgumentException("Requisite must contain a CorequisiteCourseId");
            }

            // Set to true when a valid coreq found
            bool coreqFound = false;
            // Get planned course's term from repository
            Term plannedTerm = null;
            if (plannedCourseTermCode != null)
            {
                plannedTerm = terms.Where(t => t.Code == plannedCourseTermCode).FirstOrDefault();
            }
            // Get any Equates defined for the coreq course
            var allowedCourseIds = courses.Where(c => c.Id == coreq.CorequisiteCourseId).SelectMany(c => c.EquatedCourseIds).ToList();
            // Add in the original corequisite course Id
            allowedCourseIds.Add(coreq.CorequisiteCourseId);
            // Check to see if the coreq course or any of the equated courses is found in academic credits
            foreach (var crsId in allowedCourseIds)
            {
                var creditsFound = credits.Where(c => c.Course != null && c.Course.Id == crsId);
                if ((creditsFound != null) && (creditsFound.Count() > 0))
                {
                    // Check each item found for this course in the credits
                    foreach (var credit in creditsFound)
                    {
                        // If an item has been found, don't keep checking
                        if (!coreqFound)
                        {
                            coreqFound = CheckIfCoreqFound(plannedCourseTermCode, plannedTerm, plannedCourse.SectionId, credit.TermCode, credit.StartDate, terms, sections);
                        }
                    }
                }
            }
            if (!coreqFound)
            {
                // Check each course in the allowed courses list
                foreach (var crsId in allowedCourseIds)
                {
                    // Return the terms in which this course is planned.
                    var termCodes = TermsCoursePlanned(crsId);
                    if (termCodes != null)
                    {
                        // If in the plan, make sure it's on or before the term of the requiring course
                        foreach (var coreqTermCode in termCodes)
                        {
                            if (!coreqFound)
                            {
                                coreqFound = CheckIfCoreqFound(plannedCourseTermCode, plannedTerm, plannedCourse.SectionId, coreqTermCode, null, terms, sections);
                            }
                        }
                    }
                }
            }
            // If the coreq has not been found in the term-based courses, check the planned non-term courses
            if (!coreqFound)
            {
                // Check for the coreq course and all its equates
                foreach (var crsId in allowedCourseIds)
                {
                    // Find course in non term planned courses
                    var nonTermCoreqCourses = NonTermPlannedCourses.Where(c => c.CourseId == crsId);
                    if (nonTermCoreqCourses != null)
                    {
                        // for each time this course is found in the nonterm planned courses, check that it starts on time.
                        foreach (var nonTermCoreqCourse in nonTermCoreqCourses)
                        {
                            if (!coreqFound)
                            {
                                // Nonterm courses MUST have a section planned (but check for sectionId just in case)
                                if (!string.IsNullOrEmpty(nonTermCoreqCourse.SectionId))
                                {
                                    // This is a coreq if the start date of the coreq section is on or before start date
                                    // of the term of the planned course.
                                    var coreqSection = sections.Where(s => s.Id == nonTermCoreqCourse.SectionId).FirstOrDefault();
                                    if (coreqSection != null)
                                    {
                                        if (plannedTerm != null)
                                        {
                                            // Check against the planned term start date when we are working with all but nonterm sections
                                            coreqFound = coreqSection.StartDate <= plannedTerm.EndDate;
                                        }
                                        else
                                        {
                                            // For nonterm sections, check the section against the requiring section's start date
                                            if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                                            {
                                                var plannedCourseSection = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
                                                if (plannedCourseSection != null)
                                                {
                                                    coreqFound = coreqSection.StartDate <= plannedCourseSection.EndDate;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!coreqFound)
            {
                // If no coreqs found, build the missing coreq message
                var warning = new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite) { Requisite = coreq };
                plannedCourse.AddWarning(warning);

            }
        }

        /// <summary>
        /// Common logic used when checking either a credit or a planned course to determine if it meets a co-requisite
        /// of a planned course.
        /// The "planned" variables are related to the planned course for which requisites are being checked.  
        /// The "item" variables are related to the item that is being considered as a co-requisite (whether credit or pc).
        /// 
        /// </summary>
        private bool CheckIfCoreqFound(string plannedCourseTermCode, Term plannedTerm, string plannedSectionId, string itemTermCode, DateTime? itemStartDate, IEnumerable<Term> terms, IEnumerable<Section> sections)
        {
            bool coreqFound = false;
            // If the Term Code is empty, we are checking requisites for a non-term planned section.
            if (string.IsNullOrEmpty(plannedCourseTermCode))
            {
                if (!string.IsNullOrEmpty(plannedSectionId))
                {
                    var term = terms.Where(t => t.Code == itemTermCode).FirstOrDefault();
                    var section = sections.Where(s => s.Id == plannedSectionId).FirstOrDefault();
                    if (term != null && section != null)
                    {
                        // This is a valid coreq/acadCredit if the start date of the term in which it is taken
                        // is on or before the end date of the planned section.  (i.e. must be taken prior to or overlapping current item)
                        if (section.EndDate != null)
                        {
                            coreqFound = term.StartDate <= section.EndDate;
                        }
                        else
                        {
                            // if there is a non-term section with no end date and we have any found item, consider the requisite met and move on.
                            // No warning will be produced.
                            coreqFound = true;
                        }
                    }
                }
            }
            else
            {
                // If a term code is passed in, then the original planned course is term-based.
                // We must compare the planned course's term end date to the planned course or academic credit start date
                // to determine if the coreq is met.
                // Compare terms and term dates
                if (itemTermCode == plannedCourseTermCode)
                {
                    // Planned in the same term, coreq is met
                    coreqFound = true;
                }
                else
                {
                    // terms don't match - see if the item has a start date that is less than the planned term's end date
                    if (itemStartDate != null && plannedTerm != null)
                    {
                        coreqFound = itemStartDate <= plannedTerm.EndDate;
                    }
                    else
                    {
                        // Terms don't match and item doesn't have a start date to compare. 
                        // Get the term in which the course was planned, and the term of the item
                        var coreqTerm = terms.Where(t => t.Code == itemTermCode).FirstOrDefault();
                        if (plannedTerm != null && coreqTerm != null)
                        {
                            // Coreq has been found if the start date of the item is on or before the end date
                            // of the planned course. (i.e. must be taken prior to or overlapping current item)
                            coreqFound = coreqTerm.StartDate <= plannedTerm.EndDate;
                        }
                    }
                }
            }
            return coreqFound;
        }

        /// <summary>
        /// Returns the list of term codes in which the given course is found in the plan
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public IEnumerable<string> TermsCoursePlanned(string courseId)
        {
            var plannedTerms = new List<String>();
            foreach (var planTerm in this.TermCourses)
            {
                var course = planTerm.Value.Where(pc => pc.CourseId == courseId).FirstOrDefault();
                if (course != null)
                {
                    plannedTerms.Add(planTerm.Key);
                }
            }
            return plannedTerms;
        }

        /// <summary>
        /// Returns the list of term codes in which the given course placeholder is found in the plan
        /// </summary>
        /// <param name="coursePlaceholderId"></param>
        /// <returns></returns>
        public IEnumerable<string> TermsCoursePlaceholderPlanned(string coursePlaceholderId)
        {
            var plannedTerms = new List<String>();
            foreach (var planTerm in this.TermCourses)
            {
                var coursePlaceholder = planTerm.Value.Where(pc => pc.CoursePlaceholderId == coursePlaceholderId).FirstOrDefault();
                if (coursePlaceholder != null)
                {
                    plannedTerms.Add(planTerm.Key);
                }
            }
            return plannedTerms;
        }

        /// <summary>
        /// Checks time conflicts between all scheduled sections on the degree plan. Since terms may overlap
        /// and non-term sections can have any date range, compare all sections on the registration
        /// terms and in the non-term planned courses.
        /// </summary>
        private void CheckTimeConflicts(IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits)
        {
            // Get the term/plannedCourse dictionary array of all planned courses eligible for validation
            var plannedTerms = GetPlannedCoursesForValidation(null, terms, registrationTerms, credits, sections);
            // Put into a flat list of planned courses, and only those that have a section Id
            var plannedCourses = plannedTerms.SelectMany(pt => pt.Value).Where(pc => (!string.IsNullOrEmpty(pc.SectionId))).ToArray();
            // Get any academic credits for the registration terms
            var acadCredits = (from term in registrationTerms
                               join acadCredit in credits
                               on term.Code equals acadCredit.TermCode into joinCredit
                               from credit in joinCredit
                               where CreditShouldBeValidated(credit)
                               select credit).ToArray();

            // Outer and inner loop through all planned courses, checking for overlapping date AND time
            for (int i = 0; i < plannedCourses.Count(); i++)
            {
                // Get section for outer loop planned course
                var section1 = sections.Where(s => s.Id == plannedCourses[i].SectionId).FirstOrDefault();
                // start at outer loop's section + 1 and examine all other planned courses against outer loop item
                for (int j = i + 1; j < plannedCourses.Count(); j++)
                {
                    // Get both sections (planned courses without section planned were filtered out already)
                    var section2 = sections.Where(s => s.Id == plannedCourses[j].SectionId).FirstOrDefault();
                    // Both sections must exist and both must have meeting information that can be in either Meetings or PrimarySectionMeetings properties
                    if ((section1 != null && section2 != null) &&
                        (section1.Meetings.Count() > 0 || section1.PrimarySectionMeetings.Count() > 0) && (section2.Meetings.Count() > 0 || section2.PrimarySectionMeetings.Count() > 0))
                    {
                        var isOverlap = CheckIfSectionsOverlap(section1, section2);
                        if (isOverlap)
                        {
                            // Time Overlap found, create a conflict message for both sections, referencing each other
                            plannedCourses.ElementAt(i).AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.TimeConflict) { SectionId = section2.Id });
                            plannedCourses.ElementAt(j).AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.TimeConflict) { SectionId = section1.Id });
                        }
                    }
                }
                // Check section overlap against the subarray of academic credits
                for (int j = 0; j < acadCredits.Count(); j++)
                {
                    // Get both sections (planned courses without section planned were filtered out already)
                    var section2 = sections.Where(s => s.Id == acadCredits[j].SectionId).FirstOrDefault();
                    // Both sections must exist and both must have meeting information either in Meetings or PrimarySectionMeetings properties.
                    if ((section1 != null && section2 != null) &&
                       (section1.Meetings.Count() > 0 || section1.PrimarySectionMeetings.Count() > 0) && (section2.Meetings.Count() > 0 || section2.PrimarySectionMeetings.Count() > 0))
                    {
                        var isOverlap = CheckIfSectionsOverlap(section1, section2);
                        if (isOverlap)
                        {
                            // Time Overlap found, create a conflict message for the planned section.
                            plannedCourses.ElementAt(i).AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.TimeConflict) { SectionId = section2.Id });
                        }
                    }
                }
            }
        }

        private bool CheckIfSectionsOverlap(Section section1, Section section2)
        {
            // First check for term/date overlap
            // Check for same term
            var isOverlap = false;
            if (section1.TermId == section2.TermId && (!string.IsNullOrEmpty(section1.TermId) && !string.IsNullOrEmpty(section2.TermId)))
            {
                isOverlap = true;
            }
            else
            {
                // If term not the same, check for a date overlap
                if ((section1.StartDate <= section2.EndDate || section2.EndDate == null) &&
                    (section1.EndDate >= section2.StartDate || section1.EndDate == null))
                {
                    isOverlap = true;
                }
            }
            // We have a date overlap, check for time overlap
            if (isOverlap)
            {
                isOverlap = false;
                //combine meetings + primary meetings for section1 & section2
                List<SectionMeeting> section1Meetings = section1.Meetings.Union(section1.PrimarySectionMeetings).Distinct().ToList();
                List<SectionMeeting> section2Meetings = section2.Meetings.Union(section2.PrimarySectionMeetings).Distinct().ToList();
                for (int k = 0; k < section1Meetings.Count(); k++)
                {
                    for (int l = 0; l < section2Meetings.Count(); l++)
                    {
                        // If meeting days overlap
                        if (section1Meetings.ElementAt(k).Days.Intersect(section2Meetings.ElementAt(l).Days).Count() != 0)
                        {
                            // Check meeting pattern dates and frequency... if dates overlap and frequency matches, continue on to check overlapping times
                            // In reality, end date for section meeting should never be null, but account for the possibility that it has a min date (essentially, null)
                            if (((section1Meetings.ElementAt(k).StartDate < section2Meetings.ElementAt(l).EndDate) || section2Meetings.ElementAt(l).EndDate == DateTime.MinValue) &&
                                ((section1Meetings.ElementAt(k).EndDate > section2Meetings.ElementAt(l).StartDate) || section1Meetings.ElementAt(k).EndDate == DateTime.MinValue) &&
                                 (section1Meetings.ElementAt(k).Frequency == section2Meetings.ElementAt(l).Frequency))
                            {
                                // The meeting times AND days must overlap
                                // Only compare meeting start and end times if at least one of the meetings has a start or end time
                                if (section1Meetings.ElementAt(k).StartTime.HasValue || section2Meetings.ElementAt(l).StartTime.HasValue || section1Meetings.ElementAt(k).EndTime.HasValue || section2Meetings.ElementAt(l).EndTime.HasValue)
                                {
                                    if (((section1Meetings.ElementAt(k).StartTime < section2Meetings.ElementAt(l).EndTime) || !section2Meetings.ElementAt(l).EndTime.HasValue) &&
                                    ((section1Meetings.ElementAt(k).EndTime > section2Meetings.ElementAt(l).StartTime) || !section1Meetings.ElementAt(k).EndTime.HasValue))
                                    {
                                        isOverlap = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return isOverlap;
        }

        /// <summary>
        /// Evaluate Requisite - evaluate all requisites that have a requirement
        /// </summary>
        /// <param name="plannedCourse"></param>
        /// <param name="requisite"></param>
        /// <param name="completedByDate"></param>
        /// <param name="terms"></param>
        /// <param name="registrationTerms"></param>
        /// <param name="courses"></param>
        /// <param name="sections"></param>
        /// <param name="credits"></param>
        /// <param name="requirements"></param>
        /// <param name="ruleResults"></param>
        private void EvaluateRequisite(PlannedCourse plannedCourse, string plannedCourseTermCode, Requisite requisite, DateTime plannedCourseStart, DateTime plannedCourseEnd, IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<Course> courses, IEnumerable<Section> sections, IEnumerable<AcademicCredit> credits, IEnumerable<Requirement> requirements, IEnumerable<RuleResult> ruleResults)
        {
            // Get the pertinent planned courses from this degree plan to consider during the evaluation of this planned course's requisite (items selected depend on requisite order and dates supplied)
            var pTerms = GetPlannedCoursesForRequisiteValidation(requisite.CompletionOrder, plannedCourseStart, plannedCourseEnd, plannedCourseTermCode, terms, registrationTerms, credits, sections);
            // Get the pertinent academic credits from student to consider during the evaluation of this planned course's requisite (items selected depend on the requisite completion order and dates suppllied)
            var evalCourseList = ConvertPlannedTermToEvalPlannedCourses(pTerms, courses);
            var evalAcadCredits = GetAcademicCreditsForValidation(requisite.CompletionOrder, plannedCourseStart, plannedCourseEnd, plannedCourseTermCode == _NonTermKey ? null : plannedCourseTermCode, terms, registrationTerms, credits, sections);
            if (requisite.RequirementCode != null)
            {
                var requirement = requirements.Where(r => r.Code == requisite.RequirementCode).FirstOrDefault();
                if (requirement != null)
                {
                    var requirementList = new List<Requirement>() { requirement };

                    ProgramEvaluation programEvaluation = new ProgramEvaluator(requirementList, evalAcadCredits, evalCourseList, ruleResults, courses).Evaluate();

                    if (!programEvaluation.IsSatisfied && !programEvaluation.IsPlannedSatisfied)
                    {
                        plannedCourse.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite) { Requisite = requisite });
                    }
                }
            }
        }

        private void ValidateCredits(IEnumerable<Term> registrationTerms, IEnumerable<Section> sections)
        {
            // Combine with the CheckForTimeConflicts above.
            // 1) PlannedCourse Credits must be null or greater than 0 and less than 10,000)
            //        (Colleague's upper max is 9999.99)
            // 2) If a PlannedCourse has a SectionId and Credits, and the section is a "registration section", then the credits must agree with the section min and max range.
            //        If the section has no min or max then no validation on the planned credits is done.  (Could have come from the course and 
            //             isn't currently applicable for the associated section.
            //        If the section only has min cred then the planned course credits must equal min cred.
            //        If the section only has max cred then the credits must be less then that or null. 
            //        If the section has min and max credits then the planned course credits must be between them.
            //        If the section has min and max and an increment then the credits must be a selectable value.

            // Get all the planned courses in the registation terms and nonterms that have a planned section.
            var plannedCourses = new List<PlannedCourse>();
            var termIds = registrationTerms.Select(t => t.Code);
            foreach (var termId in termIds)
            {
                plannedCourses.AddRange(GetPlannedCourses(termId).Where(pc => (!string.IsNullOrEmpty(pc.SectionId))));
            }
            plannedCourses.AddRange(NonTermPlannedCourses.Where(pc => (!string.IsNullOrEmpty(pc.SectionId))));
            foreach (var plannedCourse in plannedCourses)
            {
                bool validCredits = true;
                if (plannedCourse.Credits != null)
                {
                    if (plannedCourse.Credits < 0 || plannedCourse.Credits > 9999.99m)
                    {
                        validCredits = false;
                    }
                    else
                    {
                        var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();
                        if (section != null)
                        {
                            if (section.MaximumCredits == null)
                            {
                                if (section.MinimumCredits != null && plannedCourse.Credits != section.MinimumCredits)
                                {
                                    // Planned credits must equal section min credits. (This is the fixed credit situation.)
                                    validCredits = false;
                                }
                            }
                            else
                            {
                                if (section.MinimumCredits == null)
                                {
                                    if (plannedCourse.Credits > section.MaximumCredits)
                                    {
                                        // Planned credits greater than section's maximum credits.
                                        validCredits = false;
                                    }
                                }
                                else
                                {
                                    if ((section.MinimumCredits != null && plannedCourse.Credits < section.MinimumCredits) || (plannedCourse.Credits > section.MaximumCredits))
                                    {
                                        // Planned credits is less than the section's minimum credits or greater than max.
                                        validCredits = false;
                                    }
                                    else
                                    {
                                        // Credits is between Min and a max, but if there is an increment, make sure credits is a selectable value. (Increment is irrelevant without a min and max.)
                                        if (section.VariableCreditIncrement > 0 && section.VariableCreditIncrement < section.MaximumCredits)
                                        {
                                            // Build a list of possible values
                                            List<decimal> validEntries = new List<decimal>();
                                            decimal x = section.MinimumCredits.GetValueOrDefault(0);
                                            while (x <= section.MaximumCredits)
                                            {
                                                validEntries.Add(x);
                                                x += section.VariableCreditIncrement.GetValueOrDefault(0);
                                            }
                                            // See if course.Credits is in the list.
                                            if (!validEntries.Any(e => e == plannedCourse.Credits))
                                            {
                                                validCredits = false;
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                // Are credits valid?
                if (validCredits == false)
                {
                    plannedCourse.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.InvalidPlannedCredits));
                }
            }
        }

        public List<AcademicCredit> GetAcademicCreditsForValidation(RequisiteCompletionOrder requisiteCompletionOrder, DateTime plannedCourseStart, DateTime plannedCourseEnd, string plannedCourseTermCode, IEnumerable<Term> terms, IEnumerable<Term> registrationTerms, IEnumerable<AcademicCredit> credits, IEnumerable<Section> sections)
        {
            // Get the academic credits from the passed list to consider during the evaluation
            var evalAcadCredits = new List<AcademicCredit>();

            // PREVIOUS: Take all acad creds with end date LT plannedCourseStart
            if (requisiteCompletionOrder == RequisiteCompletionOrder.Previous)
            {
                // using the credit end date or the section end date.
                foreach (var acadCredit in credits)
                {
                    if (acadCredit.EndDate != null && acadCredit.EndDate < plannedCourseStart)
                    {
                        evalAcadCredits.Add(acadCredit);
                    }
                    else
                    {
                        if (acadCredit.EndDate == null && !string.IsNullOrEmpty(acadCredit.SectionId))
                        {
                            // section end date must fall before the start
                            // of the planned course
                            var section = sections.Where(s => s.Id == acadCredit.SectionId).FirstOrDefault();
                            if (section != null && section.EndDate != null && section.EndDate < plannedCourseStart)
                            {
                                evalAcadCredits.Add(acadCredit);
                            }
                        }
                        else
                        {
                            if (acadCredit.EndDate == null && acadCredit.TermCode != null)
                            {
                                // if the acad cred has no end date and no section, check for a term. Term end date must fall before the start
                                // of the planned course
                                var acadTerm = terms.Where(t => t.Code == acadCredit.TermCode).FirstOrDefault();
                                if (acadTerm != null && acadTerm.EndDate < plannedCourseStart)
                                {
                                    evalAcadCredits.Add(acadCredit);
                                }
                            }
                        }
                    }
                }
            }

            // PREVIOUSORCONCURRENT: Take all acad credits with a term matching the item's term OR with a start date LE plannedCourseEnd 
            if (requisiteCompletionOrder == RequisiteCompletionOrder.PreviousOrConcurrent)
            {
                // using the credit start date or the section start date.
                foreach (var acadCredit in credits)
                {
                    if ((plannedCourseTermCode != null && plannedCourseTermCode == acadCredit.TermCode) || (acadCredit.StartDate != null && acadCredit.StartDate <= plannedCourseEnd))
                    {
                        evalAcadCredits.Add(acadCredit);
                    }
                    else
                    {
                        // if there is no start date on the acad cred - see if there is a section with a start date to use in the comparison.
                        if (acadCredit.StartDate == null && !string.IsNullOrEmpty(acadCredit.SectionId))
                        {
                            // section start date must fall before the completed by date (Section start date is required)
                            var section = sections.Where(s => s.Id == acadCredit.SectionId).FirstOrDefault();
                            if (section != null && section.StartDate <= plannedCourseEnd)
                            {
                                evalAcadCredits.Add(acadCredit);
                            }
                        }
                        else
                        {
                            if (acadCredit.StartDate == null && acadCredit.TermCode != null)
                            {
                                // if the acad cred has no end date and no section, check for a term. Term end date must fall before the start
                                // of the planned course
                                var acadTerm = terms.Where(t => t.Code == acadCredit.TermCode).FirstOrDefault();
                                if (acadTerm != null && acadTerm.StartDate <= plannedCourseEnd)
                                {
                                    evalAcadCredits.Add(acadCredit);
                                }
                            }
                        }
                    }
                }
            }

            // CONCURRENT: Take all acad credits the SAME term as the item's term, OR with an start date LE completedByDate and a end date GE startByDate
            if (requisiteCompletionOrder == RequisiteCompletionOrder.Concurrent)
            {
                foreach (var acadCredit in credits)
                {

                    if ((plannedCourseTermCode != null && plannedCourseTermCode == acadCredit.TermCode))
                    {
                        // All credits with the same term as the item being evaluated should be considered in "concurrency". 
                        evalAcadCredits.Add(acadCredit);
                    }
                    else
                    {
                        if ((acadCredit.StartDate != null && acadCredit.StartDate <= plannedCourseEnd) && (acadCredit.EndDate != null && acadCredit.EndDate >= plannedCourseStart))
                        {
                            // This means academic credit has both a start and an end date and they are valid 
                            evalAcadCredits.Add(acadCredit);
                        }
                        else
                        {
                            // If the academic credit doesn't have the correct dates to use, see if there is a section with start and end dates to use in comparison.
                            if ((acadCredit.EndDate == null || acadCredit.StartDate == null) && !string.IsNullOrEmpty(acadCredit.SectionId))
                            {
                                var section = sections.Where(s => s.Id == acadCredit.SectionId).FirstOrDefault();
                                // Note: section start date is required but section end date is not. If there is a section end date
                                if (section != null)
                                {
                                    if (((section.EndDate != null && section.EndDate >= plannedCourseStart) && section.StartDate <= plannedCourseEnd) || (section.EndDate == null && (section.StartDate <= plannedCourseEnd && section.EndDate >= plannedCourseStart)))
                                    {
                                        evalAcadCredits.Add(acadCredit);
                                    }
                                }
                            }
                            else
                            {
                                if ((acadCredit.EndDate == null || acadCredit.StartDate == null) && !string.IsNullOrEmpty(acadCredit.TermCode))
                                {
                                    // if the acad cred is missing dates date and there is no section, check for a term. Term end date must fall before the start
                                    // of the planned course
                                    var acadTerm = terms.Where(t => t.Code == acadCredit.TermCode).FirstOrDefault();
                                    if (acadTerm != null && acadTerm.StartDate <= plannedCourseEnd && acadTerm.EndDate >= plannedCourseStart)
                                    {
                                        evalAcadCredits.Add(acadCredit);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return evalAcadCredits;
        }

        public List<PlannedCourse> PlannedCourses
        {
            get
            {
                return TermCourses.Values.SelectMany(c => c.ToList()).Union(NonTermPlannedCourses).ToList();
            }
        }

        /// <summary>
        /// Returns true if review date/advisor or approval count or approval content has changed.
        /// </summary>
        /// <param name="cachedDegreePlan"></param>
        /// <returns></returns>
        public bool HasReviewChange(DegreePlan cachedDegreePlan)
        {
            if (cachedDegreePlan == null)
            {
                throw new ArgumentNullException("cachedDegreePlan", "Cached degree plan not provided, cannot verify approval updates.");
            }
            if (cachedDegreePlan.Version != this.Version)
            {
                throw new ArgumentException("cachedDegreePlan", "Cached degree plan has different version, cannot verify approval updates.");
            }
            if (cachedDegreePlan.PersonId != this.PersonId)
            {
                throw new ArgumentException("cachedDegreePlan", "Cached degree plan person does not match this degree plan, cannot verify approval updates.");
            }
            // A review change has occurred if the last review date or advisor id has changed. 
            // Last clause ensures a null advisor and an empty string advisor are considered the same.
            if (this.LastReviewedDate != cachedDegreePlan.LastReviewedDate || (this.LastReviewedAdvisorId != cachedDegreePlan.LastReviewedAdvisorId &&
                !(string.IsNullOrEmpty(this.LastReviewedAdvisorId) && string.IsNullOrEmpty(cachedDegreePlan.LastReviewedAdvisorId))))
            {
                return true;
            }
            if (this.Approvals.Count() != cachedDegreePlan.Approvals.Count())
            {
                return true;
            }
            IEnumerable<DegreePlanApproval> thisApprovals = this.Approvals.OrderBy(a => a.CourseId).ThenBy(a => a.Date).ThenBy(a => a.PersonId).ThenBy(a => a.Status).ThenBy(a => a.TermCode).ToList();
            IEnumerable<DegreePlanApproval> cachedApprovals = cachedDegreePlan.Approvals.OrderBy(a => a.CourseId).ThenBy(a => a.Date).ThenBy(a => a.PersonId).ThenBy(a => a.Status).ThenBy(a => a.TermCode).ToList();
            for (int i = 0; i < this.Approvals.Count(); i++)
            {
                if (thisApprovals.ElementAt(i).CourseId != cachedApprovals.ElementAt(i).CourseId) return true;
                if (thisApprovals.ElementAt(i).Date != cachedApprovals.ElementAt(i).Date) return true;
                if (thisApprovals.ElementAt(i).PersonId != cachedApprovals.ElementAt(i).PersonId) return true;
                if (thisApprovals.ElementAt(i).Status != cachedApprovals.ElementAt(i).Status) return true;
                if (thisApprovals.ElementAt(i).TermCode != cachedApprovals.ElementAt(i).TermCode) return true;
            }
            return false;
        }

        public bool ReviewOnlyChange(DegreePlan target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "Must provide a degree plan to compare to.");
            }
            // Describes the difference between the target degree plan and the source (this) degree plan.
            // If the Id, PersonId, or the Version is different between plans, this is considered an "invalid" compare. 
            // If terms or anything about the planned courses is different this is considered a "Full" changes.
            // Anything else - approvals, review date, last reviewed info, or public/restricted notes - is considered a ReviewOnly changes.

            // Test general data
            if (Id != target.Id || PersonId != target.PersonId || Version != target.Version)
            {
                return false;
            }

            // Test terms
            if (TermIds.Except(target.TermIds).Count() > 0)
            {
                return false;
            }
            var targetTermIds = target.TermIds;
            if (targetTermIds.Except(TermIds).Count() > 0)
            {
                return false;
            }

            // Test nonTerm planned courses have neither been added nor removed
            var sortedCourses = NonTermPlannedCourses.OrderBy(n => n.CourseId).ThenBy(n => n.SectionId).ThenBy(n => n.Credits).ThenBy(n => n.GradingType).ToList();
            var sortedTargetCourses = target.NonTermPlannedCourses.OrderBy(n => n.CourseId).ThenBy(n => n.SectionId).ThenBy(n => n.Credits).ThenBy(n => n.GradingType).ToList();
            if (sortedCourses.Count() != sortedTargetCourses.Count())
            {
                return false;
            }
            else
            {
                for (int i = 0; i < sortedCourses.Count(); i++)
                {
                    if (!sortedCourses.ElementAt(i).Equals(sortedTargetCourses.ElementAt(i)))
                    {
                        return false;
                    }
                }
            }

            // Test planned courses by term have neither been added nor removed
            // (Need to do it this way because a course could have simply changed terms and that would not be a ReviewOnly change)
            foreach (var term in TermIds)
            {
                sortedCourses = GetPlannedCourses(term).OrderBy(n => n.CourseId).ThenBy(n => n.SectionId).ThenBy(n => n.Credits).ThenBy(n => n.GradingType).ToList();
                sortedTargetCourses = target.GetPlannedCourses(term).OrderBy(n => n.CourseId).ThenBy(n => n.SectionId).ThenBy(n => n.Credits).ThenBy(n => n.GradingType).ToList();
                if (sortedCourses.Count() != sortedTargetCourses.Count())
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < sortedCourses.Count(); i++)
                    {
                        if (!sortedCourses.ElementAt(i).Equals(sortedTargetCourses.ElementAt(i)))
                        {
                            return false;
                        }
                    }
                }
            }

            // If none of the above tests have produced a false the only remaining items that could have change constitute a ReviewOnly type of change.
            // This includes approvals, public/restricted notes, last reviewed by, last reviewed date, and review requested. 
            return true;
        }

        /// <summary>
        /// Return the plannedCourses that should be included during requisite validation, based on the given
        /// completion date and the credits for a student to-date. The degree plan planned courses 
        /// must be filtered by the credits (registered/taken courses that match up) and the given
        /// date, that causes planned courses to be excluded that have not been completed by that date.
        /// </summary>
        /// <param name="requisiteCompletionOrder">Depending on the requisite completion order - different date checks are performed</param>
        /// <param name="plannedCourseStart">Start date of the item being evaluated for requisites - if a non-term section it is the section start, otherwise it is the term start</param>
        /// <param name="plannedCourseEnd">End date of the item being evaluated for requisites</param>
        /// <param name="plannedCourseTermCode">Term that the item being evaluated falls in</param>
        /// <param name="allTerms">all terms</param>
        /// <param name="registrationTerms">terms open for registration - used to determine the earliest date</param>
        /// <param name="credits">student's academic credit list that is being filtered to the appropriate items</param>
        /// <param name="sections">used to find section information</param>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<PlannedCourse>> GetPlannedCoursesForRequisiteValidation(RequisiteCompletionOrder requisiteCompletionOrder, DateTime plannedCourseStart, DateTime plannedCourseEnd, string plannedCourseTermCode, IEnumerable<Term> allTerms, IEnumerable<Term> registrationTerms, IEnumerable<AcademicCredit> credits, IEnumerable<Section> sections)
        {
            // Initialize returned list of courses
            var plannedCoursesDict = new Dictionary<string, IEnumerable<PlannedCourse>>();

            // Set minimum start date from which we will draw planned courses.  
            // NOTE: Planned courses that exist in past terms (terms no longer open for registration) are always disregarded,
            // because only academic credits completed should be considered in past terms. This routine's primary purpose is to collect pertinent
            // current and future planned courses (that do not match up to any existing (in progress) credits). 
            DateTime earliestStartDate = DateTime.Today;
            if (registrationTerms != null && registrationTerms.Count() > 0)
            {
                earliestStartDate = registrationTerms.Select(t => t.StartDate).Min();
            }

            // This list used to track the academic credits as they are matched up with planned courses on the plan
            var creditsUsed = new List<string>();

            // First loop through nonterm planned courses, return items that fall within the date
            // range and do not correspond with an academic credit.
            var nonTermPlannedCourses = new List<PlannedCourse>();
            foreach (var plannedCourse in NonTermPlannedCourses)
            {
                // NonTerm courses must have an associated section and the section must have valid start and end dates. (if not, we'll just skip them)
                if (!string.IsNullOrEmpty(plannedCourse.SectionId))
                {
                    bool validPlannedCourse = false;
                    var section = sections.Where(s => s.Id == plannedCourse.SectionId).FirstOrDefault();

                    if (section != null && section.EndDate != null)
                    {
                        // PREVIOUS - Consider this item only if the section starts on or after earliestStartDate and has a end-date before the plannedCourseStart
                        if (requisiteCompletionOrder == RequisiteCompletionOrder.Previous && section.StartDate >= earliestStartDate && section.EndDate < plannedCourseStart)
                        {
                            validPlannedCourse = true;
                        }
                        // PREVIOUSORCONCURRENT - Consider this item only if it's section starts on or after earliestStartDate and has a start-date before or equal to the planned course end 
                        if (requisiteCompletionOrder == RequisiteCompletionOrder.PreviousOrConcurrent && section.StartDate >= earliestStartDate && section.StartDate <= plannedCourseEnd)
                        {
                            validPlannedCourse = true;
                        }
                        // CONCURRENT - - select planned courses with a section start-date before the plannedCourseEnd and a  end-date after the plannedCourseStart
                        if (requisiteCompletionOrder == RequisiteCompletionOrder.Concurrent && section.StartDate >= earliestStartDate && section.StartDate <= plannedCourseEnd && section.EndDate >= plannedCourseStart)
                        {
                            validPlannedCourse = true;
                        }
                    }
                    if (validPlannedCourse)
                    {
                        // Find an academic credit for this section and see if it's already been used
                        var credit = credits.Where(c => c.SectionId == plannedCourse.SectionId).FirstOrDefault();
                        if ((credit != null) && (!creditsUsed.Contains(credit.Id)))
                        {
                            // acad credit that hasn't been used yet was found for this planned course so don't 
                            // add the planned course to the return list, and track the credit used so it's not used again
                            creditsUsed.Add(credit.Id);
                        }
                        else
                        {
                            // acad credit not found for this planned course, so add it to the list of returned planned courses
                            nonTermPlannedCourses.Add(plannedCourse);
                        }
                    }
                }
            }
            if (nonTermPlannedCourses.Count() > 0)
            {
                plannedCoursesDict[_NonTermKey] = nonTermPlannedCourses.Distinct();
            }

            // Get planned courses from each of the terms, if term dates fall within minimum and completedBy dates AND planned course not already
            // included in credits (in the case of registration terms)
            foreach (var termCode in this.TermIds)
            {
                var plannedCourses = new List<PlannedCourse>();

                // Sequence items so that those with section Id are examined first. and exclude course placeholders
                IEnumerable<PlannedCourse> plannedTermCourses = this.GetPlannedCourses(termCode).Where(c => c.CourseId != null).OrderByDescending(p => p.SectionId).ToList();
                if (plannedTermCourses.Count() > 0)
                {
                    // Get the term and continue if the term dates conform to the set earliest start date and the planned course start and end dates.
                    // Changed check from term start to term end to account for minimesters within a
                    // term, so a course can be 'planned' until a term ends.

                    var term = allTerms.Where(t => t.Code == termCode).FirstOrDefault();
                    bool validPlannedCourse = false;
                    if (term != null)
                    {
                        // PREVIOUS - Consider this item only if it's term starts on or after earliestStartDate and has a term-end-date before the plannedCourseStart
                        if (requisiteCompletionOrder == RequisiteCompletionOrder.Previous && term.StartDate >= earliestStartDate && term.EndDate < plannedCourseStart)
                        {
                            validPlannedCourse = true;
                        }
                        // PREVIOUSORCONCURRENT - select planned courses with same term as the provided plannedCourseTermCode ORConsider this item only if it's term starts on or after earliestStartDate and has a term-start-date before or equal to the planned course end 
                        if ((requisiteCompletionOrder == RequisiteCompletionOrder.PreviousOrConcurrent && term.Code == plannedCourseTermCode) ||
                            (requisiteCompletionOrder == RequisiteCompletionOrder.PreviousOrConcurrent && term.StartDate >= earliestStartDate && term.StartDate <= plannedCourseEnd))
                        {
                            validPlannedCourse = true;
                        }
                        // CONCURRENT - - select planned courses with same term as the provided plannedCourseTermCode OR a term start-date before the plannedCourseEnd and a term end-date after the plannedCourseStart
                        if ((requisiteCompletionOrder == RequisiteCompletionOrder.Concurrent && term.Code == plannedCourseTermCode) ||
                            (requisiteCompletionOrder == RequisiteCompletionOrder.Concurrent && term.StartDate >= earliestStartDate && term.StartDate <= plannedCourseEnd && term.EndDate >= plannedCourseStart))
                        {
                            validPlannedCourse = true;
                        }
                        if (validPlannedCourse)
                        {
                            // All planned courses need to be verified, regardless of term. None will be included in terms before the specified minimum date.
                            // Examine each course and only return the ones not included in the academic credits found for that term
                            foreach (var plannedCourse in plannedTermCourses)
                            {
                                var matched = false;
                                if (credits != null && credits.Count() > 0)
                                {
                                    // If this course can be found in the list of acad credit for this term then don't include in the returned list.
                                    var crsCredits = credits.Where(c => c.Course != null && c.Course.Id == plannedCourse.CourseId && c.TermCode == term.Code);
                                    // In case there is more than one instance of credit for the same course on the plan, loop through and match individually.
                                    foreach (var credit in crsCredits)
                                    {
                                        if ((credit != null) && (!creditsUsed.Contains(credit.Id)) && (!matched) && (string.IsNullOrEmpty(plannedCourse.SectionId) || plannedCourse.SectionId == credit.SectionId))
                                        {
                                            // acad credit that hasn't been used yet was found for this planned course so don't 
                                            // add it to the return list; track the one found so it's not used again
                                            creditsUsed.Add(credit.Id);
                                            matched = true;
                                        }
                                    }
                                }
                                if (!matched)
                                {
                                    // acad credit not found for this planned course, so add it to the list of returned planned courses
                                    plannedCourses.Add(plannedCourse);

                                }
                            }
                        }
                    }
                }
                if (plannedCourses.Count() > 0)
                {
                    plannedCoursesDict[termCode] = plannedCourses;
                }
            }
            return plannedCoursesDict;
        }

        /// <summary>
        /// Determines whether a change has been made to a protected planned course in the degree plan.
        /// </summary>
        /// <param name="cachedDegreePlan"></param>
        public bool HasProtectedChange(DegreePlan cachedDegreePlan)
        {
            // Error if cached degree plan is missing
            if (cachedDegreePlan == null)
            {
                throw new ArgumentNullException("cachedDegreePlan", "Cached plan needs to be provided to verify protection updates.");
            }

            // A mismatched version must be rejected
            if (cachedDegreePlan.Version != this.Version)
            {
                throw new ArgumentException("cachedDegreePlan", "Cached plan version does not match. Cannot verify protection updates.");
            }

            // Make sure person is same
            if (cachedDegreePlan.PersonId != this.PersonId)
            {
                throw new ArgumentException("cachedDegreePlan", "Cached plan person does not match. Cannot verify protection updates.");
            }

            // Verifying Nonterm Planned Courses

            // All previously locked nonterm planned courses must still have a locked counterpart.
            foreach (var cachedPlannedCourse in cachedDegreePlan.NonTermPlannedCourses)
            {
                if (cachedPlannedCourse != null && cachedPlannedCourse.IsProtected == true)
                {
                    var currentPlannedCourse = this.NonTermPlannedCourses
                        .Where(c => c.CourseId == cachedPlannedCourse.CourseId && c.IsProtected == cachedPlannedCourse.IsProtected)
                        .FirstOrDefault();

                    if (currentPlannedCourse == null)
                    {
                        return true;
                    }
                }
            }
            // Nonterm courses may have been added with the protected attribute set to true
            foreach (var currentNonTermPlannedCourse in this.NonTermPlannedCourses)
            {
                if (currentNonTermPlannedCourse != null && currentNonTermPlannedCourse.IsProtected == true)
                {
                    if (!cachedDegreePlan.NonTermPlannedCourses.Where(pc => pc.IsProtected == true)
                                                               .Select(c => c.CourseId)
                                                               .Contains(currentNonTermPlannedCourse.CourseId))
                    {
                        return true;
                    }
                }
            }

            // Verifying Term-based Planned Courses

            // All previously locked planned courses should still have a locked counterpart 

            // In the current world, a course can be planned multiple times.  The advisor may protect MATH-100 but we
            // are still allowing a student to add MATH-100 on to the plan a second time unprotected.  This is necessary to allow
            // the student to experiment with choosing multiple sections of the same course. Also, note that if the advisor adds
            // the SAME planned course to a plan twice and protects both, the API doesn't currently prevent one of the protected ones from
            // being removed. 

            foreach (var cachedPlannedTerm in cachedDegreePlan.TermIds)
            {
                if (!string.IsNullOrEmpty(cachedPlannedTerm))
                {
                    var cachedPlannedCourses = cachedDegreePlan.GetPlannedCourses(cachedPlannedTerm);
                    var plannedCourses = this.GetPlannedCourses(cachedPlannedTerm);
                    foreach (var cachedPlannedCourse in cachedPlannedCourses)
                    {
                        if (cachedPlannedCourse != null && cachedPlannedCourse.IsProtected == true)
                        {
                            var plannedCourse = plannedCourses.Where(c => c.CourseId == cachedPlannedCourse.CourseId &&
                                                                          c.IsProtected == cachedPlannedCourse.IsProtected)
                                                                .FirstOrDefault();
                            if (plannedCourse == null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            // Term-based courses may have been added with locked attribute
            foreach (var currentPlannedTerm in this.TermIds)
            {
                if (!string.IsNullOrEmpty(currentPlannedTerm))
                {
                    var plannedCourses = this.GetPlannedCourses(currentPlannedTerm);
                    var cachedPlannedCourses = cachedDegreePlan.GetPlannedCourses(currentPlannedTerm);
                    // No nonterm courses may be added with the locked attribute set to true
                    foreach (var currentPlannedCourse in plannedCourses)
                    {
                        if (currentPlannedCourse != null && currentPlannedCourse.IsProtected == true)
                        {
                            if (!cachedPlannedCourses.Where(pc => pc.IsProtected == true)
                                                     .Select(c => c.CourseId)
                                                     .Contains(currentPlannedCourse.CourseId))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if there are any courses on the plan that are protected, thus indicating if the whole plan
        /// is protected. Used to determine if apply sample should add courses as protected.
        /// </summary>
        public bool IsPlanProtected
        {
            get
            {
                if (this.NonTermPlannedCourses.Where(pc => pc.IsProtected == true).Count() > 0)
                {
                    return true;
                }
                foreach (var termId in TermIds)
                {
                    if (GetPlannedCourses(termId).Where(pc => pc.IsProtected == true).Count() > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// If the degree plan has null protection flags reset the values to the stored versions.
        /// This will guarantee older update version have all the necessary new data.
        /// </summary>
        /// <param name="storedDegreePlan">Stored version of the degree plan - pulled from the database</param>
        public void UpdateMissingProtectionFlags(DegreePlan storedDegreePlan)
        {
            // Error if stored degree plan is missing
            if (storedDegreePlan == null)
            {
                throw new ArgumentNullException("storedDegreePlan", "Stored plan needs to be provided to fill in any missing protection flags.");
            }

            foreach (var storedPlannedCourse in storedDegreePlan.NonTermPlannedCourses)
            {
                // Reset any protected flags on "this" using the protected planned courses in the stored plan.
                if (storedPlannedCourse != null && storedPlannedCourse.IsProtected == true)
                {
                    // Now find the closest match to this item. While it is true we really only "protect" courses it would be less confusing to the user if
                    // we re-applied the protected flag to the planned course that is the "closest fit". 

                    // Nonterm planned courses MUST have a section Id which simplifies matters. Look for that one first - else take first one.

                    var plannedCourse = this.NonTermPlannedCourses.Where(c => c.CourseId == storedPlannedCourse.CourseId && c.SectionId == storedPlannedCourse.SectionId && c.IsProtected != true)
                                                                     .FirstOrDefault();
                    if (plannedCourse != null)
                    {
                        // Found a match - update it
                        plannedCourse.IsProtected = true;
                    }
                    else
                    {
                        // If we didn't find a match, take any unprotected planned course regardless what section and protect it.  This is no guarentee
                        // that we will match up perfectly with the sections we really don't protect the sections just the course so getting the same number
                        // of protection flags back on the plan is more important.
                        var anyPlannedCourse = this.NonTermPlannedCourses.Where(c => c.CourseId == storedPlannedCourse.CourseId && c.IsProtected != true)
                                                                         .FirstOrDefault();
                        if (anyPlannedCourse != null)
                        {
                            // Found a match - update it
                            anyPlannedCourse.IsProtected = true;
                        }
                    }
                }
            }

            foreach (var plannedTerm in this.TermIds)
            {
                if (!string.IsNullOrEmpty(plannedTerm))
                {
                    var storedPlannedCourses = storedDegreePlan.GetPlannedCourses(plannedTerm).Where(c => c.CourseId != null);
                    var plannedCourses = this.GetPlannedCourses(plannedTerm).Where(c => c.CourseId != null);

                    foreach (var storedPlannedCourse in storedPlannedCourses)
                    {
                        // Reset any protected flags on "this" using the protected planned courses in the stored plan.
                        if (storedPlannedCourse != null && storedPlannedCourse.IsProtected == true)
                        {
                            // Now find the closest match to this item. While it is true we really only "protect" courses it would be less confusing to the user if
                            // we re-applied the protected flag to the planned course that is the "closest fit".   In this case there can be courses without sections.
                            bool matchFound = false;
                            // If it has a section see if you can find that one.
                            if (!string.IsNullOrEmpty(storedPlannedCourse.SectionId))
                            {
                                var plannedCourse = plannedCourses
                                    .Where(c => c.CourseId == storedPlannedCourse.CourseId && c.SectionId == storedPlannedCourse.SectionId && c.IsProtected != true)
                                    .FirstOrDefault();

                                if (plannedCourse != null)
                                {
                                    // Found a match - update it
                                    plannedCourse.IsProtected = true;
                                    matchFound = true;
                                }
                            }
                            if (!matchFound)
                            {
                                // If it doesn't have a section - or if the course with that exact section was not already found, see
                                // if there is an unprotected planned course without any section on the plan that can be used.  (i.e. take that over another one with a different section)
                                var plannedCourse = plannedCourses
                                    .Where(c => c.CourseId == storedPlannedCourse.CourseId && string.IsNullOrEmpty(c.SectionId) && c.IsProtected != true)
                                    .FirstOrDefault();

                                if (plannedCourse != null)
                                {
                                    // Found a match - update it
                                    plannedCourse.IsProtected = true;
                                    matchFound = true;
                                }
                            }
                            if (!matchFound)
                            {
                                // If we still haven't found a match, take any unprotected planned course regardless what section and protect it.  This is no guarentee
                                // that we will match up perfectly with the sections we really don't protect the sections just the course so getting the same number
                                // of protection flags back on the plan is more important.
                                var plannedCourse = plannedCourses
                                    .Where(c => c.CourseId == storedPlannedCourse.CourseId && c.IsProtected != true)
                                    .FirstOrDefault();
                                if (plannedCourse != null)
                                {
                                    // Found a match - update it
                                    plannedCourse.IsProtected = true;
                                    matchFound = true;
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}