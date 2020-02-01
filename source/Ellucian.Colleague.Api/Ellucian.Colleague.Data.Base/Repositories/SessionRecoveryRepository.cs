// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class SessionRecoveryRepository : BaseColleagueRepository, ISessionRecoveryRepository
    {
        private IColleagueTransactionInvoker anonymousTransactionInvoker;
        private const string userIdRecoveryMatchCriteria = "USER.ID.RECOVERY";

        /// <summary>
        /// Session recovery repository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public SessionRecoveryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ColleagueSettings colleagueSettings) : base(cacheProvider, transactionFactory, logger)
        {
            // transactionInvoker will only be non-null when a user is logged in
            // If this is being requested anonymously, create a transaction invoker 
            // without any user context.
            anonymousTransactionInvoker = transactionInvoker ?? new ColleagueTransactionInvoker(null, null, logger, colleagueSettings.DmiSettings);
        }

        /// <summary>
        /// Requests User ID recovery via email
        /// </summary>
        /// <param name="firstName">First name of the user</param>
        /// <param name="lastName">Last name of the user</param>
        /// <param name="emailAddress">Email of the user</param>
        public async Task RequestUserIdRecoveryAsync(string firstName, string lastName, string emailAddress)
        {
            try
            {
                var userIdRecoveryRequest = new RecoverPersonUserIdRequest()
                {
                    MatchCriteriaId = userIdRecoveryMatchCriteria,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailAddress = emailAddress
                };
                var recoveryResponse = await anonymousTransactionInvoker.ExecuteAnonymousAsync<RecoverPersonUserIdRequest, RecoverPersonUserIdResponse>(userIdRecoveryRequest);
                if (recoveryResponse != null && !string.IsNullOrEmpty(recoveryResponse.ErrorOccurred) && recoveryResponse.ErrorOccurred != "0")
                {
                    logger.Error(string.Format("Error occurred during user ID recovery request: ({0}) {1}", recoveryResponse.ErrorOccurred, string.Join(" - ", recoveryResponse.ErrorMessages)));
                    throw new Exception("Error occurred during user ID recovery request.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occurred while executing user ID recovery request.");
                throw;
            }
        }

        /// <summary>
        /// Requests a password reset token via email
        /// </summary>
        /// <param name="userId">User ID (user name)</param>
        /// <param name="emailAddress">Email address of the user</param>
        public async Task RequestPasswordResetTokenAsync(string userId, string emailAddress)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId", "userId is required for requesting a password reset token.");
            }

            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentNullException("emailAddress", "emailAddress is required for requesting a password reset token.");
            }

            var requestPasswordResetTokenRequest = new RequestPasswordResetTokenRequest()
            {
                UserId = userId,
                EmailAddress = emailAddress
            };
            var tokenRequestResponse = await anonymousTransactionInvoker.ExecuteAnonymousAsync<RequestPasswordResetTokenRequest, RequestPasswordResetTokenResponse>(requestPasswordResetTokenRequest);
            if (tokenRequestResponse != null && !string.IsNullOrEmpty(tokenRequestResponse.ErrorOccurred) && tokenRequestResponse.ErrorOccurred != "0")
            {
                logger.Error(string.Format("Error occurred during password reset token request: ({0}) {1}", tokenRequestResponse.ErrorOccurred, string.Join(" - ", tokenRequestResponse.ErrorMessages)));
                throw new Exception("Error occurred during password reset token request.");
            }
        }

        /// <summary>
        /// Resets a password using a reset token
        /// </summary>
        /// <param name="userId">User ID (user name)</param>
        /// <param name="resetToken">Password reset token</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        public async Task ResetPasswordAsync(string userId, string resetToken, string newPassword)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId");
            }
            if (string.IsNullOrEmpty(resetToken))
            {
                throw new ArgumentNullException("resetToken");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentNullException("newPassword");
            }
            var resetRequest = new ResetPasswordWithTokenRequest()
            {
                UserId = userId,
                Token = resetToken,
                NewPassword = newPassword
            };
            var resetResponse = await anonymousTransactionInvoker.ExecuteAnonymousAsync<ResetPasswordWithTokenRequest, ResetPasswordWithTokenResponse>(resetRequest);
            if (resetResponse == null)
            {
                throw new Exception("Could not complete reset password transaction.");
            }

            if (resetResponse.ErrorOccurred)
            {
                if (!string.IsNullOrEmpty(resetResponse.ErrorCode))
                {
                    if (resetResponse.ErrorCode == "P1")
                    {
                        throw new PasswordComplexityException();
                    }
                    else if (resetResponse.ErrorCode == "P2")
                    {
                        throw new PasswordUsedException();
                    }
                    else if (resetResponse.ErrorCode == "T1")
                    {
                        logger.Warn(string.Format("User {0} tried to use expired password reset token.", userId));
                        throw new PasswordResetTokenExpiredException();
                    }
                    else if (resetResponse.ErrorCode == "T2")
                    {
                        logger.Error(string.Format("User {0} tried to use an invalid password reset token.", userId));
                        throw new Exception("Password reset token invalid.");
                    }
                }
                logger.Error("Error occurred in password reset transaction");
                if (resetResponse.ErrorMessages != null)
                {
                    foreach (var message in resetResponse.ErrorMessages)
                    {
                        logger.Error(message);
                    }
                }
                throw new Exception("Failure in reset password transaction.");
            }

        }
    }
}
