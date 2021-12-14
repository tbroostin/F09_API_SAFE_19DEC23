// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    /// <summary>
    /// Generic Colleague exceptions
    /// </summary>
    public class ColleagueException : ApplicationException
    {
        /// <summary>
        /// Configuration exception with default message
        /// </summary>
        public ColleagueException()
            : base("Configuration exception")
        {
        }

        /// <summary>
        /// Colleague exception with a descriptive message.
        /// </summary>
        /// <param name="message">Description of the cause of this message</param>
        public ColleagueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Colleague exception with a descriptive message and its underlying exception
        /// </summary>
        /// <param name="message">Description of the cause of this message</param>
        /// <param name="e">The underlying exception to wrap in this exception</param>
        public ColleagueException(string message, Exception e)
            : base(message, e)
        {
        }

        protected ColleagueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
