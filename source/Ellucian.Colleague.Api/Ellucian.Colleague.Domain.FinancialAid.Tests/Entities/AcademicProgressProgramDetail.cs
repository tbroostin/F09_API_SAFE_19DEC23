/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AcademicProgressProgramDetailTests
    {
        public string code;
        public decimal? maxCredits;
        public decimal? minCredits;

        public AcademicProgressProgramDetail academicProgramDetail;

        [TestInitialize]
        public void Initialize()
        {
            code = "Economics";
            maxCredits = 180.0m;
            minCredits = 120.0m;

            academicProgramDetail = new AcademicProgressProgramDetail(code, maxCredits, minCredits);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(academicProgramDetail);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullProgramCode_ThrowsArgumentNullExceptionTest()
        {
            new AcademicProgressProgramDetail(null, maxCredits, minCredits);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LessThanZeroMaxCredits_ThrowsArgumentExceptionTest()
        {
            new AcademicProgressProgramDetail(code, -67.0m, minCredits);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LessThanZeroMinCredits_ThrowsArgumentExceptionTest()
        {
            new AcademicProgressProgramDetail(code, maxCredits, -1.0m);
        }

        [TestMethod]
        public void ProgramCode_EqualsExpectedTest()
        {
            Assert.AreEqual(code, academicProgramDetail.ProgramCode);
        }

        [TestMethod]
        public void ProgramMaxCredits_EqualExpectedTest()
        {
            Assert.AreEqual(maxCredits, academicProgramDetail.ProgramMaxCredits);
        }

        [TestMethod]
        public void ProgramMinCredits_EqualExpectedTest()
        {
            Assert.AreEqual(minCredits, academicProgramDetail.ProgramMinCredits);
        }
    }
}
