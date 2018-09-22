//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using Moq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    /// <summary>
    /// StudentAwardYearDtoAdapter test class
    /// </summary>
    [TestClass]
    public class StudentAwardYearDtoAdapterTests
    {
        private string studentId;
        private string awardYearCode;

        private Dtos.FinancialAid.StudentAwardYear inputStudentAwardYear;
        private StudentAwardYear expectedStudentAwardYear;
        private StudentAwardYear actualStudentAwardYear;

        private StudentAwardYearDtoToEntityAdapter studentAwardYearDtoAdapter;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0004791";
            awardYearCode = "2014";
            inputStudentAwardYear = new Dtos.FinancialAid.StudentAwardYear()
            {
                StudentId = studentId,
                Code = awardYearCode,
                IsPaperCopyOptionSelected = true
            };

            expectedStudentAwardYear = new StudentAwardYear(studentId, awardYearCode) { IsPaperCopyOptionSelected = inputStudentAwardYear.IsPaperCopyOptionSelected };

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            studentAwardYearDtoAdapter = new StudentAwardYearDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        /// <summary>
        /// Tests if the dto adapter returns a studentAwardYear entity(not null)
        /// </summary>
        [TestMethod]
        public void ActualStudentAwardYear_NotNullTest()
        {
            Assert.IsNotNull(studentAwardYearDtoAdapter.MapToType(inputStudentAwardYear));
        }

        /// <summary>
        /// Tests if the entities of the actual student award year equal the corresponding ones
        /// of the expected one
        /// </summary>
        [TestMethod]
        public void ActualStudentAwardYearEntities_EqualExpectedEntities()
        {
            actualStudentAwardYear = studentAwardYearDtoAdapter.MapToType(inputStudentAwardYear);
            Assert.AreEqual(expectedStudentAwardYear.StudentId, actualStudentAwardYear.StudentId);
            Assert.AreEqual(expectedStudentAwardYear.Code, actualStudentAwardYear.Code);
            Assert.AreEqual(expectedStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
        }

        /// <summary>
        /// Tests if ArgumentNullException is thrown when the input studentAwardYearDto
        /// is null
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void NullStudentAwardYearDto_ExceptionThrownTest()
        {
            actualStudentAwardYear = studentAwardYearDtoAdapter.MapToType(null);
        }
    }

}
