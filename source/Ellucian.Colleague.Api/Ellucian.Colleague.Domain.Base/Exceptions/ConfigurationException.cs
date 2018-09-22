// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    /// <summary>
    /// Exceptions related to configuration
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Configuration exception with default message
        /// </summary>
        public ConfigurationException()
            : base("Configuration exception")
        {
        }

        /// <summary>
        /// Configuration exception with a descriptive message.
        /// </summary>
        /// <param name="message">Description of the cause of this message</param>
        public ConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Configuration exception with a descriptive message and its underlying exception
        /// </summary>
        /// <param name="message">Description of the cause of this message</param>
        /// <param name="e">The underlying exception to wrap in this exception</param>
        public ConfigurationException(string message, Exception e)
            : base(message, e)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
