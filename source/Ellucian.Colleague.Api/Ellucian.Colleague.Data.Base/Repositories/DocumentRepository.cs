// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for documents
    /// </summary>
    [RegisterType]
    public class DocumentRepository : BaseColleagueRepository, IDocumentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public DocumentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Build a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>
        /// A text document
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// documentId;Document ID must be specified.
        /// or
        /// primaryEntity;The primary entity must be specified.
        /// or
        /// primaryId;The primary ID must be specified.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The specified document could not be built using the supplied data.</exception>
        public TextDocument Build(string documentId, string primaryEntity, string primaryId, string personId, IDictionary<string, string> secondaryEntities = null)
        {
            if (String.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID must be specified.");
            }
            if (String.IsNullOrEmpty(primaryEntity))
            {
                throw new ArgumentNullException("primaryEntity", "The primary entity must be specified.");
            }
            if (String.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", "The primary ID must be specified.");
            }

            Dictionary<string, string> entities = new Dictionary<string, string>();
            entities.Add(primaryEntity, primaryId);
            if (secondaryEntities != null)
            {
                foreach (var entity in secondaryEntities)
                {
                    // Eliminate any duplicate entries and any where the secondary ID was not provided
                    if (!entities.ContainsKey(entity.Key) && !String.IsNullOrEmpty(entity.Value))
                    {
                        entities.Add(entity.Key, entity.Value);
                    }
                }
            }

            var request = new BuildDocumentRequest()
                {
                    DocumentId = documentId,
                    PersonId = personId,
                    EntityNames = entities.Keys.ToList(),
                    EntityKeys = entities.Values.ToList()
                };
            BuildDocumentResponse response = transactionInvoker.Execute<BuildDocumentRequest, BuildDocumentResponse>(request);

            if (response == null || response.DocumentText == null || response.DocumentText.Count == 0)
            {
                throw new InvalidOperationException("The specified document could not be built using the supplied data.");
            }

            var document = new TextDocument(response.DocumentText);
            document.Subject = response.SubjectLineText;

            return document;
        }

        /// <summary>
        /// Build a text document asynchronously
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>
        /// A text document
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// documentId;Document ID must be specified.
        /// or
        /// primaryEntity;The primary entity must be specified.
        /// or
        /// primaryId;The primary ID must be specified.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The specified document could not be built using the supplied data.</exception>
        public async Task<TextDocument> BuildAsync(string documentId, string primaryEntity, string primaryId, string personId, IDictionary<string, string> secondaryEntities = null)
        {
            if (String.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID must be specified.");
            }
            if (String.IsNullOrEmpty(primaryEntity))
            {
                throw new ArgumentNullException("primaryEntity", "The primary entity must be specified.");
            }
            if (String.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", "The primary ID must be specified.");
            }

            Dictionary<string, string> entities = new Dictionary<string, string>();
            entities.Add(primaryEntity, primaryId);
            if (secondaryEntities != null)
            {
                foreach (var entity in secondaryEntities)
                {
                    // Eliminate any duplicate entries and any where the secondary ID was not provided
                    if (!entities.ContainsKey(entity.Key) && !String.IsNullOrEmpty(entity.Value))
                    {
                        entities.Add(entity.Key, entity.Value);
                    }
                }
            }

            var request = new BuildDocumentRequest()
            {
                DocumentId = documentId,
                PersonId = personId,
                EntityNames = entities.Keys.ToList(),
                EntityKeys = entities.Values.ToList()
            };
            BuildDocumentResponse response = await transactionInvoker.ExecuteAsync<BuildDocumentRequest, BuildDocumentResponse>(request);

            if (response == null || response.DocumentText == null || response.DocumentText.Count == 0)
            {
                throw new InvalidOperationException("The specified document could not be built using the supplied data.");
            }

            var document = new TextDocument(response.DocumentText);
            document.Subject = response.SubjectLineText;

            return document;
        }
    
    }
}
