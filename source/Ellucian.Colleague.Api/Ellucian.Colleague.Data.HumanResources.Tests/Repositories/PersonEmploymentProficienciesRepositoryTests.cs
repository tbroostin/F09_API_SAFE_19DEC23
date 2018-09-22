using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PersonEmploymentProficienciesRepositoryTests_V10
    {
        [TestClass]
        public class PersonEmploymentProficienciesRepositoryTests_GET_GET_BY_ID : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PersonEmploymentProficienciesRepository repository;

            private List<string> hrIndSkillIds;
            private Collection<DataContracts.HrIndSkill> hrIndSkills;
            private Dictionary<string, GuidLookupResult> dicResult;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hrIndSkillIds.ToArray());
                

                repository = new PersonEmploymentProficienciesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                hrIndSkillIds = new List<string>() { "1" };

                hrIndSkills = new Collection<DataContracts.HrIndSkill>
                {
                    new DataContracts.HrIndSkill() { RecordGuid = guid }
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    { guid, new GuidLookupResult() { Entity = "", PrimaryKey = "1" } }
                };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonEmploymentProficienciesAsync_KeyNotFoundException_HrIndSkills_Null()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(null);

                await repository.GetPersonEmploymentProficienciesAsync(0, 10, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonEmploymentProficienciesAsync_RepositoryException()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ThrowsAsync(new RepositoryException());

                await repository.GetPersonEmploymentProficienciesAsync(0, 10, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonEmploymentProficienciesAsync_Exception()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ThrowsAsync(new Exception());

                await repository.GetPersonEmploymentProficienciesAsync(0, 10, true);
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesAsync()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(hrIndSkills);

                var result = await repository.GetPersonEmploymentProficienciesAsync(0, 10, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(hrIndSkills.Count, result.Item1.Count());
                Assert.AreEqual(hrIndSkills.FirstOrDefault().RecordGuid, result.Item1.FirstOrDefault().Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonEmploymentProficiency_ArgumentNullException_Guid_Null()
            {
                await repository.GetPersonEmploymentProficiency(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonEmploymentProficiency_KeyNotFoundException_RecordKey_Null()
            {
                dicResult.FirstOrDefault().Value.PrimaryKey = null;

                dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);

                await repository.GetPersonEmploymentProficiency(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonEmploymentProficiency_KeyNotFoundException_HrIndSkill_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), true)).ReturnsAsync(null);

                await repository.GetPersonEmploymentProficiency(guid);
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficiency()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.HrIndSkill>(It.IsAny<string>(), true)).ReturnsAsync(hrIndSkills.FirstOrDefault());

                var result = await repository.GetPersonEmploymentProficiency(guid);

                Assert.IsNotNull(result);
            }
        }
    }
}
