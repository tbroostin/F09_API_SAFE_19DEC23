/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestProfileApplicationRepository : IProfileApplicationRepository
    {
        public static string studentId = "0003914";

        public class CsStudentRecord
        {
            public string studentId;
            public string awardYear;
            public List<string> isirRecordIds;
            public string federalIsirId;
            public string federalFamilyContribution;
            public string instiutionIsirId;
            public int? institutionalFamilyContribution;
        }

        public List<CsStudentRecord> csStudentData = new List<CsStudentRecord>()
        {
            new CsStudentRecord()
            {
                studentId = studentId,
                awardYear = "2014",
                isirRecordIds = new List<string>() {"1","2"},
                federalIsirId = "1",
                federalFamilyContribution = "54321",
                instiutionIsirId = "2",
                institutionalFamilyContribution = 12345
            },
            new CsStudentRecord()
            {
                studentId = studentId,
                awardYear = "2015",
                isirRecordIds = new List<string>() {"3"},
                federalIsirId = "3",
                federalFamilyContribution = "54321",
                instiutionIsirId = "3",
                institutionalFamilyContribution = 12345
            },
        };

        public class IsirFafsaRecord
        {
            public string id;
            public string studentId;
            public string isirType;
            public string awardYear;
        }

        public List<IsirFafsaRecord> isirFafsaData = new List<IsirFafsaRecord>()
        {
            new IsirFafsaRecord()
            {
                id = "1",
                studentId = studentId,
                isirType = "ISIR",
                awardYear = "2014",
            },
            new IsirFafsaRecord()
            {
                id = "2",
                studentId = studentId,
                isirType = "PROF",
                awardYear = "2014",
            },
            new IsirFafsaRecord()
            {
                id = "3",
                studentId = studentId,
                isirType = "PROF",
                awardYear = "2015",
            }
        };

        public Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            var profileEntities = new List<ProfileApplication>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var csRecord = csStudentData.FirstOrDefault(c => c.awardYear == studentAwardYear.Code && c.studentId == studentId);
                if (csRecord != null)
                {
                    profileEntities.AddRange(isirFafsaData
                        .Where(i => csRecord.isirRecordIds.Contains(i.id) & i.isirType == "PROF")
                        .Select(isirFafsaRecord =>
                            {
                                var profile = new ProfileApplication(isirFafsaRecord.id, csRecord.awardYear, studentId)
                                {
                                    IsFederallyFlagged = (csRecord.federalIsirId == isirFafsaRecord.id),
                                    IsInstitutionallyFlagged = (csRecord.instiutionIsirId == isirFafsaRecord.id),
                                    InstitutionalFamilyContribution = (csRecord.instiutionIsirId == isirFafsaRecord.id) ? csRecord.institutionalFamilyContribution : null
                                };
                                if (profile.IsFederallyFlagged && !string.IsNullOrEmpty(csRecord.federalFamilyContribution))
                                {
                                    int familyContribution;
                                    if (int.TryParse(csRecord.federalFamilyContribution, out familyContribution))
                                    {
                                        profile.FamilyContribution = familyContribution;
                                    }
                                }
                                return profile;
                            }));
                }
            }

            return Task.FromResult(profileEntities.AsEnumerable());
        }
    }
}
