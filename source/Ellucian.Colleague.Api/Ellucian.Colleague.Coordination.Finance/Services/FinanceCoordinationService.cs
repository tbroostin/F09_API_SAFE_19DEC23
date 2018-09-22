// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    /// <summary>
    /// Base class for all coordination services in Student Finance
    /// </summary>
    public abstract class FinanceCoordinationService : BaseCoordinationService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="currentUserFactory">Interface to CurrentUserFactory</param>
        /// <param name="roleRepository">Interface to RoleRepository</param>
        /// <param name="logger">Interface to Logger</param>
        protected FinanceCoordinationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
        }

        /// <summary>
        /// A helper method to determine if the logged in user is the student whose data is being accessed.
        /// </summary>
        /// <param name="studentId">ID of student from data</param>
        /// <returns>Indicates whether the user is the student</returns>
        protected bool UserIsSelf(string studentId)
        {
            return CurrentUser.IsPerson(studentId);
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view a student's account information.
        /// This information is only permitted for the student himself or someone with the permission
        /// code to view student's account activity.
        /// </summary>
        /// <param name="studentId">ID of student from the data</param>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        protected void CheckAccountPermission(string studentId)
        {
            bool hasAdminPermission = HasPermission(FinancePermissionCodes.ViewStudentAccountActivity);
            
            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();
            
            // They're allowed to see another's data if they are a proxy for that user or have the admin permission
            if (!UserIsSelf(studentId) && !HasProxyAccessForPerson(studentId) && !hasAdminPermission)
            {
                logger.Info(CurrentUser + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }
        }

        /// <summary>
        /// Helper method to determine whether the user has permission to pay on a student's account.
        /// Currently, only the student has that permission available.
        /// </summary>
        /// <param name="studentId">ID of student from the data</param>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        protected void CheckPaymentPermission(string studentId, string message = null)
        {
            if (!UserIsSelf(studentId))
            {
                if (String.IsNullOrEmpty(message))
                {
                    message = CurrentUser + " cannot pay on account for student " + studentId;
                }
                logger.Info(message);
                throw new PermissionsException();
            }
        }

        /// <summary>
        /// Helper method to determine whether the user has permission to create a receivable invoice.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        protected void CheckCreateArInvoicePermission()
        {
            if (!HasPermission(FinancePermissionCodes.CreateArInvoices))
            {
                logger.Info(CurrentUser + " does not have permission code " + FinancePermissionCodes.CreateArInvoices);
                throw new PermissionsException();
            }
        }

        /// <summary>
        /// Helper method to determine whether the user has permission to create a receipt.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        protected void CheckCreateReceiptPermission()
        {
            if (!HasPermission(FinancePermissionCodes.CreateReceipts))
            {
                logger.Info(CurrentUser + " does not have permission code " + FinancePermissionCodes.CreateReceipts);
                throw new PermissionsException();
            }
        }

    }
}
