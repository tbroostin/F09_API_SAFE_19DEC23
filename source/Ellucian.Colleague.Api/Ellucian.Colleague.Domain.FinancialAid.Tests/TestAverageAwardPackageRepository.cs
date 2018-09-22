//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAverageAwardPackageRepository : IAverageAwardPackageRepository
    {

        #region CsAcyrData
        public class CsAcyrRecord
        {
            public string AwardYear;
            public string StudentId;
            public string FederalIsirId;
        }

        public List<CsAcyrRecord> CsAcyrRecordList = new List<CsAcyrRecord>()
        {
            new CsAcyrRecord()
            {
                AwardYear = "2014",
                StudentId = "0004791",
                FederalIsirId = "5"
            },
            new CsAcyrRecord()
            {
                AwardYear = "2013",
                StudentId = "0004791",
                FederalIsirId = "4"
            },
            new CsAcyrRecord()
            {
                AwardYear = "2013",
                StudentId = "0004791",
                FederalIsirId = "3"
            },
            new CsAcyrRecord()
            {
                AwardYear = "2012",
                StudentId = "0004791",
                FederalIsirId = "2"
            },
        };
        #endregion

        #region GraduateLevelTransactionData

        public class GraduateLevelTransactionResponse
        {
            public string GraduateLevel;
            public string ErrorMessage;
        }

        public GraduateLevelTransactionResponse GraduateLevelTransactionResponseData = new GraduateLevelTransactionResponse()
        {
            GraduateLevel = "Y",
            ErrorMessage = ""
        };

        #endregion

        #region IsirFafsaData

        public class IsirFafsaRecord
        {
            public string Id;
            public string Type;
            public string AttendingGradSchool;
        }

        public List<IsirFafsaRecord> isirFafsaData = new List<IsirFafsaRecord>()
        {
            new IsirFafsaRecord()
            {
                Id = "5",
                Type = "ISIR",
                AttendingGradSchool = "Y"
            },

            new IsirFafsaRecord()
            {
                Id="2",
                Type="ISIR",
                AttendingGradSchool = "Y"
            }
            ,

            new IsirFafsaRecord()
            {
                Id="3",
                Type="CPSSG",
                AttendingGradSchool = "N"
            },

            new IsirFafsaRecord()
            {
                Id="4",
                Type="ISIR",
                AttendingGradSchool = "N"
            }
        };

        #endregion

        public Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId)) throw new ArgumentNullException("studentId");
            if (studentAwardYears == null || studentAwardYears.Count() == 0) throw new ArgumentNullException("studentAwardYears");

            var federalIsirId = string.Empty;
            var averageAwardPackages = new List<AverageAwardPackage>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                if (studentAwardYear.CurrentConfiguration != null)
                {
                    var csDataRecord = CsAcyrRecordList.FirstOrDefault(c => c.AwardYear == studentAwardYear.Code);
                    if (csDataRecord != null)
                    {
                        federalIsirId = csDataRecord.FederalIsirId;
                    }

                    var studentLevel = CalculateStudentLevel(federalIsirId);

                    if (studentLevel == "GR")
                    {
                        averageAwardPackages.Add(studentAwardYear.CurrentConfiguration.GraduatePackage);
                    }
                    else
                    {
                        averageAwardPackages.Add(studentAwardYear.CurrentConfiguration.UndergraduatePackage);
                    }
                }
            }
            return Task.FromResult(averageAwardPackages.AsEnumerable());
        }

        #region Helpers

        /// <summary>
        /// Calculates student's level
        /// </summary>
        /// <param name="federalIsirId">federal Isir id</param>
        /// <returns>student level</returns>
        private string CalculateStudentLevel(string federalIsirId)
        {
            var studentLevel = "";
            if (GraduateLevelTransactionResponseData.GraduateLevel.ToUpper() == "Y")
            {
                studentLevel = "GR";
            }
            else if (GraduateLevelTransactionResponseData.GraduateLevel.ToUpper() == "N")
            {
                studentLevel = "UG";
            }
            else if (!string.IsNullOrEmpty(federalIsirId))
            {
                var isirRecord = isirFafsaData.FirstOrDefault(i => i.Id == federalIsirId);
                if (isirRecord != null &&
                   (isirRecord.Type.ToUpper() == "ISIR" || isirRecord.Type.ToUpper() == "CPSSG") &&
                   isirRecord.AttendingGradSchool.ToUpper() == "Y")
                {
                    studentLevel = "GR";
                }
                else
                {
                    studentLevel = "UG";
                }
            }
            else
            {
                studentLevel = "UG";
            }
            return studentLevel;
        }

        #endregion

    }
}
