// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Data.Colleague;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class PreferredSectionRepositoryTests : BaseRepositorySetup
    {

        [TestClass]
        public class GetPreferredSectionsTests
        {
            PreferredSectionRepository repository;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            GetPreferredSectionsResponse getResponse1;
            GetPreferredSectionsResponse getResponse2;
            GetPreferredSectionsResponse getResponse3;

            [TestInitialize]
            public async void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                repository =await BuildPreferredSectionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
                cacheProviderMock = null;
                transFactoryMock = null;
                getResponse1 = null;
                getResponse2 = null;
                getResponse3 = null;
            }

            private async Task<PreferredSectionRepository> BuildPreferredSectionRepository()
            {
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(mockManager.Object);

                var response1 = await new TestPreferredSectionRepository().GetAsync("STU001");
                getResponse1 = new GetPreferredSectionsResponse();
                foreach (var ps in response1.PreferredSections)
                {
                    PreferredSections tps = new PreferredSections();
                    tps.CourseSectionId = ps.SectionId;
                    tps.Credits = ps.Credits;
                    getResponse1.PreferredSections.Add(tps);
                }
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetPreferredSectionsRequest, GetPreferredSectionsResponse>(It.Is<GetPreferredSectionsRequest>(r => r.StudentId == "STU001"))).ReturnsAsync(getResponse1);

                var response2 = await new TestPreferredSectionRepository().GetAsync("STU002");
                getResponse2 = new GetPreferredSectionsResponse();
                foreach (var ps in response2.PreferredSections)
                {
                    PreferredSections tps = new PreferredSections();
                    tps.CourseSectionId = ps.SectionId;
                    if (ps.Credits.HasValue)
                    {
                        tps.Credits = ps.Credits;
                    }
                    getResponse2.PreferredSections.Add(tps);
                }
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetPreferredSectionsRequest, GetPreferredSectionsResponse>(It.Is<GetPreferredSectionsRequest>(r => r.StudentId == "STU002"))).ReturnsAsync(getResponse2);

                var response3 = await new TestPreferredSectionRepository().GetAsync("STU003");
                getResponse3 = new GetPreferredSectionsResponse();
                foreach (var ps in response3.PreferredSections)
                {
                    PreferredSections tps = new PreferredSections();
                    tps.CourseSectionId = ps.SectionId;
                    tps.Credits = ps.Credits;
                    getResponse3.PreferredSections.Add(tps);
                }
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetPreferredSectionsRequest, GetPreferredSectionsResponse>(It.Is<GetPreferredSectionsRequest>(r => r.StudentId == "STU003"))).ReturnsAsync(getResponse3);

                repository = new PreferredSectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecGet_ThrowsWithNullStudentId()
            {
                var result = await repository.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecGet_ThrowsWithEmptyStudentId()
            {
                var result = await repository.GetAsync("");
            }

            [TestMethod]
            public async Task PrefSecGet_OneSection_TestProperties()
            {
                var result = await repository.GetAsync("STU001");
                Assert.AreEqual(getResponse1.PreferredSections.Count(), result.PreferredSections.Count());
                foreach (var ps in result.PreferredSections)
                {
                    var seedData = getResponse1.PreferredSections.Where(s => s.CourseSectionId == ps.SectionId).FirstOrDefault();
                    Assert.AreEqual(seedData.CourseSectionId, ps.SectionId);
                    Assert.AreEqual(seedData.Credits, ps.Credits);
                }
            }

            [TestMethod]
            public async Task PrefSecGet_PreferredSectionWithNullCredits()
            {
                var result = await repository.GetAsync("STU002");
                // testing that nullable decimal has no value
                Assert.AreEqual(1, result.PreferredSections.Count());
                var ps = result.PreferredSections.ElementAt(0);
                Assert.AreEqual(false, ps.Credits.HasValue);
            }

            [TestMethod]
            public async Task PrefSecGet_StudentWithMultiplePreferredSections()
            {
                var result = await repository.GetAsync("STU003");
                Assert.AreEqual(getResponse3.PreferredSections.Count(), result.PreferredSections.Count());
            }

        }

        [TestClass]
        public class UpdatePreferredSectionsTests
        {
            PreferredSectionRepository repository;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            UpdatePreferredSectionsResponse updateResponse1;
            UpdatePreferredSectionsResponse updateResponse2;
            UpdatePreferredSectionsResponse updateResponse3;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                repository = BuildPreferredSectionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
                cacheProviderMock = null;
                transFactoryMock = null;
            }

            private PreferredSectionRepository BuildPreferredSectionRepository()
            {
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(mockManager.Object);

                // STU001 generates an empty response
                updateResponse1 = new UpdatePreferredSectionsResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdatePreferredSectionsRequest, UpdatePreferredSectionsResponse>(It.Is<UpdatePreferredSectionsRequest>(r => r.StudentId == "STU001"))).ReturnsAsync(updateResponse1);

                // STU002 generates a message (from Colleauge)
                updateResponse2 = new UpdatePreferredSectionsResponse();
                PrefSecUpdateMessages msg = new PrefSecUpdateMessages();
                msg.MessageSectionId = "12345";
                msg.MessageText = "Some Colleague Message";
                updateResponse2.PrefSecUpdateMessages.Add(msg);
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdatePreferredSectionsRequest, UpdatePreferredSectionsResponse>(It.Is<UpdatePreferredSectionsRequest>(r => r.StudentId == "STU002"))).ReturnsAsync(updateResponse2);

                repository = new PreferredSectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecUpdate_ThrowsWithNullStudentId()
            {
                var result = await repository.UpdateAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecUpdate_ThrowsWithEmptyStudentId()
            {
                var result = await repository.UpdateAsync("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecUpdate_ThrowsWithNullSections()
            {
                var result = await repository.UpdateAsync("STU001", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecUpdate_ThrowsWithEmptySections()
            {
                var result = await repository.UpdateAsync("STU001", new List<PreferredSection>());
            }

            [TestMethod]
            public async Task PrefSecUpdate_MessageIncorrectStudentId()
            {
                List<PreferredSection> prefSecs = new List<PreferredSection>();
                prefSecs.Add(new PreferredSection("STU002", "12345", null));
                prefSecs.Add(new PreferredSection("STU001", "23456", null));
                IEnumerable<PreferredSectionMessage> result = await repository.UpdateAsync("STU001", prefSecs);
                Assert.AreEqual(1, result.Count());
                var psm = result.ElementAt(0);
                Assert.AreEqual("12345", psm.SectionId);
                Assert.AreEqual("PreferredSection not for Student", psm.Message);
            }

            [TestMethod]
            public async Task PrefSecUpdate_MessageIfNoValidSections()
            {
                List<PreferredSection> prefSecs = new List<PreferredSection>();
                prefSecs.Add(new PreferredSection("STU002", "12345", null));
                IEnumerable<PreferredSectionMessage> result = await repository.UpdateAsync("STU001", prefSecs);
                Assert.AreEqual(2, result.Count());
                var psm = result.Where(m => m.SectionId == "sections").FirstOrDefault(); ;
                Assert.AreEqual("No valid sections for update", psm.Message);
            }

            [TestMethod]
            public async Task PrefSecUpdate_EmptyListWithNoErrors()
            {
                List<PreferredSection> prefSecs = new List<PreferredSection>();
                prefSecs.Add(new PreferredSection("STU001", "12345", null));
                IEnumerable<PreferredSectionMessage> result = await repository.UpdateAsync("STU001", prefSecs);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task PrefSecUpdate_PassesThruColleagueMessage()
            {
                List<PreferredSection> prefSecs = new List<PreferredSection>();
                prefSecs.Add(new PreferredSection("STU002", "12345", null));
                IEnumerable<PreferredSectionMessage> result = await repository.UpdateAsync("STU002", prefSecs);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task PrefSecUpdate_MergesColleagueAndRepoMessages()
            {
                List<PreferredSection> prefSecs = new List<PreferredSection>();
                prefSecs.Add(new PreferredSection("STU001", "12345", null));
                IEnumerable<PreferredSectionMessage> result = await repository.UpdateAsync("STU002", prefSecs);
                Assert.AreEqual(2, result.Count());
            }
        }

        [TestClass]
        public class DeletePreferredSectionsTests
        {
            PreferredSectionRepository repository;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;

            DeletePreferredSectionsResponse deleteResponse1;
            DeletePreferredSectionsResponse deleteResponse2;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                repository = BuildPreferredSectionRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
                cacheProviderMock = null;
                transFactoryMock = null;
            }

            private PreferredSectionRepository BuildPreferredSectionRepository()
            {
                var mockManager = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(mockManager.Object);

                // All for STU001
                // SEC001 generates no messages
                deleteResponse1 = new DeletePreferredSectionsResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<DeletePreferredSectionsRequest, DeletePreferredSectionsResponse>(It.Is<DeletePreferredSectionsRequest>(r => r.StudentId == "STU001" && r.CourseSectionIds.FirstOrDefault() == "SEC001"))).ReturnsAsync(deleteResponse1);
                // BOGUS generates message
                deleteResponse2 = new DeletePreferredSectionsResponse();
                PrefSecDeleteMessages msg = new PrefSecDeleteMessages();
                msg.MessageSectionId = "BOGUS";
                msg.MessageText = "Some Colleague Message";
                deleteResponse2.PrefSecDeleteMessages.Add(msg);
                mockManager.Setup(mgr => mgr.ExecuteAsync<DeletePreferredSectionsRequest, DeletePreferredSectionsResponse>(It.Is<DeletePreferredSectionsRequest>(r => r.StudentId == "STU001" && r.CourseSectionIds.FirstOrDefault() == "BOGUS"))).ReturnsAsync(deleteResponse2);

                repository = new PreferredSectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return repository;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecDelete_ThrowsWithNullStudentId()
            {
                var result = await repository.DeleteAsync(null, "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecDelete_ThrowsWithEmptyStudentId()
            {
                var result = await repository.DeleteAsync("", "SEC001");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecDelete_ThrowsWithNullSectionIds()
            {
                var result = await repository.DeleteAsync("STU001", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PrefSecDelete_ThrowsWithEmptySectionId()
            {
                var result = await repository.DeleteAsync("STU001", "");
            }

            [TestMethod]
            public async Task PrefSecDelete_PassesThruColleagueMessage()
            {
                IEnumerable<PreferredSectionMessage> result = await repository.DeleteAsync("STU001", "BOGUS");
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task PrefSecDelete_SuccessfulInvoke()
            {
                IEnumerable<PreferredSectionMessage> result = await repository.DeleteAsync("STU001", "SEC001");
                Assert.AreEqual(0, result.Count());
            }

        }
    
    }
}
