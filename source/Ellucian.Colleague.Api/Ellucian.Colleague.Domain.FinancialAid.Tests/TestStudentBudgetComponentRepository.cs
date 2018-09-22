/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
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

        public class CsStudentRecord
        {
            public string awardYear;
            public List<StudentBudgetRecord> budgetComponents;
        }

        public class StudentBudgetRecord
        {
            public string budgetComponentCode;
            public int? campusBasedOriginalAmount;
            public int? campusBasedOverrideAmount;
        }

        public List<CsStudentRecord> csStudentRecords = new List<CsStudentRecord>()
        {
            new CsStudentRecord()
            {
                awardYear = "2014",
                budgetComponents = new List<StudentBudgetRecord>()
                {
                    new StudentBudgetRecord()
                    {
                        budgetComponentCode = "TUITION",
                        campusBasedOriginalAmount = 12345,
                        campusBasedOverrideAmount = 54312
                    },
                    new StudentBudgetRecord()
                    {
                        budgetComponentCode = "WORK",
                        campusBasedOriginalAmount = 22222,
                        campusBasedOverrideAmount = null
                    }
                }
            },
            new CsStudentRecord()
            {
                awardYear = "2015",
                budgetComponents = new List<StudentBudgetRecord>()
                {
                    new StudentBudgetRecord()
                    {
                        budgetComponentCode = "TUITION",
                        campusBasedOriginalAmount = 33221,
                        campusBasedOverrideAmount = null
                    },
                    new StudentBudgetRecord()
                    {
                        budgetComponentCode = "WORK",
                        campusBasedOriginalAmount = 98765,
                        campusBasedOverrideAmount = 56789
                    }
                }
            }
        };

        public Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            return Task.FromResult(csStudentRecords
                .Where(csStudentRecord => studentAwardYears.Select(y => y.Code).Contains(csStudentRecord.awardYear))
                .SelectMany(csStudentRecord => csStudentRecord.budgetComponents
                    .Select(studentBudget =>
                        new StudentBudgetComponent(
                            csStudentRecord.awardYear,
                            studentId,
                            studentBudget.budgetComponentCode,
                            studentBudget.campusBasedOriginalAmount ?? 0)
                            {
                                CampusBasedOverrideAmount = studentBudget.campusBasedOverrideAmount
                            })));
        }
    }
}
