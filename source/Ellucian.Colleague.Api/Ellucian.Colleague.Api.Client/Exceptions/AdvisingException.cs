using System;

namespace Ellucian.Colleague.Api.Client.Exceptions
{
    /// <summary>
    /// Encapsulates an exception which may occur within the context of advising.
    /// </summary>
    public class AdvisingException : Exception
    {
        public AdvisingException(AdvisingExceptionCodes code, Exception ex)
            : base(code.ToString(), ex)
        {

        }

        public AdvisingException(AdvisingExceptionCodes code)
            : base()
        {

        }

        /// <summary>
        /// Gets or sets the error code associated with this AdvisingException. Further details can be
        /// relayed in the Message property.
        /// </summary>
        public AdvisingExceptionCodes Code { get; set; }
    }
}