// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CommentSubjectAreaControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICommentSubjectAreaService> commentSubjectAreaServiceMock;
        private Mock<ILogger> loggerMock;
  
        private CommentSubjectAreaController commentSubjectAreaController;
       
        private IEnumerable<Domain.Base.Entities.RemarkType> allRemarkTypes;
        private List<Dtos.CommentSubjectArea> commentSubjectAreaCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            commentSubjectAreaServiceMock = new Mock<ICommentSubjectAreaService>();
            loggerMock = new Mock<ILogger>();
            
            commentSubjectAreaCollection = new List<Dtos.CommentSubjectArea>();

            allRemarkTypes = new TestRemarkTypeRepository().GetRemarkType().ToList();
           
            foreach (var source in allRemarkTypes)
            {
                var interest = new Ellucian.Colleague.Dtos.CommentSubjectArea
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,

                };              
                commentSubjectAreaCollection.Add(interest);
            }

            commentSubjectAreaController = new CommentSubjectAreaController(commentSubjectAreaServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            commentSubjectAreaController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            commentSubjectAreaController = null;
            commentSubjectAreaCollection = null;
            loggerMock = null;
            allRemarkTypes = null;
            commentSubjectAreaServiceMock = null;
        }

        [TestMethod]
        public async Task CommentSubjectAreaController_GetCommentSubjectArea_ValidateFields_Cache()
        {
            commentSubjectAreaController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaAsync(false)).ReturnsAsync(commentSubjectAreaCollection);

            var commentSubjectArea = (await commentSubjectAreaController.GetCommentSubjectAreaAsync()).ToList();
            Assert.AreEqual(commentSubjectAreaCollection.Count, commentSubjectArea.Count);
            for (var i = 0; i < commentSubjectArea.Count; i++)
            {
                var expected = commentSubjectAreaCollection[i];
                var actual = commentSubjectArea[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CommentSubjectAreaController_GetCommentSubjectArea_ValidateFields_BypassCache()
        {
            commentSubjectAreaController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaAsync(true)).ReturnsAsync(commentSubjectAreaCollection);

            var commentSubjectArea = (await commentSubjectAreaController.GetCommentSubjectAreaAsync()).ToList();
            Assert.AreEqual(commentSubjectAreaCollection.Count, commentSubjectArea.Count);
            for (var i = 0; i < commentSubjectArea.Count; i++)
            {
                var expected = commentSubjectAreaCollection[i];
                var actual = commentSubjectArea[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CommentSubjectAreaController_GetCommentSubjectAreaById_ValidateFields()
        {
            var expected = commentSubjectAreaCollection.FirstOrDefault();
            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await commentSubjectAreaController.GetCommentSubjectAreaByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_GetCommentSubjectArea_Exception()
        {
            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaAsync(It.IsAny<bool>())).Throws<Exception>();
            await commentSubjectAreaController.GetCommentSubjectAreaAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_GetCommentSubjectAreaById_Exception()
        {
            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await commentSubjectAreaController.GetCommentSubjectAreaByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_GetCommentSubjectAreaById_KeyNotFoundException()
        {
            commentSubjectAreaServiceMock.Setup(x => x.GetCommentSubjectAreaByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await commentSubjectAreaController.GetCommentSubjectAreaByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_PostCommentSubjectAreaAsync_Exception()
        {
            await commentSubjectAreaController.PostCommentSubjectAreaAsync(commentSubjectAreaCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_PutCommentSubjectArea_Exception()
        {
            var commentSubjectArea = commentSubjectAreaCollection.FirstOrDefault();
            await commentSubjectAreaController.PutCommentSubjectAreaAsync(commentSubjectArea.Id, commentSubjectArea);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentSubjectAreaController_DeleteCommentSubjectArea_Exception()
        {
            await commentSubjectAreaController.DeleteCommentSubjectAreaAsync(commentSubjectAreaCollection.FirstOrDefault().Id);
        }
    }
}