// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class ProgramTests
    {
        [TestClass]
        public class ProgramConstructor
        {
            private string code;
            private string title;
            private string description;
            private string academicLevel;
            private bool isActive;
            private bool isGraduationAllowed;
            private IEnumerable<string> depts;
            private Program Program;
            private CreditFilter cf;

            [TestInitialize]
            public void Initialize()
            {
                code = "MATH.BS";
                title = "Bachelor of Science in Math";
                description = "This is a descriptive statement about the whole thing";
                academicLevel = "UG";
                isActive = true;
                isGraduationAllowed = true;
                cf = new CreditFilter() { AcademicLevels = new List<string>() { academicLevel } };
                depts = new List<string> { "FAKEDEPT1", "FAKEDEPT2", "FAKEDEPT3" };


                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                Program = new Program(code, title, depts, isActive, academicLevel, cf, isGraduationAllowed, description);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramCodeExceptionIfNull()
            {
                new Program(null, "junk", depts, isActive, academicLevel, cf, isGraduationAllowed);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramTitleExceptionIfNull()
            {
                new Program("junk", null, depts, isActive, academicLevel, cf, isGraduationAllowed);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentsExceptionIfNull()
            {
                new Program(code, title, null, isActive, academicLevel, cf, isGraduationAllowed);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevelExceptionIfNull()
            {
                new Program(code, title, depts, isActive, null, cf, isGraduationAllowed);
            }

            [TestMethod]
            public void ProgramCode()
            {
                Assert.AreEqual(code, Program.Code);
            }

            [TestMethod]
            public void ProgramTitle()
            {
                Assert.AreEqual(title, Program.Title);
            }

            [TestMethod]
            public void ProgramDepartments()
            {
                Assert.IsTrue(depts.SequenceEqual(Program.Departments));
            }

            [TestMethod]
            public void ProgramIsActive()
            {
                Assert.AreEqual(Program.IsActive, isActive);
            }

            [TestMethod]
            public void ProgramAcademicLevel()
            {
                Assert.AreEqual("UG", Program.AcademicLevelCode);
            }

            [TestMethod]
            public void ProgramCatalogs()
            {
                var catalogs = new List<string>() { "2012", "2013", "2014" };
                Assert.AreEqual(0, Program.Catalogs.Count());
                Program.Catalogs = catalogs;
                Assert.AreEqual(catalogs, Program.Catalogs);
            }

            [TestMethod]
            public void ProgramIsGraduationAllowed_True()
            {
                Assert.AreEqual(isGraduationAllowed, Program.IsGraduationAllowed);
            }

            [TestMethod]
            public void ProgramIsGraduationAllowed_False()
            {
                var newProgram = new Program(code, title, depts, isActive, academicLevel, cf, false, description);
                Assert.IsFalse(newProgram.IsGraduationAllowed);
            }
        }

        [TestClass]
        public class ProgramEquals
        {
            private string code;
            private string title;
            private IEnumerable<string> depts;
            private bool isActive;
            private bool isGraduationAllowed;
            private Program p1;
            private Program p2;
            private Program p3;
            private Program p4;
            private CreditFilter cf;
            private string academicLevel;

            [TestInitialize]
            public void Initialize()
            {
                code = "FOO";
                title = "Foobar";
                isActive = true;
                isGraduationAllowed = true;
                depts = new List<string> { "FAKEDEPT1", "FAKEDEPT2", "FAKEDEPT3" };
                cf = new CreditFilter() { AcademicLevels = new List<string>() { "UG" } };
                academicLevel = "UG";
                p1 = new Program(code, title, depts, isActive, academicLevel, cf, isGraduationAllowed);
                p2 = new Program(code, "junk", depts, isActive, academicLevel, cf, isGraduationAllowed);
                p3 = new Program("junk", title, depts, isActive, academicLevel, cf, isGraduationAllowed);
                p4 = new Program(code, title, depts, isActive, academicLevel, cf, isGraduationAllowed);
            }

            [TestMethod]
            public void ProgramSameCodesEqual()
            {
                Assert.IsTrue(p1.Equals(p4));
            }

            [TestMethod]
            public void ProgramDifferentCodeNotEqual()
            {
                Assert.IsFalse(p1.Equals(p3));
            }

            [TestMethod]
            public void ProgramDifferentTitleNotEqual()
            {
                Assert.IsFalse(p1.Equals(p2));
            }
        }

        [TestClass]
        public class ProgramGetHashCode
        {
            private string code;
            private string title;
            private bool isActive;
            private bool isGraduationAllowed;
            private IEnumerable<string> depts;
            private Program s1;
            private Program s2;
            private Program s3;
            private Program s4;
            private CreditFilter cf;
            private string academicLevel;

            [TestInitialize]
            public void Initialize()
            {
                code = "FOO";
                title = "Foobar";
                isActive = true;
                isGraduationAllowed = true;
                academicLevel = "UG";
                cf = new CreditFilter() { AcademicLevels = new List<string>() { academicLevel } };
                depts = new List<string> { "FAKEDEPT1", "FAKEDEPT2", "FAKEDEPT3" };
                s1 = new Program(code, title, depts, isActive, academicLevel, cf, isGraduationAllowed);
                s2 = new Program(code, "junk", depts, isActive, academicLevel, cf, isGraduationAllowed);
                s3 = new Program("junk", title, depts, isActive, academicLevel, cf, isGraduationAllowed);
                s4 = new Program(code, title, depts, isActive, academicLevel, cf, isGraduationAllowed);
            }

            [TestMethod]
            public void ProgramSameCodeHashEqual()
            {
                Assert.AreEqual(s1.GetHashCode(), s4.GetHashCode());
            }

            [TestMethod]
            public void ProgramDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(s1.GetHashCode(), s3.GetHashCode());
            }
        }

        [TestClass]
        public class GetCurrentCatalogCode
        {
            private Program p1;
            private Program p2;
            private Program p3;
            private ICollection<Catalog> allCatalogs = new List<Catalog>();

            [TestInitialize]
            public void Initialize()
            {
                // Set up some catalogs
                allCatalogs.Add(new Catalog("2008", new DateTime(2008, 1, 1)));
                allCatalogs.Add(new Catalog("2009", new DateTime(2009, 1, 1)));
                allCatalogs.Add(new Catalog("2010", new DateTime(2010, 1, 1)));
                Catalog catalog2012 = new Catalog("2012", new DateTime(2012, 1, 1));
                catalog2012.EndDate = new DateTime(2012, 12, 31);
                allCatalogs.Add(catalog2012);
                allCatalogs.Add(new Catalog("2020", new DateTime(2020, 1, 1)));

                //Set up programs to work with
                string academicLevel = "UG";
                CreditFilter cf = new CreditFilter() { AcademicLevels = new List<string>() { academicLevel } };
                IEnumerable<string> depts = new List<string> { "FAKEDEPT1", "FAKEDEPT2", "FAKEDEPT3" };
                p1 = new Program("BA.ENGL", "Bachelors of Arts English", depts, true, academicLevel, cf, true);
                p2 = new Program("BA.ENGL", "Bachelors of Arts English", depts, true, academicLevel, cf, true);
                p3 = new Program("BA.ENGL", "Bachelors of Arts English", depts, true, academicLevel, cf, true);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            public void NoCatalogs()
            {
                p3.Catalogs = new List<string>() { "2008", "2009", "2010", "2011", "2012", "2013", "2020" };
                Assert.IsNull(p3.GetCurrentCatalogCode(new List<Catalog>()));
            }

            [TestMethod]
            public void ReturnCurrentCatalogCode()
            {
                p1.Catalogs = new List<string>() { "2008", "2009", "2010", "2011", "2012", "2013", "2020" };
                Assert.AreEqual("2010", p1.GetCurrentCatalogCode(allCatalogs));
            }

            [TestMethod]
            public void ProgramWithNoCatalogCodes()
            {
                Assert.IsNull(p2.GetCurrentCatalogCode(allCatalogs));
            }

            [TestMethod]
            public void ProgramWithNoQualifyingCatalogCodes()
            {
                p3.Catalogs = new List<string>() { "2012", "2013", "2020" };
                Assert.IsNull(p3.GetCurrentCatalogCode(allCatalogs));
            }


        }
    }
}

