using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPersonBenefitDeductionRepository : IPersonBenefitDeductionRepository
    {
        
        public class PersonBenefitDeductionSummaryRecord
        {
            public string benefitDeductionId;
            public DateTime? cancelDate;
            public DateTime? enrollmentDate;
            public DateTime? lastPayDate;
        }

        public List<PersonBenefitDeductionSummaryRecord> personBenefitDeductionSummaryRecords = new List<PersonBenefitDeductionSummaryRecord>()
        {
            new PersonBenefitDeductionSummaryRecord()
            {
                benefitDeductionId = "401k",
                cancelDate = null,
                enrollmentDate = new DateTime(2010, 1, 1),
                lastPayDate = null
            },
            new PersonBenefitDeductionSummaryRecord()
            {
                benefitDeductionId = "TRPF",
                cancelDate = null,
                enrollmentDate = new DateTime(2010, 1, 1),
                lastPayDate = null
            },
            new PersonBenefitDeductionSummaryRecord()
            {
                benefitDeductionId = "DENT",
                cancelDate = null,
                enrollmentDate = new DateTime(2010, 1, 1),
                lastPayDate = null
            },
            new PersonBenefitDeductionSummaryRecord()
            {
                benefitDeductionId = "BNUS",
                cancelDate = new DateTime(2010, 12, 15),
                enrollmentDate = new DateTime(2010, 1, 1),
                lastPayDate = new DateTime(2010, 12, 31)
            }
        };

        public IEnumerable<PersonBenefitDeduction> GetPersonBenefitDeductions(string personId)
        {
            if (personBenefitDeductionSummaryRecords == null) return null;
            var records = personBenefitDeductionSummaryRecords.Select(record =>
                new PersonBenefitDeduction(personId, record.benefitDeductionId, record.enrollmentDate.Value, record.cancelDate, record.lastPayDate));
            return records;
        }

        public async Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(string personId)
        {
            return await Task.FromResult(GetPersonBenefitDeductions(personId));
        }

        public IEnumerable<PersonBenefitDeduction> GetPersonBenefitDeductions(IEnumerable<string> personIds)
        {
            var personBenefitDeductions = new List<PersonBenefitDeduction>();
            foreach (var personId in personIds)
            {
                var pbds = GetPersonBenefitDeductions(personId);
                if (pbds != null)
                {
                    personBenefitDeductions.AddRange(pbds);
                }
            }
            return personBenefitDeductions;
        }

        public async Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(IEnumerable<string> personIds)
        {
            return await Task.FromResult(GetPersonBenefitDeductions(personIds));
        }
    }
}
