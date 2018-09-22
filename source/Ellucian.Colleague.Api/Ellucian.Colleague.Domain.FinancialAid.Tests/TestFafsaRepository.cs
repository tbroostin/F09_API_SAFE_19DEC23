/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestFafsaRepository : IFafsaRepository
    {
        public static List<string> inputStudentIds = new List<string>() { "0003914" };

        public class CsStudentRecord
        {
            public string studentId;
            public string awardYear;
            public List<string> isirRecordIds;
            public string federalIsirId;
            public int? federalFamilyContribution;
            public string insitutionIsirId;
            public int? institutionalFamilyContribution;
        }

        public List<CsStudentRecord> csStudentData = new List<CsStudentRecord>()
        {
            new CsStudentRecord()
            {
                studentId = "0003914",
                awardYear = "2013",
                isirRecordIds = new List<string>() {"1","2"},
                federalIsirId = "1",
                federalFamilyContribution = 54321,
                insitutionIsirId = "2",
                institutionalFamilyContribution = 12345
            }
        };

        public class IsirFafsaRecord
        {
            public string id;
            public string guid;
            public string studentId;
            public int? studentAgi;
            public int? parentAgi;
            public string correctionId;
            public string correctedFromId;
            public string isirType;
            public string awardYear;
            public string titleIVCode1;
            public string titleIVCode2;
            public string titleIVCode3;
            public string titleIVCode4;
            public string titleIVCode5;
            public string titleIVCode6;
            public string titleIVCode7;
            public string titleIVCode8;
            public string titleIVCode9;
            public string titleIVCode10;
            public string housingCode1;
            public string housingCode2;
            public string housingCode3;
            public string housingCode4;
            public string housingCode5;
            public string housingCode6;
            public string housingCode7;
            public string housingCode8;
            public string housingCode9;
            public string housingCode10;
        }

        public List<IsirFafsaRecord> isirFafsaData = new List<IsirFafsaRecord>()
        {
            new IsirFafsaRecord()
            {
                id = "1",
                guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                studentId = "0003914",
                studentAgi = 1111,
                parentAgi = 2222,
                correctionId = string.Empty,
                correctedFromId = string.Empty,
                isirType = "ISIR",
                awardYear = "2013",
                titleIVCode1 = "R5637",
                housingCode1 = "2"
            },
            new IsirFafsaRecord()
            {
                studentId = "0003914",
                id = "2",
                guid = "6a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                studentAgi = 1212,
                parentAgi = 2323,
                correctionId = "3",
                correctedFromId = string.Empty,
                isirType = "ISIR",
                awardYear = "2013",
                titleIVCode1 = "A3425",
                housingCode1 = "3",
                titleIVCode2 = "E563425",
                housingCode2 = "1",
                titleIVCode3 = "U5364",
                housingCode3 = "3"
            },
            new IsirFafsaRecord()
            {                
                studentId = "0003914",
                id = "3",
                guid = "5a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                studentAgi = 9999,
                parentAgi = 8888,
                correctionId = string.Empty,
                correctedFromId = "2",
                isirType = "CPSSG",
                awardYear = "2013",
                titleIVCode1 = "B3425",
                housingCode1 = "3",
                titleIVCode2 = "M563425",
                housingCode2 = "1",
                titleIVCode3 = "U5364",
                housingCode3 = "2",
                titleIVCode4 = "A23020",
                housingCode4 = "1",
                titleIVCode5 = "1241111",
                housingCode5 = string.Empty,
                titleIVCode6 = "222A4444",
                housingCode6 = "1",                
            }
        };

        public class IsirResultRecord
        {
            public string id;
            public bool isPellEligible;
        }

        public List<IsirResultRecord> isirResultData = new List<IsirResultRecord>()
        {
            new IsirResultRecord()
            {
                id = "1",
                isPellEligible = true
            },
            new IsirResultRecord()
            {
                id = "2",
                isPellEligible = false
            },
        };

        public Task<IEnumerable<Fafsa>> GetFafsaByStudentIdsAsync(IEnumerable<string> studentIds, string awardYear)
        {
            return GetFafsasAsync(studentIds, new List<string>() { awardYear });
        }

        public Task<IEnumerable<Fafsa>> GetFafsasAsync(IEnumerable<string> studentIds, IEnumerable<string> awardYearCodes)
        {
            var fafsaEntities = new List<Fafsa>();
            foreach (var studentId in studentIds)
            {
                fafsaEntities.AddRange(GetStudentFafsas(studentId, awardYearCodes));
            }
            return Task.FromResult(fafsaEntities.AsEnumerable());
        }

        private IEnumerable<Fafsa> GetStudentFafsas(string studentId, IEnumerable<string> awardYearCodes)
        {
            var fafsaEntities = new List<Fafsa>();
            foreach (var awardYear in awardYearCodes)
            {
                var csRecord = csStudentData.FirstOrDefault(c => c.awardYear == awardYear && c.studentId == studentId);
                if (csRecord != null)
                {
                    var isirFafsaRecords = isirFafsaData.Where(i => csRecord.isirRecordIds.Contains(i.id) && i.isirType != "PROF" && i.isirType != "IAPP");

                    var isirFafsaRecordIds = isirFafsaRecords.Select(i => i.id);
                    var isirResultRecords = isirResultData.Where(i => isirFafsaRecordIds.Contains(i.id));

                    var correctionIds = isirFafsaRecords.Where(i => !string.IsNullOrEmpty(i.correctionId)).Select(i => i.correctionId);
                    var isirFafsaCorrectionRecords = isirFafsaData.Where(i => correctionIds.Contains(i.id));

                    foreach (var isirFafsaRecord in isirFafsaRecords)
                    {
                        IsirFafsaRecord isirFafsaToUse = isirFafsaRecord;
                        IsirResultRecord isirResultToUse = isirResultRecords.FirstOrDefault(i => i.id == isirFafsaRecord.id);
                        if (!string.IsNullOrEmpty(isirFafsaRecord.correctionId))
                        {
                            isirFafsaToUse = isirFafsaCorrectionRecords.FirstOrDefault(c => c.id == isirFafsaRecord.correctionId);
                        }

                        if (isirFafsaToUse != null)
                            {
                        try
                        {
                            fafsaEntities.Add(BuildFafsa(studentId, csRecord.awardYear, csRecord, isirFafsaToUse, isirResultToUse));
                        }
                        catch (Exception) { }
                    }
                }
            }
            }
            return fafsaEntities;
        }

        private Fafsa BuildFafsa(string studentId, string awardYear, CsStudentRecord csStudentRecord, IsirFafsaRecord isirFafsaRecord, IsirResultRecord isirResultRecord)
        {
            var fafsaEntity = new Fafsa(isirFafsaRecord.id, awardYear, studentId, isirFafsaRecord.guid)
            {
                CalcResultsGuid = isirFafsaRecord.guid,
                ParentsAdjustedGrossIncome = isirFafsaRecord.parentAgi,
                StudentsAdjustedGrossIncome = isirFafsaRecord.studentAgi,
                IsPellEligible = (isirResultRecord != null) ? isirResultRecord.isPellEligible : false
            };
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode1))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode1, isirFafsaRecord.housingCode1, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode2) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode2))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode2, isirFafsaRecord.housingCode2, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode3) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode3))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode3, isirFafsaRecord.housingCode3, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode4) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode4))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode4, isirFafsaRecord.housingCode4, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode5) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode5))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode5, isirFafsaRecord.housingCode5, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode6) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode6))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode6, isirFafsaRecord.housingCode6, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode7) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode7))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode7, isirFafsaRecord.housingCode7, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode8) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode8))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode8, isirFafsaRecord.housingCode8, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode9) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode9))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode9, isirFafsaRecord.housingCode9, fafsaEntity);
            }
            if (!string.IsNullOrEmpty(isirFafsaRecord.titleIVCode10) && !fafsaEntity.HousingCodes.ContainsKey(isirFafsaRecord.titleIVCode10))
            {
                AddHousingCodesKeyValuePair(isirFafsaRecord.titleIVCode10, isirFafsaRecord.housingCode10, fafsaEntity);
            }

            if (csStudentRecord.federalIsirId == isirFafsaRecord.id || csStudentRecord.federalIsirId == isirFafsaRecord.correctedFromId)
            {
                fafsaEntity.IsFederallyFlagged = true;
                fafsaEntity.FamilyContribution = csStudentRecord.federalFamilyContribution;
            }
            if (csStudentRecord.insitutionIsirId == isirFafsaRecord.id || csStudentRecord.insitutionIsirId == isirFafsaRecord.correctedFromId)
            {
                fafsaEntity.IsInstitutionallyFlagged = true;
                fafsaEntity.InstitutionalFamilyContribution = csStudentRecord.institutionalFamilyContribution;
            }

            return fafsaEntity;
        }

        private static void AddHousingCodesKeyValuePair(string titleIV, string housingCode, Fafsa fafsaEntity)
        {
            switch (housingCode)
            {
                case "1":
                    fafsaEntity.HousingCodes.Add(titleIV, HousingCode.OnCampus);
                    break;
                case "2":
                    fafsaEntity.HousingCodes.Add(titleIV, HousingCode.WithParent);
                    break;
                case "3":
                    fafsaEntity.HousingCodes.Add(titleIV, HousingCode.OffCampus);
                    break;
                default:
                    fafsaEntity.HousingCodes.Add(titleIV, null);
                    break;
            }          
        }
    }
}
