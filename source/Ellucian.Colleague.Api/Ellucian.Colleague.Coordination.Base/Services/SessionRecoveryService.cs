// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Session recovery service
    /// </summary>
    [RegisterType]
    public class SessionRecoveryService : BaseCoordinationService, ISessionRecoveryService
    {
        private ISessionRecoveryRepository sessionRecoveryRepository;

        /// <summary>
        /// Session recovery service
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="sessionRecoveryRepository"></param>
        public SessionRecoveryService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, ISessionRecoveryRepository sessionRecoveryRepository) : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.sessionRecoveryRepository = sessionRecoveryRepository;
        }

        /// <summary>
        /// Requests a password reset token via email
        /// </summary>
        /// <param name="userId">User ID (user name)</param>
        /// <param name="emailAddress">Email address of the user</param>
        /// <returns></returns>
        public async Task RequestPasswordResetTokenAsync(string userId, string emailAddress)
        {
            if (!HasPermission(BasePermissionCodes.AdminResetAllPasswords))
            {
                logger.Error(string.Format("Attempted password reset by user without required permission {0}: {1}", BasePermissionCodes.AdminResetAllPasswords, CurrentUser.UserId));
                throw new PermissionsException(string.Format("Permission required to reset passwords: {0}", BasePermissionCodes.AdminResetAllPasswords));
            }
            await sessionRecoveryRepository.RequestPasswordResetTokenAsync(userId, emailAddress);
        }

        /// <summary>
        /// Requests User ID (user name) recovery via email
        /// </summary>
        /// <param name="firstName">First name of the user</param>
        /// <param name="lastName">Last name of the user</param>
        /// <param name="emailAddress">Email address of the user</param>
        /// <returns></returns>
        public async Task RequestUserIdRecoveryAsync(string firstName, string lastName, string emailAddress)
        {
            if (!HasPermission(BasePermissionCodes.AdminResetAllPasswords))
            {
                logger.Error(string.Format("Attempted password reset by user without required permission {0}: {1}", BasePermissionCodes.AdminResetAllPasswords, CurrentUser.UserId));
                throw new PermissionsException(string.Format("Permission required to reset passwords: {0}", BasePermissionCodes.AdminResetAllPasswords));
            }
            await sessionRecoveryRepository.RequestUserIdRecoveryAsync(firstName, lastName, emailAddress);
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
            if (!HasPermission(BasePermissionCodes.AdminResetAllPasswords))
            {
                logger.Error(string.Format("Attempted password reset by user without required permission {0}: {1}", BasePermissionCodes.AdminResetAllPasswords, CurrentUser.UserId));
                throw new PermissionsException(string.Format("Permission required to reset passwords: {0}", BasePermissionCodes.AdminResetAllPasswords));
            }
            await sessionRecoveryRepository.ResetPasswordAsync(userId, resetToken, newPassword);
        }
    }
}
