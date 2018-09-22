//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Adapters
{
    [TestClass]
    public class AwardLetter2DtoToEntityAdapterTests
    {
        private AwardLetter2DtoToEntityAdapter dtoToEntityAdapter;
        private Mock<IAdapterRegistry> adapterRegistry;
        private Mock<ILogger> logger;

        private StudentAwardYear inputStudentAwardYearEntity;
        private Dtos.FinancialAid.AwardLetter2 inputAwardLetterDto;

        private string studentId;

        private AwardLetter2 actualAwardLetter;

        [TestInitialize]
        public void Initialize()
        {
            
            studentId = "0004791";

            #region award letter dto
            inputAwardLetterDto = new Dtos.FinancialAid.AwardLetter2()
            {
                AcceptedDate = new DateTime(2016, 04, 22),
                AwardLetterAnnualAwards = new List<Dtos.FinancialAid.AwardLetterAnnualAward>()
                {
                    new Dtos.FinancialAid.AwardLetterAnnualAward(){
                        AwardId = "ZEBRA",
                        AnnualAwardAmount = 5000,
                        AwardLetterAwardPeriods = new List<Dtos.FinancialAid.AwardLetterAwardPeriod>(){
                            new Dtos.FinancialAid.AwardLetterAwardPeriod(){
                                AwardId = "ZEBRA",
                                AwardPeriodAmount = 5000,
                                ColumnName = "15/FA"
                            }
                        }
                    }
                },
                AwardLetterGroups = new List<Dtos.FinancialAid.AwardLetterGroup>()
                {
                    new Dtos.FinancialAid.AwardLetterGroup(){
                        GroupName = "Awards",
                        GroupType = Dtos.FinancialAid.GroupType.AwardCategories,
                        GroupNumber = 1
                    }
                },
                AwardLetterParameterId = "ALTR",
                AwardLetterYear = "2016",
                AwardYearDescription = "2016 award year",
                BudgetAmount = 50000,
                ClosingParagraph = "Closing par",
                ContactAddress = new List<Dtos.FinancialAid.AwardLetterAddress>(),
                ContactName = "john Doe",
                EstimatedFamilyContributionAmount = 3456,
                NeedAmount = 5647,
                OpeningParagraph = "Opening par",
                StudentId = studentId,
                StudentName = "Student name",
                StudentOfficeCode = "LAW",
                StudentAddress = new List<Dtos.FinancialAid.AwardLetterAddress>(),
                CreatedDate = new DateTime(2016, 04, 20),
                HousingCode = Dtos.FinancialAid.HousingCode.OnCampus,
                Id = "13"
            };
            #endregion

            inputStudentAwardYearEntity = new StudentAwardYear(studentId, "2016");

            adapterRegistry = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>();
            dtoToEntityAdapter = new AwardLetter2DtoToEntityAdapter(adapterRegistry.Object, logger.Object);
        }

        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void NullAwardLetterDto_ExceptionThrownTest()
        {
            dtoToEntityAdapter.MapToType(null, inputStudentAwardYearEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAwardYearEntity_ExceptionThrownTest()
        {
            dtoToEntityAdapter.MapToType(inputAwardLetterDto, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AwardYearMismatch_ExceptionThrownTest()
        {
            inputAwardLetterDto.AwardLetterYear = "foo";
            dtoToEntityAdapter.MapToType(inputAwardLetterDto, inputStudentAwardYearEntity);
        }

        [TestMethod]
        public void AwardLetterEntity_NotNullTest()
        {
            actualAwardLetter = dtoToEntityAdapter.MapToType(inputAwardLetterDto, inputStudentAwardYearEntity);
            Assert.IsNotNull(actualAwardLetter);
        }

        [TestMethod]
        public void ImportantProperties_EqualExpectedTest()
        {
            actualAwardLetter = dtoToEntityAdapter.MapToType(inputAwardLetterDto, inputStudentAwardYearEntity);
            Assert.AreEqual(inputAwardLetterDto.Id, actualAwardLetter.Id);
            Assert.AreEqual(inputAwardLetterDto.AcceptedDate, actualAwardLetter.AcceptedDate);
        }
    }
}
