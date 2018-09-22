// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Document services
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Retrieves a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <param name="secondaryEntities">Secondary entities and IDs</param>
        /// <returns>A text document</returns>
        Task<TextDocument> GetTextDocumentAsync(string documentId, string primaryEntity, string primaryId, string personId, IDictionary<string, string> secondaryEntities = null);
    }
}
