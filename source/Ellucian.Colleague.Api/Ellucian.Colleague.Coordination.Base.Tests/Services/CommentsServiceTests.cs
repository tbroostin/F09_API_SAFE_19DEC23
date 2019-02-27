// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CommentsServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetComments : CurrentUserSetup
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private IRemarkRepository _remarkRepository;
            private Mock<IRemarkRepository> _remarkRepositoryMock;
            private Mock<IColleagueTransactionInvoker> _transManagerMock;
            private IPersonRepository _personRepository;
            private Mock<IPersonRepository> _personRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private ILogger _logger;
            private CommentsService _commentsService;
            IEnumerable<Remark> _allRemarks;
       
            //private IEnumerable<Domain.Base.Entities.RemarkType> allRemarkTypes;
            private List<Dtos.Comments> commentsCollection;
            private const string commentSubjectAreaGuid = "a830e686-7692-4012-8da5-b1b5d44389b4"; //BU
            private const string remarkGuid =  "a3a2e49b-df50-4133-9507-ecad4e04004d";
            private const string subjectMatterPersonId = "62349056-354b-4576-a7cd-c4b8ce28bb80";

            private Domain.Entities.Permission permissionViewAnyComment;
            private Domain.Entities.Permission permissionCreateUpdateAnyComment;
            private Domain.Entities.Permission permissionDeleteAnyComment;

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                _remarkRepositoryMock = new Mock<IRemarkRepository>();
                _remarkRepository = _remarkRepositoryMock.Object;

                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;
                _transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock permissions
                permissionViewAnyComment = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewComment);
                permissionCreateUpdateAnyComment = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdateComment);
                permissionDeleteAnyComment = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.DeleteComment);
                facultyRole.AddPermission(permissionViewAnyComment);
                facultyRole.AddPermission(permissionCreateUpdateAnyComment);
                facultyRole.AddPermission(permissionDeleteAnyComment);
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });

                commentsCollection = new List<Dtos.Comments>();

                _allRemarks = new TestRemarksRepository().GetRemarkCode().ToList();
                var allRemarkTypes = new TestRemarkTypeRepository().GetRemarkType().ToList();  //commentSubjectArea
                var allRemarkCodes = new TestRemarkCodeRepository().GetRemarkCode().ToList();  //source

                foreach (var source in _allRemarks)
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

                    comment.SubjectMatter = new SubjectMatterDtoProperty()
                    {
                        Person = new Dtos.GuidObject2() { Id = subjectMatterPersonId }
                    };

                    comment.EnteredBy = new EnteredByDtoProperty
                    {
                        Name = source.RemarksIntgEnteredBy
                    };

                    commentsCollection.Add(comment);
                }


                _referenceDataRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(It.IsAny<bool>()))
               .ReturnsAsync(allRemarkTypes);

                _referenceDataRepositoryMock.Setup(repo => repo.GetRemarkCodesAsync(It.IsAny<bool>()))
              .ReturnsAsync(allRemarkCodes);


                _commentsService = new CommentsService(_adapterRegistry, _currentUserFactory,
                    _roleRepo, _remarkRepository, _referenceDataRepository, _personRepository, baseConfigurationRepository, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _allRemarks = null;
                _referenceDataRepository = null;
                _commentsService = null;
                _adapterRegistry = null;
                _adapterRegistryMock = null;         
                _logger = null;
                _personRepository = null;
                _personRepositoryMock = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _remarkRepository = null;
                _remarkRepositoryMock = null;
                _roleRepo = null;
                _roleRepoMock = null;
                _transManagerMock = null;
                
            }

            #region GetCommentsAsync

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task CommentsService_Comments_ArgumentNullException()
            //{
            //    await _commentsService.GetCommentsAsync(, "", "", "");
            //}

            [TestMethod]
            public async Task CommentsService_GetComments_CommentSubjectArea()
            {
                //GUID                                   AUTHOR     CODE   DONORID   INTG   PRVT  TEXT               TYPE                              DONORID
                //{"a3a2e49b-df50-4133-9507-ecad4e04004d", "0011905", "ST", "0013395", "BSF", "1", "This is a comment", "BU"}, 
                Tuple<IEnumerable<Remark>, int> remark = new Tuple<IEnumerable<Remark>, int>(_allRemarks.Where(x => x.Guid == remarkGuid), 1);
                _remarkRepositoryMock.Setup( x => x.GetRemarksAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<string>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);

                var actualComments = (await _commentsService.GetCommentsAsync(0,100,"", commentSubjectAreaGuid));

                var expectedComments = (commentsCollection.Where(x => x.Id == remarkGuid)).ToList();

               // Assert.IsTrue(actualComments is Tuple<IEnumerable<Remark>, int>);
                
                var actual = actualComments.Item1.ElementAtOrDefault(0);
                var expected = expectedComments.ElementAtOrDefault(0);

                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
                Assert.AreEqual(expected.CommentSubjectArea.Id, actual.CommentSubjectArea.Id, "CommentSubjectArea");
                Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
                Assert.AreEqual(expected.EnteredBy.Id, actual.EnteredBy.Id, "EnteredBy");
                Assert.AreEqual(expected.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
                Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
                Assert.AreEqual(expected.Source.Id, actual.Source.Id, "Source");
                if (expected.SubjectMatter != null)
                {
                    if (expected.SubjectMatter.Organization != null)
                    {
                        Assert.AreEqual(expected.SubjectMatter.Organization.Id, actual.SubjectMatter.Organization.Id, "SubjectMatter");
                    }
                    if (expected.SubjectMatter.Person != null)
                    {
                        Assert.AreEqual(expected.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
                    }
                }
            }

            [TestMethod]
            public async Task CommentsService_GetAllComments()
            {
                Tuple<IEnumerable<Remark>, int> remark = new Tuple<IEnumerable<Remark>, int>(_allRemarks, _allRemarks.Count());
                _remarkRepositoryMock.Setup(x => x.GetRemarksAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<string>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);

                var actualComments = (await _commentsService.GetCommentsAsync(0, 100, "", ""));
                Assert.IsNotNull(actualComments);
                var expectedComments = commentsCollection;

                foreach(var actual in actualComments.Item1)
                {
                    var expected = expectedComments.FirstOrDefault(i => i.Id.Equals(actual.Id));

                    Assert.AreEqual(expected.Id, actual.Id, "Id");
                    Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
                    Assert.AreEqual(expected.CommentSubjectArea.Id, actual.CommentSubjectArea.Id, "CommentSubjectArea");
                    Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
                    Assert.AreEqual(expected.EnteredBy.Id, actual.EnteredBy.Id, "EnteredBy");
                    Assert.AreEqual(expected.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
                    Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
                    Assert.AreEqual(expected.Source.Id, actual.Source.Id, "Source");
                    if (expected.SubjectMatter != null)
                    {
                        if (expected.SubjectMatter.Organization != null)
                        {
                            Assert.AreEqual(expected.SubjectMatter.Organization.Id, actual.SubjectMatter.Organization.Id, "SubjectMatter");
                        }
                        if (expected.SubjectMatter.Person != null)
                        {
                            Assert.AreEqual(expected.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
                        }
                    }
                }
                
            }


            #endregion GetCommentsAsync

            #region GetCommentById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_GetCommentById_ArgumentNullException()
            {
                await _commentsService.GetCommentByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_GetCommentById_InvalidID()
            {
                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.GetRemarkByGuidAsync(remark.Guid)).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");

                var actual = await _commentsService.GetCommentByIdAsync("invalid");
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CommentsService_GetCommentById_InvalidOperationException()
            {
                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.GetRemarkByGuidAsync(remark.Guid)).Throws<InvalidOperationException>();
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");

                var actual = await _commentsService.GetCommentByIdAsync(remarkGuid);
            }

            [TestMethod]
            public async Task CommentsService_GetCommentById()
            {
                //GUID                                   AUTHOR     CODE   DONORID   INTG   PRVT  TEXT               TYPE                              DONORID
                //{"a3a2e49b-df50-4133-9507-ecad4e04004d", "0011905", "ST", "0013395", "BSF", "1", "This is a comment", "BU"}, 
                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.GetRemarkByGuidAsync(remark.Guid)).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("48285630-3f7f-4b16-b73d-1b1a4bdd27ee");
                
                var actual = await _commentsService.GetCommentByIdAsync(remark.Guid);

                var expected = commentsCollection.FirstOrDefault(x => x.Id == remark.Guid);

                Assert.IsTrue(actual is Dtos.Comments);

                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Comment, actual.Comment, "Comment");
                Assert.AreEqual(expected.CommentSubjectArea.Id, actual.CommentSubjectArea.Id, "CommentSubjectArea");
                Assert.AreEqual(expected.Confidentiality, actual.Confidentiality, "Confidentiality");
               
               // Assert.AreEqual(expected.EnteredBy, actual.EnteredBy, "EnteredBy");
                Assert.AreEqual(expected.EnteredOn, actual.EnteredOn, "EnteredOn");
               // Assert.AreEqual(expected.Source, actual.Source, "Source");
               //Assert.AreEqual(expected.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");

            }
            #endregion GetCommentById

            #region PostComment

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_ArgumentNullException()
            {
                await _commentsService.PostCommentAsync(null);
            }

           
            [TestMethod]       
            public async Task CommentsService_PostComment()
            {
               var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);
        
                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");
               
            
                var actual = await _commentsService.PostCommentAsync(comment);

                Assert.AreEqual(comment.Id, actual.Id, "Id");
                Assert.AreEqual(comment.Comment, actual.Comment, "Comment");
                Assert.AreEqual(comment.CommentSubjectArea.Id, actual.CommentSubjectArea.Id, "CommentSubjectArea");
                Assert.AreEqual(comment.Confidentiality, actual.Confidentiality, "Confidentiality");
             
                Assert.AreEqual(comment.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
                Assert.AreEqual(comment.EnteredOn, actual.EnteredOn, "EnteredOn");
                Assert.AreEqual(comment.Source.Id, actual.Source.Id, "Source");
                Assert.AreEqual(comment.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_NullId()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.Id = null;

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_CommentNull()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.Comment = null;

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_CommentEmpty()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.Comment = string.Empty;

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_SubjectMatterNull()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.SubjectMatter = null;

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_SubjectMatterPersonIdNull()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.SubjectMatter = new SubjectMatterDtoProperty()
                {
                    Person = new GuidObject2() {  Id = null}
                };

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_CommentSubjectAreaIdNull()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.CommentSubjectArea = new GuidObject2 { Id = null };

                await _commentsService.PostCommentAsync(comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PostComment_SourceIdNull()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");

                comment.Source = new GuidObject2 { Id = null };

                await _commentsService.PostCommentAsync(comment);
            }
          
            #endregion PostComment

            #region PutComment

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PutComment_NullArguments()
            {
                await _commentsService.PutCommentAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PutComment_NullId()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);
                await _commentsService.PutCommentAsync(null, comment);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_PutComment_NullComment()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);
                await _commentsService.PutCommentAsync(comment.Id, null);
            }

            [TestMethod]
            public async Task CommentsService_PutComment()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");


                var actual = await _commentsService.PutCommentAsync(comment.Id, comment);

                Assert.AreEqual(comment.Id, actual.Id, "Id");
                Assert.AreEqual(comment.Comment, actual.Comment, "Comment");
                Assert.AreEqual(comment.CommentSubjectArea.Id, actual.CommentSubjectArea.Id, "CommentSubjectArea");
                Assert.AreEqual(comment.Confidentiality, actual.Confidentiality, "Confidentiality");

                Assert.AreEqual(comment.EnteredBy.Name, actual.EnteredBy.Name, "EnteredBy");
                Assert.AreEqual(comment.EnteredOn, actual.EnteredOn, "EnteredOn");
                Assert.AreEqual(comment.Source.Id, actual.Source.Id, "Source");
                Assert.AreEqual(comment.SubjectMatter.Person.Id, actual.SubjectMatter.Person.Id, "SubjectMatter");
            }
            #endregion

            #region DeleteComment

          
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_DeleteComment_NullId()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);
                await _commentsService.DeleteCommentByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CommentsService_DeleteComment_EmptyId()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);
                await _commentsService.DeleteCommentByIdAsync("");
            }
          
            [TestMethod]
            public async Task CommentsService_DeleteComment()
            {
                var comment = commentsCollection.FirstOrDefault(x => x.Id == remarkGuid);

                IEnumerable<Remark> remarks = _allRemarks.Where(x => x.Guid == remarkGuid);
                var remark = remarks.ElementAtOrDefault(0);
                _remarkRepositoryMock.Setup(x => x.UpdateRemarkAsync(It.IsAny<Remark>())).ReturnsAsync(remark);
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(subjectMatterPersonId);
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0011905");
                _remarkRepositoryMock.Setup(x => x.GetRemarkByGuidAsync(remark.Guid)).ReturnsAsync(remark);
                await _commentsService.DeleteCommentByIdAsync(comment.Id);
            }
            #endregion

            #region helper methods

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
            #endregion

        }
    }
}