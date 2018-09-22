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
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CommentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ICommentsService> commentsServiceMock;
        private Mock<ILogger> loggerMock;

        private CommentsController commentsController;

        private IEnumerable<Domain.Base.Entities.RemarkType> allRemarkTypes;
        private List<Dtos.Comments> commentsCollection;

        private const string commentSubjectAreaGuid = "a830e686-7692-4012-8da5-b1b5d44389b4"; //BU

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            commentsServiceMock = new Mock<ICommentsService>();
            loggerMock = new Mock<ILogger>();

            commentsCollection = new List<Dtos.Comments>();

            var allRemarks = new TestRemarksRepository().GetRemarkCode().ToList();
            allRemarkTypes = new TestRemarkTypeRepository().GetRemarkType().ToList();  //commentSubjectArea
            var allRemarkCodes = new TestRemarkCodeRepository().GetRemarkCode().ToList();  //source

            foreach (var source in allRemarks)
            {
                var comment = new Ellucian.Colleague.Dtos.Comments
                {
                    Id = source.Guid,
                    Comment = source.RemarksText,
                    Confidentiality = ConvertConfidentialityTypeEnumToConfidentialityCategoryEnum(source.RemarksPrivateType),
                    EnteredOn = source.RemarksDate
                };

                var commentSubjectArea = allRemarkTypes.FirstOrDefault(x => x.Code == source.RemarksType);
                if (commentSubjectArea != null)
                    comment.CommentSubjectArea = new Dtos.GuidObject2(commentSubjectArea.Guid);

                var remarksCode = allRemarkCodes.FirstOrDefault(x => x.Code == source.RemarksCode);
                if (remarksCode != null)
                    comment.Source = new Dtos.GuidObject2(remarksCode.Guid);


                commentsCollection.Add(comment);
            }

            var expected = commentsCollection.FirstOrDefault();

            commentsController = new CommentsController(commentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            commentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            commentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            commentsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(expected));
        }

        [TestCleanup]
        public void Cleanup()
        {
            commentsController = null;
            commentsCollection = null;
            loggerMock = null;
            allRemarkTypes = null;
            commentsServiceMock = null;
        }

        #region Comments

        [TestMethod]
        public async Task CommentsController_GetComments_CommentSubjectArea()
        {
            commentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false , Public = true};

            var commentCollectionSubjectsArea = commentsCollection.Where(x => x.CommentSubjectArea.Id == commentSubjectAreaGuid);

            int offset = 0;
            int Limit = commentCollectionSubjectsArea.Count();

            var expectedCollection = new Tuple<IEnumerable<Dtos.Comments>, int>(commentCollectionSubjectsArea, Limit);
            commentsServiceMock.Setup(x => x.GetCommentsAsync(offset, Limit, "", commentSubjectAreaGuid, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

            Paging paging = new Paging(Limit, offset);
            var comments = (await commentsController.GetCommentsAsync(paging,"",  commentSubjectAreaGuid));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await comments.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.Comments> results = ((ObjectContent<IEnumerable<Dtos.Comments>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Comments>;

            Assert.IsNotNull(results);
            Assert.AreEqual(Limit, results.Count());

            foreach (var actual in results)
            {
                var expected = commentCollectionSubjectsArea.FirstOrDefault(i => i.Id.Equals(actual.Id));
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.CommentSubjectArea, actual.CommentSubjectArea);
                Assert.AreEqual(expected.Confidentiality, actual.Confidentiality);
               
                Assert.AreEqual(expected.EnteredBy, actual.EnteredBy);
                Assert.AreEqual(expected.EnteredOn, actual.EnteredOn);
                Assert.AreEqual(expected.Source, actual.Source);
                Assert.AreEqual(expected.SubjectMatter, actual.SubjectMatter);
            }
        }

        [TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_Null_ArgumentNullException()
        {
            //await commentsController.GetCommentsAsync(null, null);
            var comments = await commentsController.GetCommentsAsync(null, null);
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await comments.ExecuteAsync(cancelToken);
            IEnumerable<Dtos.Comments> results = ((ObjectContent<IEnumerable<Dtos.Comments>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Comments>;
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_String_ArgumentNullException()
        {
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", "");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_PermissionsException()
        {

            commentsServiceMock.Setup(x => x.GetCommentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).Throws<PermissionsException>();
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", commentSubjectArea: commentSubjectAreaGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_ArgumentException()
        {

            commentsServiceMock.Setup(x => x.GetCommentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).Throws<ArgumentException>();
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", commentSubjectArea: commentSubjectAreaGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_RepositoryException()
        {

            commentsServiceMock.Setup(x => x.GetCommentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).Throws<RepositoryException>();
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", commentSubjectArea: commentSubjectAreaGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_IntegrationApiException()
        {

            commentsServiceMock.Setup(x => x.GetCommentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).Throws<IntegrationApiException>();
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", commentSubjectArea: commentSubjectAreaGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetComments_Exception()
        {

            commentsServiceMock.Setup(x => x.GetCommentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true)).Throws<Exception>();
            await commentsController.GetCommentsAsync(It.IsAny<Paging>(), "", commentSubjectArea: commentSubjectAreaGuid);
        }

        #endregion GetComments

        #region GetCommentsByGuid

        [TestMethod]
        public async Task CommentsController_GetCommentsByGuid()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = (await commentsController.GetCommentsByGuidAsync(expected.Id));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
            Assert.AreEqual(expected.CommentSubjectArea, actual.CommentSubjectArea, "CommentSubjectArea");
            Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
           
            Assert.AreEqual(expected.EnteredBy, actual.EnteredBy, "EnteredBy");
            Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
            Assert.AreEqual(expected.Source, actual.Source, "Source");
            Assert.AreEqual(expected.SubjectMatter, actual.SubjectMatter, "SubjectMatter");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_NullArgument()
        {
            await commentsController.GetCommentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_EmptyArgument()
        {
            await commentsController.GetCommentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_PermissionsException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).Throws<PermissionsException>();
            await commentsController.GetCommentsByGuidAsync(expected.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_ArgumentException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).Throws<ArgumentException>();
            await commentsController.GetCommentsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_RepositoryException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).Throws<RepositoryException>();
            await commentsController.GetCommentsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_IntegrationApiException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).Throws<IntegrationApiException>();
            await commentsController.GetCommentsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_GetCommentsByGuid_Exception()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).Throws<Exception>();
            await commentsController.GetCommentsByGuidAsync(expected.Id);
        }

        #endregion GetCommentsByGuid

        #region Put

        [TestMethod]
        public async Task CommentsController_PutComments()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, It.IsAny<Dtos.Comments>())).ReturnsAsync(expected);
            commentsServiceMock.Setup(x => x.GetCommentByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = (await commentsController.PutCommentsAsync(expected.Id, expected));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
            Assert.AreEqual(expected.CommentSubjectArea, actual.CommentSubjectArea, "CommentSubjectArea");
            Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
           
            Assert.AreEqual(expected.EnteredBy, actual.EnteredBy, "EnteredBy");
            Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
            Assert.AreEqual(expected.Source, actual.Source, "Source");
            Assert.AreEqual(expected.SubjectMatter, actual.SubjectMatter, "SubjectMatter");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_NullArgument()
        {
            await commentsController.PutCommentsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_EmptyArgument()
        {
            await commentsController.PutCommentsAsync("", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_PermissionsException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<PermissionsException>();
            await commentsController.PutCommentsAsync(expected.Id, expected);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_ArgumentException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<ArgumentException>();
            await commentsController.PutCommentsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_RepositoryException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<RepositoryException>();
            await commentsController.PutCommentsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_IntegrationApiException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<IntegrationApiException>();
            await commentsController.PutCommentsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_ConfigurationException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<ConfigurationException>();
            await commentsController.PutCommentsAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PutComments_Exception()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PutCommentAsync(expected.Id, expected)).Throws<Exception>();
            await commentsController.PutCommentsAsync(expected.Id, expected);
        }
        #endregion


        #region Post

        [TestMethod]
        public async Task CommentsController_PostComments()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).ReturnsAsync(expected);

            var actual = (await commentsController.PostCommentsAsync(expected));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
            Assert.AreEqual(expected.CommentSubjectArea, actual.CommentSubjectArea, "CommentSubjectArea");
            Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
           
            Assert.AreEqual(expected.EnteredBy, actual.EnteredBy, "EnteredBy");
            Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
            Assert.AreEqual(expected.Source, actual.Source, "Source");
            Assert.AreEqual(expected.SubjectMatter, actual.SubjectMatter, "SubjectMatter");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_NullArgument()
        {
            await commentsController.PostCommentsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_PermissionsException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<PermissionsException>();
            await commentsController.PostCommentsAsync(expected);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_ArgumentException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<ArgumentException>();
            await commentsController.PostCommentsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_RepositoryException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<RepositoryException>();
            await commentsController.PostCommentsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_IntegrationApiException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<IntegrationApiException>();
            await commentsController.PostCommentsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_ConfigurationException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<ConfigurationException>();
            await commentsController.PostCommentsAsync(expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_PostComments_Exception()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.PostCommentAsync(expected)).Throws<Exception>();
            await commentsController.PostCommentsAsync(expected);
        }
        #endregion


        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_EmptyArgument()
        {
            var expected = commentsCollection.FirstOrDefault();
            await commentsController.DeleteCommentByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_PermissionsException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<PermissionsException>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_ArgumentException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<ArgumentException>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_RepositoryException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<RepositoryException>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_IntegrationApiException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<IntegrationApiException>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_ConfigurationException()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<ConfigurationException>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CommentsController_DeleteComments_Exception()
        {
            var expected = commentsCollection.FirstOrDefault();
            commentsServiceMock.Setup(x => x.DeleteCommentByIdAsync(expected.Id)).Throws<Exception>();
            await commentsController.DeleteCommentByGuidAsync(expected.Id);
        }
        #endregion


        private Dtos.EnumProperties.ConfidentialCategory ConvertConfidentialityTypeEnumToConfidentialityCategoryEnum(ConfidentialityType confidentialityType)
        {
            switch (confidentialityType)
            {
                case ConfidentialityType.Public:
                    return Dtos.EnumProperties.ConfidentialCategory.Public;
                case ConfidentialityType.Private:
                    return Dtos.EnumProperties.ConfidentialCategory.Private;
                default:
                    return Dtos.EnumProperties.ConfidentialCategory.Public;
            }
        }
    }
}