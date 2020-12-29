// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Moq;
using slf4net;
using System;
using System.Threading;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using System.Collections.Generic;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Repositories;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentTransferWorkRepositoryTest
    {
        [TestClass]
        public class StudentTransferWorkRepository_GetStudentTransferWorkAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IStudentTransferWorkRepository studentTransferWorkRepository;
            GetStudentInstTxfrWorkRequest getRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                studentTransferWorkRepository = new StudentTransferWorkRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentTransferWorkRepository = null;
            }

            [TestMethod]
            public async Task GetStudentTransferWorkAsync_Success()
            {
                GetStudentInstTxfrWorkResponse response = new GetStudentInstTxfrWorkResponse()
                {
                    TransferWork = new List<TransferWork>()
                    {
                        new TransferWork()
                        {
                            AcademicPrograms = "",
                            EquivAcadLevelIds = "UG",
                            EquivCourseId = "855",
                            EquivCourseLevels = "UG",
                            EquivCourseNames = "MATH-000",
                            EquivCourseTitles = "Linn's Test For Cr 174.312"
                        }
                    }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetStudentInstTxfrWorkRequest, GetStudentInstTxfrWorkResponse>(It.IsAny<GetStudentInstTxfrWorkRequest>())).Returns(Task.FromResult(response)).Callback<GetStudentInstTxfrWorkRequest>(req => getRequest = req);
                var transferSummary = await studentTransferWorkRepository.GetStudentTransferWorkAsync("000013");
                Assert.IsNotNull(transferSummary);
            }
        }
    }
}
