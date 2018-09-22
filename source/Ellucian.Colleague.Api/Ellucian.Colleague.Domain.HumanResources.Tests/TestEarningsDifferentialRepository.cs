/* Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEarningsDifferentialRepository
    {

        public class EarnDiffRecord
        {
            public string code;
            public string description;
        }

        public List<EarnDiffRecord> earnDiffRecords = new List<EarnDiffRecord>()
        {
            new EarnDiffRecord()
            {
                code = "LATE",
                description = "late shift",
            },
            new EarnDiffRecord()
            {
                code = "NITE",
                description = "overnight shift",
            },
            new EarnDiffRecord()
            {
                code = "HOLI",
                description = "holiday shift"
            }
        };

        public async Task<IEnumerable<EarningsDifferential>> GetEarningsDifferentialsAsync()
        {
            return await Task.FromResult(GetEarningsDifferentials());
        }

        public IEnumerable<EarningsDifferential> GetEarningsDifferentials()
        {
            var earningsDifferentials = earnDiffRecords.Select(d => new EarningsDifferential(d.code, d.description));
            return earningsDifferentials;
        }
    }
}
