//Copyright 2015-2018 Ellucian Company L.P. and its affiliates
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using Moq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    /// <summary>
    /// StudentAwardYear2DtoToEntityAdapter test class
    /// </summary>
    [TestClass]
    public class StudentAwardYear2DtoToEntityAdapterTests
    {
        private string studentId;
        private string awardYearCode;

        private Dtos.Student.StudentAwardYear2 inputStudentAwardYear;
        private StudentAwardYear expectedStudentAwardYear;
        private StudentAwardYear actualStudentAwardYear;

        private StudentAwardYear2DtoToEntityAdapter studentAwardYearDtoAdapter;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0004791";
            awardYearCode = "2014";
            inputStudentAwardYear = new Dtos.Student.StudentAwardYear2()
            {
                StudentId = studentId,
                Code = awardYearCode,
                IsPaperCopyOptionSelected = true
            };

            expectedStudentAwardYear = new StudentAwardYear(studentId, awardYearCode) { IsPaperCopyOptionSelected = inputStudentAwardYear.IsPaperCopyOptionSelected };

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            studentAwardYearDtoAdapter = new StudentAwardYear2DtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentAwardYearDto_ExceptionThrownTest()
        {
            actualStudentAwardYear = studentAwardYearDtoAdapter.MapToType(null);
        }
    }

}
