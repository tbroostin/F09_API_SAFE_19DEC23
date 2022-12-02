//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmployeeLeavePlansService : BaseCoordinationService, IEmployeeLeavePlansService
    {
        private readonly IPersonRepository personRepository;
        private readonly IEmployeeLeavePlansRepository employeeLeavePlansRepository;
        private readonly ILeavePlansRepository leavePlansRepository;
        private readonly IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository;
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly ILeaveBalanceConfigurationRepository leaveBalanceConfiguratioRepository;

        public EmployeeLeavePlansService(
            ISupervisorsRepository supervisorsRepository,
            IEmployeeLeavePlansRepository employeeLeavePlansRepository,
            ILeavePlansRepository leavePlansRepository,
            IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            ILeaveBalanceConfigurationRepository leaveBalanceConfiguratioRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.supervisorsRepository = supervisorsRepository;
            this.leavePlansRepository = leavePlansRepository;
            this.employeeLeavePlansRepository = employeeLeavePlansRepository;
            this.humanResourcesReferenceDataRepository = humanResourcesReferenceDataRepository;
            this.personRepository = personRepository;
            this.leaveBalanceConfiguratioRepository = leaveBalanceConfiguratioRepository;
        }

        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>> GetLeavePlansV2Async(bool bypassCache)
        {
            logger.Debug("********* Start - Service to get leave plans - Start *********");
            var leaveplans = await leavePlansRepository.GetLeavePlansV2Async(bypassCache);
            logger.Debug("********* End - Service to get leave plans - End *********");
            return leaveplans;
        }

        //get all leave type categories from reference repository
        private IEnumerable<LeaveType> leaveTypes = null;
        private async Task<IEnumerable<LeaveType>> GetLeaveCategoriesAsync(bool bypassCache)
        {
            logger.Debug("********* Start - Service to get leave categories - Start *********");
            if (leaveTypes == null)
            {
                leaveTypes = await humanResourcesReferenceDataRepository.GetLeaveTypesAsync(bypassCache);
            }
            logger.Debug("********* End - Service to get leave categories - End *********");
            return leaveTypes;
        }

        //get all earnings types from reference repository
        private IEnumerable<EarningType2> earningTypes = null;
        private async Task<IEnumerable<EarningType2>> GetEarningTypes2Async(bool bypassCache)
        {
            logger.Debug("********* Start - Service to get earning types - Start *********");
            if (earningTypes == null)
            {
                earningTypes = await humanResourcesReferenceDataRepository.GetEarningTypesAsync(bypassCache);
            }
            logger.Debug("********* End - Service to get earning types - End *********");
            return earningTypes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employee-leave-plans
        /// </summary>
        /// <returns>Collection of EmployeeLeavePlans DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EmployeeLeavePlans>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            logger.Debug("********* Start - Service to get employee leave plans - Start *********");
            var empLeavePlansCollection = new List<Ellucian.Colleague.Dtos.EmployeeLeavePlans>();

            Tuple<IEnumerable<Perleave>, int> empLeavePlansEntities = null;

            try
            {
                empLeavePlansEntities = await employeeLeavePlansRepository.GetEmployeeLeavePlansAsync(offset, limit, bypassCache);

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Bad.Data");
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }


            if ((empLeavePlansEntities == null) || (empLeavePlansEntities.Item1 == null) || (!empLeavePlansEntities.Item1.Any()))
            {
                return new Tuple<IEnumerable<EmployeeLeavePlans>, int>(new List<Dtos.EmployeeLeavePlans>(), 0);
            }

            var personIds = empLeavePlansEntities.Item1
               .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
               .Select(x => x.PersonId).Distinct().ToList();

            var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(personIds);

            var leavePlanIds = empLeavePlansEntities.Item1
              .Where(x => (!string.IsNullOrEmpty(x.LeavePlan)))
              .Select(x => x.LeavePlan).Distinct().ToList();

            var leavePlanGuidCollection = await leavePlansRepository.GetLeavplanGuidsCollectionAsync(leavePlanIds);

            foreach (var leavePlans in empLeavePlansEntities.Item1)
            {
                try
                {
                    empLeavePlansCollection.Add(ConvertEmployeeLeavePlansEntityToDto(leavePlans, personGuidCollection,
                        leavePlanGuidCollection));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            logger.Debug("********* End - Service to get employee leave plans - End *********");
            return new Tuple<IEnumerable<EmployeeLeavePlans>, int>(empLeavePlansCollection, empLeavePlansEntities.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmployeeLeavePlans from its GUID
        /// </summary>
        /// <returns>EmployeeLeavePlans DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmployeeLeavePlans> GetEmployeeLeavePlansByGuidAsync(string guid, bool bypassCache = false)
        {
            logger.Debug("********* Start - Service to get employee leave plans by guid - Start *********");
            if (string.IsNullOrEmpty(guid))
            {
                logger.Debug("Argument guid cannot be null or empty");
                throw new ArgumentNullException("guid", "A GUID is required to obtain a employee-leave-plans.");
            }

            Perleave perleaveEntity = null;
            try
            {
                logger.Debug(string.Format("Calling GetEmployeeLeavePlansByGuidAsync repository method with guid {0}", guid));
                perleaveEntity = await employeeLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                logger.Debug(string.Format("No employee-leave-plans was found for GUID '{0}'", guid));
                throw new KeyNotFoundException(string.Format("No employee-leave-plans was found for GUID '{0}'", guid));
            }
            Ellucian.Colleague.Dtos.EmployeeLeavePlans retval = null;
            try
            {

                var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(new List<string> { perleaveEntity.PersonId });
                var  leavplanGuidCollection = await leavePlansRepository.GetLeavplanGuidsCollectionAsync(new List<string> { perleaveEntity.LeavePlan });

                retval = ConvertEmployeeLeavePlansEntityToDto(perleaveEntity, personGuidCollection, leavplanGuidCollection);
            }
            catch (KeyNotFoundException)
            {
                logger.Debug(string.Format("No employee-leave-plans was found for GUID '{0}'", guid));
                throw new KeyNotFoundException(string.Format("No employee-leave-plans was found for GUID '{0}'", guid));
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            logger.Debug("********* End - Service to get employee leave plans by guid - End *********");
            return retval;
        }

        /// <summary>
        /// Gets all EmployeeLeavePlan objects for the effective person id (supports employee, supervisor, leave approver, supervisor proxy access, and time history admin)
        /// Used by Self Service.
        /// </summary>
        /// <returns>A list of EmployeeLeavePlan Dto objects</returns>
        public async Task<IEnumerable<Dtos.HumanResources.EmployeeLeavePlan>> GetEmployeeLeavePlansV2Async(string effectivePersonId = null, bool bypassCache = false)
        {
            logger.Debug("********* Start - Service to get employee leave plans - Start *********");
            //determine the effective person id and whether the current user needs/has proxy authority for that effective person id
            if (string.IsNullOrWhiteSpace(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or time history admin or a leave approver
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory) || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view employee leave plan information for person {1}", CurrentUser.PersonId, effectivePersonId));
            }

            var leavePlans = await GetLeavePlansV2Async(bypassCache);
            var leaveTypes = await GetLeaveCategoriesAsync(bypassCache);
            var earningTypes = await GetEarningTypes2Async(bypassCache);

            //returns this list when we have no leave plans or leave types
            var EmptyLeavePlans = new List<Dtos.HumanResources.EmployeeLeavePlan>();

            if (leavePlans == null || !leavePlans.Any())
            {
                logger.Error("No leave plans defined.");
                return EmptyLeavePlans;
            }

            if (leaveTypes == null || !leaveTypes.Any())
            {
                logger.Error("No leave categories defined.");
                return EmptyLeavePlans;
            }

            if (earningTypes == null || !earningTypes.Any())
            {
                throw new ArgumentException("No earning types defined.");
            }

            var employeeIds = new List<string>() { effectivePersonId };
            // Supervisees for time card approver
            if (HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
            {
                try
                {
                    employeeIds.AddRange((await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId)));
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;              
                }
                catch (Exception e)
                {
                    logger.Error("Error getting supervisor employees", e.Message);
                }
            }
            // Supervisees for leave approver
            if (HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest))
            {
                try
                {
                    employeeIds.AddRange((await supervisorsRepository.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId)));
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    logger.Error("Error getting supervisor employees", e.Message);
                }
            }
            // Include the  leave plans without any associated earning types.
            var employeeLeavePlanEntities = await employeeLeavePlansRepository.GetEmployeeLeavePlansByEmployeeIdsAsync(employeeIds.Distinct(), leavePlans, leaveTypes, earningTypes, true);
            var employeeLeavePlanEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>();
            logger.Debug("********* End - Service to get employee leave plans - End *********");
            return employeeLeavePlanEntities.Select(lp => employeeLeavePlanEntityToDtoAdapter.MapToType(lp)).ToList();

        }

        /// <summary>
        /// Gets EmployeeLeavePlan objects for the specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>A collection of EmployeeLeavePlan Dto objects</returns>
        public async Task<IEnumerable<Dtos.HumanResources.EmployeeLeavePlan>> QueryEmployeeLeavePlanAsync(EmployeeLeavePlanQueryCriteria criteria)
        {
            logger.Debug("********* Start - Service to query employee leave plan - Start *********");
            // verify criteria exists and if so, extract parts
            if (criteria == null)
            {
                logger.Debug("Argument criteria cannot be null or empty");
                throw new ArgumentNullException("criteria");
            }

            var supervisorId = criteria.SupervisorId;
            var employeeIds = criteria.SuperviseeIds;
            var lookupStartDate = criteria.LookupStartDate;

            // verify supervisor or employee has permission to view employee leave information
            CheckEmployeeLeavePlansPermission(supervisorId, employeeIds);

            // obtain the list of valid employee ids to get leave information for
            var validEmployeeIds = new List<string>();

            if (!string.IsNullOrEmpty(supervisorId))
            {
                // obtain a list of all supervisees for this supervisor, then if any employee ids were passed in the criteria, verify they are supervised by the provided supervisor id
                validEmployeeIds = await GetValidEmployeeIds(supervisorId, employeeIds, lookupStartDate);
            }
            else
            {
                validEmployeeIds.Add(CurrentUser.PersonId);
            }

            var leavePlans = await GetLeavePlansV2Async(false);
            var leaveTypes = await GetLeaveCategoriesAsync(false);
            var earningTypes = await GetEarningTypes2Async(false);

            //returns this list when we have no leave plans or leave types
            var EmptyLeavePlans = new List<Dtos.HumanResources.EmployeeLeavePlan>();

            if (leavePlans == null || !leavePlans.Any())
            {
                logger.Error("No leave plans defined.");
                return EmptyLeavePlans;
            }

            if (leaveTypes == null || !leaveTypes.Any())
            {
                logger.Error("No leave categories defined.");
                return EmptyLeavePlans;
            }

            if (earningTypes == null || !earningTypes.Any())
            {
                logger.Debug("No earning types defined.");
                throw new ArgumentException("No earning types defined.");
            }

            var employeeLeavePlanEntities = await employeeLeavePlansRepository.GetEmployeeLeavePlansByEmployeeIdsAsync(validEmployeeIds.Distinct(), leavePlans, leaveTypes, earningTypes);
            var employeeLeavePlanEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>();
            logger.Debug("********* End - Service to query employee leave plan - End *********");
            return employeeLeavePlanEntities.Select(lp => employeeLeavePlanEntityToDtoAdapter.MapToType(lp)).ToList();
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Perleave domain entity to its corresponding EmployeeLeavePlans DTO
        /// </summary>
        /// <param name="source">Perleave domain entity</param>
        /// <returns>EmployeeLeavePlans DTO</returns>
        private Ellucian.Colleague.Dtos.EmployeeLeavePlans ConvertEmployeeLeavePlansEntityToDto(Perleave source,
            Dictionary<string, string> personGuidCollection, Dictionary<string, string> leavplanGuidCollection)
        {
            logger.Debug("********* Start - Private Service method to convert employee leave plans entity to dto - Start *********");
            var employeeLeavePlans = new Ellucian.Colleague.Dtos.EmployeeLeavePlans()
            {
                Id = source.Guid,
                StartOn = source.StartDate,
                EndOn = source.EndDate
            };

            if (!string.IsNullOrEmpty(source.PersonId))
            {
                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for personId: '", source.PersonId, "'"),
                        "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var personGuid = string.Empty;
                    personGuidCollection.TryGetValue(source.PersonId, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for personId: '", source.PersonId, "'"),
                        "GUID.Not.Found", source.Guid, source.Id);
                    }
                    employeeLeavePlans.Person = new GuidObject2(personGuid);
                }
            }

            if (!string.IsNullOrEmpty(source.LeavePlan))
            {
                if (leavplanGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for LeavePlan: '", source.LeavePlan, "'"),
                        "GUID.Not.Found", source.Guid, source.Id);
                }
                else
                {
                    var leavePlanGuid = string.Empty;
                    leavplanGuidCollection.TryGetValue(source.LeavePlan, out leavePlanGuid);
                    if (string.IsNullOrEmpty(leavePlanGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to locate GUID for LeavePlan: '", source.LeavePlan, "'"),
                        "GUID.Not.Found", source.Guid, source.Id);
                    }
                    employeeLeavePlans.Plan = new GuidObject2(leavePlanGuid);
                }
            }
            logger.Debug("********* End - Private Service method to convert employee leave plans entity to dto - End *********");
            return employeeLeavePlans;
        }

        /// <summary>
        /// Check permissions to view employee leave plans:
        /// 1) when the supervisor id is defined in the criteria:
        ///     a) the authenticated user id must be the same as the supervisor id or be a proxy of a user with supervisor permission codes
        ///     b) the current user must have supervisor permissions to view superviee data
        /// 2) when no supervisor id is defined in the criteria, the employee id defined in the criteria must be the current authenticated user
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="employeeIds"></param>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckEmployeeLeavePlansPermission(string supervisorId, IEnumerable<string> employeeIds)
        {
            if (!string.IsNullOrEmpty(supervisorId))
            {
                var hasProxyAccess = HasProxyAccessForPerson(supervisorId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval);
                // the authenticated user id must be the same as the supervisor id or be a proxy of a user with supervisor permission codes
                if (!(CurrentUser.IsPerson(supervisorId) || hasProxyAccess))
                {
                    throw new PermissionsException(string.Format("CurrentUser {0} is not a supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, supervisorId));
                }

                // the current user must have supervisor permissions to view superviee data
                if (!HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
                {
                    throw new PermissionsException(string.Format("Id {0} does not have permission to view employee leave plan information", supervisorId));
                }
            }
            else
            {
                // when no supervisor id is defined in the criteria, the employee id defined in the criteria must be the current authenticated user
                if (employeeIds != null)
                {
                    foreach (var employeeId in employeeIds)
                    {
                        if (!CurrentUser.IsPerson(employeeId))
                        {
                            throw new PermissionsException(string.Format("CurrentUser {0} must be employee {1} to view this employee leave plan information", CurrentUser.PersonId, employeeId));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of valid supervisee ids: 
        /// 1) when both the supervisor id and a limit list of employee ids are defined in criteria, verify all supervisees in list are supervised by this supervisor and return that list
        /// 2) when only the supervisor id is defined, return all supervised employee ids
        /// </summary>
        /// <param name="supervisorId"></param>
        /// <param name="employeeIds"></param>
        /// <param name="supervisedEmployeeIds"></param>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<List<string>> GetValidEmployeeIds(string supervisorId, IEnumerable<string> employeeIds, DateTime? lookupStartDate)
        {
            logger.Debug("********* Start - Private Service method to get valid employee ids - Start *********");
            List<string> validEmployeeIds = new List<string>();
            IEnumerable<string> supervisedEmployeeIds = new List<string>();
            try
            {
                supervisedEmployeeIds = await supervisorsRepository.GetSuperviseesBySupervisorAsync(supervisorId, lookupStartDate);
            }
            catch (Exception e)
            {
                logger.Error("Error getting supervisor employees", e.Message);
            }

            // when both the supervisor id and a limit list of employee ids are defined in criteria, verify all supervisees in list are supervised by this supervisor and return that list
            if (employeeIds != null && employeeIds.Any())
            {
                if (supervisedEmployeeIds == null || !supervisedEmployeeIds.Any())
                {
                    logger.Error(string.Format("Supervisor {0} does supervise any employees", supervisorId));
                    throw new PermissionsException(string.Format("Supervisor {0} does not have permission to access to employee leave plan information", supervisorId));
                }

                var sortedSuperviseeIds = supervisedEmployeeIds.OrderBy(s => s).ToList();
                foreach (var employeeId in employeeIds)
                {
                    if (sortedSuperviseeIds.BinarySearch(employeeId) < 0)
                    {
                        continue;
                    }
                    else
                    {
                        validEmployeeIds.Add(employeeId);
                    }
                }
            }
            else
            {
                // when only the supervisor id is defined, return all supervised employee ids
                validEmployeeIds.AddRange(supervisedEmployeeIds);
            }
            logger.Debug("********* End - Private Service method to get valid employee ids - End *********");
            return validEmployeeIds;
        }
    }
}