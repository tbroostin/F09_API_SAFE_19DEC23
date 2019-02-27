//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAwardLetterHistoryRepository : IAwardLetterHistoryRepository
    {
        #region AwardLetterParameters

        public class AwardLetterParameter
        {
            public string Id { get; set; }
            public string OpeningParagraph { get; set; }
            public string ClosingParagraph { get; set; }
            public bool IsOfficeBlockActive { get; set; }
            public bool IsNeedBlockActive { get; set; }
            public bool IsHousingCodeActive { get; set; }
            public string AwardCategoryGroup1Title { get; set; }
            public List<string> AwardCategoriesGroup1 { get; set; }
            public string AwardCategoryGroup2Title { get; set; }
            public List<string> AwardCategoriesGroup2 { get; set; }
            public string AwardCategoryGroup3Title { get; set; }
            public string AwardPeriodGroup1Title { get; set; }
            public List<string> AwardPeriodsGroup1 { get; set; }
            public string AwardPeriodGroup2Title { get; set; }
            public List<string> AwardPeriodsGroup2 { get; set; }
            public string AwardPeriodGroup3Title { get; set; }
            public List<string> AwardPeriodsGroup3 { get; set; }
            public string AwardPeriodGroup4Title { get; set; }
            public List<string> AwardPeriodsGroup4 { get; set; }
            public string AwardPeriodGroup5Title { get; set; }
            public List<string> AwardPeriodsGroup5 { get; set; }
            public string AwardPeriodGroup6Title { get; set; }
            public List<string> AwardPeriodsGroup6 { get; set; }

            public string AwardColumnTitle { get; set; }
            public string TotalColumnTitle { get; set; }
        }

        //Award Category codes and Award Period codes come from the StudentAwards/StudentAwardPeriods from TestStudentAwardRepository
        public List<AwardLetterParameter> awardLetterParameterData = new List<AwardLetterParameter>()
        {
            new AwardLetterParameter() 
            {
                Id = "UNDERGRAD",
                OpeningParagraph = "Opening Paragraph the first.",
                ClosingParagraph = "Closing Paragraph the first.",
                IsOfficeBlockActive = true,
                IsNeedBlockActive = true,
                IsHousingCodeActive = false,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",
                AwardCategoriesGroup1 = new List<string>() {"PELL" },
                AwardCategoryGroup2Title = "Group 2 Title",
                AwardCategoriesGroup2 = new List<string>() {"USTF", "GPLUS" },
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",
                AwardPeriodsGroup1 = new List<string>() {"14/FA"},
                AwardPeriodGroup2Title = "Period Group 2",
                AwardPeriodsGroup2 = new List<string>() {"13/WI"},
                AwardPeriodGroup3Title = "Period Group 3",
                AwardPeriodsGroup3 = new List<string>() {"12/WI"},
                AwardPeriodGroup4Title = "Period Group 4",
                AwardPeriodsGroup4 = new List<string>() {"13/SP"},
                AwardPeriodGroup5Title = "Period Group 5",
                AwardPeriodsGroup5 = new List<string>() {"13/SU"},
                AwardPeriodGroup6Title = "Period Group 6",
                AwardPeriodsGroup6 = new List<string>() {"12/FA"}
            },
            new AwardLetterParameter() 
            {
                Id = "GRADUATE",
                OpeningParagraph = "Opening Paragraph the second.",
                ClosingParagraph = "Closing Paragraph the second.",
                IsOfficeBlockActive = false,
                IsNeedBlockActive = true,
                IsHousingCodeActive = true,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",
                AwardCategoriesGroup1 = new List<string>() {"PELL" },
                AwardCategoryGroup2Title = "Group 2 Title",
                AwardCategoriesGroup2 = new List<string>() {"USTF", "GPLUS" },
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",
                AwardPeriodsGroup1 = new List<string>() {"14/FA"},
                AwardPeriodGroup2Title = "Period Group 2",
                AwardPeriodsGroup2 = new List<string>() {"13/WI"},
                AwardPeriodGroup3Title = "Period Group 3",
                AwardPeriodsGroup3 = new List<string>() {"12/WI"},
                AwardPeriodGroup4Title = "Period Group 4",
                AwardPeriodsGroup4 = new List<string>() {"13/SP"},
                AwardPeriodGroup5Title = "Period Group 5",
                AwardPeriodsGroup5 = new List<string>() {"13/SU"},
                AwardPeriodGroup6Title = "Period Group 6",
                AwardPeriodsGroup6 = new List<string>() {"12/FA"}
            },
            new AwardLetterParameter() 
            {
                Id = "EDUCATION",
                OpeningParagraph = "Opening Paragraph the third.",
                ClosingParagraph = "Closing Paragraph the third.",
                IsOfficeBlockActive = true,
                IsNeedBlockActive = false,
                IsHousingCodeActive = false,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",
                AwardCategoriesGroup1 = new List<string>() {"PELL" },
                AwardCategoryGroup2Title = "Group 2 Title",
                AwardCategoriesGroup2 = new List<string>() {"USTF", "GPLUS" },
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",
                AwardPeriodsGroup1 = new List<string>() {"14/FA"},
                AwardPeriodGroup2Title = "Period Group 2",
                AwardPeriodsGroup2 = new List<string>() {"13/WI"},
                AwardPeriodGroup3Title = "Period Group 3",
                AwardPeriodsGroup3 = new List<string>() {"12/WI"},
                AwardPeriodGroup4Title = "Period Group 4",
                AwardPeriodsGroup4 = new List<string>() {"13/SP"},
                AwardPeriodGroup5Title = "Period Group 5",
                AwardPeriodsGroup5 = new List<string>() {"13/SU"},
                AwardPeriodGroup6Title = "Period Group 6",
                AwardPeriodsGroup6 = new List<string>() {"12/FA"}
            },
            new AwardLetterParameter() 
            {
                Id = "ALTR",
                OpeningParagraph = "Opening Paragraph the first.",
                ClosingParagraph = "Closing Paragraph the first.",
                IsOfficeBlockActive = true,
                IsNeedBlockActive = true,
                IsHousingCodeActive = false,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",
                AwardCategoriesGroup1 = new List<string>() {"PELL" },
                AwardCategoryGroup2Title = "Group 2 Title",
                AwardCategoriesGroup2 = new List<string>() {"USTF", "GPLUS" },
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",
                AwardPeriodsGroup1 = new List<string>() {"14/FA"},
                AwardPeriodGroup2Title = "Period Group 2",
                AwardPeriodsGroup2 = new List<string>() {"13/WI"},
                AwardPeriodGroup3Title = "Period Group 3",
                AwardPeriodsGroup3 = new List<string>() {"12/WI"},
                AwardPeriodGroup4Title = "Period Group 4",
                AwardPeriodsGroup4 = new List<string>() {"13/SP"},
                AwardPeriodGroup5Title = "Period Group 5",
                AwardPeriodsGroup5 = new List<string>() {"13/SU"},
                AwardPeriodGroup6Title = "Period Group 6",
                AwardPeriodsGroup6 = new List<string>() {"12/FA"}
            }
        };
        #endregion

        #region AwardLetterParamsTransaction

        public class AwardLetterParamsTransaction
        {
            public string Year;
            public string StudentId;
            public List<string> LogMessages;
            public string Result;
        }

        public List<AwardLetterParamsTransaction> paramsTransactionData = new List<AwardLetterParamsTransaction>()
        {
            new AwardLetterParamsTransaction()
            {
                Year = "2012",
                StudentId = "0003914",
                LogMessages = new List<string>(),
                Result = "UNDERGRAD"
            },
            new AwardLetterParamsTransaction()
            {
                Year = "2013",
                StudentId = "0003914",
                LogMessages = new List<string>() {"Undergraduate rule failed"},
                Result = "GRADUATE"
            },
            new AwardLetterParamsTransaction()
            {
                Year = "2014",
                StudentId = "0003914",
                LogMessages = new List<string>() {"Undergraduate rule failed", "Graduate rule failed"},
                Result = "EDUCATION"
            },
            new AwardLetterParamsTransaction()
            {
                Year = "2015",
                StudentId = "0003914",
                LogMessages = new List<string>() {"Undergraduate rule failed", "Graduate rule failed"},
                Result = "ALTR"
            },
            new AwardLetterParamsTransaction()
            {
                Year = "2015",
                StudentId = "0003914",
                LogMessages = new List<string>() {"Undergraduate rule failed", "Graduate rule failed"},
                Result = "GRADUATE"
            }
        };

        #endregion

        #region StudentCsYear

        public class StudentCsYear
        {
            public string StudentId { get; set; }
            public string AwardYear { get; set; }
            public int TotalExpenses { get; set; }
            public int BudgetAdjustment { get; set; }
            public int Efc { get; set; }
            public int InstitutionAdjustment { get; set; }
            public int Need { get; set; }
            public int IsirFedId { get; set; }
        }

        public List<StudentCsYear> studentCsYearData = new List<StudentCsYear>()
        {
            new StudentCsYear()
            {
                StudentId = "0003914",
                AwardYear = "2014",
                TotalExpenses = 1234,
                BudgetAdjustment = -232,
                Efc = 1242,
                InstitutionAdjustment = 232,
                Need = 2339,
                IsirFedId = 0004880
            },
            new StudentCsYear()
            {
                StudentId = "0003914",
                AwardYear = "2013",
                TotalExpenses = 1234,
                BudgetAdjustment = 424,
                Efc = 1242,
                InstitutionAdjustment = -39,
                Need = 2339,
                IsirFedId = 3452
            },
            new StudentCsYear()
            {
                StudentId = "0003914",
                AwardYear = "2012",
                TotalExpenses = 1234,
                BudgetAdjustment = -232,
                Efc = 1242,
                InstitutionAdjustment = -39,
                Need = 23232,
                IsirFedId = 54637
            }
        };
        #endregion

        #region StudentYsYear
        public class StudentYsYear
        {
            public string studentId;
            public string awardYear;
            public DateTime? acceptedDate;
        }

        public List<StudentYsYear> studentYsYearData = new List<StudentYsYear>()
        {
            new StudentYsYear()
            {
                studentId = "0003914",
                awardYear = "2014",
                acceptedDate = null
            },
            new StudentYsYear()
            {
                studentId = "0003914",
                awardYear = "2013",
                acceptedDate = new DateTime(2013, 4, 9)
            },
            new StudentYsYear()
            {
                studentId = "0003914",
                awardYear = "2012",
                acceptedDate = new DateTime(2012, 5, 10)
            }
        };
        #endregion

        #region SystemParameters

        public class SystemParameters
        {
            public string InstitutionName;
            public List<string> Address;
            public string CityStateZip;
            public string PhoneNumber;
            // public string DefaultOfficeId;
            public string TitleIVCode;
        }

        public SystemParameters systemParametersData = new SystemParameters()
        {
            InstitutionName = "Ellucian University",
            Address = new List<string>() { "4375 Fair Lakes Court", "Building 1" },
            CityStateZip = "Fairfax, VA 22033",
            PhoneNumber = "555-555-5555",
            TitleIVCode = "G5678"
            // DefaultOfficeId = "DEFAULT"
        };
        #endregion

        #region AwardLetterHistory

        public class StudentAwardLetterHistory
        {
            public string AwardYear;
            public DateTime? AcceptedDate;
            public DateTime CreatedDate;
            public string AwardLetterParametersId;
            public string OpeningParagraph;
            public string ClosingParagraph;
            public int Cost;
            public int? EFC;
            public string HousingCode;
            public int Need;
            public string OfficeId;
            public string StudentId;
            public string Id;
            public string StudentName;
            public string PreferredName;
            public string StudentAddressLine1;
            public string StudentAddressLine2;
            public string StudentAddressLine3;
            public string StudentAddressLine4;
        }

        public List<StudentAwardLetterHistory> awardLetterHistoryData = new List<StudentAwardLetterHistory>()
        {            new StudentAwardLetterHistory()
            {
                StudentId = "0003914",
                AwardYear = "2015",
                AcceptedDate = new DateTime(2015, 8, 15),
                CreatedDate = new DateTime(2015, 8, 1),
                AwardLetterParametersId = "ALTR",
                OpeningParagraph = "This is the Opening Paragraph",
                ClosingParagraph = "This is the Closing Paragraph",
                Cost = 5000,
                EFC = 2500,
                Need = 2500,
                HousingCode = "O",
                OfficeId = "Main",
                Id = "67",
                StudentName = "Oliver",
                PreferredName = "Oliver Kane",
                StudentAddressLine1 = "148 Main Street",
                StudentAddressLine2 = "Smallville, PK",
                StudentAddressLine3 = "USA",
                StudentAddressLine4 = "99999"
            },
            new StudentAwardLetterHistory()
            {
                StudentId = "0003914",
                AwardYear = "2014",
                AcceptedDate = new DateTime(2014, 8, 15),
                CreatedDate = new DateTime(2014, 8, 1),
                AwardLetterParametersId = "ALTR",
                OpeningParagraph = "This is the Opening Paragraph",
                ClosingParagraph = "This is the Closing Paragraph",
                Cost = 5000,
                EFC = 2500,
                Need = 2500,
                HousingCode = "O",
                OfficeId = "Main",
                Id = "45",
                StudentName = "Oliver",
                PreferredName = "Oliver Kane",
                StudentAddressLine1 = "148 Main Street",
                StudentAddressLine2 = "Smallville, PK",
                StudentAddressLine3 = "USA",
                StudentAddressLine4 = "99999"
            },
            new StudentAwardLetterHistory()
            {
                StudentId = "0003914",
                AwardYear = "2012",
                AcceptedDate = null,
                CreatedDate = new DateTime(2012, 8, 1),
                AwardLetterParametersId = "ALTR",
                OpeningParagraph = "This is the Opening Paragraph",
                ClosingParagraph = "This is the Closing Paragraph",
                Cost = 5000,
                EFC = 2500,
                Need = 2500,
                HousingCode = "O",
                OfficeId = "Main",
                Id = "46",
                StudentName = "Oliver",
                PreferredName = "Oliver Kane",
                StudentAddressLine1 = "148 Main Street",
                StudentAddressLine2 = "Smallville, PK",
                StudentAddressLine3 = "USA",
                StudentAddressLine4 = "99999"
            }
        };

        #endregion

        public async Task <IEnumerable<AwardLetter2>> GetAwardLettersAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
 
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }

            //if the student has no year-specific financial aid data, return an empty list
            if (studentAwardYears.Count() == 0)
            {
                //logger.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId));
                return new List<AwardLetter2>();
            }

            //instantiate the return list
            var awardLetterEntities = new List<AwardLetter2>();

            foreach (var year in studentAwardYears)
            {
                try
                {
                    var awardLetterEntity = await Task.FromResult(BuildAwardLetter(studentId, year, allAwards));
                    awardLetterEntities.Add(awardLetterEntity);
                }
                catch (Exception e)
                {
                    //logger.Error(e, e.Message);
                }
            }
            return awardLetterEntities;
        }

        public async Task<AwardLetter2> GetAwardLetterAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord)
        {

            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (createAwardLetterHistoryRecord == true)
            {
                CreateAwardLetterHistoryRecord(studentId, studentAwardYear.Code);
            }

            return await Task.FromResult(BuildAwardLetter(studentId, studentAwardYear, allAwards));
            
        }
      
        public async Task<AwardLetter2> GetAwardLetterByIdAsync(string studentId, string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                throw new ArgumentNullException("studentAwardYears");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            return await Task.FromResult(BuildAwardLetter(studentId, recordId, studentAwardYears, allAwards));
        }

        private AwardLetter2 BuildAwardLetter(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            var awardLetterHistoryRecord = awardLetterHistoryData.FirstOrDefault(alhr => alhr.AwardYear == studentAwardYear.Code);

            var awardLetterEntity = new AwardLetter2(studentId, studentAwardYear);
            
            awardLetterEntity.AcceptedDate = awardLetterHistoryRecord.AcceptedDate;
            awardLetterEntity.AwardLetterParameterId = awardLetterHistoryRecord.AwardLetterParametersId;
            awardLetterEntity.CreatedDate = awardLetterHistoryRecord.CreatedDate;

            awardLetterEntity.OpeningParagraph = awardLetterHistoryRecord.OpeningParagraph;
            awardLetterEntity.ClosingParagraph = awardLetterHistoryRecord.ClosingParagraph;


            awardLetterEntity.NeedAmount = awardLetterHistoryRecord.Need;
            awardLetterEntity.BudgetAmount = awardLetterHistoryRecord.Cost;
            awardLetterEntity.EstimatedFamilyContributionAmount = awardLetterHistoryRecord.EFC.HasValue ? awardLetterHistoryRecord.EFC.Value : 0;

            awardLetterEntity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.HousingCode);
            awardLetterEntity.Id = awardLetterHistoryRecord.Id;

            return awardLetterEntity;
        }

        private AwardLetter2 BuildAwardLetter(string studentId, string recordId, IEnumerable<StudentAwardYear> awardYears, IEnumerable<Award> allAwards)
        {
            var awardLetterHistoryRecord = awardLetterHistoryData.FirstOrDefault(alhr => alhr.Id == recordId);

            var awardLetterEntity = new AwardLetter2(studentId, awardYears.FirstOrDefault(y => y.Code == awardLetterHistoryRecord.AwardYear));

            awardLetterEntity.AcceptedDate = awardLetterHistoryRecord.AcceptedDate;
            awardLetterEntity.AwardLetterParameterId = awardLetterHistoryRecord.AwardLetterParametersId;
            awardLetterEntity.CreatedDate = awardLetterHistoryRecord.CreatedDate;

            awardLetterEntity.OpeningParagraph = awardLetterHistoryRecord.OpeningParagraph;
            awardLetterEntity.ClosingParagraph = awardLetterHistoryRecord.ClosingParagraph;


            awardLetterEntity.NeedAmount = awardLetterHistoryRecord.Need;
            awardLetterEntity.BudgetAmount = awardLetterHistoryRecord.Cost;
            awardLetterEntity.EstimatedFamilyContributionAmount = awardLetterHistoryRecord.EFC.HasValue ? awardLetterHistoryRecord.EFC.Value : 0;

            awardLetterEntity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.HousingCode);
            awardLetterEntity.Id = awardLetterHistoryRecord.Id;

            awardLetterEntity.StudentName = awardLetterHistoryRecord.StudentName;
            awardLetterEntity.StudentAddress = new List<string>();
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.PreferredName))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.PreferredName);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine1))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine1);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine2))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine2);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine3))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine3);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine4))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine4);
            }

            return awardLetterEntity;
        }

        /// <summary>
        /// Helper method to translate a Housing Code. 
        /// Default code is OnCampus
        /// </summary>
        /// <param name="housingCode">Housing Code to translate</param>
        /// <returns></returns>
        private HousingCode TranslateHousingCode(string housingCode)
        {
            if (housingCode == null) housingCode = "";
            switch (housingCode.ToUpper())
            {
                case "1":
                    return HousingCode.OnCampus;

                case "2":
                    return HousingCode.WithParent;

                case "3":
                    return HousingCode.OffCampus;

                default:
                    return HousingCode.OnCampus;
            }
        }

        public void CreateAwardLetterHistoryRecord(string studentId, string awardYear)
        {
            return;
        }

        /// <summary>
        /// Method to test the Update of the Accepted Date
        /// </summary>
        /// <param name="awardLetter">The award letter to update</param>
        /// <param name="studentAwardYear">A student Award Year record</param>
        /// <param name="allAwards">A List of all student Awards</param>
        /// <returns></returns>
        public async Task<AwardLetter2> UpdateAwardLetterAsync(string studentId, AwardLetter2 awardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }
            if (awardLetter.AwardLetterYear == null)
            {
                throw new ArgumentNullException("AwardLetterYear");
            }

            if (awardLetter.StudentId != awardLetter.StudentId)
            {
                throw new ArgumentException("StudentIds of awardLetter and studentAwardYear do not match");
            }

            var originalAwardLetter = await Task.FromResult(BuildAwardLetter(studentId, studentAwardYear, allAwards));
            originalAwardLetter.AcceptedDate = DateTime.Today;

            return originalAwardLetter;
        }

        public async Task<IEnumerable<AwardLetter3>> GetAwardLetters2Async(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null)
            {
                throw new ArgumentNullException("studentAwardYears");
            }

            //if the student has no year-specific financial aid data, return an empty list
            if (studentAwardYears.Count() == 0)
            {
                //logger.Info(string.Format("Student {0} has a Financial Aid record, but no award year data", studentId));
                return new List<AwardLetter3>();
            }

            //instantiate the return list
            var awardLetterEntities = new List<AwardLetter3>();

            foreach (var year in studentAwardYears)
            {
                try
                {
                    var awardLetterEntity = await Task.FromResult(BuildAwardLetter2(studentId, year, allAwards));
                    awardLetterEntities.Add(awardLetterEntity);
                }
                catch (Exception e)
                {
                    //logger.Error(e, e.Message);
                }
            }
            return awardLetterEntities;
        }

        public async Task<AwardLetter3> GetAwardLetter2Async(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, bool createAwardLetterHistoryRecord)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (createAwardLetterHistoryRecord == true)
            {
                CreateAwardLetterHistoryRecord(studentId, studentAwardYear.Code);
            }

            return await Task.FromResult(BuildAwardLetter2(studentId, studentAwardYear, allAwards));
        }

        public async Task<AwardLetter3> GetAwardLetterById2Async(string studentId, string recordId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                throw new ArgumentNullException("studentAwardYears");
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            return await Task.FromResult(BuildAwardLetter2(studentId, recordId, studentAwardYears, allAwards));
        }

        public async Task<AwardLetter3> UpdateAwardLetter2Async(string studentId, AwardLetter3 studentAwardLetter, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            if (studentAwardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }
            if (studentAwardLetter.AwardLetterYear == null)
            {
                throw new ArgumentNullException("AwardLetterYear");
            }

            if (studentAwardLetter.StudentId != studentAwardYear.StudentId)
            {
                throw new ArgumentException("StudentIds of awardLetter and studentAwardYear do not match");
            }

            var originalAwardLetter = await Task.FromResult(BuildAwardLetter2(studentId, studentAwardYear, allAwards));
            originalAwardLetter.AcceptedDate = DateTime.Today;

            return originalAwardLetter;
        }

        private AwardLetter3 BuildAwardLetter2(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards)
        {
            var awardLetterHistoryRecord = awardLetterHistoryData.FirstOrDefault(alhr => alhr.AwardYear == studentAwardYear.Code);

            var awardLetterEntity = new AwardLetter3(studentId, studentAwardYear);

            awardLetterEntity.AcceptedDate = awardLetterHistoryRecord.AcceptedDate;
            awardLetterEntity.AwardLetterParameterId = awardLetterHistoryRecord.AwardLetterParametersId;
            awardLetterEntity.CreatedDate = awardLetterHistoryRecord.CreatedDate;

            awardLetterEntity.OpeningParagraph = awardLetterHistoryRecord.OpeningParagraph;
            awardLetterEntity.ClosingParagraph = awardLetterHistoryRecord.ClosingParagraph;


            awardLetterEntity.NeedAmount = awardLetterHistoryRecord.Need;
            awardLetterEntity.BudgetAmount = awardLetterHistoryRecord.Cost;
            awardLetterEntity.EstimatedFamilyContributionAmount = awardLetterHistoryRecord.EFC;

            awardLetterEntity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.HousingCode);
            awardLetterEntity.Id = awardLetterHistoryRecord.Id;

            return awardLetterEntity;
        }

        private AwardLetter3 BuildAwardLetter2(string studentId, string recordId, IEnumerable<StudentAwardYear> awardYears, IEnumerable<Award> allAwards)
        {
            var awardLetterHistoryRecord = awardLetterHistoryData.FirstOrDefault(alhr => alhr.Id == recordId);

            var awardLetterEntity = new AwardLetter3(studentId, awardYears.FirstOrDefault(y => y.Code == awardLetterHistoryRecord.AwardYear));

            awardLetterEntity.AcceptedDate = awardLetterHistoryRecord.AcceptedDate;
            awardLetterEntity.AwardLetterParameterId = awardLetterHistoryRecord.AwardLetterParametersId;
            awardLetterEntity.CreatedDate = awardLetterHistoryRecord.CreatedDate;

            awardLetterEntity.OpeningParagraph = awardLetterHistoryRecord.OpeningParagraph;
            awardLetterEntity.ClosingParagraph = awardLetterHistoryRecord.ClosingParagraph;


            awardLetterEntity.NeedAmount = awardLetterHistoryRecord.Need;
            awardLetterEntity.BudgetAmount = awardLetterHistoryRecord.Cost;
            awardLetterEntity.EstimatedFamilyContributionAmount = awardLetterHistoryRecord.EFC;

            awardLetterEntity.HousingCode = TranslateHousingCode(awardLetterHistoryRecord.HousingCode);
            awardLetterEntity.Id = awardLetterHistoryRecord.Id;

            awardLetterEntity.StudentName = awardLetterHistoryRecord.StudentName;
            awardLetterEntity.StudentAddress = new List<string>();
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.PreferredName))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.PreferredName);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine1))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine1);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine2))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine2);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine3))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine3);
            }
            if (!string.IsNullOrEmpty(awardLetterHistoryRecord.StudentAddressLine4))
            {
                awardLetterEntity.StudentAddress.Add(awardLetterHistoryRecord.StudentAddressLine4);
            }

            return awardLetterEntity;
        }
    }
}

