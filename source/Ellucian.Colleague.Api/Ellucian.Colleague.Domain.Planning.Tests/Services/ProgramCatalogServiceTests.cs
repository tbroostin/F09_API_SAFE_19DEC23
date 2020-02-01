using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Services;

namespace Ellucian.Colleague.Domain.Planning.Tests.Services
{
    [TestClass]
    public class ProgramCatalogServiceTests
    {
        [TestClass]
        public class DeriveDefaultCatalogTests
        {
            private string studentId;
            private Program p1;
            private Program p2;
            private Program p3;
            private ICollection<Catalog> allCatalogs = new List<Catalog>();
            private CatalogPolicy catalogPolicy;
            private List<StudentProgram> studentPrograms = new List<StudentProgram>();

            [TestInitialize]
            public void Initialize()
            {
                studentId = "1000000";

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
                p2 = new Program("BA.MATH", "Bachelors of Arts Math", depts, true, academicLevel, cf, true);
                p3 = new Program("BA.ART", "Bachelors of Arts Art", depts, true, academicLevel, cf, true);
                p3.Catalogs = new List<string>() { "2008", "2009", "2010", "2011", "2012", "2013", "2020" };

                //Set up the student programs
                StudentProgram sp1 = new StudentProgram(studentId, p1.Code, "2009");
                studentPrograms.Add(sp1);
                StudentProgram sp2 = new StudentProgram(studentId, p2.Code, "2008");
                studentPrograms.Add(sp2);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            public void Policy_CurrentProgramCatalog_ReturnValue()
            {
                catalogPolicy = CatalogPolicy.CurrentCatalogYear;
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, studentPrograms, allCatalogs, catalogPolicy);
                Assert.AreEqual("2010", defaultCatalog);
            }

            [TestMethod]
            public void Policy_StudentCatalogYear_ReturnValue()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, studentPrograms, allCatalogs, catalogPolicy);
                Assert.AreEqual("2008", defaultCatalog);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentHasZeroPrograms()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                IEnumerable<StudentProgram> emptyStudentPrograms = new List<StudentProgram>();
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, emptyStudentPrograms, allCatalogs, catalogPolicy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentHasNullPrograms()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, null, allCatalogs, catalogPolicy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ZeroCatalogs()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                ICollection<Catalog> emptyCatalogs = new List<Catalog>();
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, studentPrograms, emptyCatalogs, catalogPolicy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullCatalogs()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(p3, studentPrograms, null, catalogPolicy);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullProgram()
            {
                catalogPolicy = CatalogPolicy.StudentCatalogYear;
                string defaultCatalog = ProgramCatalogService.DeriveDefaultCatalog(null, studentPrograms, allCatalogs, catalogPolicy);
            }
        }
    }
}
