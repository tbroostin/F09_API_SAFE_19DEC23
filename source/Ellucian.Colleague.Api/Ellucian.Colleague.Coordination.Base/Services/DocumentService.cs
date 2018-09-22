// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination service for DocumentService
    /// </summary>
    [RegisterType]
    public class DocumentService : BaseCoordinationService, IDocumentService  //BaseCoordinationService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentService"/> class.
        /// </summary>
        /// <param name="documentRepository">Interface to document repository</param>
        /// <param name="logger">Logger</param>
        public DocumentService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, 
            IDocumentRepository documentRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>A text document</returns>
        public async Task<TextDocument> GetTextDocumentAsync(string documentId, 
            string primaryEntity, 
            string primaryId, 
            string personId, 
            IDictionary<string, string> secondaryEntities = null)
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

            var documentEntity = await _documentRepository.BuildAsync(documentId, primaryEntity, primaryId, personId, secondaryEntities);
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TextDocument, TextDocument>();
            return adapter.MapToType(documentEntity);
        }

    }
}