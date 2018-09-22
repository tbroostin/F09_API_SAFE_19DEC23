/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardLetterTests
    {
        [TestClass]
        public class AwardLetterConstructorTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private List<string> contactAddress;
            private FinancialAidOffice currentOffice;

            private AwardLetter awardLetter;

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

                awardLetter = new AwardLetter(studentId, awardYear);
            }

            [TestMethod]
            public void NumberOfAttributesTest()
            {
                var properties = typeof(AwardLetter).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(20, properties.Length);
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
                new AwardLetter("", awardYear);
            }

            [TestMethod]
            public void AwardYear_EqualsTest()
            {
                Assert.AreEqual(awardYear, awardLetter.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYear_RequiredTest()
            {
                new AwardLetter(studentId, null);
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
                var testAwardLetter = new AwardLetter(studentId, awardYear);
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
                var testAwardLetter = new AwardLetter(studentId, awardYear);
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
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.ClosingParagraph);
            }

            [TestMethod]
            public void IsContactBlockActive_GetSetTest()
            {
                var isContactBlockActive = true;
                awardLetter.IsContactBlockActive = isContactBlockActive;
                Assert.AreEqual(isContactBlockActive, awardLetter.IsContactBlockActive);
            }

            [TestMethod]
            public void IsContactBlockInitializedToFalseTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsFalse(testAwardLetter.IsContactBlockActive);
            }

            [TestMethod]
            public void ContactName_GetSetTest()
            {
                var contactName = "Ellucian University";
                awardLetter.ContactName = contactName;
                Assert.AreEqual(contactName, awardLetter.ContactName);
            }

            [TestMethod]
            public void ContactNameInitializedToEmptyTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.ContactName);
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
            public void ContactAddressList_InitializedTest()
            {
                Assert.IsNotNull(awardLetter.ContactAddress);
                Assert.AreEqual(contactAddress.Count(), awardLetter.ContactAddress.Count());
            }

            [TestMethod]
            public void ContactPhone_GetSetTest()
            {
                var contactPhone = "847-847-4774";
                awardLetter.ContactPhoneNumber = contactPhone;
                Assert.AreEqual(contactPhone, awardLetter.ContactPhoneNumber);
            }

            [TestMethod]
            public void ContactPhoneInitializedToEmptyTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.ContactPhoneNumber);
            }

            [TestMethod]
            public void IsNeedBlockActive_GetSetTest()
            {
                var isNeedBlockActive = false;
                awardLetter.IsNeedBlockActive = isNeedBlockActive;
                Assert.AreEqual(isNeedBlockActive, awardLetter.IsNeedBlockActive);
            }

            [TestMethod]
            public void IsNeedBlockInitializedToFalseTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsFalse(testAwardLetter.IsNeedBlockActive);
            }

            [TestMethod]
            public void IsHousingCodeActive_GetSetTest()
            {
                var isHousingCodeActive = false;
                awardLetter.IsHousingCodeActive = isHousingCodeActive;
                Assert.AreEqual(isHousingCodeActive, awardLetter.IsHousingCodeActive);
            }

            [TestMethod]
            public void IsHousingCodeActive_InitializedToFalseTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsFalse(testAwardLetter.IsHousingCodeActive);
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
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsNull(awardLetter.HousingCode);
            }

            [TestMethod]
            public void BudgetAmount_GetSetTest()
            {
                var expenses = 12393;
                var adjustment = -4824;
                var budget = expenses + adjustment;
                awardLetter.SetBudgetAmount(expenses, adjustment);

                Assert.AreEqual(budget, awardLetter.BudgetAmount);
            }

            [TestMethod]
            public void BudgetAmountInitializedToZeroTest()
            {
                int expectedInitBudgetAmount = 0;
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(expectedInitBudgetAmount, testAwardLetter.BudgetAmount);
            }

            [TestMethod]
            public void EfcAmount_GetSetTest()
            {
                var efc = 3483;
                var adjustment = 100;
                var efcAmount = efc + adjustment;
                awardLetter.SetEstimatedFamilyContributionAmount(efc, adjustment);

                Assert.AreEqual(efcAmount, awardLetter.EstimatedFamilyContributionAmount);
            }

            [TestMethod]
            public void EfcAmountInitializedToZeroTest()
            {
                int expectedInitEfcAmount = 0;
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(expectedInitEfcAmount, testAwardLetter.EstimatedFamilyContributionAmount);
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
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(expectedInitNeedAmount, testAwardLetter.NeedAmount);
            }

            /// <summary>
            /// Test if award name title gets initialized to an emoty string
            /// </summary>
            [TestMethod]
            public void AwardNameTitle_InitializedToEmptyString()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.AwardNameTitle);
            }

            /// <summary>
            /// Test if award name title sets correctly
            /// </summary>
            [TestMethod]
            public void AwardNameTitle_GetSetTest()
            {
                var awardNameTitle = "Assistance";
                awardLetter.AwardNameTitle = awardNameTitle;
                Assert.AreEqual(awardNameTitle, awardLetter.AwardNameTitle);
            }

            /// <summary>
            /// Test if award total title gets initialized to an emoty string
            /// </summary>
            [TestMethod]
            public void AwardTotalTitle_InitializedToEmptyString()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.AreEqual(string.Empty, testAwardLetter.AwardTotalTitle);
            }

            /// <summary>
            /// Test if award total title sets correctly
            /// </summary>
            [TestMethod]
            public void AwardTotalTitle_GetSetTest()
            {
                var awardTotalTitle = "Total";
                awardLetter.AwardTotalTitle = awardTotalTitle;
                Assert.AreEqual(awardTotalTitle, awardLetter.AwardTotalTitle);
            }

            /// <summary>
            /// Test if AwardCategoriesGroups collection is not null once award 
            /// letter is created
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_IsNotNull()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsNotNull(testAwardLetter.AwardCategoriesGroups);
            }

            /// <summary>
            /// Test if award categories groups list is initialized
            /// as an empty list
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_InitializedAsEmptyList()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 0);
            }

            /// <summary>
            /// Test if award period column groups list is not null once the award
            /// letter is created
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_IsNotNull()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsNotNull(testAwardLetter.AwardPeriodColumnGroups);
            }

            /// <summary>
            /// Test if award period column groups list is initialized
            /// as an empty list
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_InitializedAsEmptyList()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                Assert.IsTrue(testAwardLetter.AwardPeriodColumnGroups.Count == 0);
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
            private bool isContactBlockActive;
            private string contactName;
            private List<string> contactAddress;
            private string contactPhone;
            private bool isNeedBlockActive;
            private bool isHousingCodeActive;
            private HousingCode? housingCode;

            private int totalExpenses;
            private int expenseAdjustment;
            private int efc;
            private int efcAdjustment;
            private int need;

            private AwardLetter actualAwardLetter;
            private AwardLetter expectedAwardLetter;

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
                isContactBlockActive = true;
                contactName = "Ellucian University";
                contactAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" };
                contactPhone = "984-849-4844";
                isNeedBlockActive = true;
                totalExpenses = 3484;
                expenseAdjustment = -39;
                efc = 98;
                efcAdjustment = 0;
                need = 2339;
                isHousingCodeActive = true;
                housingCode = HousingCode.OnCampus;

                actualAwardLetter = new AwardLetter(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };

                actualAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                actualAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);

                expectedAwardLetter = new AwardLetter(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };

                expectedAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);
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
                expectedAwardLetter = new AwardLetter(studentId, anotherAwardYear)
                {
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };
                expectedAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);

                Assert.AreNotEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void NewAwardLetter_EqualTest()
            {
                expectedAwardLetter = new AwardLetter(studentId, awardYear);
                actualAwardLetter = new AwardLetter(studentId, awardYear);
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
            public void DiffIsContactBlockActive_EqualTest()
            {
                expectedAwardLetter.IsContactBlockActive = false;
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
            public void DiffIsNeedBlockActive_EqualTest()
            {
                expectedAwardLetter.IsNeedBlockActive = false;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffBudgetAmount_EqualTest()
            {
                expectedAwardLetter.SetBudgetAmount(0, 0);
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffEfcAmount_EqualTest()
            {
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(0, 0);
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffNeedAmount_EqualTest()
            {
                expectedAwardLetter.NeedAmount = 0;
                Assert.AreEqual(expectedAwardLetter, actualAwardLetter);
            }

            [TestMethod]
            public void DiffIsHousingCodeActive_EqualTest()
            {
                expectedAwardLetter.IsHousingCodeActive = false;
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
            private bool isContactBlockActive;
            private string contactName;
            private List<string> contactAddress;
            private string contactPhone;
            private bool isNeedBlockActive;
            private int totalExpenses;
            private int expenseAdjustment;
            private int efc;
            private int efcAdjustment;
            private int need;
            private bool isHousingCodeActive;
            private HousingCode? housingCode;

            private AwardLetter actualAwardLetter;
            private AwardLetter expectedAwardLetter;

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
                isContactBlockActive = true;
                contactName = "Ellucian University";
                contactAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" };
                contactPhone = "984-849-4844";
                isNeedBlockActive = true;
                totalExpenses = 3484;
                expenseAdjustment = -39;
                efc = 98;
                efcAdjustment = 0;
                need = 2339;
                isHousingCodeActive = true;
                housingCode = HousingCode.WithParent;

                actualAwardLetter = new AwardLetter(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };

                actualAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                actualAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);

                expectedAwardLetter = new AwardLetter(studentId, awardYear)
                {
                    AcceptedDate = acceptedDate,
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };

                expectedAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);
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
                expectedAwardLetter = new AwardLetter("foobar", awardYear);
            }

            [TestMethod]
            public void DiffAwardYear_NotEqualHashCodeTest()
            {
                var anotherAwardYear = new StudentAwardYear(studentId, "2015", currentOffice);
                expectedAwardLetter = new AwardLetter(studentId, anotherAwardYear)
                {
                    OpeningParagraph = openingParagraph,
                    ClosingParagraph = closingParagraph,
                    IsContactBlockActive = isContactBlockActive,
                    ContactName = contactName,
                    ContactAddress = contactAddress,
                    ContactPhoneNumber = contactPhone,
                    IsNeedBlockActive = isNeedBlockActive,
                    NeedAmount = need,
                    IsHousingCodeActive = isHousingCodeActive,
                    HousingCode = housingCode
                };
                expectedAwardLetter.SetBudgetAmount(totalExpenses, expenseAdjustment);
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(efc, efcAdjustment);

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
                expectedAwardLetter = new AwardLetter(studentId, awardYear);
                actualAwardLetter = new AwardLetter(studentId, awardYear);

                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffClosingParagraph_EqualHashCodeTest()
            {
                expectedAwardLetter.ClosingParagraph = "foobar";
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffIsContactBlockActive_EqualHashCodeTest()
            {
                expectedAwardLetter.IsContactBlockActive = false;
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
            public void DiffIsNeedBlockActive_EqualHashCodeTest()
            {
                expectedAwardLetter.IsNeedBlockActive = false;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffBudgetAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.SetBudgetAmount(0, 0);
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffEfcAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.SetEstimatedFamilyContributionAmount(0, 0);
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffNeedAmount_EqualHashCodeTest()
            {
                expectedAwardLetter.NeedAmount = 0;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffIsHousingCodeActive_EqualHashCodeTest()
            {
                expectedAwardLetter.IsHousingCodeActive = false;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }

            [TestMethod]
            public void DiffHousingCode_EqualHashCodeTest()
            {
                expectedAwardLetter.HousingCode = null;
                Assert.AreEqual(expectedAwardLetter.GetHashCode(), actualAwardLetter.GetHashCode());
            }
        }

        [TestClass]
        public class AddRemoveAwardLetterGroupTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private FinancialAidOffice currentOffice;
            private List<string> contactAddress;

            private AwardLetter awardLetter;

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

                awardLetter = new AwardLetter(studentId, awardYear);
            }

            /// <summary>
            /// Test if award categories group gets added to the corresponding list
            /// and all properties match expected ones
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_AddGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count != 0);
                Assert.AreEqual("awardCat1", testAwardLetter.AwardCategoriesGroups.First().Title);
                Assert.AreEqual(0, testAwardLetter.AwardCategoriesGroups.First().SequenceNumber);
                Assert.AreEqual(GroupType.AwardCategories, testAwardLetter.AwardCategoriesGroups.First().GroupType);
            }

            /// <summary>
            /// Test if a group with the same sequence number but different title does not get added to the
            /// group list
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_AddSameSequenceNumberedGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
                testAwardLetter.AddAwardCategoryGroup("awardCat2", 0, GroupType.AwardCategories);

                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 1);
                Assert.IsTrue(!testAwardLetter.AwardCategoriesGroups.Select(acg => acg.Title).Contains("awardCat2"));
            }

            /// <summary>
            /// Test if an award categories group gets removed from the list
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_RemoveGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
                testAwardLetter.AddAwardCategoryGroup("awardCat2", 1, GroupType.AwardCategories);

                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 2);
                testAwardLetter.RemoveAwardCategoryGroup(0);
                Assert.IsTrue(!testAwardLetter.AwardCategoriesGroups.Select(acg => acg.Title).Contains("awardCat1"));
                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 1);
            }

            /// <summary>
            /// Test if award period column group gets added to the corresponding list
            /// and all properties match expected ones
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_AddGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("title1", 0, GroupType.AwardPeriodColumn);
                Assert.IsTrue(testAwardLetter.AwardPeriodColumnGroups.Count != 0);
                Assert.AreEqual("title1", testAwardLetter.AwardPeriodColumnGroups.First().Title);
                Assert.AreEqual(0, testAwardLetter.AwardPeriodColumnGroups.First().SequenceNumber);
                Assert.AreEqual(GroupType.AwardPeriodColumn, testAwardLetter.AwardPeriodColumnGroups.First().GroupType);
            }

            /// <summary>
            /// Test if a group with the same sequence number but different title does not get added to the
            /// group list
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_AddSameSequenceNumberedGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn1", 0, GroupType.AwardPeriodColumn);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn2", 0, GroupType.AwardPeriodColumn);

                Assert.IsTrue(testAwardLetter.AwardPeriodColumnGroups.Count == 1);
                Assert.IsTrue(!testAwardLetter.AwardPeriodColumnGroups.Select(acg => acg.Title).Contains("awardPeriodColumn2"));
            }

            /// <summary>
            /// Tests if an ArgumentException is thrown if the sequence number 
            /// argument passed to the AddAwardPeriodColumnGroup method is less than zero
            /// </summary>
            [ExpectedException(typeof(ArgumentException))]
            [TestMethod]
            public void SequenceNumberLessThanZero_AddAwardPeriodColumnGroupThrowsExceptionTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn1", -1, GroupType.AwardPeriodColumn);
            }

            /// <summary>
            /// Tests if an ArgumentException is thrown if the groupType argument passed to
            /// the AddAwardPeriodColumnGroup method does not match
            /// </summary>
            [ExpectedException(typeof(ArgumentException))]
            [TestMethod]
            public void AddAwardPeriodColumnGroup_WrongGroupThrowsExceptionTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn1", 0, GroupType.AwardCategories);
            }

            /// <summary>
            /// Tests if an ArgumentException is thrown if the sequence number 
            /// argument passed to the AddAwardCategoryGroup method is less than zero
            /// </summary>
            [ExpectedException(typeof(ArgumentException))]
            [TestMethod]
            public void SequenceNumberLessThanZero_AddAwardCategoryGroupThrowsExceptionTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", -1, GroupType.AwardCategories);
            }

            /// <summary>
            /// Tests if an ArgumentException is thrown if the groupType argument passed to
            /// the AddAwardCategoryGroup method does not match
            /// </summary>
            [ExpectedException(typeof(ArgumentException))]
            [TestMethod]
            public void AddAwardCategoryGroup_WrongGroupThrowsExceptionTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardPeriodColumn);
            }

            /// <summary>
            /// Test if an award categories group gets removed from the list
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_RemoveGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn1", 0, GroupType.AwardPeriodColumn);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn2", 1, GroupType.AwardPeriodColumn);

                Assert.IsTrue(testAwardLetter.AwardPeriodColumnGroups.Count == 2);
                testAwardLetter.RemoveAwardPeriodColumnGroup(0);
                Assert.IsTrue(!testAwardLetter.AwardPeriodColumnGroups.Select(acg => acg.Title).Contains("awardPeriodColumn1"));
                Assert.IsTrue(testAwardLetter.AwardPeriodColumnGroups.Count == 1);
            }

            /// <summary>
            /// Test if an award categories group gets removed from the list
            /// </summary>
            [TestMethod]
            public void AwardCategoryGroups_RemoveGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
                testAwardLetter.AddAwardCategoryGroup("awardCat2", 1, GroupType.AwardCategories);

                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 2);
                testAwardLetter.RemoveAwardCategoryGroup(0);
                Assert.IsTrue(!testAwardLetter.AwardCategoriesGroups.Select(acg => acg.Title).Contains("awardCat1"));
                Assert.IsTrue(testAwardLetter.AwardCategoriesGroups.Count == 1);
            }

            /// <summary>
            /// Tests if an attempt to remove an invalid group - with an invalid sequence number from the 
            /// AwardPeriodColumnGroups returns false
            /// </summary>
            [TestMethod]
            public void AwardPeriodColumnGroups_RemoveInvalidGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardPeriodColumnGroup("awardPeriodColumn1", 0, GroupType.AwardPeriodColumn);
                Assert.IsFalse(testAwardLetter.RemoveAwardPeriodColumnGroup(1));
            }

            /// <summary>
            /// Tests if an attempt to remove an invalid group - with an invalid sequence number from the 
            /// AwardCategoriesGroups returns false
            /// </summary>
            [TestMethod]
            public void AwardCategoriesGroups_RemoveInvalidGroupTest()
            {
                var testAwardLetter = new AwardLetter(studentId, awardYear);
                testAwardLetter.AddAwardCategoryGroup("awardCat1", 0, GroupType.AwardCategories);
                Assert.IsFalse(testAwardLetter.RemoveAwardCategoryGroup(1));
            }

        }
    }
}
