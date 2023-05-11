// Copyright 2014 - 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;

namespace Ellucian.Colleague.Domain.Student.Services
{
    /// <summary>
    /// Class to perform special processing on sections
    /// </summary>
    public static class SectionProcessor
    {
        private static ILogger _logger = NOPLogger.Instance;

        /// <summary>
        /// Initialize the logger for this class to use.  Note that only the logger first used
        /// to initialize will be used; subsequent calls will be ignored.
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void InitializeLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Determine the term the is the best fit for a given section
        /// </summary>
        /// <param name="section">The section</param>
        /// <param name="terms">List of terms</param>
        /// <param name="checkAcademicLevel">Use academic level of the section to limit the terms checked</param>
        /// <returns>The code of the best-fit term</returns>
        public static string DetermineTerm(Section section, IEnumerable<Term> terms, bool checkAcademicLevel = true)
        {
            // If there are no terms, we simply return
            List<Term> potentialTerms = terms.Where(x => x.UseTermInBestFitCalculations).ToList();

            //if checkAcademicLevel is true we need to try and find a term by academic level is present
            if (checkAcademicLevel)
            {
                // Limit the terms by the academic level of the section
                potentialTerms = LimitTermsByAcademicLevel(section.AcademicLevelCode, potentialTerms.ToList());
                //if there are no terms that have the level, recursively call this method with checkacademiclevel false
                if (!potentialTerms.Any())
                {
                    return DetermineTerm(section, terms, false);
                }

                // If there's only 1 potential term, then return it
                if (potentialTerms.Count == 1)
                {
                    return potentialTerms.First().Code;
                }
                //if it gets here it means there are more than one term with matching academic level so let the rest of the method go.
            }
            else
            {
                potentialTerms = potentialTerms.Where(x => x.AcademicLevels == null || x.AcademicLevels.Count == 0).ToList();
            }

            // Narrow down the possible terms to those that the section's start date falls into
            potentialTerms = potentialTerms.Where(x => x.StartDate <= section.StartDate && section.StartDate <= x.EndDate).ToList();

            //if no terms with academic level match start dates recursively call with check off
            if (!potentialTerms.Any() && checkAcademicLevel)
            {
                return DetermineTerm(section, terms, false);
            }

            // If there's only 1 potential term, then return it
            if (potentialTerms.Count == 1)
            {
                return potentialTerms.First().Code;
            }

            // If the section has an end date, see if we can narrow the terms down further
            if (section.EndDate.HasValue)
            {
                potentialTerms = LimitTermsByEndDate(section.EndDate.Value, potentialTerms);
            }
            // If there's only 1 potential term, then return it
            if (potentialTerms.Count == 1)
            {
                return potentialTerms.First().Code;
            }

            // We have 2 or more terms, so sort them in the order we want them in
            var sortedTerms = SortTerms(section.StartDate, section.EndDate, potentialTerms);

            // After sorting, the first term should be the one we want - the best fit
            var term = sortedTerms.FirstOrDefault();
            return term == null ? null : term.Code;
        }

        /// <summary>
        /// Limit the terms by the section's ending date
        /// </summary>
        /// <param name="endDate">Section end date</param>
        /// <param name="terms">List of terms</param>
        /// <returns>Terms limited by end date</returns>
        public static List<Term> LimitTermsByEndDate(DateTime endDate, List<Term> terms)
        {
            var potentialTerms = terms.Where(x => x.EndDate >= endDate).ToList();

            // If there are any potential terms, return them; otherwise, return the original list
            return (potentialTerms == null || potentialTerms.Count == 0) ? terms : potentialTerms;
        }

        /// <summary>
        /// Limit the terms by the sections' academic level
        /// </summary>
        /// <param name="acadLevel">Academic level of section</param>
        /// <param name="terms">List of terms to narrow down</param>
        /// <returns>List of terms that match the academic level</returns>
        public static List<Term> LimitTermsByAcademicLevel(string acadLevel, List<Term> terms)
        {
            return terms.Where(x => x.AcademicLevels.Contains(acadLevel)).ToList();
        }

        /// <summary>
        /// Exclude reporting terms from the list of terms
        /// </summary>
        /// <param name="terms">List of potential terms</param>
        /// <param name="allTerms">List of all terms - used to identify reporting terms</param>
        /// <returns>List of potential terms with all reporting terms excluded</returns>
        public static List<Term> ExcludeReportingTerms(List<Term> terms, List<Term> allTerms)
        {
            // A term is a reporting term if it's used as a reporting term on another term
            var reportingTerms = allTerms.Where(x => !string.IsNullOrEmpty(x.ReportingTerm)).Select(x => x.ReportingTerm).Distinct().ToList();
            var potentialTerms = terms.Where(x => !reportingTerms.Contains(x.Code)).ToList();

            // If there are any potential terms, return them; otherwise, return the original list
            return (potentialTerms == null || potentialTerms.Count == 0) ? terms : potentialTerms;
        }

