/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentAwardsControllerTests
    {

        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #endregion

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IStudentAwardService> studentAwardServiceMock;
        public Mock<ILogger> loggerMock;

        public string studentId;
        public string year;
        public string awardId;

        public List<StudentAward> expectedStudentAwards;

        public StudentAwardsController studentAwardsController;

        public FunctionEqualityComparer<StudentAward> studentAwardComparer;

        public void BaseInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            studentAwardServiceMock = new Mock<IStudentAwardService>();
            loggerMock = new Mock<ILogger>();

            studentAwardComparer = new FunctionEqualityComparer<StudentAward>(
                (s1, s2) => s1.AwardId == s2.AwardId && s1.StudentId == s2.StudentId && s1.AwardYearId == s2.AwardYearId,
                (s) => s.AwardId.GetHashCode() ^ s.StudentId.GetHashCode() ^ s.AwardYearId.GetHashCode());

            expectedStudentAwards = BuildStudentAwardData();
            studentId = expectedStudentAwards.First().StudentId;
            year = expectedStudentAwards.First().AwardYearId;
            awardId = expectedStudentAwards.First().AwardId;

            studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, It.IsAny<bool>())).ReturnsAsync(expectedStudentAwards);
            studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, It.IsAny<bool>())).ReturnsAsync(expectedStudentAwards.Where(a => a.AwardYearId == year));
            studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, awardId)).ReturnsAsync(expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
            studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, awardId, It.IsAny<StudentAward>())).ReturnsAsync(expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
            studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).ReturnsAsync(expectedStudentAwards.Where(a => a.AwardYearId == year));

            studentAwardsController = new StudentAwardsController(adapterRegistryMock.Object, studentAwardServiceMock.Object, loggerMock.Object);
        }

        private List<StudentAward> BuildStudentAwardData()
        {
            return new List<StudentAward>() 
            {
                new StudentAward() 
                {
                    AwardId = "FOO",
                    AwardYearId = "2014",
                    StudentId = "0003914",
                    IsAmountModifiable = false,
                    IsEligible = true,
                    StudentAwardPeriods = new List<StudentAwardPeriod>() 
                    {
                        new StudentAwardPeriod()
                        {
                            AwardAmount = 1000,
                            AwardId = "FOO",
                            AwardPeriodId = "14/FA",
                            AwardStatusId = "E",
                            AwardYearId = "2014",
                            IsAmountModifiable = false,
                            IsFrozen = false,
                            IsStatusModifiable = true,
                            IsTransmitted = false,
                            StudentId = "0003914",
                            UpdateActionTaken = null
                        },
                        new StudentAwardPeriod()
                        {
                            AwardAmount = 1000,
                            AwardId = "FOO",
                            AwardPeriodId = "15/SP",
                            AwardStatusId = "P",
                            AwardYearId = "2014",
                            IsAmountModifiable = false,
                            IsFrozen = false,
                            IsStatusModifiable = true,
                            IsTransmitted = false,
                            StudentId = "0003914",
                            UpdateActionTaken = null
                        }
                    }
                },
                new StudentAward()
                {
                    AwardId = "BAR",
                    AwardYearId = "2013",
                    StudentId = "0003914",
                    IsAmountModifiable = false,
                    IsEligible = true,
                    StudentAwardPeriods = new List<StudentAwardPeriod>() 
                    {
                        new StudentAwardPeriod()
                        {
                            AwardAmount = 1234,
                            AwardId = "BAR",
                            AwardPeriodId = "13/FA",
                            AwardStatusId = "R",
                            AwardYearId = "2013",
                            IsAmountModifiable = true,
                            IsFrozen = false,
                            IsStatusModifiable = true,
                            IsTransmitted = false,
                            StudentId = "0003914",
                            UpdateActionTaken = null
                        },
                        new StudentAwardPeriod()
                        {
                            AwardAmount = 9383,
                            AwardId = "BAR",
                            AwardPeriodId = "14/SP",
                            AwardStatusId = "A",
                            AwardYearId = "2014",
                            IsAmountModifiable = false,
                            IsFrozen = true,
                            IsStatusModifiable = false,
                            IsTransmitted = true,
                            StudentId = "0003914",
                            UpdateActionTaken = null
                        }
                    }
                }

            };
        }

        [TestClass]
        public class GetAllStudentAwardsTests : StudentAwardsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualStudentAwards = await studentAwardsController.GetStudentAwardsAsync(studentId);

                CollectionAssert.AreEqual(expectedStudentAwards.ToArray(), actualStudentAwards.ToArray());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, It.IsAny<bool>())).Throws(new PermissionsException("pe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, It.IsAny<bool>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, It.IsAny<bool>())).Throws(new InvalidOperationException("ioe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, It.IsAny<bool>())).Throws(new Exception("e"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetYearStudentAwardsTests : StudentAwardsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualStudentAwards = await studentAwardsController.GetStudentAwardsAsync(studentId, year);
                expectedStudentAwards = expectedStudentAwards.Where(a => a.AwardYearId == year).ToList();

                CollectionAssert.AreEqual(expectedStudentAwards.ToArray(), actualStudentAwards.ToArray());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(null, year);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, It.IsAny<bool>())).Throws(new PermissionsException("pe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId, year);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, It.IsAny<bool>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId, year);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, It.IsAny<bool>())).Throws(new InvalidOperationException("ioe"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId, year);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, It.IsAny<bool>())).Throws(new Exception("e"));

                try
                {
                    await studentAwardsController.GetStudentAwardsAsync(studentId, year);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

        }

        [TestClass]
        public class GetSingleStudentAwardsTests : StudentAwardsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actualStudentAward = await studentAwardsController.GetStudentAwardAsync(studentId, year, awardId);
                var expectedStudentAward = expectedStudentAwards.First(a => a.StudentId == studentId && a.AwardYearId == year && a.AwardId == awardId);

                Assert.AreEqual(expectedStudentAward, actualStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardAsync(null, year, awardId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, null, awardId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, year, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, awardId)).Throws(new PermissionsException("pe"));

                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, year, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, awardId)).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, year, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, awardId)).Throws(new InvalidOperationException("ioe"));

                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, year, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.GetStudentAwardsAsync(studentId, year, awardId)).Throws(new Exception("e"));

                try
                {
                    await studentAwardsController.GetStudentAwardAsync(studentId, year, awardId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }


        [TestClass]
        public class PutStudentAwardPackageTests : StudentAwardsControllerTests
        {
            public StudentAwardPackage inputStudentAwardPackage;
            public StudentAwardPackage outputStudentAwardPackage;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                inputStudentAwardPackage = new StudentAwardPackage() { StudentAwards = expectedStudentAwards.Where(a => a.AwardYearId == year) };

            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var outputStudentAwardPackage =
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, inputStudentAwardPackage);

                CollectionAssert.AreEqual(inputStudentAwardPackage.StudentAwards.ToList(), outputStudentAwardPackage.StudentAwards.ToList(), studentAwardComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(null, year, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, null, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentAwardListThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new PermissionsException("pe"));

                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateRequiresReviewExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new UpdateRequiresReviewException("urre"));

                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<UpdateRequiresReviewException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new Exception("e"));

                try
                {
                    await studentAwardsController.PutStudentAwardPackageAsync(studentId, year, inputStudentAwardPackage);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class PutStudentAwardTests : StudentAwardsControllerTests
        {
            public StudentAward inputStudentAward;
            public StudentAward outputStudentAward;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                inputStudentAward = expectedStudentAwards.First();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                outputStudentAward = await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, inputStudentAward);

                Assert.AreEqual(inputStudentAward, outputStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardAsync(null, year, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, null, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputAwardIdThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, null, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullInputStudentAwardThrowsExceptionTest()
            {
                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, awardId, It.IsAny<StudentAward>())).Throws(new PermissionsException("pe"));

                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, awardId, It.IsAny<StudentAward>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateRequiresReviewExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, awardId, It.IsAny<StudentAward>())).Throws(new UpdateRequiresReviewException("urre"));

                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<UpdateRequiresReviewException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(studentId, year, awardId, It.IsAny<StudentAward>())).Throws(new Exception("e"));

                try
                {
                    await studentAwardsController.PutStudentAwardAsync(studentId, year, awardId, inputStudentAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        [Obsolete("Obsolete as of version 1.7.")]
        public class UpdateYearStudentAwardsTests : StudentAwardsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                var expectedStudentAwardsForYear = expectedStudentAwards.Where(a => a.AwardYearId == year);
                var updatedStudentAwards = studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwardsForYear);

                CollectionAssert.AreEqual(expectedStudentAwardsForYear.ToArray(), updatedStudentAwards.ToArray());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAwards(null, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAwards(studentId, null, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputStudentAwardListThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new PermissionsException("pe"));

                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InvalidOperationExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new InvalidOperationException("ioe"));

                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void OperationCanceledExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new OperationCanceledException("oce"));

                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<OperationCanceledException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<StudentAward>>(), It.IsAny<bool>())).Throws(new Exception("e"));

                try
                {
                    studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwards.Where(a => a.AwardYearId == year));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        [Obsolete("Obsolete as of version 1.7.")]
        public class UpdateSingleStudentAwardsTests : StudentAwardsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                var expectedStudentAwardsForYear = expectedStudentAwards.Where(a => a.AwardYearId == year);
                var updatedStudentAwards = studentAwardsController.PostStudentAwards(studentId, year, expectedStudentAwardsForYear);

                CollectionAssert.AreEqual(expectedStudentAwardsForYear.ToArray(), updatedStudentAwards.ToArray());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputStudentIdThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAward(null, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputAwardYearThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAward(studentId, null, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputAwardIdThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, null, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullInputStudentAwardListThrowsExceptionTest()
            {
                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PermisisonsExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentAward>())).Throws(new PermissionsException("pe"));

                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void KeyNotFoundExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentAward>())).Throws(new KeyNotFoundException("knfe"));

                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InvalidOperationExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentAward>())).Throws(new InvalidOperationException("ioe"));

                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void OperationCanceledExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentAward>())).Throws(new OperationCanceledException("oce"));

                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<OperationCanceledException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void ExceptionLogsMessageTest()
            {
                studentAwardServiceMock.Setup(s => s.UpdateStudentAwardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentAward>())).Throws(new Exception("e"));

                try
                {
                    studentAwardsController.PostStudentAward(studentId, year, awardId, expectedStudentAwards.First(a => a.AwardYearId == year && a.AwardId == awardId));
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
