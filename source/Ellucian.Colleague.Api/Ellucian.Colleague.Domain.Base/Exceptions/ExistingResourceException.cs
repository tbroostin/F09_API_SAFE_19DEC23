using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    /// <summary>
    /// The Exception that can be thrown when a duplicate resource already exists in the dataset.
    /// </summary>
    [Serializable]
    public class ExistingResourceException : Exception
    {
        /// <summary>
        /// The identifier of the resource that already exists. Can be used to inform a client the location
        /// of the existing resource.
        /// </summary>
        public string ExistingResourceId { get { return existingResourceId; } }
        private readonly string existingResourceId;

        /// <summary>
        /// Create an empty ExistingResourceException
        /// </summary>
        public ExistingResourceException()
        {

        }

        /// <summary>
        /// Create an ExistingResourceException with a message and the identifier of the resource
        /// that already exists.
        /// </summary>
        /// <param name="message">Error message explaining the exception</param>
        /// <param name="existingResourceId">Identifier of the resource that already exists.</param>
        public ExistingResourceException(string message, string existingResourceId)
            : base(message)
        {
            this.existingResourceId = existingResourceId;
        }
    }
}
