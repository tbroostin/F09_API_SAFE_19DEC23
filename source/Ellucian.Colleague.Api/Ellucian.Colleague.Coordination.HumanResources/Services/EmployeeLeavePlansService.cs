//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos.HumanResources;

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


        //get all leaveplans
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan> _leaveplans = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>> GetLeavePlansAsync(bool bypassCache)
        {
            if (_leaveplans == null)
            {
                _leaveplans = await leavePlansRepository.GetLeavePlansAsync(bypassCache);
            }
            return _leaveplans;
        }

        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.LeavePlan>> GetLeavePlansV2Async(bool bypassCache)
        {
            return await leavePlansRepository.GetLeavePlansV2Async(bypassCache);
        }

        //get all leave type categories from reference repository
        private IEnumerable<LeaveType> leaveTypes = null;
        private async Task<IEnumerable<LeaveType>> GetLeaveCategoriesAsync(bool bypassCache)
        {
            if (leaveTypes == null)
            {
                leaveTypes = await humanResourcesReferenceDataRepository.GetLeaveTypesAsync(bypassCache);
            }
            return leaveTypes;
        }

        //get all earnings types from reference repository
        private IEnumerable<EarningType2> earningTypes = null;
        private async Task<IEnumerable<EarningType2>> GetEarningTypes2Async(bool bypassCache)
        {
            if (earningTypes == null)
            {
                earningTypes = await humanResourcesReferenceDataRepository.GetEarningTypesAsync(bypassCache);
            }
            return earningTypes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employee-leave-plans
        /// </summary>
        /// <returns>Collection of EmployeeLeavePlans DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EmployeeLeavePlans>, int>> GetEmployeeLeavePlansAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewEmployeeLeavePlansPermission();
            var empLeavePlansCollection = new List<Ellucian.Colleague.Dtos.EmployeeLeavePlans>();
            int empLeavePlansCount = 0;
            try
            {
                var empLeavePlansEntities = await employeeLeavePlansRepository.GetEmployeeLeavePlansAsync(offset, limit, bypassCache);
                if (empLeavePlansEntities != null)
                {
                    empLeavePlansCount = empLeavePlansEntities.Item2;
                    foreach (var leavePlans in empLeavePlansEntities.Item1)
                    {
                        var leaveplanDto = await ConvertEmployeeLeavePlansEntityToDto(leavePlans, bypassCache);
                        if (leaveplanDto != null)
                        {
                            empLeavePlansCollection.Add(leaveplanDto);
                        }

                    }
                    return new Tuple<IEnumerable<EmployeeLeavePlans>, int>(empLeavePlansCollection, empLeavePlansCount);

                }
                else
                {
                    return new Tuple<IEnumerable<EmployeeLeavePlans>, int>(new List<Dtos.EmployeeLeavePlans>(), 0);

                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmployeeLeavePlans from its GUID
        /// </summary>
        /// <returns>EmployeeLeavePlans DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmployeeLeavePlans> GetEmployeeLeavePlansByGuidAsync(string guid, bool bypassCache = false)
        {
            CheckViewEmployeeLeavePlansPermission();
            try
            {
                return await ConvertEmployeeLeavePlansEntityToDto(await employeeLeavePlansRepository.GetEmployeeLeavePlansByGuidAsync(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employee-leave-plans not found for GUID " + guid, ex);
            }

        }

        /// <summary>
        /// Gets all EmployeeLeavePlan objects for the effective person id (supports employee, supervisor, supervisor proxy access,and time history admin)
        /// Used by Self Service.
        /// </summary>
        /// <returns>A list of EmployeeLeavePlan Dto objects</returns>
        public async Task<IEnumerable<Dtos.HumanResources.EmployeeLeavePlan>> GetEmployeeLeavePlansV2Async(string effectivePersonId = null, bool bypassCache = false)
        {
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

            //determine the effective person id and whether the current user needs/has proxy authority for that effective person id
            if (string.IsNullOrWhiteSpace(effectivePersonId))
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or admin
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory)))
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view employee leave plan information for person {1}", CurrentUser.PersonId, effectivePersonId));
            }

            var employeeIds = new List<string>() { effectivePersonId };
            if (HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
            {
                try
                {
                    employeeIds.AddRange((await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId)));
                }
                catch (Exception e)
                {
                    logger.Error("Error getting supervisor employees", e.Message);
                }
            }

            var employeeLeavePlanEntities = await employeeLeavePlansRepository.GetEmployeeLeavePlansByEmployeeIdsAsync(employeeIds, leavePlans, leaveTypes, earningTypes);

            var employeeLeavePlanEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>();
            return employeeLeavePlanEntities.Select(lp => employeeLeavePlanEntityToDtoAdapter.MapToType(lp)).ToList();


        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Perleave domain entity to its corresponding EmployeeLeavePlans DTO
        /// </summary>
        /// <param name="source">Perleave domain entity</param>
        /// <returns>EmployeeLeavePlans DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.EmployeeLeavePlans> ConvertEmployeeLeavePlansEntityToDto(Perleave source, bool bypassCache)
        {
            if (source != null)
            {
                try
                {
                    var employeeLeavePlans = new Ellucian.Colleague.Dtos.EmployeeLeavePlans();
                    employeeLeavePlans.Id = source.Guid;
                    //get person
                    if (!string.IsNullOrEmpty(source.PersonId))
                    {
                        var personGuid = await personRepository.GetPersonGuidFromIdAsync(source.PersonId);
                        if (!string.IsNullOrEmpty(personGuid))
                        {
                            employeeLeavePlans.Person = new GuidObject2(personGuid);
                        }
                        else
                        {
                            throw new ArgumentException(string.Concat("Unable to get person guid for person id '", source.PersonId, "'. Entity: ‘LEAVPLAN’, Record ID: '", source.Id, "'"));
                        }
                    }

                    //get leaveplans
                    if (!string.IsNullOrEmpty(source.LeavePlan))
                    {
                        var plans = await GetLeavePlansAsync(bypassCache);
                        if (plans == null || !plans.Any())
                        {
                            throw new ArgumentException("Leave plans are missing.");
                        }
                        var type = plans.FirstOrDefault(c => c.Id == source.LeavePlan);
                        if (type == null)
                        {
                            throw new ArgumentException("Invalid leave plan '" + source.LeavePlan + "' in the arguments");
                        }
                        employeeLeavePlans.Plan = new GuidObject2(type.Guid);
                    }

                    //get start date
                    employeeLeavePlans.StartOn = source.StartDate;
                    employeeLeavePlans.EndOn = source.EndDate;
                    return employeeLeavePlans;
                }
                catch
                {
                    throw new ArgumentException(string.Concat("Invalid Employee Leave Plans. Entity: ‘LEAVPLAN’, Record ID: '", source.Id, "'"));
                }
            }
            else
            {
                throw new ArgumentException(string.Concat("Invalid Employee Leave Plans."));
            }

        }

        /// <summary>
        /// Permissions code that allows an external system to do view employee leave plans
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewEmployeeLeavePlansPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewEmployeeLeavePlans);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view employee leave plans.");
            }
        }
    }
}