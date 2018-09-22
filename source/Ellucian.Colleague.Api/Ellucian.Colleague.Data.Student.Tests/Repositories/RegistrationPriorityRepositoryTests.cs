// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RegistrationPriorityRepositoryTests
    {
        [TestClass]
        public class GetStudentPriorityTests
        {
            RegistrationPriorityRepository registrationPriorityRepo;
            Collection<RegPriorities> regPrioritiesResponseData;
            List<RegistrationPriority> registrationPriorities = new List<RegistrationPriority>();
            Ellucian.Colleague.Domain.Student.Entities.Student student;
            List<RegistrationPriority> studentpriorities = new List<RegistrationPriority>();
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            [TestInitialize]
            public async void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();

                student = new Ellucian.Colleague.Domain.Student.Entities.Student("0000002", "Smith", null, null, null, null);
                student.AddRegistrationPriority("2");
                student.AddRegistrationPriority("3");
                studentpriorities = (await new TestRegistrationPriorityRepository().GetAsync(student.Id)).ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                registrationPriorityRepo = null;
            }

            [TestMethod]
            public async Task Priority_PropertiesValid()
            {
                await SetUpRegistrationPriorityRepo();

                var priorities = await registrationPriorityRepo.GetAsync(student.Id);
                Assert.AreEqual(studentpriorities.Count(), priorities.Count());
                foreach (var priority in priorities)
                {
                    Assert.AreEqual("0000002", priority.StudentId);
                    Assert.AreEqual("2014/FA", priority.TermCode);
                    RegistrationPriority testPriority = studentpriorities.Where(sp => sp.Id == priority.Id).FirstOrDefault();
                    Assert.AreEqual(testPriority.Start, priority.Start);
                    Assert.AreEqual(testPriority.End, priority.End);
                }

            }            

            [TestMethod]
            public async Task NoStartTime_StartDateIsNotNullTest()
            {
                await SetUpRegistrationPriorityRepo();

                var priority = regPrioritiesResponseData.First();
                priority.RgprStartTime = null;

                var priorities = await registrationPriorityRepo.GetAsync(student.Id);
                var actualPriority = priorities.FirstOrDefault(p => p.Id == priority.Recordkey);
                Assert.IsNotNull(actualPriority);
                Assert.IsNotNull(actualPriority.Start);
            }

            [TestMethod]
            public async Task NoStartTime_StartDateEqualsExpectedTest()
            {
                await SetUpRegistrationPriorityRepo();

                var expectedPriority = regPrioritiesResponseData.First();
                expectedPriority.RgprStartTime = null;

                var priorities = await registrationPriorityRepo.GetAsync(student.Id);
                var actualPriority = priorities.FirstOrDefault(p => p.Id == expectedPriority.Recordkey);
                Assert.IsNotNull(actualPriority);
                Assert.AreEqual(expectedPriority.RgprStartDate, actualPriority.Start);
            }

            [TestMethod]
            public async Task StudentWithNoPriorities_ReturnsEmptyList()
            {
                var student2 = new Ellucian.Colleague.Domain.Student.Entities.Student("0000005", "Smith", null, null, null, null);
                registrationPriorityRepo = BuildInvalidRegistrationPriorityRepository();
                var priorities = await registrationPriorityRepo.GetAsync(student2.Id);
                Assert.AreEqual(0, priorities.Count());
            }

            [TestMethod]
            public async Task StudentWithNoPrioritiesSelected_ReturnsEmptyList()
            {                
                // Arrange - set up no select response for this user
                registrationPriorityRepo = BuildValidRegistrationPriorityRepository();
                // Mock select of student that has no reg priorities on file
                string[] zeroRegPrioritiesSelectResponse = new List<string>().ToArray();
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("REG.PRIORITIES", "0000006")).ReturnsAsync(zeroRegPrioritiesSelectResponse);
                // Act - Get priorities for this student Id
                var priorities = await registrationPriorityRepo.GetAsync("0000006");
                // Assert -- zero items returned
                Assert.AreEqual(0, priorities.Count());
            }

            [TestMethod]
            public async Task StudentWithInvalidPriorities_ReturnsEmptyList()
            {
                var student3 = new Ellucian.Colleague.Domain.Student.Entities.Student("0000005", "Smith", null, null, null, null);
                student3.AddRegistrationPriority("junk");
                registrationPriorityRepo = BuildInvalidRegistrationPriorityRepository();
                var priorities = await registrationPriorityRepo.GetAsync(student3.Id);
                Assert.AreEqual(0, priorities.Count());
            }

            private RegistrationPriorityRepository BuildValidRegistrationPriorityRepository()
            {
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                ApiSettings apiSettingsMock = new ApiSettings("null");

                registrationPriorityRepo = new RegistrationPriorityRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return registrationPriorityRepo;

            }

            private RegistrationPriorityRepository BuildInvalidRegistrationPriorityRepository()
            {
                var transFactoryMock = new Mock<IColleagueTransactionFactory>();
                var loggerMock = new Mock<ILogger>();
                var cacheProviderMock = new Mock<ICacheProvider>();
                var dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                dataAccessorMock.Setup<Task<Collection<RegPriorities>>>(acc => acc.BulkReadRecordAsync<RegPriorities>("REG.PRIORITIES", It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<RegPriorities>());

                ApiSettings apiSettingsMock = new ApiSettings("null");

                registrationPriorityRepo = new RegistrationPriorityRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return registrationPriorityRepo;

            }

            private async Task<Collection<RegPriorities>> BuildRegPrioritiesResponse(Ellucian.Colleague.Domain.Student.Entities.Student student)
            {
                Collection<RegPriorities> regPriorityData = new Collection<RegPriorities>();
                registrationPriorities = (await new TestRegistrationPriorityRepository().GetAsync(student.Id)).ToList();
                foreach (var registrationPriority in registrationPriorities)
                {
                    var regPriority = new RegPriorities();
                    regPriority.Recordkey = registrationPriority.Id;
                    regPriority.RgprStudent = registrationPriority.StudentId;
                    regPriority.RgprTerm = registrationPriority.TermCode;
                    if (registrationPriority.Start.HasValue)
                    {
                        regPriority.RgprStartDate = registrationPriority.Start.Value.Date;
                        var hours = registrationPriority.Start.Value.Hour;
                        var minutes = registrationPriority.Start.Value.Minute;
                        var seconds = registrationPriority.Start.Value.Second;
                        regPriority.RgprStartTime = new DateTime(1, 1, 1, hours, minutes, seconds);
                    }
                    else
                    {
                        regPriority.RgprStartDate = null;
                        regPriority.RgprStartTime = null;
                    }
                    if (registrationPriority.End.HasValue)
                    {
                        regPriority.RgprEndDate = registrationPriority.End.Value.Date;
                        var hours = registrationPriority.End.Value.Hour;
                        var minutes = registrationPriority.End.Value.Minute;
                        var seconds = registrationPriority.End.Value.Second;
                        regPriority.RgprEndTime = new DateTime(1, 1, 1, hours, minutes, seconds);
                    }
                    else
                    {
                        regPriority.RgprEndDate = null;
                        regPriority.RgprEndTime = null;
                    }
                    regPriorityData.Add(regPriority);
                }
                return regPriorityData;
            }

            /// <summary>
            /// Builds the response and mocks dataAccessor to return the data
            /// </summary>
            /// <returns></returns>
            private async Task SetUpRegistrationPriorityRepo()
            {
                regPrioritiesResponseData = await BuildRegPrioritiesResponse(student);

                registrationPriorityRepo = BuildValidRegistrationPriorityRepository();

                // Now that repository selects reg priorities for the given student, mock that response
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("REG.PRIORITIES", It.IsAny<string>())).ReturnsAsync(student.RegistrationPriorityIds.ToArray());
                dataAccessorMock.Setup<Task<Collection<RegPriorities>>>(acc => acc.BulkReadRecordAsync<RegPriorities>("REG.PRIORITIES", It.IsAny<string[]>(), true)).ReturnsAsync(regPrioritiesResponseData);
            }
        }

    }
}

