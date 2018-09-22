//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentNsldsInformationRepository : IStudentNsldsInformationRepository
    {
        public class FinAidRecord
        {
            public string recordKey;
            public List<string> nsldsIds;
        }
        public class IsirNsldsRecord
        {
            public string recordKey;
            public decimal? pellLifetimeEligibilityUsed;
        }

        public FinAidRecord finAidRecord = new FinAidRecord()
        {
            recordKey = "0004791",
            nsldsIds = new List<string>() { "1","15","45","88"}
        };

        public List<IsirNsldsRecord> nsldsRecords = new List<IsirNsldsRecord>()
        {
            new IsirNsldsRecord()
            {
                recordKey = "1",
                pellLifetimeEligibilityUsed = 345
            },
            new IsirNsldsRecord()
            {
                recordKey = "15",
                pellLifetimeEligibilityUsed = 23456
            },
            new IsirNsldsRecord()
            {
                recordKey = "45",
                pellLifetimeEligibilityUsed = null
            },
            new IsirNsldsRecord()
            {
                recordKey = "88",
                pellLifetimeEligibilityUsed = 34567
            }
        };

        public Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId)
        {
            string nsldsRecordId = finAidRecord.nsldsIds.First();
            var nsldsRecord = nsldsRecords.FirstOrDefault(r => r.recordKey == nsldsRecordId);
            return Task.FromResult(new StudentNsldsInformation(studentId, nsldsRecord.pellLifetimeEligibilityUsed));
        }
    }
}
