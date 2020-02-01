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
    /// Password used exception
    /// </summary>
    public class PasswordUsedException : ColleagueApiException
    {
        /// <summary>
        /// Creates a new <see cref="PasswordUsedException"/>
        /// </summary>
        public PasswordUsedException() : base("")
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordUsedException"/> with message
        /// </summary>
        public PasswordUsedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordUsedException"/> with message and inner exception
        /// </summary>
        public PasswordUsedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
