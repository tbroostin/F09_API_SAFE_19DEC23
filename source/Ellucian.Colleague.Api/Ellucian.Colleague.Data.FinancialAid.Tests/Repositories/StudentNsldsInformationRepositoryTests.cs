//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Moq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentNsldsInformationRepositoryTests : BaseRepositorySetup
    {
        public class GetPellLeuResponseTest : GetPellLeuResponse
        {
            public decimal PellLeuPct;
        }

        public string studentId;

        public TestStudentNsldsInformationRepository expectedRespository;
        public StudentNsldsInformationRepository actualRepository;

        public GetPellLeuRequest getPellLeuRequest;
        public GetPellLeuResponse getPellLeuResponse;

        public StudentNsldsInformation expectedNsldsInfo;
        public StudentNsldsInformation actualNsldsInfo;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            studentId = "0004791";
            expectedRespository = new TestStudentNsldsInformationRepository();
            expectedNsldsInfo = expectedRespository.GetStudentNsldsInformationAsync(studentId).Result;
            
            BuildStudentNsldsInformationRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            expectedRespository = null;
            actualRepository = null;
            expectedNsldsInfo = null;
            actualNsldsInfo = null;
        }

        private void BuildStudentNsldsInformationRepository()
        {
            getPellLeuResponse = new GetPellLeuResponse(){PellLeu = expectedNsldsInfo.PellLifetimeEligibilityUsedPercentage};
            transManagerMock.Setup(m => m.ExecuteAsync<GetPellLeuRequest, GetPellLeuResponse>(It.IsAny<GetPellLeuRequest>()))
                .Returns(Task.FromResult(getPellLeuResponse))            
                .Callback<GetPellLeuRequest>(req => getPellLeuRequest = req);

              actualRepository = new StudentNsldsInformationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentId_ThrowsArgumentNullExceptionTest()
        {
            await actualRepository.GetStudentNsldsInformationAsync(null);
        }

        [TestMethod]
        public async Task ActualNsldsInformationIsNotNullTest()
        {
            Assert.IsNotNull(await actualRepository.GetStudentNsldsInformationAsync(studentId));
        }

        [TestMethod]
        public async Task ActualNsldsInformationProperties_EqualExpectedTest()
        {
            actualNsldsInfo = await actualRepository.GetStudentNsldsInformationAsync(studentId);
            Assert.AreEqual(expectedNsldsInfo.StudentId, actualNsldsInfo.StudentId);
            Assert.AreEqual(expectedNsldsInfo.PellLifetimeEligibilityUsedPercentage, actualNsldsInfo.PellLifetimeEligibilityUsedPercentage);
        }

    }
}
