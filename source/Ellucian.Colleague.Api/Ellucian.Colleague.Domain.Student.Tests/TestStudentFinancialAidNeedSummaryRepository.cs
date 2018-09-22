// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentFinancialAidNeedSummaryRepository
    {
        private string[,] studentFinancialAidNeedSummary = {
                                            //GUID   CODE   DESCRIPTION
                                            { "Code2", "Student2", "2006", "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CPSSG"},
                                            { "Code3", "Student3", "2007", "b769e6a9-da86-47a9-ab21-b17198880439", "CORR"}, 
                                            { "Code4", "Student4", "2008", "e297656e-8d50-4c63-a2dd-0fcfc46647c4", "PROF"}, 
                                            { "Code5", "Student5", "2009", "8d0e291e-7246-4067-aff1-47ff6adc0392", "IAPP"},
                                            { "Code6", "Student6", "2010", "b91bbee8-88d1-4063-86e2-e7cb1865b45a", "SUPP"}, 
                                            { "Code7", "Student7", "2011", "4eaca2e7-fb59-44b6-be64-ce9e2ad73e81", "ISIR"}, 
                                            { "Code8", "Student8", "2012", "c76a6755-7594-4a24-a821-be2c8293ff78", "PROF"}, 
                                            { "Code9", "Student9", "2013", "95860685-7a99-476b-99f0-34066a5c20f6", "ISIR"}, 
                                            { "Code10", "Student10", "2014", "119cdf92-18b4-44f0-9fcb-6b3dd9702f67", "ISIR"}, 
                                            { "Code11", "Student11", "2015", "b772f098-77f3-48ef-b691-ea5b8aff5646", "ISIR"}, 
                                            { "Code12", "Student12", "2016", "e692812d-a23f-4601-a112-dc2d58389045", "ISIR"},
                                            };

        public IEnumerable<StudentNeedSummary> GetStudentFinancialAidNeedSummaries()
        {
            var studentFinancialAidNeedSummaryList = new List<StudentNeedSummary>();

            // There are 3 fields for each student financial aid need summary in the array
            var items = studentFinancialAidNeedSummary.Length / 5;

            for (int x = 0; x < items; x++)
            {
                studentFinancialAidNeedSummaryList.Add(
                    new StudentNeedSummary(studentFinancialAidNeedSummary[x, 1], studentFinancialAidNeedSummary[x, 2], studentFinancialAidNeedSummary[x, 3])
                    { 
                        FederalNeedAmount = 10000,
                        InstitutionalNeedAmount = 10000,
                        CsFederalIsirId = "1",
                        CsInstitutionalIsirId = "2",
                        BudgetDuration = 2,
                        FederalTotalExpenses = 2000,
                        FederalFamilyContribution = 100,
                        FederalTotalNeedReduction = 500,
                        InstitutionalTotalExpenses = 400,
                        InstitutionalFamilyContribution = 900,
                        InstitutionalTotalNeedReduction = 780
                    });
            }
            return studentFinancialAidNeedSummaryList;
        }

    }
}