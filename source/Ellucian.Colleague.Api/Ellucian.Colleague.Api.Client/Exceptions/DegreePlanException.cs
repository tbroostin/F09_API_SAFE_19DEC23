using System;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    /// <summary>
    /// Represents an Exception that occurred while performing an action on a DegreePlan.
    /// </summary>
    public class DegreePlanException : Exception
    {
        /// <summary>
        /// Gets or sets the error code associated with this DegreePlanException. Further details can be
        /// relayed in the Message property.
        /// </summary>
        public DegreePlanExceptionCodes Code { get; set; }
    }
}