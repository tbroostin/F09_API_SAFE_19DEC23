// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestFinancialAidApplicationRepository
    {
        private string[,] financialAidApplication = {
                                            //GUID   CODE   DESCRIPTION
                                            { "Code1", "Student1", "2013", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "ISIR", "1", "1"}, 
                                            { "Code2", "Student2", "2013", "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CPSSG", "2", "2"},
                                            { "Code3", "Student3", "2013", "b769e6a9-da86-47a9-ab21-b17198880439", "CORR", "3", "3"}, 
                                            { "Code4", "Student4", "2013", "e297656e-8d50-4c63-a2dd-0fcfc46647c4", "PROF", "4", "4"}, 
                                            { "Code5", "Student5", "2014", "8d0e291e-7246-4067-aff1-47ff6adc0392", "IAPP", "5", "1"},
                                            { "Code6", "Student6", "2014", "b91bbee8-88d1-4063-86e2-e7cb1865b45a", "SUPP", "6", "2"}, 
                                            { "Code7", "Student7", "2014", "4eaca2e7-fb59-44b6-be64-ce9e2ad73e81", "ISIR", "1", "3"}, 
                                            { "Code8", "Student8", "2014", "c76a6755-7594-4a24-a821-be2c8293ff78", "PROF", "2", "4"}, 
                                            { "Code9", "Student9", "2014", "95860685-7a99-476b-99f0-34066a5c20f6", "ISIR", "3", "1"}, 
                                            { "Code10", "Student10", "2014", "119cdf92-18b4-44f0-9fcb-6b3dd9702f67", "ISIR", "4", "2"}, 
                                            { "Code11", "Student11", "2015", "b772f098-77f3-48ef-b691-ea5b8aff5646", "ISIR", "5", "3"}, 
                                            { "Code12", "Student12", "2015", "e692812d-a23f-4601-a112-dc2d58389045", "ISIR", "6", "4"},
                                            { "Code13", "Student13", "2015", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "ISIR", "1", "1"}, 
                                            { "Code14", "Student14", "2015", "13660156-d481-4b3d-b617-92136979314c", "ISIR", "2", "2"}, 
                                            { "Code15", "Student15", "2015", "bcea6b4e-01ff-4d52-b4d5-7f6a5aa10820", "ISIR", "3", "3"}, 
                                            { "Code16", "Student16", "2016", "2198dcfa-cd4b-4df3-ab17-73b63ad595ee", "ISIR", "4", "4"},
                                            { "Code17", "Student17", "2016", "c37a2fde-4bac-4c84-b530-6b6f7d1f490a", "ISIR", "5", "4"}, 
                                            { "Code18", "Student18", "2016", "400dce82-2cdc-4990-a864-fc9943084d1a", "ISIR", "6", "3"}

                                            };

        public IEnumerable<Fafsa> GetFinancialAidApplications()
        {
            var financialAidApplicationList = new List<Fafsa>();

            // There are 3 fields for each financial aid application in the array
            var items = financialAidApplication.Length / 7;

            for (int x = 0; x < items; x++)
            {
                financialAidApplicationList.Add(
                    new Fafsa(
                        financialAidApplication[x, 0], financialAidApplication[x, 1], financialAidApplication[x, 2], financialAidApplication[x, 3]
                    )
                    {
                        FafsaPrimaryType = financialAidApplication[x, 4],
                        FafsaMaritalStatus = financialAidApplication[ x, 5 ],
                        FatherEducationLevel = financialAidApplication[ x, 6 ],
                        MotherEducationLevel = financialAidApplication[ x, 6 ],
                        ApplicantFamilySize = 1,
                        ParentFamilySize = 4,
                        ApplicantNumberInCollege = 2,
                        ParentNoInCollege = 2,
                        CorrectedFromId = "Y",
                        Type = financialAidApplication[x, 4],
                        ApplicationCompletedOn = new DateTime(2013, 10, 23),
                        StateOfLegalResidence = "MD",
                        WorkStudyInterest = "1",
                        IsAtRiskHomeless = true,
                        IsAdvancedDegreeStudent = true,
                        HasDependentChildren = true,
                        HasOtherDependents = true,
                        IsOrphanOrWard = true,
                        IsEmancipatedMinor = true,
                        HasGuardian = true,
                        IsHomelessBySchool = true,
                        IsHomelessByHud = true,
                        IsBornBeforeDate = true,
                        IsMarried = true,
                        IsVeteran = true,
                        IsActiveDuty = true,
                        ParentTaxReturnStatus = "1",
                        ParentAdjustedGrossIncome = 100000,
                        Parent1EducationLevel = "3",
                        Parent2EducationLevel = "3",
                        HousingCode = "1",
                        StudentTotalIncomeProfileCorrected = 100000,
                        StudentDependencyStatus = "D",
                        StudentTotalIncome = 50000,
                        StudentTaxReturnStatus = "1",
                        StudentAdjustedGrossIncome = 70000,
                        StudentEarnedIncome = 25000,
                        SpouseEarnedIncome = 10000,
                        ParentTotalIncomeProfileCorrected = 100000,
                        ParentPrimaryTotalIncome = 50000,
                        Parent1EarnedIncome = 25000,
                        Parent2EarnedIncome = 25000,
                        NoncustodialParentTotalIncomeProfile = 50000,
                        NoncustodialParent1EarnedIncomeProfile = 20000,
                        NoncustodialParent2EarnedIncomeProfile = 30000,
                        ApplicationCompletedOnProfile = new DateTime(2013, 02, 26),
                        StateOfLegalResidenceProfile = "MD",
                        HasDependentChildrenProfile = true,
                        IsWardProfile = true,
                        IsHomelessProfile = true,
                        IsVeteranProfile = true,
                        MaritalStatusProfile = "2",
                        StudentTaxReturnStatusProfile = "1",
                        StudentAdjustedGrossIncomeProfile = 10000,
                        StudentEarnedIncomeProfile = 5000,
                        SpouseEarnedIncomeProfile = 25000,
                        ParentTaxReturnStatusProfile = "1",
                        ParentAdjustedGrossIncomeProfile = 1000,
                        Parent1EducationLevelProfile = "3",
                        Parent2EducationLevelProfile = "3"
                    });
            }
            return financialAidApplicationList;
        }
    }
}