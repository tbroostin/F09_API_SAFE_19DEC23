// Copyright 2013-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;



namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentDegreePlanRepository : IStudentDegreePlanRepository
    {
        private IEnumerable<DegreePlan> BuildDegreePlans()
        {
            List<DegreePlan> degreePlans = new List<DegreePlan>();

            string[,] planData = 
                {
                 // PLAN ID     PLAN NAME               STUDENT ID    VERSION
                {  "2",     "AA Two years",              "0000894", "1"},
                {  "3",     "BA.PSYC 3 year plan",       "0000896", "1"},
                {  "4",     "BA.PSYC plan",              "0000896", "1"},
                {  "5",     "MATH.BS plan",              "0000896", "1"},
                {"802",     "MATH.BS plan",             "00004002", "16"},
                {"808",     "MATH.BS plan",             "00004008", "96"},
                {"809",     "lang   plan",             "0016301", "1"},
                {"810",     "repeats with planned courses",             "0016302", "1"}
                };

            for (int i = 0; i < planData.Length / 4; i++)
            {
                // NOT USING THE NAME IN DEGREE PLAN AT LEAST FOR NOW
                // degreePlans.Add(new DegreePlan(Int32.Parse(planData[i, 0]), planData[i, 1], planData[i, 2]));
                degreePlans.Add(new DegreePlan(Int32.Parse(planData[i, 0]), planData[i, 2], Int32.Parse(planData[i, 3])));
            }

            // Add terms to each plan except 4.

            string[,] planTerms = {
                                   {"2","2008/FA"},  // DegreePlanControllerTests expecting 6 terms on this plan
                                   {"2","2009/SP"},
                                   {"2","2009/S1"},
                                   {"2","2009/FA"},
                                   {"2","2010/SP"},
                                   {"2","2010/S1"},

                                   {"3","2008/FA"},
                                   {"3","2009/SP"},
                                   {"3","2009/S1"},
                                   {"3","2009/FA"},
                                   {"3","2010/SP"},
                                   {"3","2010/S1"},

                                   {"802","2013/FA"},  // This will match up to student 4002 in teststudentrepo
                                   {"802","2014/SP"},
                                   {"802","2014/FA"},
                                   {"802","2015/SP"},


                                   {"808","2008/FA"},  // This will match up to student 4008 in teststudentrepo
                                   {"808","2009/SP"},
                                   {"808","2009/S1"},
                                   {"808","2009/FA"},
                                   {"808","2010/SP"},
                                   {"808","2010/S1"},

                                   {"809","2015/FA" },//This will match with student 0016301 in teststudentrepo

                                   {"810","2014/FA" },
                                   {"810","2016/FA" },
                                   {"810","2029/FA" },

                                  };

            for (int i = 0; i < planTerms.Length / 2; i++)
            {
                    var planId = Int32.Parse(planTerms[i, 0]);
                    var termId = planTerms[i, 1];
                    var degreePlan = degreePlans.Where(d => d.Id == planId).FirstOrDefault();
                    if (degreePlan != null)
                    {
                        degreePlan.AddTerm(termId);
                    }
              
            }

            // Add same set of courses to each plan's terms.
            //                     Plan  Term     Course Section Credits GradingType WaitlistStatus Protected
            string[,] planCourses = {
                                       {"2","2008/FA","130", "", "1.0", "G", "", "N"}, // PlanRepositoryTests planning on 5 courses on this plan
                                       {"2","2008/FA","143", "", "3.0", "G", "", "N"},
                                       {"2","2008/FA","139", "", "4.0", "G", "", "N"},
                                       {"2","2009/SP","110", "", "", "G", "", "N"},
                                       {"2","2009/SP","117", "", "3.0", "G", "", "N"},
                                       {"2","2011/SP", "56", "", "2.0", "G", "", "N"}, // there is an academic credit for this one, withdraw grade

                                       {"3","2008/FA","130", "100", "3.0", "G", "", "N"},
                                       {"3","2008/FA","143", "200", "4.0", "G", "A", "N"},
                                       {"3","2008/FA","139", "300", "3.0", "A", "", "Y"},
                                       {"3","2009/SP","110", "400", "5.0", "P", "", "N"},
                                       {"3","2009/SP","117", "", "", "N", "", "N"},
                                       {"3","2009/SP","333", "", "", "N", "", "N"},

                                       {"802","2014/FA","143", "", "3.0", "G", "", "N"}, // MATH-200
                                       {"802","2014/FA","46", "", "3.0", "G", "", "N"},  // MATH-100
                                       
                                       {"808","2008/FA", "46", "", "3.0", "G", "", "N"},  // MATH-100
                                       {"808","2008/FA","139", "", "3.0", "G", "", "N"},  // HIST-100
                                       {"808","2008/FA","139", "", "3.0", "G", "", "N"} ,  // HIST-100 (planned twice)

                                        {"809","2015/FA","7444","","3.0","G","","N" }, //GERM-100
                                        {"809","2015/FA","7442","","3.0","G","","N" },//ARTH-100

                                        //degree plan used by student 0016302 for repeats with planned courses
                                         {"810","2014/FA","7435","01","3.0","G","","N" },//planned for MATH-300BB
                                         {"810","2016/FA","7435","02","3.0","G","","N" },//planned for MATH-300BB
                                         {"810","2029/FA","7435","03","3.0","G","","N" },//planned for MATH-300BB
                                    };
            try
            {
                for (int i = 0; i < planCourses.Length / 8; i++)
                {

                    var planId = Int32.Parse(planCourses[i, 0]);
                    var termId = planCourses[i, 1];
                    var courseId = planCourses[i, 2];
                    var sectionId = planCourses[i, 3] == "" ? null : planCourses[i, 3];
                    decimal? credits = null;
                    if (!string.IsNullOrEmpty(planCourses[i, 4]))
                    {
                        credits = decimal.Parse(planCourses[i, 4]);
                    }
                    var gradingType = planCourses[i, 5];
                    var waitlistStatus = planCourses[i, 6];
                    var isProtected = planCourses[i, 7] == "Y" ? true : false;
                    var degreePlan = degreePlans.Where(d => d.Id == planId).FirstOrDefault();
                    if (degreePlan != null)
                    {
                        Student.Entities.GradingType gt = Student.Entities.GradingType.Graded;
                        if (gradingType == "A")
                        {
                            gt = Student.Entities.GradingType.Audit;
                        }
                        if (gradingType == "P")
                        {
                            gt = Student.Entities.GradingType.PassFail;
                        }
                        Domain.Student.Entities.DegreePlans.WaitlistStatus wlstat = Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
                        if (waitlistStatus == "A")
                        {
                            wlstat = Student.Entities.DegreePlans.WaitlistStatus.Active;
                        }
                        PlannedCourse pc = new PlannedCourse(courseId, sectionId, gt, wlstat, null, null) { Credits = credits, IsProtected = isProtected };
                        // Add a unmet requisite warning to one of the planned courses on the plan
                        if (courseId == "117")
                        {
                            var requisite = new Requisite("PREREQ1", true, RequisiteCompletionOrder.Previous, false);
                            pc.AddWarning(new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite) { Requisite = requisite });
                        }
                        degreePlan.AddCourse(pc, termId);
                    }

                }

                // Add a nonterm course to plan 3.
                var degreePlan3 = degreePlans.Where(d => d.Id == 3).FirstOrDefault();
                var nonTermPlannedCourse = new PlannedCourse("444", "111");
                var ncwRequisite = new Requisite("444", false);
                var pcWarning = new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite) { Requisite = ncwRequisite };
                nonTermPlannedCourse.AddWarning(pcWarning);
                degreePlan3.AddCourse(nonTermPlannedCourse, null);

                // Add a course placeholder to plan 3 should be ignored by eval and archive.
                degreePlan3.AddCourse(new PlannedCourse(course: null, section: null, coursePlaceholder: "MUSC-200"), "2009/SP");

                // Add approvals for two of the planned courses on 2.
                var degreePlan2 = degreePlans.Where(d => d.Id == 2).FirstOrDefault();
                var approvals = new List<DegreePlanApproval>();
                approvals.Add(new DegreePlanApproval("00004001", DegreePlanApprovalStatus.Approved, new DateTime(2008, 06, 01, 10, 0, 0), "130", "2008/FA"));
                approvals.Add(new DegreePlanApproval("00004002", DegreePlanApprovalStatus.Denied, new DateTime(2008, 06, 03, 8, 30, 0), "143", "2008/FA"));
                degreePlan2.Approvals = approvals;

                // Add a course placeholder to plan 2 should be ignored by eval and archive.
                degreePlan2.AddCourse(new PlannedCourse(course: null, section: null, coursePlaceholder: "MUSC-100"), "2011/SP");

                // Add approvals for two of the planned courses on 3.
                degreePlan3.Approvals = approvals;

                return degreePlans;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<DegreePlan>> GetAsync()  //Get all plans
        {
            //create a completed task
            return await Task.FromResult<IEnumerable<DegreePlan>>(BuildDegreePlans());
        }

        public async Task<DegreePlan> GetAsync(int planId)
        {
            try
            {
                var plans = await GetAsync();
                return plans.Where(dp => dp.Id == planId).FirstOrDefault();
            }
            catch
            {
                throw new KeyNotFoundException("DegreePlan " + planId.ToString() + "not found");
            }
        }

        public async Task<IEnumerable<DegreePlan>> GetAsync(IEnumerable<string> studentIds)
        {
            throw new NotImplementedException();
        }

        public async Task<DegreePlan> AddAsync(DegreePlan newPlan)
        {
            throw new NotImplementedException();
        }

        public async Task<DegreePlan> UpdateAsync(DegreePlan newPlan)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}