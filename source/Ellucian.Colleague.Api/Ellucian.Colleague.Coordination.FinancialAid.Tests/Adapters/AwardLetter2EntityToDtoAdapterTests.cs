//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Web.Adapters;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class AwardLetter2EntityToDtoAdapterTests
    {
        private string studentId;
        private Domain.FinancialAid.Entities.AwardLetter2 inputAwardLetter;
        private Domain.Base.Entities.Person inputPerson;
        private Domain.FinancialAid.Entities.StudentAwardYear inputAwardYear;        
        private Dtos.FinancialAid.AwardLetter2 actualAwardLetter;

        private AwardLetter2EntityToDtoAdapter awardLetterEntityToDtoAdapter;

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0004791";

            inputAwardYear = new Domain.FinancialAid.Entities.StudentAwardYear(studentId, "2016");

            inputAwardLetter = new Domain.FinancialAid.Entities.AwardLetter2(studentId, inputAwardYear)
            {
                AwardLetterAnnualAwards = new List<Domain.FinancialAid.Entities.AwardLetterAnnualAward>()
                {
                    new Domain.FinancialAid.Entities.AwardLetterAnnualAward(){
                        AwardId = "Award 1",
                        AnnualAnnualAmount = 2600,
                        GroupName = "Awards"
                    },
                    new Domain.FinancialAid.Entities.AwardLetterAnnualAward(){
                        AwardId = "Award 2",
                        AnnualAnnualAmount = 500,
                        GroupName = "Awards"
                    },
                    new Domain.FinancialAid.Entities.AwardLetterAnnualAward(){
                        AwardId = "Award 3",
                        AnnualAnnualAmount = 20000,
                        GroupName = "Loans"
                    }
                },
                BudgetAmount = 30000,
                ClosingParagraph = "This is the closing pargraph",
                OpeningParagraph = "This is the opening paragraph",
                AwardYearDescription = "2016 Award year",
                AwardLetterParameterId = "1",
                StudentOfficeCode = "MAIN",
                ContactName = "John Doe",
                HousingCode = Domain.FinancialAid.Entities.HousingCode.OffCampus,
                EstimatedFamilyContributionAmount = 15000,
                NeedAmount = 15000,
                CreatedDate = new DateTime(2016, 04, 20),
                Id = "45",
                AcceptedDate = new DateTime(2016, 04, 22),

                StudentAddress = new List<string>()
                {
                    "Line 1",
                    "Line 2"
                }
            };

            inputPerson = new Domain.Base.Entities.Person(studentId, "Morales")
            {
                PreferredName = "Alexia Morales",
                FirstName = "Alexia"            
            };

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            awardLetterEntityToDtoAdapter = new AwardLetter2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void AwardLetterDtoIsNotNullTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsNotNull(actualAwardLetter);
        }

        [TestMethod]
        public void PlainProperties_EqualExpectedTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.AreEqual(inputAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
            Assert.AreEqual(inputAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
            Assert.AreEqual(inputAwardLetter.AwardLetterYear, actualAwardLetter.AwardLetterYear);
            Assert.AreEqual(inputAwardLetter.AwardYearDescription, actualAwardLetter.AwardYearDescription);
            Assert.AreEqual(inputAwardLetter.BudgetAmount, actualAwardLetter.BudgetAmount);
            Assert.AreEqual(inputAwardLetter.ClosingParagraph, actualAwardLetter.ClosingParagraph);
            Assert.AreEqual(inputAwardLetter.CreatedDate, actualAwardLetter.CreatedDate);
            Assert.AreEqual(inputAwardLetter.EstimatedFamilyContributionAmount, actualAwardLetter.EstimatedFamilyContributionAmount);
            Assert.AreEqual((Dtos.FinancialAid.HousingCode)inputAwardLetter.HousingCode, actualAwardLetter.HousingCode);
            Assert.AreEqual(inputAwardLetter.Id, actualAwardLetter.Id);
            Assert.AreEqual(inputAwardLetter.NeedAmount, actualAwardLetter.NeedAmount);
            Assert.AreEqual(inputAwardLetter.OpeningParagraph, actualAwardLetter.OpeningParagraph);
            Assert.AreEqual(inputAwardLetter.StudentId, actualAwardLetter.StudentId);            
            Assert.AreEqual(inputAwardLetter.StudentOfficeCode, actualAwardLetter.StudentOfficeCode);
        }

        [TestMethod]
        public void NullInputContactAddressAndContactNumber_NoContactAddressAssignedTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsFalse(actualAwardLetter.ContactAddress.Any());
        }

        [TestMethod]
        public void InputContactAddress_ContactAddressAssignedTest()
        {
            inputAwardLetter.ContactAddress = new List<string>(){"Address line 1", "Address line 2"};
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsTrue(actualAwardLetter.ContactAddress.Any());
        }

        [TestMethod]
        public void InputContactPhoneNumber_ContactAddressAssignedTest()
        {
            inputAwardLetter.ContactPhoneNumber = "222-222-2222";
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsTrue(actualAwardLetter.ContactAddress.Any());
        }

        [TestMethod]
        public void NoPreferredAddress_StudentAddressAssignedTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsTrue(actualAwardLetter.StudentAddress.Any());
        }

        [TestMethod]
        public void PreferredAddress_StudentAddressContainsMoreThanOneLineTest()
        {
            //inputPerson.PreferredAddress = new List<string>() { "1245 Fair Lakes Ct", "Fairfax", "VA", "22033" };
            inputAwardLetter.StudentAddress = new List<string>() { "1245 Fair Lakes Ct", "Fairfax", "VA", "22033" };
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsTrue(actualAwardLetter.StudentAddress.Any());
            Assert.IsTrue(actualAwardLetter.StudentAddress.Count > 1);
        }

        [TestMethod]
        public void NoAwardLetterGroups_NoneAssignedTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsFalse(actualAwardLetter.AwardLetterGroups.Any());
        }

        [TestMethod]
        public void NumberOfAwardLetterGroups_EqualsExpectedTest()
        {
            inputAwardLetter.AwardLetterGroups = new List<Domain.FinancialAid.Entities.AwardLetterGroup2>()
            {
                new AwardLetterGroup2("Awards", 1, GroupType.AwardCategories),
                new AwardLetterGroup2("Loans", 2, GroupType.AwardCategories)
            };

            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.AreEqual(inputAwardLetter.AwardLetterGroups.Count, actualAwardLetter.AwardLetterGroups.Count);
        }

        [TestMethod]
        public void NumberOfAwardLetterAnnualAwards_EqualsExpectedTest()
        {
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.AreEqual(inputAwardLetter.AwardLetterAnnualAwards.Count, actualAwardLetter.AwardLetterAnnualAwards.Count);
        }

        [TestMethod]
        public void NoAnnualAwardLetterAwards_NoneAssignedTest()
        {
            inputAwardLetter.AwardLetterAnnualAwards = new List<AwardLetterAnnualAward>();
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            Assert.IsFalse(actualAwardLetter.AwardLetterAnnualAwards.Any());
        }

        [TestMethod]
        public void AwardPeriods_MatchExpectedTest()
        {
            var period1 = new AwardLetterAwardPeriod()
                {
                    AwardId = "Award 1",
                    AwardDescription = "Award 1 descr",
                    AwardPeriodAmount = 2000,
                    ColumnName = "16/SP",
                    ColumnNumber = 2,
                    GroupName = "Awards",
                    GroupNumber = 1
                };
            var period2 = new AwardLetterAwardPeriod()
                {
                    AwardId = "Award 1",
                    AwardDescription = "Award 1 descr",
                    AwardPeriodAmount = 600,
                    ColumnName = "15/FA",
                    ColumnNumber = 1,
                    GroupName = "Awards",
                    GroupNumber = 1
                };
            inputAwardLetter.AwardLetterAnnualAwards.First().AwardLetterAwardPeriods = new List<AwardLetterAwardPeriod>(){ period1, period2 };
            //actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter, inputPerson);
            actualAwardLetter = awardLetterEntityToDtoAdapter.MapToType(inputAwardLetter);
            for (var i = 0; i < inputAwardLetter.AwardLetterAnnualAwards.First().AwardLetterAwardPeriods.Count; i++)
            {
                var expectedPeriod = inputAwardLetter.AwardLetterAnnualAwards.First().AwardLetterAwardPeriods[i];
                var actualPeriod = actualAwardLetter.AwardLetterAnnualAwards.First().AwardLetterAwardPeriods[i];
                Assert.AreEqual(expectedPeriod.AwardId, actualPeriod.AwardId);
                Assert.AreEqual(expectedPeriod.AwardDescription, actualPeriod.AwardDescription);
                Assert.AreEqual(expectedPeriod.AwardPeriodAmount, actualPeriod.AwardPeriodAmount);
                Assert.AreEqual(expectedPeriod.ColumnName, actualPeriod.ColumnName);
                Assert.AreEqual(expectedPeriod.ColumnNumber, actualPeriod.ColumnNumber);
                Assert.AreEqual(expectedPeriod.GroupName, actualPeriod.GroupName);
                Assert.AreEqual(expectedPeriod.GroupNumber, actualPeriod.GroupNumber);
            }                
        }
    }
}
