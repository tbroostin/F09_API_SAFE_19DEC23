using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAcademicProgressAppealRepository : IAcademicProgressAppealRepository
    {
        public string id;

        public class SapAppealsRecords
        {
            public string studentId;
            public string AppealStatusCode;
            public DateTime AppealDate;
            public string AppealCounselorId;
        }

        public List<SapAppealsRecords> SapAppealsData = new List<SapAppealsRecords>()
        {
            new SapAppealsRecords()
            {
                studentId = "0003914",
                AppealStatusCode = "Pending",
                AppealDate = DateTime.Today,
                AppealCounselorId = "0000757",
            },
            new SapAppealsRecords()
            {
                studentId = "0003914",
                AppealStatusCode = "Accepted",
                AppealDate = DateTime.Today,
                AppealCounselorId = "0000757",
            },
        };

        public Task<IEnumerable<AcademicProgressAppeal>> GetStudentAcademicProgressAppealsAsync(string studentId)
        {
            //return new List<AcademicProgressAppeal>();

            var academicProgressAppeals = new List<AcademicProgressAppeal>();
            foreach (var appeal in SapAppealsData)
            {
                var singleAppeal = new AcademicProgressAppeal(studentId, "1234");
                singleAppeal.AppealCounselorId = appeal.AppealCounselorId;
                singleAppeal.AppealDate = appeal.AppealDate;
                singleAppeal.AppealStatusCode = appeal.AppealStatusCode;

                academicProgressAppeals.Add(singleAppeal);
            }
            return Task.FromResult(academicProgressAppeals.AsEnumerable());
        }
    }
}
