/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentBudgetComponentRepository : IStudentBudgetComponentRepository
    {
        public static string studentId = "0003914";

        public class GetStudentBudgetComponentsResponse
        {
            public string year;
            public string studentId;
            public List<string> StudentBudgetComponents;
            public List<string> StuBgtComponentOrigAmts;
            public List<string> StuBgtComponentOvrAmts;
        }

        public class StudentBudgetRecord
        {
            public string budgetComponentCode;
            public int? campusBasedOriginalAmount;
            public int? campusBasedOverrideAmount;
        }

        public List<GetStudentBudgetComponentsResponse> responses = new List<GetStudentBudgetComponentsResponse>() {
            new GetStudentBudgetComponentsResponse() {
                year = "2018",
                studentId = "0003914",
                StudentBudgetComponents = new List<string>() { "Tuition", "Books", "Room", "Dining"},
                StuBgtComponentOrigAmts = new List<string>() { "40000", "1245", "0" },
                StuBgtComponentOvrAmts = new List<string>() { "", "1243", "345", "1000"}
            },
            new GetStudentBudgetComponentsResponse() {
                year = "2017",
                studentId = "0003914",
                StudentBudgetComponents = new List<string>() { "Books", "Room", "Equipment"},
                StuBgtComponentOrigAmts = new List<string>() { "876", "", "0" },
                StuBgtComponentOvrAmts = new List<string>() { "4321", ""}
            }
        };

        public Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            var studentBudgetComponents = new List<StudentBudgetComponent>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var budgetComponents = responses.FirstOrDefault(r => r.year == studentAwardYear.Code && r.studentId == studentId);
                if (budgetComponents != null && budgetComponents.StudentBudgetComponents.Any())
                {
                    for (int i = 0; i < budgetComponents.StudentBudgetComponents.Count; i++)
                    {
                        int origAmt = 0, overwriteAmt = 0;
                        bool origAmtSuccess = false, overwriteAmtSuccess = false;

                        //Sometimes there can be less original or ovewrite amount values in the associated lists than component codes, skip if so
                        try { origAmtSuccess = int.TryParse(budgetComponents.StuBgtComponentOrigAmts[i], out origAmt); } catch { /*just skip*/ }
                        try { overwriteAmtSuccess = int.TryParse(budgetComponents.StuBgtComponentOvrAmts[i], out overwriteAmt); } catch { /*just skip*/ }

                        studentBudgetComponents.Add(
                                new StudentBudgetComponent(
                                    studentAwardYear.Code,
                                    studentId,
                                    budgetComponents.StudentBudgetComponents[i],
                                    origAmtSuccess ? origAmt : 0)
                                {
                                    CampusBasedOverrideAmount = overwriteAmtSuccess ? overwriteAmt : (int?)null
                                });
                    }
                }
            }

            return Task.FromResult(studentBudgetComponents.AsEnumerable());
        }
    }
}
