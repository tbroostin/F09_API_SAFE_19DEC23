// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class DocumentServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class DocumentUserFactory : ICurrentUserFactory
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
        public class GetTextDocumentAsync
        {
            private IDocumentRepository _documentRepository;
            private Mock<IDocumentRepository> _documentRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IRoleRepository _roleRepository;
            private ILogger _logger;

            private DocumentService _documentService;

            private string documentId;
            private string primaryEntity; 
            private string primaryId; 
            private string personId;
            private IDictionary<string, string> secondaryEntities;

            private TextDocument documentEntity;

            [TestInitialize]
            public void Initialize()
            {
                _logger = new Mock<ILogger>().Object;
                _documentRepositoryMock = new Mock<IDocumentRepository>();
                _documentRepository = _documentRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _roleRepository = _roleRepositoryMock.Object;
                _currentUserFactory = new CurrentUserSetup.DocumentUserFactory();

                _documentService = new DocumentService(_adapterRegistry, _currentUserFactory, _roleRepository, _documentRepository, _logger);

                documentId = "ID";
                primaryEntity = "ENTITY";
                primaryId = "PRIMARY_ID";
                personId = _currentUserFactory.CurrentUser.PersonId;
                secondaryEntities = null;

                documentEntity = new TextDocument(new List<string>() { "This is line 1.", "This is line 2." });
            }

            [TestCleanup]
            public void Cleanup()
            {
                _logger = null;
                _documentRepositoryMock = null;
                _documentRepository = null;
                _adapterRegistryMock = null;
                _adapterRegistry = null;
                _roleRepositoryMock = null;
                _roleRepository = null;

                _documentService = null;

                documentId = null;
                primaryEntity = null;
                primaryId = null;
                personId = null;
                secondaryEntities = null;

                documentEntity = null;
            }

            #region GetTextDocumentAsync

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentService_GetTextDocumentAsync_Null_DocumentId()
            {
                await _documentService.GetTextDocumentAsync(null, primaryEntity, primaryId, personId, secondaryEntities);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentService_GetTextDocumentAsync_Null_PrimaryEntity()
            {
                await _documentService.GetTextDocumentAsync(documentId, null, primaryId, personId, secondaryEntities);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentService_GetTextDocumentAsync_Null_PrimaryEntityId()
            {
                await _documentService.GetTextDocumentAsync(documentId, primaryEntity, null, personId, secondaryEntities);
            }

            [TestMethod]
            public async Task DocumentService_GetTextDocumentAsync_Valid()
            {
                _documentRepositoryMock.Setup(repo => repo.BuildAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    null)).ReturnsAsync(documentEntity);
                var textDocumentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.TextDocument, Dtos.Base.TextDocument>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.TextDocument, Dtos.Base.TextDocument>()).Returns(textDocumentAdapter);
                _documentService = new DocumentService(_adapterRegistry, _currentUserFactory, _roleRepository, _documentRepository, _logger);
                var doc = await _documentService.GetTextDocumentAsync(documentId, primaryEntity, primaryId, personId, secondaryEntities);
                Assert.IsNotNull(doc);
                Assert.AreEqual(documentEntity.Text.Count, doc.Text.Count);
            }

            #endregion GetTextDocumentAsync
        }
    }
}