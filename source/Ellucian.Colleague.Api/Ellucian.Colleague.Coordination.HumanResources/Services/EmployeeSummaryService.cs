/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmployeeSummaryService : BaseCoordinationService, IEmployeeSummaryService
    {
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly IPersonPositionRepository personPositionRepository;
        private readonly IPersonPositionWageRepository personPositionWageRepository;
        private readonly IPersonEmploymentStatusRepository personEmploymentStatusRepository;
        private readonly IPersonBaseRepository personBaseRepository;
        private readonly IPositionRepository positionRepository;

        public EmployeeSummaryService(
            ISupervisorsRepository supervisorsRepository,
            IPersonPositionRepository personPositionRepository,
            IPersonPositionWageRepository personPositionWageRepository,
            IPersonEmploymentStatusRepository personEmploymentStatusRepository,
            IPersonBaseRepository personBaseRepository,
            IPositionRepository positionRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.supervisorsRepository = supervisorsRepository;
            this.personPositionRepository = personPositionRepository;
            this.personPositionWageRepository = personPositionWageRepository;
            this.personEmploymentStatusRepository = personEmploymentStatusRepository;
            this.personBaseRepository = personBaseRepository;
            this.positionRepository = positionRepository;
        }

        /// <summary>
        /// Returns EmployeeSummary objects for requested criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <exception cref="ArgumentNullException">criteria must be provided</exception>
        /// <exception cref="PermissionsException">Authenticated user must be a supervisor or have access to requsted summary objects</exception>
        /// <returns>A collection of EmployeeSummary dtos</returns>
        public async Task<IEnumerable<Dtos.HumanResources.EmployeeSummary>> QueryEmployeeSummaryAsync(Dtos.HumanResources.EmployeeSummaryQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var supervisorId = criteria.EmployeeSupervisorId;
            var employeeIds = criteria.EmployeeIds;
            var validEmployeeIds = new List<string>();
            var employeeSummaryAdapter = _adapterRegistry.GetAdapter<EmployeeSummary, Dtos.HumanResources.EmployeeSummary>();
            bool hasProxyAccess = false;
            //will be assigned current user id if proxy
            string actingSupervisorId = null;

            // permissions and security checking:
            //
            // 1) when the supervisor Id is defined in the criteria, 
            //      a) the authenticated user Id must be the same as the supervisor Id and have supervisor permissions or be a proxy of a user with supervisor permission codes
            //      b) when the employee Ids are also defined in the criteria, the supervisor Id must be the supervisor for the employee Ids requested
            //      c) when the employee Ids are not defined in the criteria, all supervisees are calculated for the supervisor Id and returned by this service
            //
            // 2) when no supervisor Id is defined in the criteria, the employee Id defined in the criteria must be the current authenticated user
            if (!string.IsNullOrEmpty(supervisorId))
            {
                hasProxyAccess = HasProxyAccessForPerson(supervisorId, ProxyWorkflowConstants.TimeManagementTimeApproval);
                // when a supervisor ID is defined in the criteria, it must be the current person Id or have proxy access to a supervisor...
                if (!(CurrentUser.IsPerson(supervisorId) || hasProxyAccess))
                {
                    throw new PermissionsException(string.Format("CurrentUser {0} is not the supervisor {1} nor has proxy access to supervisor", CurrentUser.PersonId, supervisorId));
                }

                // when a supervisor Id is defined in the criteria, user must be a supervisor to view the summary data...
                if (!HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
                {
                    throw new PermissionsException(string.Format("Supervisor {0} does not have permission to access to EmployeeSummary data", supervisorId));
                }

                var supervisedEmployeeIds = await supervisorsRepository.GetSuperviseesBySupervisorAsync(supervisorId);

                //is current user a proxy? - set an actingSupervisorId
                actingSupervisorId = hasProxyAccess ? CurrentUser.PersonId : null;

                // when both the supervisor Id and supervisees are defined in criteria, verify all supervisees are supervised by this supervisor...
                if (employeeIds != null)
                {
                    if (supervisedEmployeeIds == null || !supervisedEmployeeIds.Any())
                    {
                        logger.Error(string.Format("Supervisor {0} does not supervise any employees", supervisorId));
                        throw new PermissionsException(string.Format("Supervisor {0} does not have permission to access to EmployeeSummary data", supervisorId));
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
                    // when the supervisor Id is defined in the criteria without supervisees, return all supervisees supervised by this supervisor...
                    validEmployeeIds.AddRange(supervisedEmployeeIds);
                }
            }
            else
            {
                // when supervisor Id is not defined in the criteria but a list of supervsiees is, the employee must be equal to current person ID...
                if (employeeIds != null)
                {
                    foreach (var employeeId in employeeIds)
                    {
                        if (!CurrentUser.IsPerson(employeeId))
                        {
                            throw new PermissionsException(string.Format("CurrentUser {0} must be employee {1} to view this EmployeeSummary info", CurrentUser.PersonId, employeeId));
                        }
                        else
                        {
                            validEmployeeIds.Add(employeeId);
                        }
                    }
                }
            }

            List<Dtos.HumanResources.EmployeeSummary> employeeSummaryDtos = new List<Dtos.HumanResources.EmployeeSummary>();

            if (validEmployeeIds.Any())
            {
                //Get supervisor ids into a list so we can get their demographics data as well
                var supervisorIds = new List<string>();
                if (!string.IsNullOrEmpty(supervisorId))
                {
                    supervisorIds.Add(supervisorId);
                }
                if (!string.IsNullOrEmpty(actingSupervisorId))
                {
                    supervisorIds.Add(actingSupervisorId);
                }

                var distinctValidEmployeeIds = validEmployeeIds.Distinct();
                var personPositions = await personPositionRepository.GetPersonPositionsAsync(distinctValidEmployeeIds, criteria.LookupStartDate);
                var personPositionWages = await personPositionWageRepository.GetPersonPositionWagesAsync(distinctValidEmployeeIds, criteria.LookupStartDate);
                var personEmploymentStatuses = await personEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(distinctValidEmployeeIds, criteria.LookupStartDate);
                var personDemographics = await personBaseRepository.GetPersonsBaseAsync(distinctValidEmployeeIds.Concat(supervisorIds).Distinct(), true);
                var allPositions = (await positionRepository.GetPositionsAsync()).ToDictionary(p => p.Id);


                foreach (var validEmployee in distinctValidEmployeeIds)
                {
                    var personPositionsForEmployee = personPositions.Where(pp => pp.PersonId == validEmployee);

                    //Get all direct supervisors for the current valid employee id
                    var allSupervisorIds = personPositionsForEmployee.Select(pp => pp.SupervisorId).Where(sid => !string.IsNullOrEmpty(sid));

                    //Now get all position-level supervisors
                    var personPositionsWithoutSupervisor = personPositionsForEmployee
                        .Where(p => string.IsNullOrWhiteSpace(p.SupervisorId) &&
                                    !string.IsNullOrWhiteSpace(p.PositionId) &&
                                    allPositions.ContainsKey(p.PositionId));

                    //Get all supervisor position ids for positions without direct supervisor
                    var supervisorPositionIds = personPositionsWithoutSupervisor
                        .Select(pp => allPositions[pp.PositionId].SupervisorPositionId)
                        .Where(pid => !string.IsNullOrWhiteSpace(pid));


                    if (supervisorPositionIds.Any())
                    {
                        var supervisors = await supervisorsRepository.GetSupervisorIdsForPositionsAsync(supervisorPositionIds);

                        foreach (var personPositionEntity in personPositionsWithoutSupervisor)
                        {
                            var supervisorPositionId = allPositions[personPositionEntity.PositionId].SupervisorPositionId;
                            if (supervisors.ContainsKey(supervisorPositionId))
                            {
                                personPositionEntity.PositionLevelSupervisorIds = supervisors[supervisorPositionId];
                                //Add position-level supervisor ids to the distinct supervisors list
                                allSupervisorIds = personPositionEntity.PositionLevelSupervisorIds.Any() ?
                                    allSupervisorIds.Concat(new List<string>() { personPositionEntity.PositionLevelSupervisorIds[0] })
                                    : allSupervisorIds;
                            }
                        }
                    }

                    IEnumerable<string> distinctSupervisorIds = new List<string>().AsEnumerable();
                    if (supervisorIds.Any())
                    {
                        distinctSupervisorIds = allSupervisorIds.Distinct().Except(supervisorIds).Where(id => !string.IsNullOrEmpty(id));
                    }
                    IEnumerable<PersonBase> supervisorsDemographics = distinctSupervisorIds.Any() ? await personBaseRepository.GetPersonsBaseAsync(distinctSupervisorIds, true)
                        : new List<PersonBase>();

                    var personPositionWagesForEmployee = personPositionWages.Where(ppw => ppw.PersonId == validEmployee);
                    var personEmploymentStatusesForEmployee = personEmploymentStatuses.Where(pes => pes.PersonId == validEmployee);
                    var personDemographicsForEmployee = personDemographics.FirstOrDefault(pd => pd.Id == validEmployee);

                    if (personDemographicsForEmployee == null)
                    {
                        logger.Error(string.Format("Demographics information missing for employee {0}. Unable to build EmployeeSummary object.", validEmployee));
                    }
                    else if (!personPositionsForEmployee.Any())
                    {
                        logger.Error(string.Format("PersonPosition information missing for employee {0}. Unable to build EmployeeSummary object.", validEmployee));
                    }
                    else if (!personPositionWagesForEmployee.Any())
                    {
                        logger.Error(string.Format("PersonPositionWage information missing for employee {0}. Unable to build EmployeeSummary object.", validEmployee));
                    }
                    else if (!personEmploymentStatusesForEmployee.Any())
                    {
                        logger.Error(string.Format("PersonStatus information missing for employee {0}. Unable to build EmployeeSummary object.", validEmployee));
                    }
                    else
                    {
                        var employeeSummaryEntity = new EmployeeSummary(validEmployee, personPositionsForEmployee,
                            personPositionWagesForEmployee, personEmploymentStatusesForEmployee, personDemographicsForEmployee, supervisorsDemographics
                            .Concat(personDemographics.Where(pd => supervisorIds.Contains(pd.Id))));

                        employeeSummaryDtos.Add(employeeSummaryAdapter.MapToType(employeeSummaryEntity));
                    }

                }
            }

            return employeeSummaryDtos;
        }
    }
}