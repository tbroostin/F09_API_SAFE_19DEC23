//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Extend this class to share permission checking logic.
    /// </summary>
    public abstract class FinancialAidCoordinationService : BaseCoordinationService
    {
        protected FinancialAidCoordinationService(IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
        }

        /// <summary>
        /// Confirms that the user is the student being accessed
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        protected bool UserIsSelf(string studentId)
        {
            // Access is Ok if the current user is this student
            if (CurrentUser.IsPerson(studentId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// If the current user does not have permissions to access the given student, returns false;
        /// </summary>
        /// <param name="studentId">student id for the student to check data access permissions to</param>
        /// <param name="proxyPermissions">(Optional) Proxy permissions list with which to check the current 
        /// user has been granted at least one, if present.</param>
        protected bool UserHasAccessPermission(string studentId, params Domain.Base.Entities.ProxyWorkflowConstants[] proxyPermissions)
        {
            if (UserIsSelf(studentId) || HasPermission(StudentPermissionCodes.ViewFinancialAidInformation) || HasProxyAccessForPerson(studentId, proxyPermissions))
            {
                return true;
            }
            return false;
        }
    }
}
