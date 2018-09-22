// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class RelationshipsControllerTests
    {
        [TestClass]
        public class RelationshipsControllerGet
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

            private RelationshipsController relationshipsController;
            private Mock<IRelationshipService> relServiceMock;
            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                relServiceMock = new Mock<IRelationshipService>();
                relServiceMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonPrimaryRelationshipsAsync(It.IsAny<string>())).Returns(Task.FromResult(relDataFromService().Where(x => x.IsActive && x.IsPrimaryRelationship)));
                relServiceMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonPrimaryRelationshipsAsync((string)null)).Throws(new Exception());
                relServiceMock.Setup<Task<IEnumerable<Relationship>>>(x => x.GetPersonPrimaryRelationshipsAsync(string.Empty)).Throws(new Exception());

                relationshipsController = new RelationshipsController(relServiceMock.Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                relationshipsController = null;
                relServiceMock = null;
            }

            #region GetPersonPrimaryRelationshipsAsync
            [TestMethod]
            public async Task RelationshipsController_GetPersonPrimaryRelationshipsAsync_Valid()
            {
                var svcData = await relationshipsController.GetPersonPrimaryRelationshipsAsync("foo");
                Assert.AreEqual(relDataFromService().Where(x=>x.IsActive&&x.IsPrimaryRelationship).Count(), svcData.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RelationshipsController_GetPersonPrimaryRelationshipsAsync_NullId()
            {
                var svcData = await relationshipsController.GetPersonPrimaryRelationshipsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RelationshipsController_GetPersonPrimaryRelationshipsAsync_EmptyId()
            {
                var svcData = await relationshipsController.GetPersonPrimaryRelationshipsAsync(string.Empty);
            }

            private IEnumerable<Dtos.Base.Relationship> relDataFromService()
            {
                return new Collection<Relationship>(){
                    new Relationship(){PrimaryEntity = "PrimaryId", OtherEntity = "OtherId", RelationshipType = "C", IsPrimaryRelationship = true, StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, IsActive = true},
                    new Relationship(){PrimaryEntity = "PrimaryId", OtherEntity = "OtherId", RelationshipType = "CZ", IsPrimaryRelationship = false, StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, IsActive = true},
                };
            }
            #endregion

        }

        [TestClass]
        public class RelationshipsControllerPost
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

            private RelationshipsController relationshipsController;
            private Mock<IRelationshipService> relServiceMock;
            ILogger logger = new Mock<ILogger>().Object;

            private Relationship badRelationship = new Relationship() 
            { 
                PrimaryEntity = "BadRelationship",
            };
            private Relationship goodRelationship = new Relationship() 
            { 
                PrimaryEntity = "P1", 
                RelationshipType = "P", 
                OtherEntity = "P2", 
                IsActive = true, 
                EndDate = null, 
                StartDate = null, 
                IsPrimaryRelationship = true,
            };

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                relServiceMock = new Mock<IRelationshipService>();

                // Mock a good relationship input
                relServiceMock.Setup<Task<Relationship>>(x => x.PostRelationshipAsync(
                    It.Is<string>(y => y.Equals(goodRelationship.OtherEntity)),
                    It.Is<string>(y => y.Equals(goodRelationship.RelationshipType)),
                    It.Is<string>(y=> y.Equals(goodRelationship.PrimaryEntity))))
                    .Returns(Task.FromResult(goodRelationship));

                // Mock a bad relationship input
                relServiceMock.Setup<Task<Relationship>>(x => x.PostRelationshipAsync(
                    It.Is<string>(y => y.Equals(badRelationship.PrimaryEntity)),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                    .Throws(new Exception());

                relationshipsController = new RelationshipsController(relServiceMock.Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                relationshipsController = null;
                relServiceMock = null;
            }

            #region PostRelationshipAsync
            [TestMethod]
            public async Task RelationshipsController_PostRelationshipAsync_Valid()
            {
                var rel = await relationshipsController.PostRelationshipAsync(goodRelationship);
                Assert.AreEqual(goodRelationship.PrimaryEntity, rel.PrimaryEntity);
                Assert.AreEqual(goodRelationship.RelationshipType, rel.RelationshipType);
                Assert.AreEqual(goodRelationship.OtherEntity, rel.OtherEntity);
                Assert.AreEqual(goodRelationship.IsActive, rel.IsActive);
                Assert.AreEqual(goodRelationship.IsPrimaryRelationship, rel.IsPrimaryRelationship);
                Assert.AreEqual(goodRelationship.EndDate, rel.EndDate);
                Assert.AreEqual(goodRelationship.StartDate, rel.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RelationshipsController_PostRelationshipAsync_InvalidRelationship()
            {
                var svcData = await relationshipsController.PostRelationshipAsync(badRelationship);
            }
            #endregion
        }
    }
}
