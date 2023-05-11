/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Reflection;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardLetter3Tests
    {
        [TestClass]
        public class AwardLetterDeafultConstructorTests
        {
            private AwardLetter3 awardLetter;

            [TestInitialize]
            public void Initialize()
            {
                awardLetter = new AwardLetter3();
            }

            [TestMethod]
            public void ObjectCreatedTest()
            {
                Assert.IsNotNull(awardLetter);
            }

            [TestMethod]
            public void AwardLetterAnnualAwards_InitializedToEmptyListTest()
            {
                Assert.IsTrue(!awardLetter.AwardLetterAnnualAwards.Any());
            }

            [TestMethod]
            public void AwardLetterGroups_InitializedToEmptyListTest()
            {
                Assert.IsTrue(!awardLetter.AwardLetterGroups.Any());
            }

            [TestMethod]
            public void StudentAddress_InitializedToEmptyListTest()
            {
                Assert.IsTrue(!awardLetter.StudentAddress.Any());
            }

        }

        [TestClass]
        public class AwardLetterConstructorTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private List<string> contactAddress;
            private FinancialAidOffice currentOffice;

            private AwardLetter3 awardLetter;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                currentOffice = new FinancialAidOffice("office");
                currentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
                {
                    new FinancialAid.Entities.FinancialAidConfiguration("office","2014") { AwardYearDescription = "2014/2014 award year" }
                });
                awardYear = new StudentAwardYear(studentId, "2014", currentOffice);
                contactAddress = new List<string>();

                awardLetter = new AwardLetter3(studentId, awardYear);
            }

            [TestMethod]
            public void NumberOfAttributesTest()
            {
                var properties = typeof(AwardLetter3).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(40, properties.Length);
            }

            [TestMethod]
            public void StudentId_EqualsTest()
            {
                Assert.AreEqual(studentId, awardLetter.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentId_RequiredTest()
            {
                new AwardLetter3("", awardYear);
            }

            [TestMethod]
            public void AwardYear_EqualsTest()
            {
                Assert.AreEqual(awardYear.Code, awardLetter.AwardLetterYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYear_RequiredTest()
            {
                new AwardLetter3(studentId, null);
            }

            [TestMethod]
            public void AcceptedDate_GetSetTest()
            {
                var acceptedDate = DateTime.Today;
                awardLetter.AcceptedDate = acceptedDate;
                Assert.AreEqual(acceptedDate, awardLetter.AcceptedDate);
            }

            [TestMethod]
            public void AccpetedDate_InitToNullTest()
            {
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.IsNull(testAwardLetter.AcceptedDate);
            }


            [TestMethod]
            public void OpeningParagraph_GetSetTest()
            {
                var openingParagraph = "This is an opening paragraph.";
                awardLetter.OpeningParagraph = openingParagraph;
                Assert.AreEqual(openingParagraph, awardLetter.OpeningParagraph);
            }

            [TestMethod]
            public void OpeningParagraphInitializedToEmptyTest()
            {
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.OpeningParagraph);
            }

            [TestMethod]
            public void ClosingParagraph_GetSetTest()
            {
                var closingParagraph = "This is a closing paragraph.";
                awardLetter.ClosingParagraph = closingParagraph;
                Assert.AreEqual(closingParagraph, awardLetter.ClosingParagraph);
            }

            [TestMethod]
            public void ClosingParagraphInitializedToEmptyTest()
            {
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public void ContactAddress_GetSetTest()
            {
                contactAddress.Add("AddressLine1");
                contactAddress.Add("AddressLine2");
                contactAddress.Add("AddressLind3");

                awardLetter.ContactAddress = contactAddress;
                foreach (var addressLine in contactAddress)
                {
                    Assert.IsTrue(awardLetter.ContactAddress.Contains(addressLine));
                }
            }

            [TestMethod]
            public void ContactPhone_GetSetTest()
            {
                var contactPhone = "847-847-4774";
                awardLetter.ContactPhoneNumber = contactPhone;
                Assert.AreEqual(contactPhone, awardLetter.ContactPhoneNumber);
            }

            [TestMethod]
            public void HousingCode_GetSetTest()
            {
                var housingCode = HousingCode.OffCampus;
                awardLetter.HousingCode = housingCode;
                Assert.AreEqual(housingCode, awardLetter.HousingCode);
            }

            [TestMethod]
            public void HousingCode_InitializedToNullTest()
            {
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.IsNull(awardLetter.HousingCode);
            }

            [TestMethod]
            public void BudgetAmount_GetSetTest()
            {
                var expenses = 12393;
                var adjustment = -4824;
                var budget = expenses + adjustment;
                awardLetter.BudgetAmount = budget;

                Assert.AreEqual(budget, awardLetter.BudgetAmount);
            }

            [TestMethod]
            public void BudgetAmountInitializedToZeroTest()
            {
                int expectedInitBudgetAmount = 0;
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(expectedInitBudgetAmount, testAwardLetter.BudgetAmount);
            }

            [TestMethod]
            public void EfcAmount_GetSetTest()
            {
                var efc = 3483;
                var adjustment = 100;
                var efcAmount = efc + adjustment;
                awardLetter.EstimatedFamilyContributionAmount = efcAmount;

                Assert.AreEqual(efcAmount, awardLetter.EstimatedFamilyContributionAmount);
            }

            [TestMethod]
            public void NullEfcAmount_GetSetTest()
            {
                awardLetter.EstimatedFamilyContributionAmount = null;

                Assert.AreEqual(null, awardLetter.EstimatedFamilyContributionAmount);
            }

            [TestMethod]
            public void EfcAmountInitializedToNullTest()
            {
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(null, testAwardLetter.EstimatedFamilyContributionAmount);
            }

            [TestMethod]
            public void NeedAmount_GetSetTest()
            {
                var needAmount = 98393;
                awardLetter.NeedAmount = needAmount;
                Assert.AreEqual(needAmount, awardLetter.NeedAmount);
            }

            [TestMethod]
            public void NeedAmountInitializedToZeroTest()
            {
                int expectedInitNeedAmount = 0;
                var testAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(expectedInitNeedAmount, testAwardLetter.NeedAmount);
            }

            [TestMethod]
            public void Id_GetSetTest()
            {
                string id = "awardLetter1";
                awardLetter.Id = id;
                Assert.AreEqual(id, awardLetter.Id);
            }

            [TestMethod]
            public void StudentName_GetSetTest()
            {
                string studentName = "Alexa Gonzalez";
                awardLetter.StudentName = studentName;
                Assert.AreEqual(studentName, awardLetter.StudentName);
            }

            [TestMethod]
            public void StudentAddress_GetSetTest()
            {
                var studentAddress = new List<string>() { "100 Maple Ave, Cherry Hill, NC" };
                awardLetter.StudentAddress = studentAddress;
                Assert.AreEqual(studentAddress, awardLetter.StudentAddress);
            }

            [TestMethod]
            public void AwardYearDescription_GetSetTest()
            {
                var description = "year description";
                awardLetter.AwardYearDescription = description;
                Assert.AreEqual(description, awardLetter.AwardYearDescription);
            }

            [TestMethod]
            public void CreatedDate_GetSetTest()
            {
                var createdDate = new DateTime(2018, 10, 5);
                awardLetter.CreatedDate = createdDate;
                Assert.AreEqual(createdDate, awardLetter.CreatedDate);
            }

            [TestMethod]
            public void AwardLetterParameterId_GetSetTest()
            {
                var awardLetterParameterId = "letter1";
                awardLetter.AwardLetterParameterId = awardLetterParameterId;
                Assert.AreEqual(awardLetterParameterId, awardLetter.AwardLetterParameterId);
            }

            [TestMethod]
            public void ContactName_GetSetTest()
            {
                var contactName = "letter1";
                awardLetter.ContactName = contactName;
                Assert.AreEqual(contactName, awardLetter.ContactName);
            }

            [TestMethod]
            public void StudentOfficeCode_GetSetTest()
            {
                var studentOfficeCode = "law";
                awardLetter.StudentOfficeCode = studentOfficeCode;
                Assert.AreEqual(studentOfficeCode, awardLetter.StudentOfficeCode);
            }

            [TestMethod]
            public void AwardLetterAnnualAwards_InitializedToEmptyListTest()
            {
                Assert.IsTrue(!awardLetter.AwardLetterAnnualAwards.Any());
            }

            [TestMethod]
            public void AwardLetterGroups_InitializedToEmptyListTest()
            {
                Assert.IsTrue(!awardLetter.AwardLetterGroups.Any());
            }
        }

        [TestClass]
        public class AwardLetterEqualsTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private FinancialAidOffice currentOffice;
            private DateTime? acceptedDate;
            private string openingParagraph;
            private string closingParagraph;
            private string contactName;
            private List<string> contactAddress;
            private string contactPhone;
            private HousingCode? housingCode;

            private int totalExpenses;
            private int expenseAdjustment;
            private int efc;
            private int efcAdjustment;
            private int need;

            private AwardLetter3 actualAwardLetter;
            private AwardLetter3 expectedAwardLetter;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                currentOffice = new FinancialAidOffice("office");
                currentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
                {
                    new FinancialAid.Entities.FinancialAidConfiguration("office","2014") { AwardYearDescription = "2014/2014 award year" }
                });
                awardYear = new StudentAwardYear(studentId, "2014", currentOffice);
                acceptedDate = DateTime.Today;
                openingParagraph = "This is an openingParagraph";
                closingParagraph = "This is a closing paragraph";
                
                contactName = "Ellucian University";
                contactAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" };
                contactPhone = "984-849-4844";
                
                totalExpenses = 3484;
                expenseAdjustment = -39;
                efc = 98;
                efcAdjustment = 0;
                need = 2339;
                
                housingCode = HousingCode.OnCampus;

                actualAwardLetter = new AwardLetter3(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };

                actualAwardLetter.BudgetAmount = totalExpenses + expenseAdjustment;
                actualAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);

                expectedAwardLetter = new AwardLetter3(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };

                expectedAwardLetter.BudgetAmount = totalExpenses + expenseAdjustment;
                expectedAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);
            }

            [TestMethod]
            public void SameAwardLetter_EqualTest()
            {
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffAwardYear_NotEqualTest()
            {
                var anotherAwardYear = new StudentAwardYear(studentId, "2015", currentOffice);
                expectedAwardLetter = new AwardLetter3(studentId, anotherAwardYear)
                {
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };
                expectedAwardLetter.BudgetAmount = totalExpenses + expenseAdjustment;
                expectedAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);

                Assert.AreNotEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void NewAwardLetter_EqualTest()
            {
                expectedAwardLetter = new AwardLetter3(studentId, awardYear);
                actualAwardLetter = new AwardLetter3(studentId, awardYear);
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffAcceptedDate_EqualTest()
            {
                expectedAwardLetter.AcceptedDate = null;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffOpeningParagraph_EqualTest()
            {
                expectedAwardLetter.OpeningParagraph = "foobar";
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffClosingParagraph_EqualTest()
            {
                expectedAwardLetter.ClosingParagraph = "foobar";
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffContactName_EqualTest()
            {
                expectedAwardLetter.ContactName = "foobar";
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffContactAddress_EqualTest()
            {
                expectedAwardLetter.ContactAddress = new List<string>() { "foobar" };
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffContactPhone_EqualTest()
            {
                expectedAwardLetter.ContactPhoneNumber = "foobar";
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffEfcAmount_EqualTest()
            {
                expectedAwardLetter.EstimatedFamilyContributionAmount = null;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffNeedAmount_EqualTest()
            {
                expectedAwardLetter.NeedAmount = 0;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

           [TestMethod]
            public void DiffHousingCode_EqualTest()
            {
                expectedAwardLetter.HousingCode = null;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void NullObj_NotEqualTest()
            {
                Assert.IsFalse(expectedAwardLetter.Equals(null));
            }

            [TestMethod]
            public void NonAwardLetterObj_NotEqualTest()
            {
                Assert.IsFalse(expectedAwardLetter.Equals(new Object()));
            }
        }

        [TestClass]
        public class GetHashCodeTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private FinancialAidOffice currentOffice;
            private DateTime? acceptedDate;
            private string openingParagraph;
            private string closingParagraph;
            private string contactName;
            private List<string> contactAddress;
            private string contactPhone;
            private int totalExpenses;
            private int expenseAdjustment;
            private int efc;
            private int efcAdjustment;
            private int need;
            private HousingCode? housingCode;

            private AwardLetter3 actualAwardLetter;
            private AwardLetter3 expectedAwardLetter;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                currentOffice = new FinancialAidOffice("office");
                currentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
                {
                    new FinancialAid.Entities.FinancialAidConfiguration("office","2014") { AwardYearDescription = "2014/2014 award year" }
                });
                awardYear = new StudentAwardYear(studentId, "2014", currentOffice);
                acceptedDate = DateTime.Today;
                openingParagraph = "This is an openingParagraph";
                closingParagraph = "This is a closing paragraph";
                
                contactName = "Ellucian University";
                contactAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" };
                contactPhone = "984-849-4844";
                
                totalExpenses = 3484;
                expenseAdjustment = -39;
                efc = 98;
                efcAdjustment = 0;
                need = 2339;
                
                housingCode = HousingCode.WithParent;

                actualAwardLetter = new AwardLetter3(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };

                actualAwardLetter.BudgetAmount = (totalExpenses + expenseAdjustment);
                actualAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);

                expectedAwardLetter = new AwardLetter3(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };

                expectedAwardLetter.BudgetAmount = (totalExpenses + expenseAdjustment);
                expectedAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);
            }

            [TestMethod]
            public void SameAwardLetter_EqualHashCodeTest()
            {
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void DiffStudentId_ThrowsExceptionTest()
            {
                expectedAwardLetter = new AwardLetter3("foobar", awardYear);
            }

            [TestMethod]
            public void DiffAwardYear_NotEqualHashCodeTest()
            {
                var anotherAwardYear = new StudentAwardYear(studentId, "2015", currentOffice);
                expectedAwardLetter = new AwardLetter3(studentId, anotherAwardYear)
                {
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    NeedAmount = need,
                    HousingCode = housingCode
                };
                expectedAwardLetter.BudgetAmount = (totalExpenses + expenseAdjustment);
                expectedAwardLetter.EstimatedFamilyContributionAmount = (efc + efcAdjustment);

                Assert.AreNotEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffAcceptedDate_EqualHashCodeTest()
            {
                expectedAwardLetter.AcceptedDate = null;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffOpeningParagraph_EqualHashCodeTest()
            {
                expectedAwardLetter.OpeningParagraph = "foobar";
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void NewAwardLetterObjectHasValidHashCode()
            {
                expectedAwardLetter = new AwardLetter3(studentId, awardYear);
                actualAwardLetter = new AwardLetter3(studentId, awardYear);

                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffClosingParagraph_EqualHashCodeTest()
            {
                expectedAwardLetter.ClosingParagraph = "foobar";
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffContactName_EqualHashCodeTest()
            {
                expectedAwardLetter.ContactName = "foobar";
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffContactAddress_EqualHashCodeTest()
            {
                expectedAwardLetter.ContactAddress = new List<string>() { "foobar" };
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffContactPhone_EqualHashCodeTest()
            {
                expectedAwardLetter.ContactPhoneNumber = "foobar";
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffBudgetAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.BudgetAmount = 0;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffEfcAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.EstimatedFamilyContributionAmount = null;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffNeedAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.NeedAmount = 0;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffHousingCode_EqualHashCodeTest()
            {
                expectedAwardLetter.HousingCode = null;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }
        }

        
    }
}

