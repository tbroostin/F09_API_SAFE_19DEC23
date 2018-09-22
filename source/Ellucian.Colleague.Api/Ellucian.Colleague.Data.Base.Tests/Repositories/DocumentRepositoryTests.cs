// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class DocumentRepositoryTests : BaseRepositorySetup
    {
        DocumentRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();

            repository = new DocumentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class Build : DocumentRepositoryTests
        {
            string documentId = "TEST";
            string primaryEntity = "ENTITY.NAME";
            string primaryId = "12345";
            string personId = "1234567";
            List<string> responseText = new List<string>() { "This is a line of text.", "This is another line of text." };

            [TestInitialize]
            public void Build_Initialize()
            {
                base.Initialize();

                transManagerMock.Setup<BuildDocumentResponse>(trans => trans.Execute<BuildDocumentRequest, BuildDocumentResponse>(It.IsAny<BuildDocumentRequest>()))
                    .Returns(new BuildDocumentResponse() { DocumentText = new List<string>(responseText) });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DocumentRepository_Build_NullDocumentId()
            {
                var result = repository.Build(null, primaryEntity, primaryId, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DocumentRepository_Build_NullPrimaryEntity()
            {
                var result = repository.Build(documentId, null, primaryId, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DocumentRepository_Build_NullPrimaryId()
            {
                var result = repository.Build(documentId, primaryEntity, null, personId);
            }

            [TestMethod]
            public void DocumentRepository_Build_Valid()
            {
                var result = repository.Build(documentId, primaryEntity, primaryId, personId);

                CollectionAssert.AreEqual(responseText, result.Text);
                Assert.IsNull(result.Subject);
            }
        }

        [TestClass]
        public class BuildAsync : DocumentRepositoryTests
        {
            string documentId = "TEST";
            string primaryEntity = "ENTITY.NAME";
            string primaryId = "12345";
            string personId = "1234567";
            List<string> responseText = new List<string>() { "This is a line of text.", "This is another line of text." };

            [TestInitialize]
            public void BuildAsync_Initialize()
            {
                base.Initialize();
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<BuildDocumentRequest, BuildDocumentResponse>(It.IsAny<BuildDocumentRequest>())).ReturnsAsync(new BuildDocumentResponse() { DocumentText = new List<string>(responseText) });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentRepository_BuildAsync_NullDocumentId()
            {
                var result = await repository.BuildAsync(null, primaryEntity, primaryId, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentRepository_BuildAsync_NullPrimaryEntity()
            {
                var result = await repository.BuildAsync(documentId, null, primaryId, personId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task DocumentRepository_BuildAsync_NullPrimaryId()
            {
                var result = await repository.BuildAsync(documentId, primaryEntity, null, personId);
            }

            [TestMethod]
            public async Task DocumentRepository_BuildAsync_Valid()
            {
                var result = await repository.BuildAsync(documentId, primaryEntity, primaryId, personId);
                CollectionAssert.AreEqual(responseText, result.Text);
                Assert.IsNull(result.Subject);
            }
        }

    }
}
