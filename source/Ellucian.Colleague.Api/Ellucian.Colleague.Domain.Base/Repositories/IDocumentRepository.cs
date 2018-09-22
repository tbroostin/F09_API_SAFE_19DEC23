// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for document operations
    /// </summary>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Build a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>A text document</returns>
        TextDocument Build(string documentId, string primaryEntity, string primaryId, string personId, IDictionary<string, string> secondaryEntities = null);

        /// <summary>
        /// Build a text document asynchronously
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>A text document</returns>
        Task<TextDocument> BuildAsync(string documentId, string primaryEntity, string primaryId, string personId, IDictionary<string, string> secondaryEntities = null);
    }
}
