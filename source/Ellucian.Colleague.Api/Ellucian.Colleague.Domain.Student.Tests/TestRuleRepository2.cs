// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestRuleRepository2 : IRuleRepository
    {
        public async Task<IEnumerable<Rule>> GetManyAsync(IEnumerable<string> ruleIds)
        {
            var results = new List<Rule>();
            foreach (var ruleId in ruleIds)
            {
                results.Add(await GetAsync(ruleId));
            }
            return results;
        }

        public async Task<Rule> GetAsync(string ruleId)
        {
            return new Rule<AcademicCredit>(ruleId);
        }

        private string[,] rules = 
        {
        //CODE                       PRIMARY FILE                    DESCRIPTION
        {"MATH100"           ,  "COURSES"           ,       "Course Name = MATH*100"        },
        {"NEWSTAT"           ,  "STUDENT.ACAD.CRED" ,       "New statuses only"             },
        {"SUBJENGL"          ,  "COURSES"            ,       "CRS.SUBJECT = ENGL"           }
        };

        public async Task<IEnumerable<RuleResult>> ExecuteAsync<T>(IEnumerable<RuleRequest<T>> ruleRequests)
        {
            return Execute<T>(ruleRequests);
        }

        public IEnumerable<RuleResult> Execute<T>(IEnumerable<RuleRequest<T>> ruleRequests)
        {
            var list = new List<RuleResult>();

            foreach (var credit in ruleRequests.Where(rr => rr.GetType() == typeof(RuleRequest<AcademicCredit>)).Select(c => c.Context as AcademicCredit))
            {
                // Make any credit with a course of MATH 100 pass the MATH100 rule
                if (credit.Course != null && credit.Course.Number == "100" && credit.Course.SubjectCode == "MATH")
                {
                    list.Add(new RuleResult() { RuleId = "MATH100", Passed = true, Context = credit });
                }
                else
                {
                    list.Add(new RuleResult() { RuleId = "MATH100", Passed = false, Context = credit });
                }

                // Make any credit with a new status pass the NEWSTAT rule
                if (credit.Status == CreditStatus.New)
                {
                    list.Add(new RuleResult() { RuleId = "NEWSTAT", Passed = true, Context = credit });
                }
                else
                {
                    list.Add(new RuleResult() { RuleId = "NEWSTAT", Passed = false, Context = credit });
                }
                // Any credit with a subject of ENGL will pass the SUBJENGL rule
                if (credit.Course != null && credit.SubjectCode == "ENGL")
                {
                    list.Add(new RuleResult() { RuleId = "SUBJENGL", Passed = true, Context = credit });
                }
                else
                {
                    list.Add(new RuleResult() { RuleId = "SUBJENGL", Passed = false, Context = credit });
                }
            }

            foreach (var course in ruleRequests.Where(rr => rr.GetType() == typeof(RuleRequest<Course>)).Select(c => c.Context as Course))
            {
                // Make any MATH 100 course pass the MATH100 rule
                if (course.Number == "100" && course.SubjectCode == "MATH")
                {
                    list.Add(new RuleResult() { RuleId = "MATH100", Passed = true, Context = course });
                }
                else
                {
                    list.Add(new RuleResult() { RuleId = "MATH100", Passed = false, Context = course });
                }

                // Any credit with a subject of ENGL will pass the SUBJENGL rule
                if (course.SubjectCode == "ENGL")
                {
                    list.Add(new RuleResult() { RuleId = "SUBJENGL", Passed = true, Context = course });
                }
                else
                {
                    list.Add(new RuleResult() { RuleId = "SUBJENGL", Passed = false, Context = course });
                }
            }
            return list;
        }
    }
}
