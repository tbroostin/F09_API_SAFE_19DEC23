// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class TranscriptGroupingRepositoryTests
    {
        TranscriptGroupingRepository transcriptGroupingRepository;
        Collection<TranscriptGroupings> transcriptGroupingsData;
        StwebDefaults stWebDefaults;

        [TestInitialize]
        public void Initialize()
        {
            transcriptGroupingsData = new Collection<TranscriptGroupings>();
            transcriptGroupingsData.Add(new TranscriptGroupings() { Recordkey = "Test1", TrgpDesc = "Test1" });
            transcriptGroupingsData.Add(new TranscriptGroupings() { Recordkey = "Test2", TrgpDesc = "Test2" });
            transcriptGroupingsData.Add(new TranscriptGroupings() { Recordkey = "Test3", TrgpDesc = "Test3" });

            stWebDefaults = new StwebDefaults() { StwebTranAllowedGroupings = new List<string>() { "Test1", "Test2" } };

            transcriptGroupingRepository = BuildValidTranscriptGroupingRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transcriptGroupingRepository = null;
        }

        [TestMethod]
        public async Task TranscriptGroupingRepository_GetTranscriptGroupings()
        {
            var transcriptGroupings = await transcriptGroupingRepository.GetAsync();

            // Verify that the overall count of groupings is correct
            Assert.AreEqual(transcriptGroupingsData.Count, transcriptGroupings.Count());

            // Verify that the count of user-selectable groupings is correct
            Assert.AreEqual(stWebDefaults.StwebTranAllowedGroupings.Count, transcriptGroupings.Where(x => x.IsUserSelectable == true).Count());

            foreach (var grouping in transcriptGroupingsData)
            {
                var transcriptGrouping = transcriptGroupings.Single(x => x.Id == grouping.Recordkey);

                // Ensure the description and isSelectable flags are correct
                Assert.AreEqual(grouping.TrgpDesc, transcriptGrouping.Description);
                Assert.AreEqual(stWebDefaults.StwebTranAllowedGroupings.Contains(transcriptGrouping.Id), transcriptGrouping.IsUserSelectable);
            }
        }

        private TranscriptGroupingRepository BuildValidTranscriptGroupingRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Set up transaction manager for mocking 
            var mockManager = new Mock<IColleagueTransactionInvoker>();
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);

            dataAccessorMock.Setup(dataReader => dataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).Returns(Task.FromResult(stWebDefaults));

            dataAccessorMock.Setup(dataReader => dataReader.BulkReadRecordAsync<TranscriptGroupings>("", true)).Returns(Task.FromResult(transcriptGroupingsData));

            // Construct testResult repository
            transcriptGroupingRepository = new TranscriptGroupingRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return transcriptGroupingRepository;
        }
    }
}