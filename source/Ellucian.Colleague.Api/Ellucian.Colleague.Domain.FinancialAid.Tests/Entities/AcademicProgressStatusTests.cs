// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressStatusTests
    {
        public string code;
        public string description;
        public AcademicProgressStatusCategory? category;
        public string explanation;

        public AcademicProgressStatus academicProgressStatus;

        public void AcademicProgressStatusTestsInitialize()
        {
            code = "S";
            description = "Satisfactory Code";
            category = AcademicProgressStatusCategory.Satisfactory;
            explanation = "This status indicates satisfactory progress";
        }

        [TestClass]
        public class AcademicProgressStatusConstructorTests : AcademicProgressStatusTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressStatusTestsInitialize();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new AcademicProgressStatus("", description);

            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DescRequiredTest()
            {
                new AcademicProgressStatus(code, "");
            }

            [TestMethod]
            public void CodeTest()
            {
                academicProgressStatus = new AcademicProgressStatus(code, description);
                Assert.AreEqual(code, academicProgressStatus.Code);
            }

            [TestMethod]
            public void DescTest()
            {
                academicProgressStatus = new AcademicProgressStatus(code, description);
                Assert.AreEqual(description, academicProgressStatus.Description);
            }

            [TestMethod]
            public void CategoryTest()
            {
                academicProgressStatus = new AcademicProgressStatus(code, description);
                academicProgressStatus.Category = category;
                Assert.AreEqual(category, academicProgressStatus.Category);
            }
            [TestMethod]
            public void ExplanationTest()
            {
                academicProgressStatus = new AcademicProgressStatus(code, description);
                academicProgressStatus.Explanation = explanation;
                Assert.AreEqual(explanation, academicProgressStatus.Explanation);
            }
        }
    }

}
