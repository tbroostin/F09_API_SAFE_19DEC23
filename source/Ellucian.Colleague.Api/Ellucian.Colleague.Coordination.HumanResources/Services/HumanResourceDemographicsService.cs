/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Human Resource Demographics Service
    /// </summary>
    [RegisterType]
    public class HumanResourceDemographicsService : BaseCoordinationService, IHumanResourceDemographicsService
    {
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly IPersonBaseRepository personBaseRepository;
        private readonly IEmployeeRepository employeeRepository;

        /// <summary>
        /// Constructor for Human Resource Demographics Service
        /// </summary>
        /// <param name="personBaseRepository"></param>
        /// <param name="supervisorsRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public HumanResourceDemographicsService(
             IPersonBaseRepository personBaseRepository,
            ISupervisorsRepository supervisorsRepository,
            IEmployeeRepository employeeRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger
            )
            : base(
              adapterRegistry, currentUserFactory, roleRepository, logger
                 )
        {
            this.supervisorsRepository = supervisorsRepository;
            this.personBaseRepository = personBaseRepository;
            this.employeeRepository = employeeRepository;
        }
        /// <summary>
        /// Gets all HumanResourceDemographics available to the user
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <returns></returns>
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographicsAsync(string effectivePersonId = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            else if (!CurrentUser.IsPerson(effectivePersonId) && !HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval))
            {
                throw new PermissionsException("User does not have permission to view human resource demographic information");
            }

            var cumulativeHrPersonIds = new List<string>() { effectivePersonId, CurrentUser.PersonId };

            var supervisorIds = (await supervisorsRepository.GetSupervisorsBySuperviseeAsync(effectivePersonId)).ToList();

            if (supervisorIds.Any())
            {
                cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(supervisorIds).Distinct().ToList();
            }

            if (HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
            {
                var subordinateIds = (await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId)).ToList();

                if (subordinateIds == null)
                {
                    var message = "Unexpected null person id list (no subordinate ids) returned from supervisors repository";
                    logger.Info(message);
                }
                if (subordinateIds.Any())
                {
                    foreach (var subordinateId in subordinateIds)
                    {
                        supervisorIds = (await supervisorsRepository.GetSupervisorsBySuperviseeAsync(subordinateId)).ToList();
                        if (supervisorIds.Any())
                        {
                            cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(supervisorIds).ToList();
                        }
                    }
                    cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(subordinateIds).ToList();
                }
            }

            if (HasPermission(HumanResourcesPermissionCodes.ViewAllEarningsStatements))
            {
                var hrPersonIds = await employeeRepository.GetEmployeeKeysAsync();
                cumulativeHrPersonIds.AddRange(hrPersonIds.ToList());
            }


            // Defaults to true to filter out bad records in GetPersonsBaseAsync
            var hasLastName = true;

            var personBaseEntities = await personBaseRepository.GetPersonsBaseAsync(cumulativeHrPersonIds.Distinct().ToList(), hasLastName);

            if (personBaseEntities == null)
            {
                var message = "Null Person Base Entities returned to coordination service";
                logger.Info(message);
            }

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBaseEntities.Select(pb => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pb)).ToList();
        }

        /// <summary>
        /// Gets all HumanResourceDemographics available to the user
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <returns></returns>
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographics2Async(string effectivePersonId = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or admin
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory)))
            {
                throw new PermissionsException("User does not have permission to view human resource demographic information");
            }

            var cumulativeHrPersonIds = new List<string>() { effectivePersonId, CurrentUser.PersonId };

            var supervisorIds = (await supervisorsRepository.GetSupervisorsBySuperviseeAsync(effectivePersonId)).ToList();

            if (supervisorIds.Any())
            {
                cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(supervisorIds).Distinct().ToList();
            }

            if (HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
            {
                var subordinateIds = (await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId)).ToList();

                if (subordinateIds == null)
                {
                    var message = "Unexpected null person id list (no subordinate ids) returned from supervisors repository";
                    logger.Info(message);
                }
                if (subordinateIds.Any())
                {
                    foreach (var subordinateId in subordinateIds)
                    {
                        supervisorIds = (await supervisorsRepository.GetSupervisorsBySuperviseeAsync(subordinateId)).ToList();
                        if (supervisorIds.Any())
                        {
                            cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(supervisorIds).ToList();
                        }
                    }
                    cumulativeHrPersonIds = cumulativeHrPersonIds.Concat(subordinateIds).ToList();
                }
            }

            // Defaults to true to filter out bad records in GetPersonsBaseAsync
            var hasLastName = true;

            var personBaseEntities = await personBaseRepository.GetPersonsBaseAsync(cumulativeHrPersonIds.Distinct().ToList(), hasLastName);

            if (personBaseEntities == null)
            {
                var message = "Null Person Base Entities returned to coordination service";
                logger.Info(message);
            }

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBaseEntities.Select(pb => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pb)).ToList();
        }

        /// <summary>
        /// Gets HumanResourceDemographics for a specific person
        /// </summary>
        /// <returns></returns>
        public async Task<HumanResourceDemographics> GetSpecificHumanResourceDemographicsAsync(string id)
        {

            //var userAndSubordinateIds = new List<string>() { CurrentUser.PersonId };
            // Domain.Base.Entities.PersonBase personBaseEntity = null;

            //make sure the supplied Id is not null or empty
            if (string.IsNullOrWhiteSpace(id))
            {
                var message = "Supplied Id is null or empty";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            if (!CurrentUser.IsPerson(id))
            {
                if (!HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
                {
                    throw new PermissionsException(string.Format("User {0} does not have permission to access demographics for {1}", CurrentUser.PersonId, id));
                }

                var superviseeIds = await supervisorsRepository.GetSuperviseesBySupervisorAsync(CurrentUser.PersonId);
                if (superviseeIds == null || !superviseeIds.Any() || !superviseeIds.Contains(id))
                {
                    throw new PermissionsException(string.Format("Supervisor {0} does not supervise {1} and is not permitted to view their data", CurrentUser.PersonId, id));
                }
            }

            //user is authorized to get hr demographics for the given id
            var personBaseEntity = await personBaseRepository.GetPersonBaseAsync(id, true);
            if (personBaseEntity == null)
            {
                throw new KeyNotFoundException(string.Format("Unable to find Person {0}", id));
            }

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(personBaseEntity);
        }

        /// <summary>
        /// Retrieves a collection of HumanResourcesDemographics from a collection of Ids
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="effectivePersonId"></param>
        /// <returns>A collection of HumanResourceDemographics DTOs</returns>
        public async Task<IEnumerable<HumanResourceDemographics>> QueryHumanResourceDemographicsAsync(Dtos.Base.HumanResourceDemographicsQueryCriteria criteria, string effectivePersonId = null)
        {
            // Ensure the Criteria is not null
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria must be provided to retrieve HumanResourceDemographics.");
            }

            // Check if the Ids are null or the Id list is empty
            if (criteria.Ids == null || !criteria.Ids.Any())
            {
                var message = "Criteria IDs are null or empty";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }

            // Assign value to effectivePersonId if none is given
            if (string.IsNullOrEmpty(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            // Current user must be the effectivePerson or be a proxy to them
            else if (!CurrentUser.IsPerson(effectivePersonId) && !HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view human resource demographic information for or on behalf of {1}", CurrentUser.PersonId, effectivePersonId));
            }

            // Distinct query ids to validate the effective user is the supervisor of
            var distinctQueryIds = criteria.Ids.Distinct().ToList();

            // If the list contains any ids that are not the effectivePersonId
            if (distinctQueryIds.Any(d => d != effectivePersonId))
            {
                // Minimum permission needed to view supervisee data
                if (!HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
                {
                    throw new PermissionsException(string.Format("User {0} does not have permission to access demographics", effectivePersonId));
                }
                var superviseeIds = await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId);

                // Checks if any of the criteria Ids aren't in the supervisee Ids list
                var invalidIds = distinctQueryIds.Except(superviseeIds).ToList();

                // Removes the effective user from the list because they can view their own demographics
                invalidIds.Remove(effectivePersonId);

                if (invalidIds.Any())
                {
                    string idList = string.Join(", ", invalidIds.ToArray());
                    throw new PermissionsException(string.Format("User {0} does not supervise the following Ids and is not permitted to view their data: {1}", effectivePersonId, idList));
                }
            }

            // Because all Ids are either the person's own ID or their supervisees, retrieve the personBase entities
            var personBases = await personBaseRepository.GetPersonsBaseAsync(distinctQueryIds, true);

            //Puts the person base IDs into a list to compare against the queried list
            var personBaseIds = personBases.Select(pb => pb.Id).ToList();
            var missingPersons = distinctQueryIds.Except(personBaseIds);

            // If there are any missing Persons records, throw an error
            if (missingPersons.Any())
            {
                string ids = string.Join(", ", missingPersons.ToArray());
                throw new KeyNotFoundException(string.Format("Unable to find the following Person Records: {0}", ids));
            }

            var personBaseEntityToHumanResourceDemographicsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
            return personBases.Select(pb => personBaseEntityToHumanResourceDemographicsDtoAdapter.MapToType(pb)).ToList();
        }

    }
}