        /// <summary>
        /// Sort the terms based on their proximity to specified start and end dates
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="terms">End date</param>
        /// <returns></returns>
        public static List<Term> SortTerms(DateTime startDate, DateTime? endDate, List<Term> terms)
        {
            // If there aren't any terms, just return an empty list
            if (terms == null || terms.Count == 0)
            {
                return new List<Term>();
            }

            // To handle a best-fit scenario, we need to calculate the number of days between the section's
            // start and end dates and the term's start and end dates, respectively. We want to sort the
            // terms according to which terms have a start date closest to the section's start date and
            // likewise with the end dates. Then, we'll sort by the sequence number, just to break any ties.

            // Note that if the section has no end date, we'll treat that as a match on the end date, which
            // effectively makes the proximity of the start date the determining factor.
            return terms.OrderBy(x => Math.Abs((x.StartDate - startDate).Days) +
                (endDate.HasValue ? Math.Abs((x.EndDate - endDate.Value).Days) : 0))
                .ThenBy(x => Math.Abs((x.StartDate - startDate).Days))
                .ThenBy(x => endDate.HasValue ? Math.Abs((x.EndDate - endDate.Value).Days) : 0)
                .ThenBy(x => x.Sequence).ToList();
        }

        /// <summary>
        /// Combine the separate instructors by instruction method into the format Colleague expects
        /// </summary>
        /// <param name="section">Current section, including the new/updated meeting</param>
        /// <param name="meetingGuid">GUID of incoming section meeting for add or update</param>
        public static void UpdateSectionFacultyFromSectionMeetings(Section section, string meetingGuid)
        {
            var roster = new List<SectionFaculty>();

            // Find the matching (i.e. current) meeting
            var match = section.Meetings.FirstOrDefault(x => x.Guid == meetingGuid);
            // A non-match should be an error - trying to update a meeting that is not a child of the same section.
            if (match == null)
            {
                var ex = new RepositoryException("Invalid operation");
                ex.AddError(new RepositoryError("InstructionalEvent.Section.NotFound",
                    "Section meeting being updated is not linked to section " + section.Id));
                throw ex;
            }

            // Now, summarize the section faculty records by faculty ID and instruction method into
            // any existing SectionFaculty on the section itself.

            foreach (var meeting in section.Meetings)
            {
                foreach (var faculty in meeting.FacultyRoster)
                {
                    var current = section.Faculty.FirstOrDefault(x => x.FacultyId == faculty.FacultyId && x.InstructionalMethodCode == faculty.InstructionalMethodCode);
                    if (current == null)
                    {
                        // No match, so just add this record to the section
                        section.AddSectionFaculty(faculty);
                    }
                    else
                    {
                        var startDate = current.StartDate < faculty.StartDate ? current.StartDate : faculty.StartDate;
                        var endDate = current.EndDate > faculty.EndDate ? current.EndDate : faculty.EndDate;
                        var contractAssignment = faculty.ContractAssignment ?? current.ContractAssignment;
                        var load = current.LoadFactor + faculty.LoadFactor;
                        var teachArrangement = faculty.TeachingArrangementCode ?? current.TeachingArrangementCode;
                        var totalMinutes = current.TotalMeetingMinutes + faculty.TotalMeetingMinutes;
                        // Take the higher number for now, but these may be adjusted once we're done.
                        var responsibility = current.ResponsibilityPercentage > faculty.ResponsibilityPercentage ? current.ResponsibilityPercentage : faculty.ResponsibilityPercentage;
                        var sectionFaculty = new SectionFaculty(current.Id, section.Id, current.FacultyId, current.InstructionalMethodCode, startDate, endDate, responsibility)
                        {
                            ContractAssignment = contractAssignment,
                            LoadFactor = load,
                            TeachingArrangementCode = teachArrangement,
                            TotalMeetingMinutes = totalMinutes
                        };
                        section.RemoveSectionFaculty(current);
                        section.AddSectionFaculty(sectionFaculty);
                    }
                }
            }

            // Evaluate and potentially adjust the loads for on the section meetings for each instruction method
            foreach (var contact in section.InstructionalContacts)
            {
                if (contact.Load.HasValue && contact.Load.Value > 0)
                {
                    IList<SectionMeeting> meetings = section.Meetings.Where(m => m.InstructionalMethodCode == contact.InstructionalMethodCode).ToList();
                    if (meetings != null && meetings.Count() > 0)
                    {
                        AdjustMeetingLoads(meetings, contact.Load.Value);

                        // For each meeting, adjust the faculty percentage responsibility and load
                        foreach (var mtg in meetings)
                        {
                            if (mtg.FacultyRoster != null && mtg.FacultyRoster.Count() > 0)
                            {
                                AdjustFacultyPercentages(mtg.FacultyRoster, mtg.Load);
                                if (mtg.Load.HasValue && mtg.Load.Value > 0)
                                {
                                    AdjustFacultyLoads(mtg.FacultyRoster, mtg.Load.Value);
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                // Create a dictionary of the instructional methods and associated loads
                var sectionInstrMethodLoads = section.InstructionalContacts.ToDictionary(i => i.InstructionalMethodCode, i => i.Load.GetValueOrDefault());
                // Call domain service method to update the FacultyRoster MeetingLoadFactor for each Meeting
                CalculateMeetingLoadFactor(section.Meetings, sectionInstrMethodLoads);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.Info("The system was unable to calculate the meeting load factor for section " + section.Name + " " + section.Title + ": " + ex.Message);
                }
            }

            return;
        }

        public static void DeleteSectionFacultyFromSectionMeetings(Section section, string meetingId)
        {
            // Find the matching (i.e. current) meeting
            var match = section.Meetings.FirstOrDefault(x => x.Id == meetingId);
            // A non-match should be an error - trying to update a meeting that is not a child of the same section.
            if (match == null)
            {
                var ex = new RepositoryException("Invalid operation");
                ex.AddError(new RepositoryError("InstructionalEvent.Section.NotFound",
                    "Section meeting being deleted is not linked to section " + section.Id));
                throw ex;
            }
            var secFacultyToRemove = new List<SectionFaculty>();
            foreach (var faculty in section.Faculty)
            {
                // Look at each faculty entry to see if we have a matching instructional method
                if (match.InstructionalMethodCode != faculty.InstructionalMethodCode)
                {
                    secFacultyToRemove.Add(faculty);
                }
            }
            foreach (var faculty in secFacultyToRemove)
            {
                // Remove any faculty that do not belong to the same instructional method
                section.RemoveSectionFaculty(faculty);
            }
        }

        /// <summary>
        /// Calculate Load Factor for each meeting faculty
        /// </summary>
        /// <param name="section"></param>
        /// <param name="sectionInstrMethodLoads"></param>
        /// <param name="facultyInstrMethodLoads"></param>
        public static void CalculateMeetingLoadFactor(IEnumerable<SectionMeeting> meetings, Dictionary<string, decimal> sectionInstrMethodLoads)
        {
            // Process each meeting in turn, populating the meeting load for each faculty assigned to the meeting
            foreach (var meeting in meetings)
            {
                // Loop through each faculty in the meeting roster and calculate the meeting load factor
                foreach (var faculty in meeting.FacultyRoster)
                {
                    // Get or calculate the load factor for this faculty and instructional method. 
                    decimal facLoadFactor;
                    if (faculty.LoadFactor.HasValue)
                    {
                        // Use the entered Faculty Load Factor
                        facLoadFactor = faculty.LoadFactor.Value;
                    }
                    else
                    {
                        // Load factor not provided so multiply the total Instructional Method Load times the faculty's percentage of responsibility.
                        // section-wide load value for this instructional method
                        decimal instrMethodLoadValue = sectionInstrMethodLoads[meeting.InstructionalMethodCode];
                        facLoadFactor = faculty.ResponsibilityPercentage * instrMethodLoadValue / 100;
                    }

                    // Find all the meetings under this instructional method where this faculty is on the roster
                    var facMeetings = new List<string>();
                    foreach (var mtg in meetings.Where(m => m.InstructionalMethodCode == meeting.InstructionalMethodCode))
                    {
                        var facRoster = mtg.FacultyRoster.Where(fr => fr.FacultyId == faculty.FacultyId).FirstOrDefault();
                        if (facRoster != null)
                        {
                            facMeetings.Add(mtg.Id);
                        }
                    }

                    // Evenly divide the faculty load across all meetings for this faculty and instructional method
                    decimal facultyPerMeetingLoad = facMeetings.Count > 0 ? facLoadFactor / facMeetings.Count : 0;
                    // Try to calculate the faculty load for this meeting by calculating a relative weight for this meeting over all
                    // meetings for this faculty.
                    try
                    {
                        decimal numerator = meetings.Where(m => m.Id == meeting.Id).First().TotalMeetingMinutes;
                        decimal denominator = 0;
                        foreach (var mtg in facMeetings)
                        {
                            denominator += meetings.Where(m => m.Id == mtg).First().TotalMeetingMinutes;
                        }
                        decimal meetingLoadPercent = denominator == 0 ? 0 : numerator / denominator;
                        facultyPerMeetingLoad = facLoadFactor * meetingLoadPercent;
                        facultyPerMeetingLoad = Math.Round(facultyPerMeetingLoad, 2, MidpointRounding.ToEven);
                    }
                    catch (Exception ex)
                    {
                        // Could not calculate faculty per meeting load based on meeting hours
                        if (_logger != null)
                        {
                            string message = string.IsNullOrEmpty(meeting.Id) ?
                                "Unable to calculate faculty load factor for the new section meeting" :
                                "Unable to calculate faculty load factor for section meeting " + meeting.Id;
                            _logger.Error(message + "for section " + meeting.SectionId + ": " + ex.Message);
                        }
                    }

                    // Assign the faculty per meeting load. Or if zero, set to the per meeting load divided by the total number of faculty assigned to the meeting.
                    faculty.MeetingLoadFactor = facultyPerMeetingLoad;
                }
            }
        }

        /// <summary>
        /// Adjust the responsibility percentages on a set of SectionFaculty objects to equal a specified amount (usually 100%)
        /// </summary>
        /// <param name="faculty">List of SectionFaculty objects</param>
        /// <param name="respAmount">The total load amount for those meetings, defaults to 100</param>
        public static void AdjustFacultyPercentages(IList<SectionFaculty> faculty, decimal? loadAmount, decimal respAmount = 100m)
        {
            // Make sure we have all the arguments so we can process the data
            if (faculty == null || faculty.Count() == 0)
            {
                return;
            }
            if (respAmount <= 0 || respAmount > 100m)
            {
                return;
            }

            // Start by checking the responsbility on the faculty.  If the total of them equals the
            // provided amount and all of them have a responsibility percentage, then we're done.
            var facultyWithResp = faculty.Where(m => m.ResponsibilityPercentage > 0).ToList();
            decimal totalResp = facultyWithResp.Sum(m => m.ResponsibilityPercentage);
            if (totalResp == respAmount && facultyWithResp.Count == faculty.Count)
            {
                return;
            }

            // If all the faculty have a load value, and the total of those loads match the provided total,
            // then we will use the load values to calculate the responsibility percentages.
            if (loadAmount.HasValue && faculty.All(f => f.LoadFactor.HasValue && f.LoadFactor.Value > 0) && faculty.Sum(f => f.LoadFactor.Value) == loadAmount.Value)
            {
                for (int i = 0; i < faculty.Count; i++)
                {
                    faculty[i].ResponsibilityPercentage = Math.Round(faculty[i].LoadFactor.Value * 100 / loadAmount.Value, 2);
                }
                // Adjust the first one to make sure they equal the specified total
                decimal difference = respAmount - faculty.Sum(f => f.LoadFactor.Value);
                faculty[0].LoadFactor += difference;
            }

            // If the calculated total is less than the provided amount and all but one have a non-zero value,
            // then calculate its responsibility percentage as the difference and we're done
            if (totalResp < respAmount && (faculty.Count - facultyWithResp.Count) == 1)
            {
                int pos = faculty.IndexOf(faculty.First(m => m.ResponsibilityPercentage == 0));
                if (pos >= 0)
                {
                    faculty[pos].ResponsibilityPercentage = respAmount - totalResp;
                    return;
                }
            }

            // We will change the load on all the meetings, splitting them as evenly as we can; the load for the
            // first faculty is adjusted so the sum of all the percentages equals the specified total amount.
            decimal baseResp = Math.Round(respAmount / faculty.Count, 2);
            decimal leftover = respAmount - (baseResp * faculty.Count);
            for (int i = 0; i < faculty.Count; i++)
            {
                faculty[i].ResponsibilityPercentage = (i == 0) ? baseResp + leftover : baseResp;
            }
        }

        /// <summary>
        /// Method to adjust the faculty loads and responsibility percentages on a list of section faculty
        /// </summary>
        /// <param name="faculty">List of section faculty to adjust</param>
        /// <param name="loadAmount">The load amount that the list can total to</param>
        public static void AdjustFacultyLoads(IList<SectionFaculty> faculty, decimal loadAmount)
        {
            // Make sure we have all the arguments so we can process the data
            if (faculty == null || faculty.Count() == 0)
            {
                return;
            }
            if (loadAmount <= 0)
            {
                return;
            }

            // Start by checking the loads on the meetings.  If the total of the meeting loads equals
            // the provided amount and all faculty have a load, then we're done.
            var facultyWithLoad = faculty.Where(m => m.LoadFactor.HasValue && m.LoadFactor.Value > 0).ToList();
            decimal totalLoad = facultyWithLoad.Sum(m => m.LoadFactor.Value);
            if (totalLoad == loadAmount && facultyWithLoad.Count == faculty.Count)
            {
                return;
            }

            // If the calculated total is less than the provided amount and all but one have a load value,
            // then calculate its load and we're done
            if (totalLoad < loadAmount && (faculty.Count - facultyWithLoad.Count) == 1)
            {
                int pos = faculty.IndexOf(faculty.First(m => !m.LoadFactor.HasValue || m.LoadFactor.Value == 0));
                if (pos >= 0)
                {
                    faculty[pos].LoadFactor = loadAmount - totalLoad;
                    return;
                }
            }

            // We will change the load on all the meetings, splitting them as evenly as we can; the load for the
            // first faculty is adjusted so the sum of all the faculty loads equals the specified total amount.
            decimal baseLoad = Math.Round(loadAmount / faculty.Count, 2);
            decimal leftover = loadAmount - (baseLoad * faculty.Count);
            for (int i = 0; i < faculty.Count; i++)
            {
                faculty[i].LoadFactor = (i == 0) ? baseLoad + leftover : baseLoad;
            }
        }

        /// <summary>
        /// Adjust the loads on a set of SectionMeeting objects to match a specified total
        /// </summary>
        /// <param name="meetings">List of SectionMeeting objects</param>
        /// <param name="loadAmount">The total load amount for those meetings</param>
        public static void AdjustMeetingLoads(IList<SectionMeeting> meetings, decimal loadAmount)
        {
            // Make sure we have all the arguments so we can process the data
            if (meetings == null || meetings.Count == 0)
            {
                return;
            }
            if (loadAmount <= 0)
            {
                return;
            }

            // Start by checking the loads on the meetings.  If the total of the meeting loads equals the provided amount, then we're done.
            var meetingsWithLoad = meetings.Where(m => m.Load.HasValue && m.Load.Value > 0).ToList();
            decimal totalLoad = meetingsWithLoad.Sum(m => m.Load.Value);
            if (totalLoad == loadAmount)
            {
                return;
            }

            // If the calculated total is less than the provided amount and all meetings have a load, then we're done
            // (we have to assume that the missing load will come in separately)
            if (totalLoad < loadAmount && meetingsWithLoad.Count == meetings.Count)
            {
                return;
            }

            // If the calculated total is less than the provided amount and all but one have a load value,
            // then calculate its load and we're done
            if (totalLoad < loadAmount && (meetings.Count - meetingsWithLoad.Count) == 1)
            {
                int pos = meetings.IndexOf(meetings.First(m => !m.Load.HasValue || m.Load.Value == 0));
                if (pos >= 0)
                {
                    meetings[pos].Load = loadAmount - totalLoad;
                    return;
                }
            }

            // We will change the load on all the meetings, splitting them as evenly as we can; the load for the
            // first meeting is adjusted so the sum of all the meeting loads equals the specified total amount.
            decimal baseLoad = Math.Round(loadAmount / meetings.Count, 2);
            decimal leftover = loadAmount - (baseLoad * meetings.Count);
            for (int i = 0; i < meetings.Count; i++)
            {
                meetings[i].Load = (i == 0) ? baseLoad + leftover : baseLoad;
            }
        }

        /// <summary>
        /// Determine the requisites that are currently in effect for the given section based on a combination of the requisites and flags
        /// set in both the course and the section. Both course and section are required by this method.
        /// </summary>
        /// <param name="section">Section to determine the list of requisites for. Required</param>
        /// <param name="course">Course that may contain the base list of section requisites. Required</param>
        /// <returns>List of <see cref="Requisite">Requisites</see> that are in effect for the given Section</returns>
        public static IEnumerable<Requisite> DetermineEffectiveSectionRequisites(Section section, Course course)
        {
            List<Requisite> effectiveRequisites = new List<Requisite>();

            if (section == null)
            {
                throw new ArgumentException("section", "Section must be provided to accurately determine requisites in effect for the section.");
            }

            if (course == null)
            {
                throw new ArgumentException("course", "Course must be provided to accurately determine requisites in effect for the section.");
            }

            // If neither the section nor the course has Requisites, return an empty list.
            if ((section.Requisites == null || section.Requisites.Count() == 0) && (course.Requisites == null || course.Requisites.Count() == 0))
            {
                return effectiveRequisites;
            }

            // Always return any protected requisites from the course
            effectiveRequisites.AddRange(course.Requisites.Where(r => r.IsProtected));

            // If the section overrides the course requisities, then add the section requisites to that list.
            if (section.OverridesCourseRequisites == true)
            {
                effectiveRequisites.AddRange(section.Requisites);
            }
            else
            {
                // The section does not override the course requisites, therefore add the unprotected course requisites to the list
                effectiveRequisites.AddRange(course.Requisites.Where(r => !r.IsProtected));
            }

            return effectiveRequisites;
        }

        /// <summary>
        /// For the given Section, determine the requisites that can be waived.
        /// </summary>
        /// <param name="section">Section to determine the list of requisites for. Required</param>
        /// <param name="course">Course that may contain the base list of section requisites. Required</param>
        /// <returns></returns>
        public static IEnumerable<Requisite> DetermineWaiverableRequisites(Section section, Course course)
        {
            IEnumerable<Requisite> effectiveRequisites = DetermineEffectiveSectionRequisites(section, course);

            return effectiveRequisites
                .Where(er => er.IsRequired && (er.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous || er.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.PreviousOrConcurrent)).ToList();
        }

        /// <summary>
        /// Returns registration date information for specific sections. 
        /// </summary>
        /// <param name="registrationGroup">The registration Group</param>
        /// <param name="sections">Sections for which registration group section date overrides are requested. </param>
        /// <param name="registrationTerms">All registration term entities.</param>
        /// <param name="considerUsersGroup">This is set to true if the registration group should be considered for dates calculation, otherwise set to false</param>
        /// <returns>Applicable SectionRegistrationDates entities for the provided sections</returns>
        public static IEnumerable<SectionRegistrationDate> GetSectionRegistrationDates(RegistrationGroup registrationGroup, IEnumerable<Section> sections, IEnumerable<Term> registrationTerms, bool considerUsersGroup = true)
        {
            if (registrationGroup == null)
            {
                throw new ArgumentNullException("registrationGroup", "Must provide a registration Group.");
            }
            if (sections == null || sections.Count() == 0)
            {
                throw new ArgumentNullException("sections", "At least one Section is required.");
            }
            if (registrationTerms == null)
            {
                registrationTerms = new List<Term>();
            }
            var allSectionIds = sections.Select(s => s.Id).ToList();
            _logger.Info(string.Format("Getting section registration dates for sections {0}", string.Join(", ", allSectionIds)));
            List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate> sectionRegistrationDatesEntities = new List<Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationDate>();

            // Limit the sections processed to those that have a term within the current registration terms or that have no term.
            _logger.Info(string.Format("Section registration dates will only be retrieved for non-term sections and sections in registration terms {0}", string.Join(", ", registrationTerms.Select(t => t.Code))));
            var filteredSections = (from t in registrationTerms
                                    join sec in sections
                                    on t.Code equals sec.TermId
                                    select sec).ToList();
            var filteredTermSectionIds = filteredSections.Select(s => s.Id).ToList();
            _logger.Info(string.Format("Sections in registration terms: {0}", string.Join(", ", filteredTermSectionIds)));
            var nonTermSections = sections.Where(s => string.IsNullOrEmpty(s.TermId)).ToList();
            var filteredNonTermSectionIds = nonTermSections.Select(s => s.Id).ToList();
            _logger.Info(string.Format("Non-term sections: {0}", string.Join(", ", filteredNonTermSectionIds)));

            // Combine non-term and registration term sections for processing
            filteredSections.Intersect(nonTermSections);

            // Identify any sections excluded from processing and log
            var notProcessedIds = allSectionIds.Except(filteredSections.Select(fs => fs.Id));
            if (notProcessedIds.Any())
            {
                _logger.Info(string.Format("The following sections will not be processed for registration dates because they are assigned to a non-registration term: {0}", string.Join(Environment.NewLine, notProcessedIds)));

            }
            foreach (var section in filteredSections)
            {
                //if consider users group is false don't consider this
                if (considerUsersGroup)
                {
                    // First see if there is a registration User Section for this combination.
                    var sectionOverride = registrationGroup.SectionRegistrationDates.Where(s => s.SectionId == section.Id).FirstOrDefault();
                    if (sectionOverride != null)
                    {
                        _logger.Debug(string.Format("Section {0} registration dates determined by section override for registration user (RGUS > RGUD > RGUC) for registration user {1}.", section.Id, registrationGroup.Id));
                        sectionRegistrationDatesEntities.Add(sectionOverride);
                        continue;
                    }
                }

                // Next try the section itself 
                if (section.RegistrationDateOverrides != null)
                {
                    _logger.Debug(string.Format("Section {0} registration dates determined by section override (SRGD).", section.Id));
                    sectionRegistrationDatesEntities.Add(new SectionRegistrationDate(section.Id,
                        section.Location,
                        section.RegistrationDateOverrides.RegistrationStartDate,
                        section.RegistrationDateOverrides.RegistrationEndDate,
                        section.RegistrationDateOverrides.PreRegistrationStartDate,
                        section.RegistrationDateOverrides.PreRegistrationEndDate,
                        section.RegistrationDateOverrides.AddStartDate,
                        section.RegistrationDateOverrides.AddEndDate,
                        section.RegistrationDateOverrides.DropStartDate,
                        section.RegistrationDateOverrides.DropEndDate,
                        section.RegistrationDateOverrides.DropGradeRequiredDate,
                        section.RegistrationDateOverrides.CensusDates));
                    continue;
                }

                if (!string.IsNullOrEmpty(section.TermId))
                {
                    var sectionTerm = registrationTerms.Where(t => t.Code == section.TermId).FirstOrDefault();


                    // Next, if the section has a location check the RegUserTermLocs file. (REGUSER+TERM+LOCATION)
                    if (!string.IsNullOrEmpty(section.Location))
                    {
                        //if consider users group is false don't consider this
                        if (considerUsersGroup)
                        {
                            var termLocationDates = registrationGroup.TermLocationRegistrationDates.Where(t => t.TermId == section.TermId && t.Location == section.Location).FirstOrDefault();
                            if (termLocationDates != null)
                            {
                                _logger.Debug(string.Format("Section {0} registration dates determined by term/location override (RGUS > RGUD > RGUL) for registration user {1}, term {2}, location {3}.", section.Id, registrationGroup.Id, section.TermId, section.Location));
                                var sectionRegistrationDates = new SectionRegistrationDate(section.Id, section.Location, termLocationDates.RegistrationStartDate, termLocationDates.RegistrationEndDate, termLocationDates.PreRegistrationStartDate, termLocationDates.PreRegistrationEndDate, termLocationDates.AddStartDate, termLocationDates.AddEndDate, termLocationDates.DropStartDate, termLocationDates.DropEndDate, termLocationDates.DropGradeRequiredDate, termLocationDates.CensusDates);
                                sectionRegistrationDatesEntities.Add(sectionRegistrationDates);
                                continue;
                            }
                        }

                        // Next, see if there is a Term RegistrationDate item available with this section's particular location. (TERM+LOCATION)
                        if (sectionTerm != null && sectionTerm.RegistrationDates != null)
                        {
                            var termLocation = sectionTerm.RegistrationDates.Where(r => r.Location == section.Location).FirstOrDefault();
                            if (termLocation != null)
                            {
                                _logger.Debug(string.Format("Section {0} registration dates determined by term/location override (RYAT > ACTM > TLOC) for term {1}, location {2}.", section.Id, section.TermId, section.Location));
                                sectionRegistrationDatesEntities.Add(new SectionRegistrationDate(
                                    section.Id,
                                    section.Location,
                                    termLocation.RegistrationStartDate,
                                    termLocation.RegistrationEndDate,
                                    termLocation.PreRegistrationStartDate,
                                    termLocation.PreRegistrationEndDate,
                                    termLocation.AddStartDate,
                                    termLocation.AddEndDate,
                                    termLocation.DropStartDate,
                                    termLocation.DropEndDate,
                                    termLocation.DropGradeRequiredDate,
                                    termLocation.CensusDates
                                    ));
                                continue;
                            }
                        }
                    }
                    //if consider users group is false don't consider this
                    if (considerUsersGroup)
                    {
                        // Next see if there is an item in RegUserTerms for this term and user. (REGUSER+TERM)
                        var termDates = registrationGroup.TermRegistrationDates.Where(t => t.TermId == section.TermId).FirstOrDefault();
                        if (termDates != null)
                        {
                            _logger.Debug(string.Format("Section {0} registration dates determined by term override (RGUS > RGUD > RGUT) for registration user {1}, term {2}.", section.Id, registrationGroup.Id, section.TermId));
                            sectionRegistrationDatesEntities.Add(new SectionRegistrationDate(section.Id, section.Location, termDates.RegistrationStartDate, termDates.RegistrationEndDate, termDates.PreRegistrationStartDate, termDates.PreRegistrationEndDate, termDates.AddStartDate, termDates.AddEndDate, termDates.DropStartDate, termDates.DropEndDate, termDates.DropGradeRequiredDate, termDates.CensusDates));
                            continue;
                        }
                    }

                    // Last, just use the term's registration dates for this term (TERM) with no location.
                    if (sectionTerm != null && sectionTerm.RegistrationDates != null)
                    {
                        var termRegDates = sectionTerm.RegistrationDates.Where(r => string.IsNullOrEmpty(r.Location)).FirstOrDefault();
                        if (termRegDates != null)
                        {
                            _logger.Debug(string.Format("Section {0} registration dates determined by term (ACTM) for term {1}.", section.Id, section.TermId));
                            sectionRegistrationDatesEntities.Add(new SectionRegistrationDate(
                                section.Id,
                                section.Location,
                                termRegDates.RegistrationStartDate,
                                termRegDates.RegistrationEndDate,
                                termRegDates.PreRegistrationStartDate,
                                termRegDates.PreRegistrationEndDate,
                                termRegDates.AddStartDate,
                                termRegDates.AddEndDate,
                                termRegDates.DropStartDate,
                                termRegDates.DropEndDate,
                                termRegDates.DropGradeRequiredDate,
                                termRegDates.CensusDates
                                ));
                            continue;
                        }
                    }
                }
                // If we haven't found registration date information for this section in any of the above tests we will not return an item for this section. 
                _logger.Error(string.Format("Registration dates could not be determined for section {0}.", section.Id));
            }
            return sectionRegistrationDatesEntities;
        }

        /// <summary>
        /// Returns census dates for specific sections. 
        /// </summary>
        /// <param name="sections">Sections for which census dates are requested.</param>
        /// <param name="terms">list of terms for sections or all terms</param>
        /// <returns>Applicable Section census dates for the provided sections</returns>
        public static Dictionary<string, List<DateTime?>> GetSectionCensusDates(IEnumerable<Section> sections, IEnumerable<Term> terms)
        {
            if (sections == null || sections.Count() == 0)
            {
                throw new ArgumentNullException("sections", "At least one Section is required.");
            }
            if (terms == null)
            {
                terms = new List<Term>();
            }

            var sectionCensusDates = new Dictionary<string, List<DateTime?>>();

            var allSectionIds = sections.Select(s => s.Id).ToList();
            _logger.Debug(string.Format("Getting section dates for sections {0}", string.Join(", ", allSectionIds)));


            // Limit the sections processed to those that have no term or a term in the terms list.
            _logger.Debug(string.Format("Section dates will only be retrieved for non-term sections and sections in following terms {0}", string.Join(", ", terms.Select(t => t.Code))));

            var filteredTermSections = (from t in terms
                                    join sec in sections
                                    on t.Code equals sec.TermId
                                    select sec).ToList();
            var filteredTermSectionIds = filteredTermSections.Select(s => s.Id).ToList();
            _logger.Debug(string.Format("Sections in terms: {0}", string.Join(", ", filteredTermSectionIds)));

            //get sections without a term
            var nonTermSections = sections.Where(s => string.IsNullOrEmpty(s.TermId)).ToList();
            var filteredNonTermSectionIds = nonTermSections.Select(s => s.Id).ToList();
            _logger.Debug(string.Format("Non-term sections: {0}", string.Join(", ", filteredNonTermSectionIds)));

            // Combine non-term and filtered term sections for processing
            var filteredSections = filteredTermSections.Union(nonTermSections);

            // Identify any sections excluded from processing and log
            var notProcessedIds = allSectionIds.Except(filteredSections.Select(fs => fs.Id));
            if (notProcessedIds.Any())
            {
                _logger.Debug(string.Format("Sections with an assigned term not contained in terms parameter that have been excluded from processing: {0}", string.Join(Environment.NewLine, notProcessedIds)));
            }

            //Determine the applicable section census dates based on SRGD > TLOC > ACTM
            foreach (var section in filteredSections)
            {
                //First, check for section override dates on the section (SRGD)
                if (section.RegistrationDateOverrides != null)
                {
                    _logger.Debug(string.Format("Section {0} census dates determined by section override (SRGD).", section.Id));

                    if (!sectionCensusDates.ContainsKey(section.Id))
                    {
                        sectionCensusDates.Add(section.Id, section.RegistrationDateOverrides.CensusDates);
                    }
                    continue;
                }

                // Next, see if there is a Term RegistrationDate item available with this section's particular location. (TERM + LOCATION)
                if (!string.IsNullOrEmpty(section.TermId))
                {
                    var sectionTerm = terms.Where(t => t.Code == section.TermId).FirstOrDefault();

                    if (!string.IsNullOrEmpty(section.Location))
                    {
                        if (sectionTerm != null && sectionTerm.RegistrationDates != null)
                        {
                            var termLocation = sectionTerm.RegistrationDates.Where(r => r.Location == section.Location).FirstOrDefault();
                            if (termLocation != null)
                            {
                                _logger.Debug(string.Format("Section {0} census dates determined by term/location override (RYAT > ACTM > TLOC) for term {1}, location {2}.", section.Id, section.TermId, section.Location));

                                if (!sectionCensusDates.ContainsKey(section.Id))
                                {
                                    sectionCensusDates.Add(section.Id, termLocation.CensusDates);
                                }
                                continue;
                            }
                        }
                    }

                    // Last, just use the term's dates for this term (TERM) with no location RYAT > ACTM.
                    if (sectionTerm != null && sectionTerm.RegistrationDates != null)
                    {
                        var termRegDates = sectionTerm.RegistrationDates.Where(r => string.IsNullOrEmpty(r.Location)).FirstOrDefault();
                        if (termRegDates != null)
                        {
                            _logger.Debug(string.Format("Section {0} census dates determined by term (ACTM) for term {1}.", section.Id, section.TermId));

                            if (!sectionCensusDates.ContainsKey(section.Id))
                            {
                                sectionCensusDates.Add(section.Id, termRegDates.CensusDates);
                            }
                        }
                    }
                }
            }
            return sectionCensusDates;
        }
    }
}