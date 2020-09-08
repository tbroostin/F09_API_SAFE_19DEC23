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
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CommentSubjectAreaService2Tests
    {
        private const string _remarkTypeGuid = "eddde9d0-b81d-4e59-850c-b439221c1e81";
        private const string _remarkTypeCode = "PE";
        private const string _remarkTypeDesc = "Personal";

        private ICollection<RemarkType> _remarkTypeCollection;

        private ICollection<CommentSubjectArea> _commentSubjectAreaCollection;

        private CommentSubjectAreaService _commentSubjectAreaService;
        private Mock<ILogger> _loggerMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            _commentSubjectAreaCollection = new List<CommentSubjectArea>();

            _remarkTypeCollection = new TestRemarkTypeRepository().GetRemarkType().ToList();

            _commentSubjectAreaService
                = new CommentSubjectAreaService(_referenceRepositoryMock.Object,
               _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
               _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _commentSubjectAreaService = null;
            _commentSubjectAreaCollection = null;
            _loggerMock = null;
            _remarkTypeCollection = null;
        }

        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreas_NonCache()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(true))
                .ReturnsAsync(_remarkTypeCollection);

            var results = await _commentSubjectAreaService.GetCommentSubjectAreaAsync(true);
            Assert.IsTrue(results is IEnumerable<CommentSubjectArea>);
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreas_Cache()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(false))
                .ReturnsAsync(_remarkTypeCollection);

            var results = await _commentSubjectAreaService.GetCommentSubjectAreaAsync(false);
            Assert.IsTrue(results is IEnumerable<CommentSubjectArea>);
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Count());
        }



        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreasAsync_Properties()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(true))
                .ReturnsAsync(_remarkTypeCollection);

            var result =
                (await _commentSubjectAreaService.GetCommentSubjectAreaAsync(true)).FirstOrDefault(x => x.Code == _remarkTypeCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
        }

        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreasAsync_Expected()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(It.IsAny<bool>()))
               .ReturnsAsync(_remarkTypeCollection);

            var expectedResults =
                _remarkTypeCollection.First(c => c.Code == _remarkTypeCode);

            var actualResult =
                (await _commentSubjectAreaService.GetCommentSubjectAreaAsync(true)).FirstOrDefault(x => x.Id == _remarkTypeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreaByIdAsync_Empty()
        {
            await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreaByIdAsync_Null()
        {
            await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreaByIdAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(It.IsAny<bool>()))
                .Throws<InvalidOperationException>();

            await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync("99");
        }

        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreaByIdAsync_Expected()
        {
            var expectedResults =
               _remarkTypeCollection.First(c => c.Code == _remarkTypeCode);

            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_remarkTypeCollection); 

            var actualResult =
                await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync(_remarkTypeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);

        }

        [TestMethod]
        public async Task CommentSubjectAreaService_GetCommentSubjectAreaByIdAsync_Properties()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRemarkTypesAsync(It.IsAny<bool>()))
               .ReturnsAsync(_remarkTypeCollection); 
            
            var result =
                await _commentSubjectAreaService.GetCommentSubjectAreaByIdAsync(_remarkTypeGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
        }
    }
}