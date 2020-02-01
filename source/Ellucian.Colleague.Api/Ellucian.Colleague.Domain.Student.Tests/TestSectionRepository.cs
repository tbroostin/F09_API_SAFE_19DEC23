// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestSectionRepository : ISectionRepository
    {
        public string frequency = "W";

        public async Task<IEnumerable<Section>> GetRegistrationSectionsAsync(IEnumerable<Term> terms)
        {
            // For now just return them all.  Terms not relevant.
            return await GetAsync();
        }
        public Task<IEnumerable<Section>> GetCourseSectionsCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> terms)
        {
            throw new NotImplementedException();
        }
        public Task<Dictionary<string, SectionSeats>> GetSectionsSeatsAsync(IEnumerable<string> sectionIds)
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<Section>> GetCourseSectionsNonCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> terms)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Section>> GetNonCachedSectionsAsync(IEnumerable<string> ids, bool bestFit = false)
        {
            var sections = await BuildSectionsAsync();

            var secsToReturn = new List<Section>();
            foreach (var id in ids)
            {
                secsToReturn.Add(sections.Where(s => s.Id == id).First());
            }
            return secsToReturn;
        }

        public Task<IEnumerable<Section>> GetNonCachedFacultySectionsAsync(IEnumerable<Term> terms, string facultyId, bool bestFit = true)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Section>> GetCachedSectionsAsync(IEnumerable<string> ids, bool bestFit = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Section>> GetAsync()
        {
            return await BuildSectionsAsync();
        }


        public async Task<ICollection<Section>> BuildSectionsWithPrimaryScetionMeetingsAsync()
        {

            List<Section> sections = new List<Section>();
            var courses = await new TestCourseRepository().GetAsync();
            Term term = await new TestTermRepository().GetAsync("2012/FA");
            var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) };
            var course1 = courses.Where(c => c.Id == "139").First();
            var course2 = courses.Where(c => c.Id == "42").First();

            //1st section
            Section sec1 = new Section("999-SEC-WITH-PRIM-MTNGS", course1.Id, "01", term.StartDate, 3m, null, course1.SubjectCode + " " + "01", "IN", course1.Departments, course1.CourseLevelCodes, "A1", statuses);
            //set meeting times on section
            String guid = Guid.NewGuid().ToString();
            SectionMeeting mp1 = new SectionMeeting("999-SEC-MTNG-1", "999-SEC-WITH-PRIM-MTNGS", "LEC", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 09, 10, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 10, 00, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday }
            };

            SectionMeeting mp2 = new SectionMeeting("999-SEC-MTNG-2", "999-SEC-WITH-PRIM-MTNGS", "LAB", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 09, 10, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 09, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Tuesday }
            };
            sec1.UpdatePrimarySectionMeetings(new List<SectionMeeting>() { mp1, mp2 });
            sec1.FirstMeetingDate = term.StartDate.AddDays(15);
            sec1.LastMeetingDate = sec1.EndDate;

            //2nd section

            Section sec2 = new Section("998-SEC-WITH-PRIM-MTNGS", course1.Id, "01", term.StartDate, 3m, null, course1.SubjectCode + " " + "01", "IN", course1.Departments, course1.CourseLevelCodes, "A1", statuses);
            //set meeting times on section
            guid = Guid.NewGuid().ToString();
            SectionMeeting mp3 = new SectionMeeting("998-SEC-MTNG-1", "998-SEC-WITH-PRIM-MTNGS", "LEC", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 13, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 13, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }
            };


            sec2.UpdatePrimarySectionMeetings(new List<SectionMeeting>() { mp3 });
            sec2.FirstMeetingDate = term.StartDate.AddDays(15);
            sec2.LastMeetingDate = sec2.EndDate;

            //3rd section

            Section sec3 = new Section("997-SEC-WITH-PRIM-MTNGS", course2.Id, "01", term.StartDate, 3m, null, course2.SubjectCode + " " + "01", "IN", course2.Departments, course2.CourseLevelCodes, "A1", statuses);
            //set meeting times on section
            guid = Guid.NewGuid().ToString();
            SectionMeeting mp4 = new SectionMeeting("997-SEC-MTNG-1", "997-SEC-WITH-PRIM-MTNGS", "LEC", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 11, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 11, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Wednesday, DayOfWeek.Friday }
            };


            sec3.UpdatePrimarySectionMeetings(new List<SectionMeeting>() { mp4 });
            sec3.FirstMeetingDate = term.StartDate.AddDays(15);
            sec3.LastMeetingDate = sec3.EndDate;

            sections.Add(sec1);
            sections.Add(sec2);
            sections.Add(sec3);
            return sections;

        }

        private DateTime ChangedRegistrationSectionsCacheBuildTime = new DateTime();
        public DateTime GetChangedRegistrationSectionsCacheBuildTime()
        {
            return ChangedRegistrationSectionsCacheBuildTime;
        }

        public Task<IEnumerable<SectionGradeResponse>> ImportGradesAsync(SectionGrades sectionGrades, bool forceNoVerifyFlag, bool checkForLocksFlag, GradesPutCallerTypes callerType)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetIdFromGuidAsync(string guid)
        {
            var section = (await GetAsync()).FirstOrDefault(x => x.Guid == guid);
            return section == null ? null : section.Id;
        }

        public async Task<Section> GetSectionAsync(string id)
        {
            return (await GetAsync()).FirstOrDefault(x => x.Id == id);
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "",
            string academicLevel = "", string course = "", string location = "", string status = "", string department = "", string subject = "", string instructor = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchableAsync(int offset, int limit, string searchable = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeywordAsync(int offset, int limit, string keyword = "", bool bypassCache = false, bool caseSensitive = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> ConvertStatusToStatusCodeAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<string> ConvertStatusToStatusCodeNoDefaultAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUnidataFormattedDate(string date)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCourseIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public async Task<Section> GetSectionByGuidAsync(string guid)
        {
            return (await GetAsync()).FirstOrDefault(x => x.Guid == guid);
        }

        public async Task<Section> GetSectionByGuid2Async(string guid, bool addToErrorCollection = false)
        {
            return (await GetAsync()).FirstOrDefault(x => x.Guid == guid);
        }

        public Task<Section> PostSectionAsync(Section section)
        {
            throw new NotImplementedException();
        }

        public Task<Section> PutSectionAsync(Section section)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRoster> GetSectionRosterAsync(string sectionId)
        {
            throw new NotImplementedException();
        }

        private async Task<ICollection<Section>> BuildSectionsAsync()
        {
            // Always build all sections for all terms
            ICollection<Section> sections = new List<Section>();
            var termCodes = new List<string>() { "2012/FA", "2013/SP", "2014/FA" };

            // Section Ids cannot be duplicated, so start the counter before looping through terms
            int sectionId = 0;
            var statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddDays(-100)) };

            foreach (var code in termCodes)
            {
                Term term = await new TestTermRepository().GetAsync(code);
                var courses = await new TestCourseRepository().GetAsync();
                //var meetingTimes = BuildMeetingTimes();
                var faculty = await new TestFacultyRepository().GetAllAsync();
                var students = await new TestStudentRepository().GetAllAsync();

                // Sections are auto-generated from the TestCourseRepository. For the given term
                // three sections will be generated with section numbers 01, 02, 03, 

                // Add corresponding meeting Time:
                //   01  LEC MW 9-9:50
                //   02  LEC TTh 1-1:50
                //   03  LEC WWF 11-11:50 
                foreach (Course course in courses)
                {
                    var depts = new List<OfferingDepartment>();
                    foreach (var dept in course.DepartmentCodes)
                    {
                        depts.Add(new OfferingDepartment(dept, 100m / course.DepartmentCodes.Count));
                    }

                    // Create special case where course title doesn't match any of the section titles
                    var title = course.Title;
                    if (course.Id == "180")
                    {
                        title = "BlahBlahBlah";
                    }
                    // Loop to create 3 sections in this term for each course
                    for (int i = 0; i < 3; i++)
                    {
                        List<Requisite> requisites;
                        List<SectionRequisite> sectionRequisites;
                        // Increment for unique section Id
                        sectionId++;
                        // Generally, start date comes from term start date
                        var startDate = term.StartDate;
                        // Change start date for a few tests
                        // Construct section, use mostly course attributes
                        var secNo = (i + 1).ToString("00");
                        // Vary some of the section's academic levels to allow testing of the academic level filter in CourseServiceTests
                        var specialCourseList = new List<string>() { "7701", "7703", "7704" };
                        Section sec;
                        if (specialCourseList.Contains(course.Id))
                        {
                            sec = new Section(sectionId.ToString(), course.Id, secNo, startDate, course.MinimumCredits, course.Ceus, title + " " + secNo, "IN", depts, course.CourseLevelCodes.ToList(), "A1", statuses);
                        }
                        else
                        {
                            sec = new Section(sectionId.ToString(), course.Id, secNo, startDate, course.MinimumCredits, course.Ceus, title + " " + secNo, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        }

                        sec.Guid = Guid.NewGuid().ToString();
                        sec.TermId = code;
                        sec.EndDate = term.EndDate;
                        sec.AddInstructionalMethod("LEC");
                        var meetingTimes = BuildMeetingTimes(sectionId.ToString(), sec.StartDate, sec.EndDate);
                        // Set up a single case where the section dates match a registration term, but the section is in a different term.
                        if (course.Id == "7706" && sec.Number == "03")
                        {
                            sec.TermId = "2012/S1";
                        }
                        try
                        {
                            // Add a meeting Time for each section generated, only for courses with an even ID
                            if (Int16.Parse(course.Id) % 2 == 0)
                            {
                                // Add the meeting that corresponds to the section number (see BuildMeetingTimes)
                                sec.AddSectionMeeting(meetingTimes.ElementAt(i));

                                // For "03" sections, also add the extra ONL meeting to create a "hybrid" section
                                if (sec.Number == "03") sec.AddSectionMeeting(meetingTimes.ElementAt(3));

                                // Add a course type
                                sec.AddCourseType("STND");
                                sec.AddCourseType("WR");

                            }
                        }
                        catch
                        {
                            // don't do anything if not numeric Id
                        }
                        // Set section location for testing
                        if (course.LocationCodes.Count() > 0)
                        {
                            // Sections with location codes not present in course.
                            // If course location code is "SC", give section the location "NW" (to have a section with a loc different from course)
                            if (course.LocationCodes.ElementAt(0) == "SC")
                            {
                                sec.Location = "NW";
                            }
                            else
                            {
                                if (course.LocationCodes.ElementAt(0) != "NW")
                                {
                                    // Course with a location code NW is not present in the section
                                    // If course has location codes (other than NW), set section location to first course location code
                                    sec.Location = course.LocationCodes.ElementAt(0);
                                }
                            }
                        }
                        else
                        {
                            // If course has no location codes, set location to NW (Northwest Campus) for sections
                            // with mod 5 Id (for randomness)
                            if (Int16.Parse(sec.Id) % 5 == 0)
                            {
                                sec.Location = "NW";
                            }
                        }
                        //if section are null then assign empty value to few of them
                        if (sec.Location == null)
                        {
                            if (Int16.Parse(sec.Id) % 6 == 0)
                            {
                                sec.Location = string.Empty;
                            }
                        }

                        // Set section topic code for testing
                        if (!string.IsNullOrEmpty(course.TopicCode))
                        {
                            // Sections a different topic code than course
                            // If course topic code is "T1", give section the topic code "T2" (to have a section with a loc different from course)
                            if (course.TopicCode == "T2")
                            {
                                sec.TopicCode = "T3";
                            }
                            else
                            {
                                // If course has any other topic code, set random sections to have a topic code of "T2" 
                                // with mod 5 Id (for randomness)
                                if (Int16.Parse(sec.Id) % 5 == 0)
                                {
                                    sec.TopicCode = "T2";
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(course.TopicCode))
                        {
                            // Courses with no topic code, set section topic code to "T3"
                            sec.TopicCode = "T3";
                        }




                        // Add two faculty per section
                        sec.AddFaculty(faculty.ElementAt(i).Id);
                        sec.AddFaculty(faculty.ElementAt(i + 4).Id);

                        // add some students and capacity
                        if (Int16.Parse(sec.Id) % 2 == 0)
                        {
                            sec.AddActiveStudent(students.ElementAt(0).Id);
                        }
                        else
                        {
                            sec.SectionCapacity = 10;
                            sec.AddActiveStudent(students.ElementAt(1).Id);
                            sec.AddActiveStudent(students.ElementAt(2).Id);
                        }

                        // COREQ testing setup: Add two term-based section-level course corequisites
                        // Book testing setup: Adding in some book information
                        // Caution: Degree plan testing HEAVILY depends upon these coreq setups and book setups
                        if (sec.CourseId == "7702")
                        {
                            // Each section of this course has these course coreqs
                            requisites = sec.Requisites.ToList();
                            requisites.Add(new Requisite("42", true)); // required coreq course
                            requisites.Add(new Requisite("21", false)); // recommended coreq course
                            sec.Requisites = requisites;
                            sec.OverridesCourseRequisites = true;

                            // Each section of this course has these Books
                            sec.AddBook("111", "R", true);
                            sec.AddBook("222", "O", false);
                            sec.GlobalCapacity = 15;

                        }
                        // Coreq test setup: Add two term-based section-level course and section corequisites
                        if (sec.CourseId == "7703")
                        {
                            // Each section of this course has these course AND section coreqs
                            requisites = sec.Requisites.ToList();
                            requisites.Add(new Requisite("42", true)); // Required course requisite
                            requisites.Add(new Requisite("21", false)); // Recommended course requisite
                            sec.Requisites = requisites;

                            // Get IDs of sections for specific course and term
                            var sec1 = sections.Where(s => s.CourseId == "91" && s.TermId == term.Code && s.Number == "02").First(); // MATH-400-02 (RECOMMENDED)
                            var sec2 = sections.Where(s => s.CourseId == "159" && s.TermId == term.Code && s.Number == "01").First(); // SOCI-100-01 (RECOMMENDED)
                            var sec3 = sections.Where(s => s.CourseId == "87" && s.TermId == term.Code && s.Number == "02").First(); // HIST-400-02
                            var sec4 = sections.Where(s => s.CourseId == "154" && s.TermId == term.Code && s.Number == "01").First(); // PHYS-100-01
                            var sec5 = sections.Where(s => s.CourseId == "333" && s.TermId == term.Code && s.Number == "03").First(); // MATH-152-03

                            sectionRequisites = sec.SectionRequisites.ToList();
                            sectionRequisites.Add(new SectionRequisite(sec1.Id)); // Recommended section requisite
                            sectionRequisites.Add(new SectionRequisite(sec2.Id)); // Recommendedsection requisite
                            sectionRequisites.Add(new SectionRequisite(new List<string>() { sec3.Id, sec4.Id, sec5.Id }, 2)); // Required multi-section requisites, 2 of 3 required
                            sec.SectionRequisites = sectionRequisites;
                        }
                        // Coreq test setup: Add a course and section coreq to a non-term section.
                        if (sec.CourseId == "7705")
                        {
                            // Set nonterm section as a coreq on 2013/SP term sections for course 7705
                            var sec1 = sections.Where(s => s.CourseId == "7704" && s.Number == "04").First();
                            sectionRequisites = sec.SectionRequisites.ToList();
                            sectionRequisites.Add(new SectionRequisite(sec1.Id, true)); // Required section requisite
                            sec.SectionRequisites = sectionRequisites;

                            // Add a required course requisite
                            requisites = sec.Requisites.ToList();
                            requisites.Add(new Requisite("7701", true)); // Required course requisite
                            sec.Requisites = requisites;
                            // Each section of this course has these Books
                            sec.AddBook("113", null, true);
                            sec.AddBook("223", "", false);
                        }
                        sec.FirstMeetingDate = sec.StartDate;
                        sec.LastMeetingDate = sec.EndDate;

                        // Attendance Tracking Type setup: add a section that does not use the default attendance tracking type
                        if (sec.CourseId == "7434")
                        {
                            sec.AttendanceTrackingType = AttendanceTrackingType.HoursByDateWithoutSectionMeeting;
                        }
                        // Add section to the returned list
                        sections.Add(sec);
                    }

                    // Coreq test setup: Add NonTerm Section for course 7704, with two course coreqs, two section coreqs
                    // Note: term code used in the setup of nonterm sections so that only one nonterm section will be build.
                    if (course.Id == "7704" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(60), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Add term section corequisite
                        // Each section of this course has these course AND section coreqs
                        // Get IDs of sections for specific course and term
                        var sec1 = sections.Where(s => s.CourseId == "87" && s.TermId == term.Code && s.Number == "02").First();
                        var sec2 = sections.Where(s => s.CourseId == "91" && s.TermId == term.Code && s.Number == "02").First();

                        // Convert the above coreqs to the new requisites format
                        var requisites = new List<Requisite>();
                        requisites.Add(new Requisite("42", true)); // Required courseRequisite
                        requisites.Add(new Requisite("21", false)); // Recommended courseRequisite
                        nonTermSec.Requisites = requisites;

                        var nonTermSectionRequisites = new List<SectionRequisite>();
                        nonTermSectionRequisites.Add(new SectionRequisite(sec1.Id, true)); // Required SectionCorequisite
                        nonTermSectionRequisites.Add(new SectionRequisite(sec2.Id)); // Recommended SectionCorequisite
                        nonTermSec.SectionRequisites = nonTermSectionRequisites;
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        nonTermSec.Synonym = "7966696";
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    // Coreq test setup: Add NonTerm Sections for course 7701, no coreqs. 
                    if (course.Id == "7701" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(-60), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    // Coreq test setup: Add NonTerm Sections for course 7701, no coreqs. 
                    if (course.Id == "333" && term.Code == termCodes.ElementAt(1))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "NT", term.StartDate.AddDays(30), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.EndDate = term.StartDate.AddDays(60);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    // Coreq test setup: Add NonTerm Course/Sections for course 7701, no coreqs.
                    // Set up with later date, but that doesn't matter for section-based coreqs.
                    // Check when verifying nonterm section ==> nonterm section coreq checking
                    if (course.Id == "7701" && term.Code == termCodes.ElementAt(1))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate.AddDays(30), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    // Coreq test setup: Add NonTerm Course/Section for course 7706, with nonterm section coreq 7701 04
                    // ending on a specified date.
                    if (course.Id == "7706" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Find the nonterm section for course 7701, add it as a section coreq to 7706
                        var sec1 = sections.Where(s => s.CourseId == "7701" && s.Number == "04").First();
                        nonTermSec.EndDate = nonTermSec.StartDate.AddDays(30);

                        nonTermSec.SectionRequisites = new List<SectionRequisite>() {
                            new SectionRequisite(sec1.Id, true), // Single required requisite section
                        };
                        nonTermSec.Requisites = new List<Requisite>()
                        {
                            new Requisite("PREREQ1", true, RequisiteCompletionOrder.Previous, false), // an override requisite
                            new Requisite("COREQ2", false, RequisiteCompletionOrder.Concurrent, false), // an override requisite
                            new Requisite("REQ1", true, RequisiteCompletionOrder.PreviousOrConcurrent, false) // an override requisite
                        };
                        nonTermSec.OverridesCourseRequisites = true;
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    // Coreq test setup: Add NonTerm Course/Section for course 7706, with nonterm section coreq 7701 05
                    if (course.Id == "7706" && term.Code == termCodes.ElementAt(1))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Find the nonterm section for course 7701, add it as a section coreq to 7706
                        var sec1 = sections.Where(s => s.CourseId == "7701" && s.Number == "05").First();
                        nonTermSec.Requisites = new List<Requisite>() {
                            new Requisite("PREREQ1", false, RequisiteCompletionOrder.Concurrent, false) // an override requisite (ignored because of overrides flag)
                        };
                        nonTermSec.SectionRequisites = new List<SectionRequisite>() {
                            new SectionRequisite(sec1.Id,true), // Required Section Requisite
                        };
                        nonTermSec.OverridesCourseRequisites = false;
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    // Date/Time conflict testing setup: set up two sections 7710 and 7711 with overlapping start/end dates
                    // and overlapping meeting dates and times.
                    if (course.Id == "7710" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(10), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.EndDate = term.StartDate.AddDays(60);
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 50, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    if (course.Id == "7711" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(20), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.EndDate = term.StartDate.AddDays(60);
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 30, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 10, 20, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    // Date/Time conflict testing setup: set up two sections 7710 and 7711 with overlapping start/end dates
                    // and overlapping meeting dates and times, but with null end dates and times.
                    if (course.Id == "7710" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate.AddDays(10), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 10, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    if (course.Id == "7711" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate.AddDays(20), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 30, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    // Date/Time conflict testing setup: set up another 7711 with overlapping start/end dates but not overlapping days
                    if (course.Id == "7711" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "06", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        nonTermSec.EndDate = nonTermSec.StartDate.AddDays(30);
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 30, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    //// Time conflict test: Date and day of week overlap, starts at same time 7710, sec 04 ends. No message
                    if (course.Id == "7711" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "07", term.StartDate.AddDays(20), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 50, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 10, 50, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }
                    //// Time conflict test: Date and day of week overlap, ends at same time 7704, sec 04 starts. No message
                    if (course.Id == "7711" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section nonTermSec = new Section(sectionId.ToString(), course.Id, "08", term.StartDate.AddDays(20), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        nonTermSec.Guid = Guid.NewGuid().ToString();
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", nonTermSec.StartDate, nonTermSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 08, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        nonTermSec.AddSectionMeeting(mp1);
                        nonTermSec.FirstMeetingDate = nonTermSec.StartDate;
                        nonTermSec.LastMeetingDate = nonTermSec.EndDate;
                        // Add section to the returned list
                        sections.Add(nonTermSec);
                    }

                    //// Time conflict test: 7714 and 7715 sections 04 - same day of week and time of day, but section meeting date ranges do not overlap. 
                    //// No message.
                    if (course.Id == "7714" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section testSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(5), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        testSec.Guid = Guid.NewGuid().ToString();
                        testSec.EndDate = term.StartDate.AddDays(60);
                        testSec.TermId = term.Code;
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", term.StartDate.AddDays(5), term.StartDate.AddDays(20), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 08, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        testSec.AddSectionMeeting(mp1);
                        testSec.FirstMeetingDate = term.StartDate.AddDays(5);
                        testSec.LastMeetingDate = testSec.EndDate;
                        // Add section to the returned list
                        sections.Add(testSec);
                    }
                    if (course.Id == "7715" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes - but set HideInCatalog to true instead of default of false
                        Section testSec = new Section(sectionId.ToString(), course.Id, "04", term.StartDate.AddDays(5), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses, true, true, false, false, false, false, true);
                        testSec.Guid = Guid.NewGuid().ToString();
                        testSec.EndDate = term.StartDate.AddDays(60);
                        testSec.TermId = term.Code;
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", term.StartDate.AddDays(21), term.StartDate.AddDays(40), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 08, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        testSec.AddSectionMeeting(mp1);
                        testSec.FirstMeetingDate = term.StartDate.AddDays(21);
                        testSec.LastMeetingDate = testSec.EndDate;
                        // Add section to the returned list
                        sections.Add(testSec);
                    }

                    //// Time conflict test: 7714 and 7715 same day of week and time of day, AND section meeting date ranges do overlap, two conflicts. Message returned.
                    if (course.Id == "7714" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes - but set HideInCatalog to true instead of default of false
                        Section testSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate.AddDays(5), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses, true, true, false, false, false, false, true);
                        testSec.Guid = Guid.NewGuid().ToString();
                        testSec.EndDate = term.StartDate.AddDays(60);
                        testSec.TermId = term.Code;
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", term.StartDate.AddDays(5), term.StartDate.AddDays(20), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 08, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        id = new Random().Next(100000).ToString();
                        guid = Guid.NewGuid().ToString();
                        SectionMeeting mp2 = new SectionMeeting(id, sectionId.ToString(), "LAB", term.StartDate.AddDays(5), term.StartDate.AddDays(20), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 10, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday }
                        };
                        testSec.AddSectionMeeting(mp1);
                        testSec.AddSectionMeeting(mp2);
                        testSec.FirstMeetingDate = term.StartDate.AddDays(5);
                        testSec.LastMeetingDate = testSec.EndDate;
                        // Add section to the returned list
                        sections.Add(testSec);
                    }
                    if (course.Id == "7715" && term.Code == termCodes.ElementAt(0))
                    {
                        // Increment for unique section Id
                        sectionId++;
                        // Construct section, use mostly course attributes
                        Section testSec = new Section(sectionId.ToString(), course.Id, "05", term.StartDate.AddDays(5), course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        testSec.Guid = Guid.NewGuid().ToString();
                        testSec.EndDate = term.StartDate.AddDays(60);
                        testSec.TermId = term.Code;
                        // Add meeting times
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting mp1 = new SectionMeeting(id, sectionId.ToString(), "LEC", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 08, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                        };
                        id = new Random().Next(100000).ToString();
                        guid = Guid.NewGuid().ToString();
                        SectionMeeting mp2 = new SectionMeeting(id, sectionId.ToString(), "LAB", term.StartDate.AddDays(15), term.StartDate.AddDays(40), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 10, 00)),
                            EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 10, 00, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday }
                        };
                        testSec.AddSectionMeeting(mp1);
                        testSec.AddSectionMeeting(mp2);
                        testSec.FirstMeetingDate = term.StartDate.AddDays(15);
                        testSec.LastMeetingDate = testSec.EndDate;
                        // Add section to the returned list
                        sections.Add(testSec);
                    }

                    // testing ILP data
                    // Made the first one of these sections (section number "99") not allow pass fail and not allow audits.
                    if (course.Id == "7272" && term.Code == termCodes.ElementAt(0))
                    {
                        // without PtlSiteId or LearningProvider
                        sectionId++;
                        Section ilpSec = new Section(sectionId.ToString(), course.Id, "99", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses, allowPassNoPass: false, allowAudit: false, allowWaitlist: false);
                        ilpSec.Guid = Guid.NewGuid().ToString();
                        ilpSec.EndDate = ilpSec.StartDate.AddDays(45);
                        var id = new Random().Next(100000).ToString();
                        var guid = Guid.NewGuid().ToString();
                        SectionMeeting ilpMeet = new SectionMeeting(id, sectionId.ToString(), "LEC", ilpSec.StartDate, ilpSec.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 07, 30, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                        };
                        ilpSec.TermId = term.Code;
                        ilpSec.AddSectionMeeting(ilpMeet);
                        ilpSec.LearningProvider = "";
                        ilpSec.LearningProviderSiteId = "";
                        ilpSec.PrimarySectionId = "";
                        sections.Add(ilpSec);

                        // with PtlSiteId, no LP
                        sectionId++;
                        Section ilpSec2 = new Section(sectionId.ToString(), course.Id, "98", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        ilpSec2.Guid = Guid.NewGuid().ToString();
                        ilpSec2.EndDate = ilpSec2.StartDate.AddDays(45);
                        id = new Random().Next(100000).ToString();
                        guid = Guid.NewGuid().ToString();
                        SectionMeeting ilpMeet2 = new SectionMeeting(id, sectionId.ToString(), "LEC", ilpSec2.StartDate, ilpSec2.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 07, 30, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                        };
                        ilpSec2.TermId = term.Code;
                        ilpSec2.AddSectionMeeting(ilpMeet2);
                        ilpSec2.LearningProvider = "SHAREPOINT";
                        ilpSec2.LearningProviderSiteId = ilpSec2.Id;
                        ilpSec2.PrimarySectionId = ilpSec2.Id;
                        sections.Add(ilpSec2);


                        // with PtlSiteId, and LP
                        sectionId++;
                        Section ilpSec3 = new Section(sectionId.ToString(), course.Id, "97", term.StartDate, course.MinimumCredits, course.Ceus, title, "IN", depts, course.CourseLevelCodes.ToList(), course.AcademicLevelCode, statuses);
                        ilpSec3.Guid = Guid.NewGuid().ToString();
                        ilpSec3.EndDate = ilpSec2.StartDate.AddDays(45);
                        id = new Random().Next(100000).ToString();
                        guid = Guid.NewGuid().ToString();
                        SectionMeeting ilpMeet3 = new SectionMeeting(id, sectionId.ToString(), "LEC", ilpSec3.StartDate, ilpSec3.EndDate.GetValueOrDefault(), frequency)
                        {
                            Guid = guid,
                            StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 07, 30, 00)),
                            Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                        };
                        ilpSec3.TermId = term.Code;
                        ilpSec3.AddSectionMeeting(ilpMeet3);
                        ilpSec3.LearningProvider = "MOODLE";
                        ilpSec3.LearningProviderSiteId = ilpSec3.Id; ;
                        ilpSec3.PrimarySectionId = "999999999";
                        sections.Add(ilpSec3);

                    }
                }
            }
            string JsonString = Newtonsoft.Json.JsonConvert.SerializeObject(sections, Newtonsoft.Json.Formatting.None);

            return sections;

        }




        private ICollection<SectionMeeting> BuildMeetingTimes(string sectionId, DateTime startDate, DateTime? endDate)
        {
            // Build meetings Times specifically for the three sections created per term
            var meetingTimes = new List<SectionMeeting>();

            // Since these are being built from section meeting dates, and section end date *MAY* be null, account for that
            // and make up a section meeting end using the start date, since section meeting end date *MAY NOT* be null.
            var adjustedEndDate = (endDate == null) ? startDate.AddDays(60) : endDate.GetValueOrDefault();

            // meeting for all sections "01"
            var id = new Random().Next(100000).ToString();
            SectionMeeting mp1 = new SectionMeeting(id, sectionId, "LEC", startDate, adjustedEndDate, frequency)
            {
                Guid = Guid.NewGuid().ToString(),
                StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 09, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                Room = "BRAN*101",
                IsOnline = false
            };
            meetingTimes.Add(mp1);

            // meeting for all sections "02"
            id = new Random().Next(100000).ToString();
            SectionMeeting mp2 = new SectionMeeting(id, sectionId, "ONL", DateTime.Today, DateTime.Today, frequency)
            {
                Guid = Guid.NewGuid().ToString(),
                StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 01, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 01, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                Room = "ABLE*201",
                IsOnline = true
            };
            meetingTimes.Add(mp2);

            // meeting for all sections "03"
            id = new Random().Next(100000).ToString();
            SectionMeeting mp3 = new SectionMeeting(id, sectionId, "LEC", DateTime.Today, DateTime.Today, frequency)
            {
                Guid = Guid.NewGuid().ToString(),
                StartTime = new DateTimeOffset(new DateTime(2014, 6, 9, 11, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2014, 6, 9, 11, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                Room = "CHAR*301",
                IsOnline = false
            };
            meetingTimes.Add(mp3);

            // Extra meeting for all sections "03", (online to make them hybrid sections, no days and times needed)
            id = new Random().Next(100000).ToString();
            SectionMeeting mp4 = new SectionMeeting(id, sectionId, "ONL", DateTime.Today, DateTime.Today, frequency)
            {
                Guid = Guid.NewGuid().ToString(),
                IsOnline = true
            };
            meetingTimes.Add(mp4);

            return meetingTimes;
        }

        public string GetRecordKeyFromGuid(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetGuidFromEntityIdAsync(string entity, string primaryKey, string secondaryField = "", string secondaryKey = "")
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> GetSectionMeetingAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeetingAsync(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string termId)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionFaculty>, int>> GetSectionFacultyAsync(int offset, int limit, string section, string instructor, List<string> instructionalEvent)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionMeetingIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> GetSectionMeetingByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionFacultyIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionFaculty> GetSectionFacultyByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionFaculty> PostSectionFacultyAsync(SectionFaculty faculty, string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionFaculty> PutSectionFacultyAsync(SectionFaculty faculty, string guid)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSectionFacultyAsync(SectionFaculty faculty, string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> PostSectionMeetingAsync(Section section, string meetingGuid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> PutSectionMeetingAsync(Section section, string meetingGuid)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSectionMeetingAsync(string id, List<SectionFaculty> faculty)
        {
            throw new NotImplementedException();
        }


        public Task<string> GetSectionIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }


        public Task<string> GetSectionGuidFromIdAsync(string id)
        {
            throw new NotImplementedException();
        }


        public Task<string> GetSectionMeetingGuidFromIdAsync(string id)
        {
            throw new NotImplementedException();
        }


        public Task<string> GetSectionCrosslistGuidFromIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<SectionCrosslist> GetSectionCrosslistByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionCrosslistIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }


        public Task<SectionCrosslist> GetSectionCrosslistAsync(string id)
        {
            throw new NotImplementedException();
        }


        public Task DeleteSectionCrosslistAsync(string id)
        {
            throw new NotImplementedException();
        }


        public Task<SectionCrosslist> CreateSectionCrosslistAsync(SectionCrosslist sectionCrosslist)
        {
            throw new NotImplementedException();
        }

        public Task<SectionCrosslist> UpdateSectionCrosslistAsync(SectionCrosslist sectionCrosslist)
        {
            throw new NotImplementedException();
        }

        public Task<StudentSectionWaitlist> GetWaitlistFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }
        public Task<Tuple<IEnumerable<StudentSectionWaitlist>, int>> GetWaitlistsAsync(int offset, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionCrosslist>, int>> GetSectionCrosslistsPageAsync(int offset, int limit, string section = "")
        {
            throw new NotImplementedException();
        }

        public Task<Section> UpdateSectionBookAsync(SectionTextbook textbook)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> PostSectionMeeting2Async(Section section, string meetingGuid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMeeting> PutSectionMeeting2Async(Section section, string meetingGuid)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            List<SectionMeetingInstance> meetingEntities = new List<SectionMeetingInstance>();
            var timeOffset = new TimeSpan(4, 0, 0);
            meetingEntities.Add(new SectionMeetingInstance("111", sectionId, new DateTime(2018, 1, 1), new DateTimeOffset(2017, 12, 15, 9, 15, 0, timeOffset), new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeOffset)) { InstructionalMethod = "LEC" });
            meetingEntities.Add(new SectionMeetingInstance("222", sectionId, new DateTime(2018, 1, 3), new DateTimeOffset(2017, 12, 15, 9, 15, 0, timeOffset), new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeOffset)) { InstructionalMethod = "LEC" });
            meetingEntities.Add(new SectionMeetingInstance("333", sectionId, new DateTime(2018, 1, 5), new DateTimeOffset(2017, 12, 15, 9, 15, 0, timeOffset), new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeOffset)) { InstructionalMethod = "LEC" });
            meetingEntities.Add(new SectionMeetingInstance("444", sectionId, new DateTime(2018, 1, 8), new DateTimeOffset(2017, 12, 15, 9, 15, 0, timeOffset), new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeOffset)) { InstructionalMethod = "LEC" });
            meetingEntities.Add(new SectionMeetingInstance("555", sectionId, new DateTime(2018, 1, 10), new DateTimeOffset(2017, 12, 15, 9, 15, 0, timeOffset), new DateTimeOffset(2017, 12, 15, 10, 15, 0, timeOffset)));
            meetingEntities.Add(new SectionMeetingInstance("666", sectionId, new DateTime(2018, 1, 12), null, null));

            return meetingEntities.AsEnumerable();
        }

        public Task<IEnumerable<SectionStatusCodeGuid>> GetStatusCodesWithGuidsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Section> PostSection2Async(Section section)
        {
            throw new NotImplementedException();
        }

        public Task<Section> PutSection2Async(Section section)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Dictionary of string, string that contains the Ethos Extended Data to send into the CTX
        /// key is column name
        /// value is value to save in, if empty string then this means it is meant to remove the data from colleague
        /// </summary>
        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        /// <summary>
        /// Takes the EthosExtendedDataList dictionary and splits it into two List string to be passed to Colleague CTX 
        /// </summary>
        /// <returns>T1 is the list of keys, T2 is a list values that match up. Returns null if the list is empty</returns>
        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "", string code = "", string number = "", string learningProvider = "", string termId = "", List<string> academicLevel = null, string course = "", string location = "", string status = "", List<string> department = null, string subject = "", string instructor = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "", string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "", List<string> academicLevel = null, string course = "", string location = "", string status = "", List<string> department = null, string subject = "", List<string> instructors = null, string scheduleTermId = "")
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetSectionGuidsCollectionAsync(IEnumerable<string> sectionIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Event>> GetSectionEventsICalAsync(string calendarScheduleType, IEnumerable<string> calendarSchedulePointers, DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        public Task<SectionWaitlist> GetSectionWaitlistAsync(string sectionId)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<SectionWaitlistStudent>> GetSectionWaitlist2Async(string sectionId)
        {
            throw new NotImplementedException();
        }

        public Task<SectionWaitlistConfig> GetSectionWaitlistConfigAsync(string sectionId)
        {
            throw new NotImplementedException();
        }

        public Task<StudentSectionWaitlistInfo> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMidtermGradingComplete> GetSectionMidtermGradingCompleteAsync(string sectionId)
        {
            throw new NotImplementedException();
        }

        public Task<SectionMidtermGradingComplete> PostSectionMidtermGradingCompleteAsync(string sectionId, int? midtermGradeNumber, string completeOperator, DateTimeOffset? dateAndTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentWaitlistStatus>> GetStudentWaitlistStatusesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeeting2Async(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string term)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSections2Async(int offset, int limit, string title = "", string startDate = "", string endDate = "", string code = "", string number = "", 
            string learningProvider = "", string termId = "", string reportingTermId = "", List<string> academicLevels = null, string course = "", string location = "", string status = "", 
            List<string> departments = null, string subject = "", List<string> instructors = null, string scheduleTermId = "", bool addToErrorCollection = false)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchable1Async(int offset, int limit, string searchable = "", bool addToCollection = false)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeyword1Async(int offset, int limit, string keyword, bool bypassCache = false, bool caseSensitive = false, bool addToCollection = false)
        {
            throw new NotImplementedException();
        }
    }
}
