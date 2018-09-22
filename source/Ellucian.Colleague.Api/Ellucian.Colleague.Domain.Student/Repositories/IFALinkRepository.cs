// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IFALinkRepository
    {
        /// <summary>
        /// Send the FALink JSON document to Colleague for processing. Returns the output JSON document.
        /// </summary>
        /// <param name="FALinkDocument">input FA Link document</param>
        /// <returns></returns>
        Task<string> PostFALinkDocumentAsync(string FALinkDocument);

    }
}
