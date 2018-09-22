using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    //public class TestRuleRepository : IEvaluationRuleRepository
    //{

    //    // This will be of limited use because rules are actually evaluated on the colleague server

    //    private string[,] rules = 
    //    {
    //    //CODE                       PRIMARY FILE                    DESCRIPTION
    //    {"MATH100"           ,  "COURSES"           ,       "Course Name = MATH*100"        },
    //    {"NEWSTAT"           ,  "STUDENT.ACAD.CRED" ,       "New statuses only"             },
    //    };

    //    public EvaluationRuleResult Execute(IEnumerable<string> RuleIds, IEnumerable<AcademicCredit> Credits, IEnumerable<Course> PlannedCourses)
    //    {
    //        // Rule IDs arg ignored, all incoming ACRs evaluated against the
    //        // two (so far) rules above.

    //        EvaluationRuleResult rr = new EvaluationRuleResult();

    //        foreach (var ac in Credits)
    //        {
    //            if (ac.Status != CreditStatus.New)
    //            {
    //                rr.AddCreditRule("NEWSTAT", ac.Id);
    //            }
    //        }
    //        IEnumerable<Course> credcourses = Credits.Where(cr=>cr.Course != null).Select(cr=>cr.Course);
    //        foreach (var cs in PlannedCourses.Union(credcourses).Distinct())
    //        {
    //            if (cs.SubjectCode != "MATH" || cs.Number != "100")
    //            {
    //                rr.AddCourseRule("MATH100", cs.Id);
    //            }
    //        }


    //        return rr;
    //    }

    //    public IEnumerable<RuleResult> Execute2(IEnumerable<Rule> rules, IEnumerable<AcademicCredit> credits, IEnumerable<Course> courses)
    //    {
    //        // Rules arg ignored, all incoming ACRs evaluated against the
    //        // two (so far) rules above.
    //        var results = new List<RuleResult>();
    //        var allCourses = credits.Where(stc => stc.Course != null).Select(stc => stc.Course).Union(courses);

    //        // Start off with them all passing
    //        foreach (var rule in this.rules/*NOT THE PARAMETER PASSED IN*/)
    //        {
    //            foreach (var credit in credits)
    //            {
    //                var result = new RuleResult();
    //                result.RuleId = rule;
    //                result.Context = credit;
    //                result.Passed = true;
    //                results.Add(result);
    //            }

    //            foreach (var course in allCourses)
    //            {
    //                var result = new RuleResult();
    //                result.RuleId = rule;
    //                result.Context = course;
    //                result.Passed = true;
    //                results.Add(result);
    //            }
    //        }

    //        foreach (var ac in credits)
    //        {
    //            if (ac.Status != CreditStatus.New)
    //            {
    //                results.FirstOrDefault(re => re.RuleId == "NEWSTAT" && re.Context == ac).Passed = false;
    //            }
    //        }
    //        foreach (var cs in allCourses)
    //        {
    //            if (cs.SubjectCode != "MATH" || cs.Number != "100")
    //            {
    //                results.FirstOrDefault(re => re.RuleId == "MATH100" && re.Context == cs).Passed = false;
    //            }
    //        }
    //        return results;
    //    }
    //}
}
