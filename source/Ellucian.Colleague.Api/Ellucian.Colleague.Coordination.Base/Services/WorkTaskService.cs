// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Based on current user, processes work tasks for current user.
    /// </summary>
    [RegisterType]
    public class WorkTaskService : BaseCoordinationService, IWorkTaskService
    {
        private IWorkTaskRepository _workTaskRepository;
        private IRoleRepository _roleRepository;
        private IProxyRepository _proxyRepository;

        /// <summary>
        /// Constructor of the Work Task Service
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="workTaskRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="proxyRepository"></param>
        /// <param name="logger"></param>
        public WorkTaskService(IAdapterRegistry adapterRegistry, IWorkTaskRepository workTaskRepository,
           ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IProxyRepository proxyRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _workTaskRepository = workTaskRepository;
            _roleRepository = roleRepository;
            _proxyRepository = proxyRepository;
        }

        /// <summary>
        /// Returns list of work tasks assigned to the current user and the current user's roles. 
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>List of WorkTask items</returns>
        public async Task<List<Ellucian.Colleague.Dtos.Base.WorkTask>> GetAsync(string personId)
        {
            var workTaskDtos = new List<Ellucian.Colleague.Dtos.Base.WorkTask>();
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException(personId);
            }
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                throw new PermissionsException(String.Format("Authenticated user (person ID {0}) does not match passed person ID {1} and has not been granted proxy access.", CurrentUser.PersonId, personId));
            }

            // Get the Role IDs for the user's roles
            var roleIds = new List<string>();
            var allRoles = await _roleRepository.GetRolesAsync();
            if (allRoles != null && allRoles.Count() > 0 && CurrentUser.Roles.Count() > 0)
            {
                roleIds = (from roleTitle in CurrentUser.Roles
                           join roleEntity in allRoles
                           on roleTitle equals roleEntity.Title into joinRoles
                           from role in joinRoles
                           select role.Id.ToString()).ToList();
            }

            // Only want to get the proxy info if user has a proxy
            IEnumerable<string> worklistCatSpecProcCodes = null;
            if (HasProxyAccessForPerson(personId))
            {
                // Get the workflows the proxy has access to based off of the proxy's permissions 
                var proxyConfig = await _proxyRepository.GetProxyConfigurationAsync();
                var workflowsHaveAccessTo = proxyConfig.WorkflowGroups.SelectMany(wfg => wfg.Workflows).Where(wf => CurrentUser.ProxySubjects.FirstOrDefault().Permissions.Contains(wf.Code));

                // Get the worklist category special process codes of those workflows the proxy has access to
                worklistCatSpecProcCodes = workflowsHaveAccessTo.Select(wfa => wfa.WorklistCategorySpecialProcessCode);
            }

            // Get the workTasks pertinent to the specified users and roles
            var workTasks = await _workTaskRepository.GetAsync(personId, roleIds);
            if (workTasks != null)
            {
                var workTaskAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>();
                foreach (var item in workTasks)
                {

                    //If user is proxy user who has TMTA permission && task valcode is that for Time Approval, allow access (add task to list)
                    if (HasProxyAccessForPerson(personId))
                    {
                        if (worklistCatSpecProcCodes != null && worklistCatSpecProcCodes.Contains(item.ProcessCode))
                        {
                            workTaskDtos.Add(workTaskAdapter.MapToType(item));
                        }
                    }
                    //Else current user, add all tasks to list
                    else
                    {
                        workTaskDtos.Add(workTaskAdapter.MapToType(item));
                    }
                }
            }
            return workTaskDtos;
        }
    }
}
