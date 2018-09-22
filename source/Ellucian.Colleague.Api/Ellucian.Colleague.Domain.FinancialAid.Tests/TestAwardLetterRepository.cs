//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAwardLetterRepository : IAwardLetterRepository
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


        public IEnumerable<AwardLetter> GetAwardLetters(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Fafsa> fafsaRecords)
        {
            var awardLetterEntities = new List<AwardLetter>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var studentCsYear = studentCsYearData.FirstOrDefault(scy => scy.AwardYear == studentAwardYear.Code);
                var studentYsYear = studentYsYearData.FirstOrDefault(syy => syy.awardYear == studentAwardYear.Code);
                var paramsTransaction = paramsTransactionData.FirstOrDefault(p => p.Year == studentAwardYear.Code);
                var fafsaRecord = fafsaRecords.FirstOrDefault(fr => fr.AwardYear == studentAwardYear.Code);
                if (paramsTransaction != null)
                {
                    var awardLetterParameters = awardLetterParameterData.FirstOrDefault(alp => alp.Id == paramsTransaction.Result);
                    if (awardLetterParameters != null)
                    {
                        var awardLetterEntity = new AwardLetter(studentId, studentAwardYear);
                        awardLetterEntities.Add(awardLetterEntity);

                        if (studentYsYear != null)
                        {
                            awardLetterEntity.AcceptedDate = studentYsYear.acceptedDate;
                        }

                        awardLetterEntity.OpeningParagraph = awardLetterParameters.OpeningParagraph;
                        awardLetterEntity.ClosingParagraph = awardLetterParameters.ClosingParagraph;

                        if (awardLetterParameters.IsNeedBlockActive && studentCsYear != null)
                        {
                            awardLetterEntity.IsNeedBlockActive = true;
                            awardLetterEntity.SetBudgetAmount(studentCsYear.TotalExpenses, studentCsYear.BudgetAdjustment);
                            awardLetterEntity.SetEstimatedFamilyContributionAmount(studentCsYear.Efc, studentCsYear.InstitutionAdjustment);
                            awardLetterEntity.NeedAmount = studentCsYear.Need;
                        }

                        if (awardLetterParameters.IsOfficeBlockActive)
                        {
                            awardLetterEntity.IsContactBlockActive = true;

                            awardLetterEntity.ContactName = systemParametersData.InstitutionName;
                            awardLetterEntity.ContactAddress = new List<string>();
                            systemParametersData.Address.ForEach(a => awardLetterEntity.ContactAddress.Add(a));
                            awardLetterEntity.ContactAddress.Add(systemParametersData.CityStateZip);
                            awardLetterEntity.ContactPhoneNumber = systemParametersData.PhoneNumber;

                            var currentOffice = studentAwardYear.CurrentOffice;
                            if (studentAwardYear.CurrentOffice != null)
                            {
                                if (!string.IsNullOrEmpty(currentOffice.Name))
                                {
                                    awardLetterEntity.ContactName = currentOffice.Name;
                                }

                                if (currentOffice.AddressLabel.Count() > 0)
                                {
                                    awardLetterEntity.ContactAddress = currentOffice.AddressLabel;
                                }

                                if (!string.IsNullOrEmpty(currentOffice.PhoneNumber))
                                {
                                    awardLetterEntity.ContactPhoneNumber = currentOffice.PhoneNumber;
                                }
                            }
                        }

                        if (awardLetterParameters.IsHousingCodeActive)
                        {
                            awardLetterEntity.IsHousingCodeActive = true;
                            if (fafsaRecord != null)
                            {
                                HousingCode? housingCode = null;
                                fafsaRecord.HousingCodes.TryGetValue(studentAwardYear.CurrentOffice.TitleIVCode, out housingCode);
                                awardLetterEntity.HousingCode = housingCode;
                            }
                        }

                        //add award category groups
                        awardLetterEntity.AwardNameTitle = awardLetterParameters.AwardColumnTitle;
                        awardLetterEntity.AwardTotalTitle = awardLetterParameters.TotalColumnTitle;

                        var awardCategoriesTitleList = new List<string>() { awardLetterParameters.AwardCategoryGroup1Title, awardLetterParameters.AwardCategoryGroup2Title };
                        var awardCategoriesGroupList = new List<List<string>>() { awardLetterParameters.AwardCategoriesGroup1, awardLetterParameters.AwardCategoriesGroup2 };

                        for (int i = 0; i < awardCategoriesGroupList.Count; i++)
                        {
                            if (awardCategoriesGroupList[i].Count() > 0)
                            {
                                awardLetterEntity.AddAwardCategoryGroup(awardCategoriesTitleList[i], i, GroupType.AwardCategories);

                                var currentGroup = awardLetterEntity.AwardCategoriesGroups.First(g => g.GroupType == GroupType.AwardCategories && g.SequenceNumber == i);
                                foreach (var category in awardCategoriesGroupList[i])
                                {
                                    currentGroup.AddGroupMember(category);
                                }
                            }
                        }

                        //add the final group of award categories
                        awardLetterEntity.NonAssignedAwardsGroup = new AwardLetterGroup("Group Title3", 2, GroupType.AwardCategories);

                        //add award period column groups
                        var awardPeriodTitleList = new List<string>()
                        {
                            awardLetterParameters.AwardPeriodGroup1Title,
                            awardLetterParameters.AwardPeriodGroup2Title,
                            awardLetterParameters.AwardPeriodGroup3Title,
                            awardLetterParameters.AwardPeriodGroup4Title,
                            awardLetterParameters.AwardPeriodGroup5Title,
                            awardLetterParameters.AwardPeriodGroup6Title                         
                        };

                        var awardPeriodGroupList = new List<List<string>>()
                        {
                            awardLetterParameters.AwardPeriodsGroup1,
                            awardLetterParameters.AwardPeriodsGroup2,
                            awardLetterParameters.AwardPeriodsGroup3,
                            awardLetterParameters.AwardPeriodsGroup4,
                            awardLetterParameters.AwardPeriodsGroup5,
                            awardLetterParameters.AwardPeriodsGroup6,
                        };

                        for (int i = 0; i < awardPeriodGroupList.Count; i++)
                        {
                            if (awardPeriodGroupList[i].Count > 0)
                            {
                                awardLetterEntity.AddAwardPeriodColumnGroup(awardPeriodTitleList[i], i, GroupType.AwardPeriodColumn);

                                var currentGroup = awardLetterEntity.AwardPeriodColumnGroups.First(g => g.GroupType == GroupType.AwardPeriodColumn && g.SequenceNumber == i);
                                foreach (var awardPeriod in awardPeriodGroupList[i])
                                {
                                    currentGroup.AddGroupMember(awardPeriod);
                                }
                            }
                        }

                    }
                }
            }

            return awardLetterEntities;
        }

        public AwardLetter GetAwardLetter(string studentId, StudentAwardYear studentAwardYear, Fafsa fafsaRecord)
        {
            var fafsaRecords = new List<Fafsa>() { fafsaRecord };
            return GetAwardLetters(studentId, new List<StudentAwardYear>() { studentAwardYear }, fafsaRecords).First();
        }


        public AwardLetter UpdateAwardLetter(AwardLetter awardLetter, StudentAwardYear studentAwardYear, Fafsa fafsaRecord)
        {
            var originalAwardLetter = GetAwardLetter(awardLetter.StudentId, studentAwardYear, fafsaRecord);

            var ysData = studentYsYearData.FirstOrDefault(ys => ys.awardYear == awardLetter.AwardYear.Code);
            if (ysData == null)
            {
                ysData = new StudentYsYear() { studentId = awardLetter.StudentId, awardYear = awardLetter.AwardYear.Code };
                studentYsYearData.Add(ysData);
            }

            ysData.acceptedDate = awardLetter.AcceptedDate;
            originalAwardLetter.AcceptedDate = awardLetter.AcceptedDate;

            return originalAwardLetter;
        }
    }
}
