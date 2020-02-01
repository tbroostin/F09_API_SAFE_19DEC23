// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Session Recovery Repository
    /// </summary>
    public interface ISessionRecoveryRepository
    {
        /// <summary>
        /// Requests User ID recovery via email
        /// </summary>
        /// <param name="firstName">First name of the user</param>
        /// <param name="lastName">Last name of the user</param>
        /// <param name="emailAddress">Email of the user</param>
        /// <returns></returns>
        Task RequestUserIdRecoveryAsync(string firstName, string lastName, string emailAddress);

        /// <summary>
        /// Requests a password reset token via email
        /// </summary>
        /// <param name="userId">User ID (user name)</param>
        /// <param name="emailAddress">Email address of the user</param>
        /// <returns></returns>
        Task RequestPasswordResetTokenAsync(string userId, string emailAddress);

        /// <summary>
        /// Resets a password using a reset token
        /// </summary>
        /// <param name="userId">User ID (user name)</param>
        /// <param name="resetToken">Password reset token</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        Task ResetPasswordAsync(string userId, string resetToken, string newPassword);
    }
}
