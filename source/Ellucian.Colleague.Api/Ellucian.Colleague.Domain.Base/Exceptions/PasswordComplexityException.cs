// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Data.Colleague.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    /// <summary>
    /// Password complexity exception
    /// </summary>
    public class PasswordComplexityException : ColleagueApiException
    {
        /// <summary>
        /// Creates a new <see cref="PasswordComplexityException"/>
        /// </summary>
        public PasswordComplexityException() : base("")
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordComplexityException"/> with message
        /// </summary>
        public PasswordComplexityException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordComplexityException"/> with message and inner exception
        /// </summary>
        public PasswordComplexityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
