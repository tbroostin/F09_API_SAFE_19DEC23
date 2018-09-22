/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    /// <summary>
    /// Test class that tests AwardLetterDtoAdapter
    /// </summary>
    [TestClass]
    public class AwardLetterDtoAdapterTests
    {
        private string studentId;
        private Domain.FinancialAid.Entities.StudentAwardYear awardYear;
        private Dtos.FinancialAid.AwardLetter inputAwardLetter;
        private Domain.FinancialAid.Entities.AwardLetter expectedAwardLetter;
        private Domain.FinancialAid.Entities.AwardLetter actualAwardLetter;

        private AwardLetterDtoToEntityAdapter awardLetterDtoAdapter;

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0004791";
            var officeConfiguration = new Domain.FinancialAid.Entities.FinancialAidConfiguration("office", "2014")
            {
                AwardYearDescription = "2014/2015 award year"
            };

            awardYear = new Domain.FinancialAid.Entities.StudentAwardYear(studentId, "2014",
                new Domain.FinancialAid.Entities.FinancialAidOffice("office"));

            awardYear.CurrentOffice.AddConfiguration(officeConfiguration);

            inputAwardLetter = new Dtos.FinancialAid.AwardLetter()
            {
                StudentId = studentId,
                AwardYearCode = awardYear.Code,
                AwardYearDescription = awardYear.Description,
                AcceptedDate = new DateTime(2014, 01, 01)
            };

            expectedAwardLetter = new Domain.FinancialAid.Entities.AwardLetter(inputAwardLetter.StudentId, awardYear) { AcceptedDate = inputAwardLetter.AcceptedDate };
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            awardLetterDtoAdapter = new AwardLetterDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        /// <summary>
        /// Tests if the returned award letter entity by the adapter is not null
        /// </summary>
        [TestMethod]
        public void AwardLetterEntityNotNullTest()
        {
            actualAwardLetter = awardLetterDtoAdapter.MapToType(inputAwardLetter, awardYear);
            Assert.IsNotNull(actualAwardLetter);
        }

        /// <summary>
        /// Tests if the award year code, student id and accepted date attributes
        /// of the actual letter equal the ones that are expected
        /// </summary>
        [TestMethod]
        public void expectedAwardLetterAttributes_EqualActualAwardLetterAttributesTest()
        {
            actualAwardLetter = awardLetterDtoAdapter.MapToType(inputAwardLetter, awardYear);
            Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
            Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
            Assert.AreEqual(expectedAwardLetter.AwardYear, actualAwardLetter.AwardYear);
        }

        /// <summary>
        /// Tests if an exception is thrown when null is passed in place of 
        /// an awardLetterDto
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardLetterDtoThrowsExceptionTest()
        {
            awardLetterDtoAdapter.MapToType(null, awardYear);
        }

        /// <summary>
        /// Tests if an exception is thrown when null is passed in place of the
        /// award year entity
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardYearEntityThrowsExceptionTest()
        {
            awardLetterDtoAdapter.MapToType(inputAwardLetter, null);
        }

        /// <summary>
        /// Tests if an exception is thrown when an awardLetterDto awardYearCode 
        /// property does not match the one of the awardYearEntity
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DifferentAwardYearThrowsExceptionTest()
        {
            inputAwardLetter.AwardYearCode = "";
            awardLetterDtoAdapter.MapToType(inputAwardLetter, awardYear);
        }
    }
}
