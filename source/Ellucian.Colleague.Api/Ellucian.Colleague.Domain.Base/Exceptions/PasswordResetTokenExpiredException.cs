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
    /// Password reset token expired exception
    /// </summary>
    public class PasswordResetTokenExpiredException : ColleagueApiException
    {
        /// <summary>
        /// Creates a new <see cref="PasswordResetTokenExpiredException"/>
        /// </summary>
        public PasswordResetTokenExpiredException() : base("")
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordResetTokenExpiredException"/> with message
        /// </summary>
        public PasswordResetTokenExpiredException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PasswordResetTokenExpiredException"/> with message and inner exception
        /// </summary>
        public PasswordResetTokenExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
