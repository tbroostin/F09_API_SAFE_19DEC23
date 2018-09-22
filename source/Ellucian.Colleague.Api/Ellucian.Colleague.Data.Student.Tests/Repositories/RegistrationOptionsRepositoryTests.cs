using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RegistrationOptionsRepositoryTests : BaseRepositorySetup
    {
        RegistrationOptionsRepository regOptionsRepository;
        Collection<RegControls> regControls;
        string studentId; //student
        string advisorId; //advisor

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            studentId = "0000011";
            advisorId = "0000012";
            regOptionsRepository = BuildValidRegistrationOptionsRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            loggerMock = null;
            transManagerMock = null;
        }

        [TestMethod]
        public async Task Get_Single_Id_OnlyGraded()
        {
            // Arrange--Set up transaction response that returns WEBREG as the reg control for this user.
            var transactionResponse = new GetRegControlsIdForUserResponse() { PersonRegControls = new List<PersonRegControls>() { new PersonRegControls() { PersonIds = studentId, RegControlsIds = "WEBREG" } } };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetRegControlsIdForUserRequest, GetRegControlsIdForUserResponse>(It.IsAny<GetRegControlsIdForUserRequest>()))
                .ReturnsAsync(transactionResponse);

            // Act
            var regOptions = await regOptionsRepository.GetAsync(new List<string>() { studentId });

            // Assert - one single person is returned with only Graded
            Assert.AreEqual(1, regOptions.Count());
            Assert.AreEqual(studentId, regOptions.First().PersonId);
            Assert.AreEqual(Domain.Student.Entities.GradingType.Graded, regOptions.First().GradingTypes.First());
            Assert.AreEqual(1, regOptions.First().GradingTypes.Count());
        }

        [TestMethod]
        public async Task Get_MultipleIds()
        {
            // arrange--Set up transaction response that returns person1 with EMPTY and person2 with OVERRIDE
            var transactionResponse = new GetRegControlsIdForUserResponse()
            {
                PersonRegControls = new List<PersonRegControls>() 
                {
                    new PersonRegControls() {PersonIds = studentId, RegControlsIds = "EMPTY"},
                    new PersonRegControls() {PersonIds = advisorId, RegControlsIds = "OVERRIDE"}
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetRegControlsIdForUserRequest, GetRegControlsIdForUserResponse>(It.IsAny<GetRegControlsIdForUserRequest>()))
                .ReturnsAsync(transactionResponse);

            // Act
            var regOptions = await regOptionsRepository.GetAsync(new List<string>() { studentId, advisorId });

            // Assert-Verify person1 has only one grading type of Graded
            Assert.AreEqual(studentId, regOptions.ElementAt(0).PersonId);
            Assert.AreEqual(Domain.Student.Entities.GradingType.Graded, regOptions.ElementAt(0).GradingTypes.First());
            Assert.AreEqual(1, regOptions.First().GradingTypes.Count());
            // Assert-Verify person2 has all three grading types
            Assert.AreEqual(advisorId, regOptions.ElementAt(1).PersonId);
            Assert.IsTrue(regOptions.ElementAt(1).GradingTypes.Contains(Domain.Student.Entities.GradingType.Graded));
            Assert.IsTrue(regOptions.ElementAt(1).GradingTypes.Contains(Domain.Student.Entities.GradingType.Audit));
            Assert.IsTrue(regOptions.ElementAt(1).GradingTypes.Contains(Domain.Student.Entities.GradingType.PassFail));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowsExceptionIfNullListOfIds()
        {
            await regOptionsRepository.GetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowsExceptionIfEmptyListOfIds()
        {
           await regOptionsRepository.GetAsync(new List<string>());
        }

        [TestMethod]
        public async Task ReturnsEmptyListIfExceptionOnRegControlsAndTransaction()
        {
            // Arrange
            regOptionsRepository = BuildInvalidRegistrationOptionsRepository();
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetRegControlsIdForUserRequest, GetRegControlsIdForUserResponse>(It.IsAny<GetRegControlsIdForUserRequest>()))
                    .Throws(new Exception());

            // Act
            var regOptions = await regOptionsRepository.GetAsync(new List<string>() {studentId});

            // Assert - Default response: one single person is returned with only Graded
            Assert.AreEqual(1, regOptions.Count());
            Assert.AreEqual(studentId, regOptions.First().PersonId);
            Assert.AreEqual(Domain.Student.Entities.GradingType.Graded, regOptions.First().GradingTypes.First());
            Assert.AreEqual(1, regOptions.First().GradingTypes.Count());
        }

        // Builds a list of reg controls that is mocked to be returned by the data reader
        private Collection<RegControls> BuildRegControlsResponse()
        {
            var regControlsList = new Collection<RegControls>();

            RegControls regCon1 = new RegControls();
            regCon1.Recordkey = "WEBREG";
            regCon1.RgcAllowAuditFlag = "N";
            regCon1.RgcAllowPassFailFlag = "N";
            regControlsList.Add(regCon1);

            RegControls regCon2 = new RegControls();
            regCon2.Recordkey = "OVERRIDE";
            regCon2.RgcAllowAuditFlag = "Y";
            regCon2.RgcAllowPassFailFlag = "Y";
            regControlsList.Add(regCon2);

            RegControls regCon3 = new RegControls();
            regCon3.Recordkey = "EMPTY";
            regCon3.RgcAllowAuditFlag = "";
            regCon3.RgcAllowPassFailFlag = "";
            regControlsList.Add(regCon3);

            return regControlsList;
        }

        private RegistrationOptionsRepository BuildValidRegistrationOptionsRepository()
        {
            // Set up repo response for "all" programs requests
            regControls = BuildRegControlsResponse();
            dataReaderMock.Setup<Task<Collection<RegControls>>>(acc => acc.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true)).ReturnsAsync(regControls);

            RegistrationOptionsRepository regOptionsRepo = new RegistrationOptionsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return regOptionsRepo;
        }

        private RegistrationOptionsRepository BuildInvalidRegistrationOptionsRepository()
        {
            Exception expectedFailure = new Exception("fail");

            dataReaderMock.Setup<Task<Collection<RegControls>>>(acc => acc.BulkReadRecordAsync<RegControls>("", true)).Throws(expectedFailure);

            RegistrationOptionsRepository repository = new RegistrationOptionsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return repository;
        }
    }
}