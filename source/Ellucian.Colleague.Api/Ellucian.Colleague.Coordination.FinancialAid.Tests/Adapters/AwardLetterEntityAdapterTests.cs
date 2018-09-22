/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class AwardLetterEntityAdapterTests
    {
        private string awardYear;
        private string studentId;

        private Domain.FinancialAid.Entities.StudentAwardYear studentAwardYear;
        private Domain.FinancialAid.Entities.Award award1;
        private Domain.FinancialAid.Entities.Award award2;
        private Domain.FinancialAid.Entities.Award award3;
        private Domain.FinancialAid.Entities.AwardCategory awardCategory1;
        private Domain.FinancialAid.Entities.AwardCategory awardCategory2;
        private Domain.FinancialAid.Entities.AwardCategory awardCategory3;
        private Domain.FinancialAid.Entities.AwardStatus awardStatus;
        private Domain.FinancialAid.Entities.StudentAwardPeriod awardPeriod1;
        private Domain.FinancialAid.Entities.StudentAwardPeriod awardPeriod2;
        private Domain.FinancialAid.Entities.StudentAwardPeriod awardPeriod3;
        private Domain.FinancialAid.Entities.StudentAwardPeriod awardPeriod4;
        private Domain.FinancialAid.Entities.StudentAwardPeriod awardPeriod5;

        private Domain.FinancialAid.Entities.AwardLetter awardLetterEntity;
        private List<Domain.FinancialAid.Entities.StudentAward> studentAwardEntities;
        private Domain.Base.Entities.Person person;

        private AwardLetterEntityToDtoAdapter awardLetterEntityAdapter;
        private Dtos.FinancialAid.AwardLetter testAwardLetterDto;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            awardYear = "2015";
            studentId = "0004791";

            studentAwardYear = new Domain.FinancialAid.Entities.StudentAwardYear(studentId, awardYear, new Domain.FinancialAid.Entities.FinancialAidOffice("office"));
            studentAwardYear.CurrentOffice.AddConfiguration(new Domain.FinancialAid.Entities.FinancialAidConfiguration("office", awardYear));

            awardLetterEntity = new Domain.FinancialAid.Entities.AwardLetter(studentId, studentAwardYear);
            awardLetterEntity.AcceptedDate = new DateTime(2014, 08, 10);
            awardLetterEntity.AwardNameTitle = "Award";
            awardLetterEntity.AwardTotalTitle = "Total assistance";
            awardLetterEntity.ClosingParagraph = "Closing paragraph";
            awardLetterEntity.ContactAddress = new List<string>()
            {
                "4375 Fair Lakes Ct. ",
                "Fairfax",
                "22033"
            };
            awardLetterEntity.ContactName = "John Doe";
            awardLetterEntity.ContactPhoneNumber = "703-255-6767";
            awardLetterEntity.IsContactBlockActive = true;
            awardLetterEntity.IsNeedBlockActive = true;
            awardLetterEntity.IsHousingCodeActive = true;
            awardLetterEntity.HousingCode = HousingCode.OffCampus;
            awardLetterEntity.SetBudgetAmount(40000, 5000);
            awardLetterEntity.SetEstimatedFamilyContributionAmount(10000, 2000);
            awardLetterEntity.NeedAmount = 23000;
            awardLetterEntity.OpeningParagraph = "Opening Pragraph";
            awardLetterEntity.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
            awardLetterEntity.AddAwardCategoryGroup("awardCat2", 1, GroupType.AwardCategories);
            awardLetterEntity.AddAwardPeriodColumnGroup("awardPeriodColumn1", 0, GroupType.AwardPeriodColumn);
            awardLetterEntity.AddAwardPeriodColumnGroup("awardPeriodColumn2", 1, GroupType.AwardPeriodColumn);
            awardLetterEntity.AddAwardPeriodColumnGroup("awardPeriodColumn3", 2, GroupType.AwardPeriodColumn);
            awardLetterEntity.NonAssignedAwardsGroup = new AwardLetterGroup("awardCat3", 2, GroupType.AwardCategories);

            //Populate groups with members
            awardLetterEntity.AwardCategoriesGroups.First().AddGroupMember("awardCat1");
            awardLetterEntity.AwardCategoriesGroups.FirstOrDefault(acg => acg.Title == "awardCat2").AddGroupMember("awardCat2");
            awardLetterEntity.AwardPeriodColumnGroups.First().AddGroupMember("14/FA");
            awardLetterEntity.AwardPeriodColumnGroups.FirstOrDefault(apcg => apcg.Title == "awardPeriodColumn2").AddGroupMember("15/SP");
            awardLetterEntity.AwardPeriodColumnGroups.FirstOrDefault(apcg => apcg.Title == "awardPeriodColumn3").AddGroupMember("15/SU");

            awardCategory1 = new AwardCategory("awardCat1", "awardCat1Description", AwardCategoryType.Scholarship);
            awardCategory2 = new AwardCategory("awardCat2", "awardCat2Description", AwardCategoryType.Loan);
            awardCategory3 = new AwardCategory("awardCat3", "awardCat3Description", AwardCategoryType.Grant);

            award1 = new Award("award1", "award1Description", awardCategory1);
            award2 = new Award("award2", "award2Description", awardCategory2);
            award3 = new Award("award3", "award3Description", awardCategory3);

            awardStatus = new AwardStatus("P", "Pending", AwardStatusCategory.Pending);

            studentAwardEntities = new List<Domain.FinancialAid.Entities.StudentAward>();

            var studentAward1 = new Domain.FinancialAid.Entities.StudentAward(studentAwardYear, studentId, award1, true);
            awardPeriod1 = new StudentAwardPeriod(studentAward1, "14/FA", awardStatus, false, false);
            awardPeriod1.AwardAmount = 2500;
            studentAward1.StudentAwardPeriods.Add(awardPeriod1);
            awardPeriod2 = new StudentAwardPeriod(studentAward1, "15/SP", awardStatus, false, false);
            awardPeriod2.AwardAmount = 300;
            studentAward1.StudentAwardPeriods.Add(awardPeriod2);
            studentAwardEntities.Add(studentAward1);


            var studentAward2 = new Domain.FinancialAid.Entities.StudentAward(studentAwardYear, studentId, award2, true);
            awardPeriod3 = new StudentAwardPeriod(studentAward2, "14/FA", awardStatus, false, false);
            awardPeriod3.AwardAmount = 345;
            studentAward2.StudentAwardPeriods.Add(awardPeriod3);
            awardPeriod4 = new StudentAwardPeriod(studentAward2, "15/SP", awardStatus, false, false);
            awardPeriod4.AwardAmount = 1200;
            studentAward2.StudentAwardPeriods.Add(awardPeriod4);
            awardPeriod5 = new StudentAwardPeriod(studentAward2, "15/SU", awardStatus, false, false);
            awardPeriod5.AwardAmount = 1000;
            studentAward2.StudentAwardPeriods.Add(awardPeriod5);
            studentAwardEntities.Add(studentAward2);

            var studentAward3 = new Domain.FinancialAid.Entities.StudentAward(studentAwardYear, studentId, award3, true);
            studentAward3.StudentAwardPeriods.Add(new StudentAwardPeriod(studentAward3, "14/FA", awardStatus, false, false));
            studentAward3.StudentAwardPeriods.Add(new StudentAwardPeriod(studentAward3, "15/SP", awardStatus, false, false));
            studentAward3.StudentAwardPeriods.Add(new StudentAwardPeriod(studentAward3, "15/SU", awardStatus, false, false));
            studentAward3.StudentAwardPeriods.Add(new StudentAwardPeriod(studentAward3, "14/WI", awardStatus, false, false));
            studentAwardEntities.Add(studentAward3);

            person = new Domain.Student.Entities.Applicant(studentId, "Flores");
            person.PreferredAddress = new List<string>()
            {
                "10080 Main Street",
                "Fairfax",
                "VA",
                "22030"
            };

            person.PreferredName = "Ana Flores";

            testAwardLetterDto = awardLetterEntityAdapter.MapToType(awardLetterEntity, studentAwardEntities, person);
        }

        [TestMethod]
        public void AwardYear_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.AwardYear.Code, testAwardLetterDto.AwardYearCode);
        }

        [TestMethod]
        public void StudentId_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.StudentId, testAwardLetterDto.StudentId);
        }

        [TestMethod]
        public void ContactAddress_NotNullTest()
        {
            Assert.IsNotNull(testAwardLetterDto.ContactAddress);
        }

        //TODO: Olga - change awardLetterEntity contact address should include name and phone number as well
        [TestMethod]
        public void ContactAddress_EqualsTest()
        {
            // +2 since ContactAddress in Dto contains Contact name and phone number lines
            Assert.AreEqual(awardLetterEntity.ContactAddress.Count + 2, testAwardLetterDto.ContactAddress.Count);
            for (var i = 0; i < awardLetterEntity.ContactAddress.Count; i++)
            {
                Assert.AreEqual(awardLetterEntity.ContactAddress[i], testAwardLetterDto.ContactAddress[i + 1].AddressLine);
            }
        }

        [TestMethod]
        public void ContactName_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.ContactName, testAwardLetterDto.ContactAddress.First().AddressLine);
        }

        [TestMethod]
        public void ContactPhoneNumber_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.ContactPhoneNumber, testAwardLetterDto.ContactAddress.Last().AddressLine);
        }

        [TestMethod]
        public void ContactBlockNotIsActive_EmptyContactAddressTest()
        {
            awardLetterEntity.IsContactBlockActive = false;
            testAwardLetterDto = awardLetterEntityAdapter.MapToType(awardLetterEntity, studentAwardEntities, person);

            Assert.IsTrue(testAwardLetterDto.ContactAddress.Count == 0);
        }

        [TestMethod]
        public void StudentAddress_NotNullTest()
        {
            Assert.IsNotNull(testAwardLetterDto.StudentAddress);
        }

        [TestMethod]
        public void StudentAddress_EqualsTest()
        {
            // +1 since the student address in the dto contains the student's preferred name in its first line
            Assert.AreEqual(person.PreferredAddress.Count + 1, testAwardLetterDto.StudentAddress.Count);
            for (var i = 0; i < person.PreferredAddress.Count; i++)
            {
                Assert.AreEqual(person.PreferredAddress[i], testAwardLetterDto.StudentAddress[i + 1].AddressLine);
            }
        }

        [TestMethod]
        public void StudentPreferredName_EqualsTest()
        {
            Assert.AreEqual(person.PreferredName, testAwardLetterDto.StudentAddress.First().AddressLine);
        }

        [TestMethod]
        public void AwardYearDescription_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.AwardYear.Description, testAwardLetterDto.AwardYearDescription);
        }

        [TestMethod]
        public void StudentName_EqualsTest()
        {
            Assert.AreEqual(person.PreferredName, testAwardLetterDto.StudentName);
        }

        [TestMethod]
        public void IsContactBlockActive_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.IsContactBlockActive, testAwardLetterDto.IsContactBlockActive);
        }

        [TestMethod]
        public void OpeningParagraph_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.OpeningParagraph, testAwardLetterDto.OpeningParagraph);
        }

        [TestMethod]
        public void IsNeedBlockActive_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.IsNeedBlockActive, testAwardLetterDto.IsNeedBlockActive);
        }

        [TestMethod]
        public void IsHousingCodeActive_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.IsHousingCodeActive, testAwardLetterDto.IsHousingCodeActive);
        }

        [TestMethod]
        public void HousingCode_EqualsTest()
        {
            var housingCode = (Colleague.Dtos.FinancialAid.HousingCode)awardLetterEntity.HousingCode;
            Assert.AreEqual(housingCode, testAwardLetterDto.HousingCode);
        }

        [TestMethod]
        public void HousingCode_EqualsNullTest()
        {
            awardLetterEntity.HousingCode = null;
            testAwardLetterDto = awardLetterEntityAdapter.MapToType(awardLetterEntity, studentAwardEntities, person);

            Assert.AreEqual(null, testAwardLetterDto.HousingCode);
        }

        [TestMethod]
        public void BudgetAmount_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.BudgetAmount, testAwardLetterDto.BudgetAmount);
        }

        [TestMethod]
        public void EstimatedFamilyContributionAmount_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.EstimatedFamilyContributionAmount, testAwardLetterDto.EstimatedFamilyContributionAmount);
        }

        [TestMethod]
        public void NeedAmount_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.NeedAmount, testAwardLetterDto.NeedAmount);
        }

        [TestMethod]
        public void AwardColumnHeader_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.AwardNameTitle, testAwardLetterDto.AwardColumnHeader);
        }

        [TestMethod]
        public void TotalColumnHeader_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.AwardTotalTitle, testAwardLetterDto.TotalColumnHeader);
        }

        [TestMethod]
        public void AwardPeriodColumnHeaders_EqualsTest()
        {
            var awardPeriodColumnGroupsCount = awardLetterEntity.AwardPeriodColumnGroups.Count;

            for (var i = 0; i < awardPeriodColumnGroupsCount; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod1ColumnHeader);
                }
                else if (i == 1)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod2ColumnHeader);
                }
                else if (i == 2)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod3ColumnHeader);
                }
                else if (i == 3)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod4ColumnHeader);
                }
                else if (i == 4)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod5ColumnHeader);
                }
                else if (i == 5)
                {
                    Assert.AreEqual(awardLetterEntity.AwardPeriodColumnGroups[i].Title, testAwardLetterDto.AwardPeriod6ColumnHeader);
                }

            }
        }

        [TestMethod]
        public void AwardTableRowsCount_EqualsTest()
        {
            // +1 for the total row
            Assert.AreEqual(studentAwardEntities.Count + 1, testAwardLetterDto.AwardTableRows.Count);
        }

        [TestMethod]
        public void AwardTableRows_EqualsTest()
        {
            for (var i = 0; i < studentAwardEntities.Count; i++)
            {
                Assert.AreEqual(studentAwardEntities[i].Award.Code, testAwardLetterDto.AwardTableRows[i].AwardId);
            }
        }

        [TestMethod]
        public void ClosingPargraph_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.ClosingParagraph, testAwardLetterDto.ClosingParagraph);
        }

        [TestMethod]
        public void AcceptedDate_EqualsTest()
        {
            Assert.AreEqual(awardLetterEntity.AcceptedDate, testAwardLetterDto.AcceptedDate);
        }

        [TestMethod]
        public void AwardCategoriesGroups_CorrectAssignmentTest()
        {
            foreach (var studentAwardEntity in studentAwardEntities)
            {
                var awardCategoryGroup = awardLetterEntity.AwardCategoriesGroups.FirstOrDefault(acg => acg.Members.Contains(studentAwardEntity.Award.AwardCategory.Code));
                var awardTableRow = testAwardLetterDto.AwardTableRows.FirstOrDefault(atr => atr.AwardId == studentAwardEntity.Award.Code);

                if (awardCategoryGroup != null)
                {
                    Assert.AreEqual(awardCategoryGroup.Title, awardTableRow.GroupName);
                    Assert.AreEqual(awardCategoryGroup.SequenceNumber, awardTableRow.GroupNumber);
                }
                //Must be the award that does not belong in any of the groups, it gets assigned to the last group
                else
                {
                    Assert.AreEqual(awardLetterEntity.NonAssignedAwardsGroup.Title, awardTableRow.GroupName);
                    Assert.AreEqual(awardLetterEntity.NonAssignedAwardsGroup.SequenceNumber, awardTableRow.GroupNumber);
                }
            }
        }

        [TestMethod]
        public void NumberAwardPeriodColumns_EqualsTest()
        {
            var studentAwardPeriods = studentAwardEntities.SelectMany(sae => sae.StudentAwardPeriods);
            var periodColumnGroupsList = new List<AwardLetterGroup>();
            foreach (var studentAwardPeriod in studentAwardPeriods)
            {
                var columnGroup = awardLetterEntity.AwardPeriodColumnGroups.FirstOrDefault(apcg => apcg.Members.Contains(studentAwardPeriod.AwardPeriodId));
                if (columnGroup != null && !periodColumnGroupsList.Select(pcg => pcg.SequenceNumber).Contains(columnGroup.SequenceNumber))
                {
                    periodColumnGroupsList.Add(columnGroup);
                }
            }

            Assert.AreEqual(periodColumnGroupsList.Count, testAwardLetterDto.NumberAwardPeriodColumns);
        }

        [TestMethod]
        public void TotalAllAwardsAmount_EqualsTest()
        {
            decimal? total = 0;
            var awardPeriods = studentAwardEntities.SelectMany(sae => sae.StudentAwardPeriods);
            foreach (var awardPeriod in awardPeriods)
            {
                total += awardPeriod.AwardAmount.HasValue ? awardPeriod.AwardAmount.Value : 0;
            }

            Assert.AreEqual(total, testAwardLetterDto.AwardTableRows.Last().Total);
        }

        [TestMethod]
        public void TotalSingleAwardAmount_EqualsTest()
        {
            foreach (var studentAwardEntity in studentAwardEntities)
            {
                decimal? awardTotal = 0;
                foreach (var awardPeriod in studentAwardEntity.StudentAwardPeriods)
                {
                    awardTotal += awardPeriod.AwardAmount.HasValue ? awardPeriod.AwardAmount.Value : 0;
                }

                Assert.AreEqual(awardTotal, testAwardLetterDto.AwardTableRows.FirstOrDefault(atr => atr.AwardId == studentAwardEntity.Award.Code).Total);
            }
        }

        [TestMethod]
        public void AwardPeriodColumnTotal_EqualsTest()
        {
            var studentAwardPeriods = studentAwardEntities.SelectMany(sae => sae.StudentAwardPeriods);
            for (var i = 0; i < awardLetterEntity.AwardPeriodColumnGroups.Count; i++)
            {
                var currentGroupPeriods = new List<StudentAwardPeriod>();
                decimal? awardPeriodColumnAmount = 0;
                foreach (var period in studentAwardPeriods)
                {
                    if (awardLetterEntity.AwardPeriodColumnGroups[i].Members.Contains(period.AwardPeriodId))
                    {
                        awardPeriodColumnAmount += period.AwardAmount.HasValue ? period.AwardAmount.Value : 0;
                    }
                }
                if (i == 0)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period1Amount);
                }
                else if (i == 1)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period2Amount);
                }
                else if (i == 2)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period3Amount);
                }
                else if (i == 3)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period4Amount);
                }
                else if (i == 4)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period5Amount);
                }
                else if (i == 5)
                {
                    Assert.AreEqual(awardPeriodColumnAmount, testAwardLetterDto.AwardTableRows.Last().Period6Amount);
                }
            }
        }
    }
}
