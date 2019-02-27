// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Ellucian.Colleague.Data.Student.Repositories;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentRepository : IStudentRepository
    {
        Ellucian.Colleague.Domain.Student.Entities.Term termData = null;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData = null;
        public Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetStudentsByIdAsync(IEnumerable<string> studentIds, Term termData, IEnumerable<Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus> citizenshipStatusData, bool inheritFromPerson = true, bool getDegreePlan = true, bool filterAdvisorsByTerm = false, bool filterEndedAdvisements = true)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, List<string>>> GetAcadCredIdsByStudentIdsAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Ellucian.Colleague.Domain.Student.Entities.Student Current(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetAllAsync()
        {
            return await BuildStudentRepositoryAsync();
        }

        public Task<IEnumerable<string>> SearchStudentByNameAsync(string lastName, string firstName = null, string middleName = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetAsync(IEnumerable<string> ids)
        {
            return await BuildStudentRepositoryAsync();
        }

        //public Student(string id, string lastName, int? degreePlanId, List<string> programIds, List<string> academicCreditIds)
        private async Task<ICollection<Ellucian.Colleague.Domain.Student.Entities.Student>> BuildStudentRepositoryAsync()
        {
            var students = new List<Ellucian.Colleague.Domain.Student.Entities.Student>();
            //                                                                                              Degree Plan                 Academic Credit
            string[,] studentdata = {
                                    {"00004001",  "Dickinson",    "",         "MATH.BS", "1,2,3" }, // none                     HIST-100,HIST-200,BIOL-100
                                    {"00004002",      "Scott", "802",         "MATH.BS", "26,36" }, // MATH-100, MATH-200       ENGL-102, ENGL-101
                                    {"00004003",    "Halford",    "",         "MATH.BS",     ""  }, //
                                    {"00004004",   "Osbourne",    "",         "MATH.BS",  "8,39" }, //                          MATH-100, MATH-200
                                    {"00004005",     "Cooper", "802",         "MATH.BS", "ALL"   }, // MATH-100, MATH-200       all the credits in the test STC repo
                                    {"00004006",       "Sixx",    "",         "MATH.BS", "36,33" }, //                          ENGL-101, MATH-460
                                    {"00004007",    "Urungus",    "",         "MATH.BS",  "5,6"  }, //                          MATH-201, MATH-152 (two "A"s)
                                    {"00004008",     "Dharma", "808",         "MATH.BS",  ""     }, // HIST-100, MATH-100
                                    {"00004009",        "Dio",    "", "HIST.BA,MATH.BS",  ""     }, //               Two programs
                                    {"00004010",  "Kilmister", "802",         "MATH.BS",  "20,21,22,1001" }, // MATH-100, MATH-200       one spanish, two dance a noncourse
                                    {"00004011",   "Morrison",    "",         "MATH.BS",  "56,74"   }, //                          MATH-4003 (withdrawn)
                                    {"00004012",     "Waters",    "",         "MATH.BS",  "57"   }, //                          MATH-502 (graduate course)
                                    {"00004013",    "Hendrix",    "",         "MATH.BS",  "26"   }, //                          ENGL-102
                                    {"00004014",   "Sortbaby",    "",         "MATH.BS",  "13,16,14,8,63,11"   }, // test to prove sort by type, then start date then descending academic credit ID
                                                                                                            // academic credits: 13: MUSC-100 TR 2010SP, 16: MATH-362 IN 2010SP, 14: MUSC-207 TR 2010SP, 
                                       
                                        {"00004015",      "Scott", "",         "MATH.BS", "26,36,8,39" },                                                                    //                    8: MATH-100 IN 2009FA, 63: MUSC-209 IN 2010SP, 11: MATH-150 IN 2009FA 

                                    // Colldev data
                                    {"1143352",     "Hawking",    "",  "STSS.MATH.BS", "46,47,48,49,50"}, //      
                                    {"0000894",     "Generic",    "",         "MATH.BS",  "26"}, // Needed for "self" permissions tests
                                    {"0000895",     "Generic",    "",         "MATH.BS",  "26"}, // Needed for "self" permissions tests
                                    {"0000999", "TrickOrTreatMinGroups", "3", "MATH.BS","1,2,35,3,5,10,12,4,6,37,38,30,27,26" },
                                    {"0000111","CourseReuse","","PROG.COURSE.REUSE", "103,104,105,106,107"},   //ENGL-200  COMPLETED ,ENGL-300  COMPLETED, COMM-1321  COMPLETED, COMM-100  COMPLETED,MATH-200  IN-PROGRESS
                                    {"0000112", "OverrideTranscriptGrouping","","MATH.BS", "36,33" }, //                          ENGL-101, MATH-460, MATH-502 (GR level course)

                                    //create replace and replacement statuses data to validate replace and Replacement statuses
                                    //this scenarios are for program MATH.BB with acad credits from MATH-300BB course. This course have retake for credit flag set to N.
                                    {"0016285","ReplaceAndReplacement-student-1","","REPEAT.BB","110,111,112" } ,//3 acad credits. all are inprogress
                                     {"0016286","ReplaceAndReplacement-student-2","","REPEAT.BB","113,114,115" },//3 acad credits. all are completed and graded
                                     {"0016287","ReplaceAndReplacement-student-3","","REPEAT.BB","111,113,115" }, //3 acad credits. 2 are completed
                                      {"0016288","ReplaceAndReplacement-student-4","","REPEAT.BB","110,112,114" }, //3 acad credits. 1 is completed
                                       {"0016289","ReplaceAndReplacement-student-5","","REPEAT.BB","113" }, //1 acad credits completed
                                        {"0016290","ReplaceAndReplacement-student-6","","REPEAT.BB","110" }, //1 acad credit inprogress

                                        //set up acad credits for course that have retake for credits set to Y

                                        //test for replace/retake with planned courses
                                    };


            for (int i = 0; i < (studentdata.Length / 5); i++)
            {
                string id = null;
                string lastname = null;
                int? dpid = null;
                List<string> programids = new List<string>();
                List<string> credids = new List<string>();

                id = studentdata[i, 0];
                lastname = studentdata[i, 1];
                if (!string.IsNullOrEmpty(studentdata[i, 2]))
                {
                    dpid = Int32.Parse(studentdata[i, 2]);
                }
                programids = studentdata[i, 3].Split(',').ToList();

                if (studentdata[i, 4] == "")
                {
                    credids = new List<string>();
                }
                else if (studentdata[i, 4] != "ALL")
                {
                    credids = studentdata[i, 4].Split(',').ToList();  // just listed
                }
                else
                {
                    credids = new TestAcademicCreditRepository().GetAsync().Result.Select(stc => stc.Id).ToList();  // all credits in test STC repo
                }
                var student = new Ellucian.Colleague.Domain.Student.Entities.Student(id, lastname, dpid, programids, credids);

                var faStudent = faStudentData.FirstOrDefault(f => f.studentId == id);
                if (faStudent != null)
                {
                    if (faStudent.faCounselors != null && faStudent.faCounselors.Count() > 0)
                    {
                        student.FinancialAidCounselorId = faStudent.faCounselors.First().counselorId;
                    }
                }

                // Add advisor to one student for evaluation permissions checking
                if (id == "00004012")
                {
                    student.AddAdvisement("0000111", new DateTime(2014, 01, 01), null, "MAJOR");
                    student.AddAdvisor("0000111");
                }

                students.Add(student);
            }

            return await Task.FromResult(students);

        }

        public async Task<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction> GetGradeRestrictionsAsync(string id)
        {
            Ellucian.Colleague.Domain.Student.Entities.GradeRestriction gradeRestriction = new Ellucian.Colleague.Domain.Student.Entities.GradeRestriction(true);
            return await Task.FromResult(gradeRestriction);
        }

        public Task<RegistrationResponse> RegisterAsync(RegistrationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RegistrationMessage>> CheckRegistrationEligibilityAsync(RegistrationRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<RegistrationEligibility> CheckRegistrationEligibilityAsync(string id)
        {
            // Set up a test RegistrationEligibility with terms to be returned regardless of student
            RegistrationEligibility regElig = new RegistrationEligibility(new List<RegistrationMessage>(), true, false);
            RegistrationEligibilityTerm term1 = new RegistrationEligibilityTerm("term1", false, true);
            term1.Status = RegistrationEligibilityTermStatus.HasOverride;
            term1.Message = "term1msg";
            term1.FailedRegistrationTermRules = true;
            regElig.AddRegistrationEligibilityTerm(term1);
            RegistrationEligibilityTerm term2 = new RegistrationEligibilityTerm("term2", false, false);
            term2.Status = RegistrationEligibilityTermStatus.NotEligible;
            term2.Message = "term2msg";
            regElig.AddRegistrationEligibilityTerm(term2);
            RegistrationEligibilityTerm term3 = new RegistrationEligibilityTerm("term3", true, false);
            term3.Status = RegistrationEligibilityTermStatus.Open;
            term3.Message = "term3msg";
            term3.AnticipatedTimeForAdds = new DateTime(2020, 9, 1, 2, 12, 0);
            regElig.AddRegistrationEligibilityTerm(term3);
            RegistrationEligibilityTerm term4 = new RegistrationEligibilityTerm("term4", false, false);
            term4.Status = RegistrationEligibilityTermStatus.Future;
            term4.Message = "term4msg";
            term4.AnticipatedTimeForAdds = new DateTime(2020, 9, 1, 2, 12, 0); ;
            regElig.AddRegistrationEligibilityTerm(term4);
            RegistrationEligibilityTerm term5 = new RegistrationEligibilityTerm("term5", false, false);
            term5.Status = RegistrationEligibilityTermStatus.Future;
            term5.Message = "term5msg";
            term5.AnticipatedTimeForAdds = new DateTime(2020, 9, 1, 2, 12, 0); ;
            regElig.AddRegistrationEligibilityTerm(term5);
            RegistrationEligibilityTerm term6 = new RegistrationEligibilityTerm("term6", false, false);
            term6.Status = RegistrationEligibilityTermStatus.NotEligible;
            term6.Message = "term6msg";
            term6.AnticipatedTimeForAdds = new DateTime(2012, 9, 1, 2, 12, 0); ;
            regElig.AddRegistrationEligibilityTerm(term6);
            return await Task.FromResult(regElig);
        }

        public async Task<RegistrationEligibility> CheckRegistrationEligibilityEthosAsync(string id, List<string> termCodes)
        {
            return await CheckRegistrationEligibilityAsync(id);
        }

        public async Task<IEnumerable<TranscriptRestriction>> GetTranscriptRestrictionsAsync(string id)
        {
            var returnval = new List<TranscriptRestriction>();
            if (id == "00000002")
            {
                returnval.Add(new TranscriptRestriction() { Code = "LIBRAR", Description = "Library fines outstanding" });
            }
            if (id == "00000003")
            {
                throw new KeyNotFoundException();
            }

            return await Task.FromResult(returnval);
        }


        public async Task<IEnumerable<Student.Entities.Student>> SearchAsync(string lastName, string firstName, DateTime? dateOfBirth, string formerName, string studentId, string governmentId)
        {
            return new List<Student.Entities.Student> { (await BuildStudentRepositoryAsync()).Where(s => s.LastName == lastName).FirstOrDefault() };
        }

        public Task<IEnumerable<string>> SearchIdsAsync(string termId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.RosterStudent>> GetRosterStudentsAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        Task<string> IStudentRepository.OrderTranscriptAsync(Student.Entities.Transcripts.TranscriptRequest order)
        {
            throw new NotImplementedException();
        }

        public Task<string> CheckTranscriptStatusAsync(string orderId, string currentStatusCode)
        {
            throw new NotImplementedException();
        }

        #region FinancialAid Dataset

        //Financial Aid dataset added by Matt DeDiana. Different style than what's above.
        public class FaStudent
        {
            public string studentId;
            public List<FaCounselor> faCounselors;

            public class FaCounselor
            {
                public string counselorId;
                public DateTime? startDate;
                public DateTime? endDate;
            }
        }

        //StudentIds come from BasePersonSetup
        public List<FaStudent> faStudentData = new List<FaStudent>()
        {
            new FaStudent()
            {
                studentId = "0000001",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                    new FaStudent.FaCounselor() {counselorId = "0001111", startDate = new DateTime(2014, 5, 1), endDate = new DateTime(2014, 6, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent()
            {
                studentId = "0000002",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent()
            {
                studentId = "0000003",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent()
            {
                studentId = "0000004",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent()
            {
                studentId = "0000005",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                }
            },
            new FaStudent()
            {
                studentId = "0000006",
                faCounselors = new List<FaStudent.FaCounselor>()
                {
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)}
                }
            }
        };

        #endregion

        public Task<string> GetTranscriptAsync(string id, string transcriptGrouping)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<StudentAccess>> GetStudentAccessAsync(IEnumerable<string> ids)
        {
            List<StudentAccess> students = new List<StudentAccess>();
            var allStudents = await BuildStudentRepositoryAsync();
            foreach (var id in ids)
            {
                students.Add(allStudents.Where(s => s.Id == id).FirstOrDefault().ConvertToStudentAccess());
            }
            return students;
        }


        public async Task<Ellucian.Colleague.Domain.Student.Entities.Student> GetAsync(string id)
        {
            return (await BuildStudentRepositoryAsync()).Where(s => s.Id == id).FirstOrDefault();
        }

        public Ellucian.Colleague.Domain.Student.Entities.Student Get(string id)
        {
            return BuildStudentRepositoryAsync().Result.Where(s => s.Id == id).FirstOrDefault();
        }


        public async Task<Ellucian.Colleague.Domain.Student.Entities.Student> GetDataModelStudentFromGuidAsync(string guid)
        {
            return (await BuildStudentRepositoryAsync()).FirstOrDefault(s => s.Id == guid);
        }

        public async Task<Ellucian.Colleague.Domain.Student.Entities.Student> GetDataModelStudentFromGuid2Async(string guid)
        {
            return (await BuildStudentRepositoryAsync()).FirstOrDefault(s => s.Id == guid);
        }

        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>, int>> GetDataModelStudentsAsync(int offset, int limit, bool bypassCache, string person, string type, string cohort, string residency)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>, int>> GetDataModelStudents2Async(int offset, int limit, bool bypassCache, string personFilter, string person, List<string> types, List<string> cohorts, List<string> residencies)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentCohort>> GetAllStudentCohortAsync(bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.ResidencyStatus>> GetResidencyStatusesAsync(bool ignoreCache = false)
        {
            return Task.FromResult<IEnumerable<Student.Entities.ResidencyStatus>>(new List<Student.Entities.ResidencyStatus>()
                {
                    new Student.Entities.ResidencyStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.ResidencyStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.ResidencyStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.ResidencyStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
        }

        public Task<string> GetResidencyStatusGuidAsync(string code)
        {
            var status = (new List<Student.Entities.ResidencyStatus>()
                {
                    new Student.Entities.ResidencyStatus("b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09", "code1", "title1"),
                    new Student.Entities.ResidencyStatus("bd54668d-50d9-416c-81e9-2318e88571a1", "code2", "title2"),
                    new Student.Entities.ResidencyStatus("5eed2bea-8948-439b-b5c5-779d84724a38", "code3", "title3"),
                    new Student.Entities.ResidencyStatus("82f74c63-df5b-4e56-8ef0-e871ccc789e8", "code4", "title4")
                });
            return Task.FromResult<string>((status.FirstOrDefault(cc => cc.Code == code)).Guid);
        }

        public Task<IEnumerable<Student.Entities.Student>> GetStudentsSearchAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student.Entities.Student>> GetStudentSearchByNameAsync(string lastName, string firstName = null, string middleName = null, int pageSize = int.MaxValue, int pageIndex = 1)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Student>> GetStudentAcademicPeriodProfileStudentInfoAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }
    }
}
