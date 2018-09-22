// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Planning.Tests.Services
{
    [TestClass]
    public class PersonNameServiceTests
    {
        [TestClass]
        public class GetHierarchyNameTests
        {
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonALL;
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonFL;
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonL;
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonOL;
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonPref;
            private Ellucian.Colleague.Domain.Base.Entities.Person testPersonInitials;
            private NameAddressHierarchy nameAddrHierarchy;

            [TestInitialize]
            public void Initialize()
            {
                nameAddrHierarchy = new NameAddressHierarchy("XXXXXXXX");

                PersonSetupWithNames();
            }

            [TestCleanup]
            public void CleanUp()
            {
                testPersonALL = null;
                testPersonFL = null;
                testPersonL = null;
                nameAddrHierarchy = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetHierarchyName_ThrowsException_PersonNull()
            {
                PersonHierarchyName result = PersonNameService.GetHierarchyName(null, nameAddrHierarchy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetHierarchyName_ThrowsException_HierarchyNull()
            {
                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, null);
            }


            [TestMethod]
            public void GetHierarchyName_EmptyNameList()
            {
                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("Mail Label Name Override", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual("LegalFirstName", result.FirstName);
                Assert.AreEqual("LegalMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // LAST FIRST MIDDLE (LFM)

            [TestMethod]
            public void GetHierarchyName_LFM_AllNameParts()
            {
                // Person has all 3 legal names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("lfm");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("LegalLastName, LegalFirstName L.", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual("LegalFirstName", result.FirstName);
                Assert.AreEqual("LegalMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_LFM_FirstLastOnly()
            {
                // Person has first and last legal names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("lfm");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonFL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("LegalLastName, LegalFirstName", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual("LegalFirstName", result.FirstName);
                Assert.AreEqual(string.Empty, result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }
            [TestMethod]
            public void GetHierarchyName_LFM_LastOnly()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("LFM");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("LegalLastName,", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.AreEqual(string.Empty, result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_LFM_InitialFirstMiddle()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("LFM");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonInitials, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, FI MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("LegalLastName, A. B.", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual("A", result.FirstName);
                Assert.AreEqual("B", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // BIRTH LAST FIRST MIDDLE (MA)

            [TestMethod]
            public void GetHierarchyName_MA_AllBirthNameParts()
            {
                // Person has all 3 birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("ma");
                nameAddrHierarchy.AddNameTypeHierarchy("LFM");


                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("BirthFirstName BirthMiddleName BirthLastName", result.FullName);
                Assert.AreEqual("BirthLastName", result.LastName);
                Assert.AreEqual("BirthFirstName", result.FirstName);
                Assert.AreEqual("BirthMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_MA_BirthFirstLastOnly()
            {
                // Person has first and last birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("MA");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonFL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("BirthFirstName BirthLastName", result.FullName);
                Assert.AreEqual("BirthLastName", result.LastName);
                Assert.AreEqual("BirthFirstName", result.FirstName);
                Assert.AreEqual(string.Empty, result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }
            [TestMethod]
            public void GetHierarchyName_MA_BirthLastOnly()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("MA");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("BirthLastName", result.FullName);
                Assert.AreEqual("BirthLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.AreEqual(string.Empty, result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_MA_NoBirthName_DefaultMailLabelName()
            {
                // Person does not have any name in list of name types.
                nameAddrHierarchy.AddNameTypeHierarchy("MA");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonOL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("Mail Label Name Override", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual(null, result.FirstName);
                Assert.AreEqual(null, result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }
            [TestMethod]
            public void GetHierarchyName_MA_BirthInitialFirstMiddleOnly()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("MA");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonInitials, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("C. D. BirthLastName", result.FullName);
                Assert.AreEqual("BirthLastName", result.LastName);
                Assert.AreEqual("C", result.FirstName);
                Assert.AreEqual("D", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // CHOSEN FIRST MI LAST (CH)


            [TestMethod]
            public void GetHierarchyName_CH_AllChosenNameParts()
            {
                // Person has all 3 birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("ch");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenFirstName ChosenMiddleName ChosenLastName", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("ChosenFirstName", result.FirstName);
                Assert.AreEqual("ChosenMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CH_ChosenFirstLastOnly()
            {
                // Person has first and last birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("CH");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonFL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenFirstName ChosenLastName", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("ChosenFirstName", result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }
            [TestMethod]
            public void GetHierarchyName_CH_ChosenLastOnly()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("CH");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenLastName", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CH_NoChosenNames_DefaultMailLabelName()
            {
                // Person does not have any name in list of name types.
                nameAddrHierarchy.AddNameTypeHierarchy("CH");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonOL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("Mail Label Name Override", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CH_ChosenInitialFirstMiddle()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("CH");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonInitials, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("E. F. ChosenLastName", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("E", result.FirstName);
                Assert.AreEqual("F", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // CHOSEN LAST FIRST MIDDLE (CHL)


            [TestMethod]
            public void GetHierarchyName_CHL_AllChosenNameParts()
            {
                // Person has all 3 birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("chl");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenLastName, ChosenFirstName C.", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("ChosenFirstName", result.FirstName);
                Assert.AreEqual("ChosenMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CHL_ChosenFirstLastOnly()
            {
                // Person has first and last birth names.
                nameAddrHierarchy.AddNameTypeHierarchy("JUNK");
                nameAddrHierarchy.AddNameTypeHierarchy("CHL");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonFL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenLastName, ChosenFirstName", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("ChosenFirstName", result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }
            [TestMethod]
            public void GetHierarchyName_CHL_ChosenLastOnly()
            {
                // Person has only last name.
                nameAddrHierarchy.AddNameTypeHierarchy("CHL");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("ChosenLastName,", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CHL_NoChosenNames_DefaultMailLabelName()
            {
                // Person does not have any name in list of name types.
                nameAddrHierarchy.AddNameTypeHierarchy("CHL");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonOL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(PersonHierarchyName));
                Assert.AreEqual("Mail Label Name Override", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            [TestMethod]
            public void GetHierarchyName_CHL_ChosenLastFirstInitialMiddleInitial()
            {

                var newPerson = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                    {
                        FirstName = "LegalFirstName",
                        MiddleName = "LegalMiddleName",
                        Prefix = "The Honorable",
                        Suffix = "Esq.",
                        BirthNameFirst = "BirthFirstName",
                        BirthNameLast = "BirthLastName",
                        BirthNameMiddle = "BirthMiddleName",
                        ChosenFirstName = "C",
                        ChosenMiddleName = "X",
                        ChosenLastName = "ChosenLastName",

                    };

                // Person does not have any name in list of name types.
                nameAddrHierarchy.AddNameTypeHierarchy("CHL");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(newPerson, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.AreEqual("ChosenLastName, C. X.", result.FullName);
                Assert.AreEqual("ChosenLastName", result.LastName);
                Assert.AreEqual("C", result.FirstName);
                Assert.AreEqual("X", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // PREFERRED OVERRIDE (PF)

            [TestMethod]
            public void GetHierarchyName_PF_PreferredOverride() 
            {

                testPersonALL.PreferredNameOverride = "Preferred Name Override";
                // Person does not have any name in list of name types.
                nameAddrHierarchy.AddNameTypeHierarchy("PF");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.AreEqual("Preferred Name Override", result.FullName);
                Assert.AreEqual("LegalLastName", result.LastName);
                Assert.AreEqual("LegalFirstName", result.FirstName);
                Assert.AreEqual("LegalMiddleName", result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            // FORMATTED NAME TYPES ("SP")

            [TestMethod]
            public void GetHierarchyName_FORMATTEDTYPE()
            {

                testPersonALL.AddFormattedName("DA", "Degree Audit Name");
                testPersonALL.AddFormattedName("SP", "Special Name");
                testPersonALL.AddFormattedName("OT", "Other");

                nameAddrHierarchy.AddNameTypeHierarchy("SP");
                nameAddrHierarchy.AddNameTypeHierarchy("CHL");

                PersonHierarchyName result = PersonNameService.GetHierarchyName(testPersonALL, nameAddrHierarchy);
                // Confirm trims are working correctly and name is returned in Last, First MI format
                Assert.AreEqual("Special Name", result.FullName);
                Assert.IsNull(result.LastName);
                Assert.IsNull(result.FirstName);
                Assert.IsNull(result.MiddleName);
                Assert.AreEqual("XXXXXXXX", result.HierarchyCode);
            }

            private void PersonSetupWithNames()
            {
                testPersonALL = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    FirstName = "LegalFirstName",
                    MiddleName = "LegalMiddleName",
                    Prefix = "The Honorable",
                    Suffix = "Esq.",
                    BirthNameFirst = "BirthFirstName",
                    BirthNameLast = "BirthLastName",
                    BirthNameMiddle = "BirthMiddleName",
                    ChosenFirstName = "ChosenFirstName",
                    ChosenMiddleName = "ChosenMiddleName",
                    ChosenLastName = "ChosenLastName",
                    MailLabelNameOverride = "Mail Label Name Override"

                };
                testPersonFL = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    FirstName = "LegalFirstName",
                    MiddleName = string.Empty,
                    Prefix = "The Honorable",
                    Suffix = "Esq.",
                    BirthNameFirst = "BirthFirstName",
                    BirthNameMiddle = null,
                    BirthNameLast = "BirthLastName",
                    ChosenFirstName = "ChosenFirstName",
                    ChosenMiddleName = null,
                    ChosenLastName = "ChosenLastName",
                    MailLabelNameOverride = "Mail Label Name Override",
                    PreferredNameOverride = "Preferred Name Override"

                };

                testPersonL = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    MiddleName = string.Empty,
                    Prefix = "The Honorable",
                    Suffix = "Esq.",
                    BirthNameLast = "BirthLastName",
                    ChosenLastName = "ChosenLastName",
                    MailLabelNameOverride = "Mail Label Name Override",
                    PreferredNameOverride = "Preferred Name Override"

                };

                testPersonOL = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    MailLabelNameOverride = "Mail Label Name Override",
                    PreferredNameOverride = "Preferred Name Override"

                };

                testPersonPref = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    FirstName = "LegalFirstName",
                    MiddleName = "LegalMiddleName",
                    Prefix = "The Honorable",
                    Suffix = "Esq.",
                    BirthNameFirst = "BirthFirstName",
                    BirthNameLast = "BirthLastName",
                    BirthNameMiddle = "BirthMiddleName",
                    ChosenFirstName = "ChosenFirstName",
                    ChosenMiddleName = "ChosenMiddleName",
                    ChosenLastName = "ChosenLastName",

                };

                testPersonInitials = new Ellucian.Colleague.Domain.Base.Entities.Person("0000001", "LegalLastName")
                {
                    FirstName = "A",
                    MiddleName = "B",
                    Prefix = "The Honorable",
                    Suffix = "Esq.",
                    BirthNameFirst = "C",
                    BirthNameLast = "BirthLastName",
                    BirthNameMiddle = "D",
                    ChosenFirstName = "E",
                    ChosenMiddleName = "F",
                    ChosenLastName = "ChosenLastName",

                };
            }
        }

    }
}





