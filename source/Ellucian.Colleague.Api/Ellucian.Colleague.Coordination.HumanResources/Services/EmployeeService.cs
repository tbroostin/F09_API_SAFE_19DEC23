/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmployeeService : BaseCoordinationService, IEmployeeService
    {
        private readonly IPersonRepository personRepository;
        private readonly IPersonBaseRepository personBaseRepository;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IHumanResourcesReferenceDataRepository hrReferenceDataRepository;
        private readonly IPositionRepository positionRepository;
        private readonly IConfigurationRepository configurationRepository;

        public EmployeeService(
            IPersonRepository personRepository,
            IPersonBaseRepository personBaseRepository,
            IEmployeeRepository employeeRepository,
            IReferenceDataRepository referenceDataRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IPositionRepository positionRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.personRepository = personRepository;
            this.personBaseRepository = personBaseRepository;
            this.employeeRepository = employeeRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.hrReferenceDataRepository = hrReferenceDataRepository;
            this.positionRepository = positionRepository;
            this.configurationRepository = configurationRepository;
        }
        //get locations 
        private IEnumerable<Domain.Base.Entities.Location> _location = null;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache)
        {
            if (_location == null)
            {
                _location = await referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _location;
        }
        //get statuses 
        private IEnumerable<HrStatuses> _statuses = null;
        private async Task<IEnumerable<HrStatuses>> GetPersonStatusesAsync(bool bypassCache)
        {
            if (_statuses == null)
            {
                _statuses = await hrReferenceDataRepository.GetHrStatusesAsync(bypassCache);
            }
            return _statuses;
        }
        //get reasons
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason> _reasons = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason>> GetEmploymentStatusEndingReasonsAsync(bool bypassCache)
        {
            if (_reasons == null)
            {
                _reasons = await hrReferenceDataRepository.GetEmploymentStatusEndingReasonsAsync(bypassCache);
            }
            return _reasons;
        }
        //get rehiretypes
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType> _rehireTypes = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType>> GetRehireTypesAsync(bool bypassCache)
        {
            if (_rehireTypes == null)
            {
                _rehireTypes = await hrReferenceDataRepository.GetRehireTypesAsync(bypassCache);
            }
            return _rehireTypes;
        }
        //get payclasses
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayClass> _payClasses = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayClass>> GetPayClassesAsync(bool bypassCache)
        {
            if (_payClasses == null)
            {
                _payClasses = await hrReferenceDataRepository.GetPayClassesAsync(bypassCache);
            }
            return _payClasses;
        }

        //get positions
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Position> _positions = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Position>> GetPositionsAsync(bool bypassCache)
        {
            if (_positions == null)
            {
                _positions = await positionRepository.GetPositionsAsync();
            }
            return _positions;
        }
        //get contract types
        private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.HrStatuses> _contractTypes = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.HrStatuses>> GetContractTypesAsync(bool bypassCache)
        {
            if (_contractTypes == null)
            {
                _contractTypes = await hrReferenceDataRepository.GetHrStatusesAsync(bypassCache);
            }
            return _contractTypes;
        }

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await employeeRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatus">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee"/> and count for paging.</returns>
        public async Task<Tuple<IEnumerable<Dtos.Employee>, int>> GetEmployeesAsync(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "")
        {
            var employeeDtos = new List<Dtos.Employee>();
            int employeeCount = 0;
            var employeePersonIds = new List<string>();

            if (!HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                throw new PermissionsException("User does not have permission to view Employees.");
            }

            var employeeEntities = new Tuple<IEnumerable<Domain.HumanResources.Entities.Employee>, int>(new List<Domain.HumanResources.Entities.Employee>(), 0);
            string newPerson = string.Empty, newCampus = string.Empty, newStartOn = string.Empty, 
                newEndOn = string.Empty, newRehireStatus = string.Empty, newRehireType = string.Empty;

            if (!string.IsNullOrEmpty(person))
            {
                newPerson = await personRepository.GetPersonIdFromGuidAsync(person);
                if (string.IsNullOrEmpty(newPerson))
                    return new Tuple<IEnumerable<Dtos.Employee>, int>(new List<Dtos.Employee>(), 0);
            }
            if (!string.IsNullOrEmpty(campus))
            {
                newCampus = ConvertGuidToCode(await GetLocationsAsync(bypassCache), campus);
                if (string.IsNullOrEmpty(newCampus))
                    return new Tuple<IEnumerable<Dtos.Employee>, int>(new List<Dtos.Employee>(), 0);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if ((!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Leave.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid Status '" + status + "' in the arguments");
                }
            }
            newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            newRehireStatus = rehireableStatusEligibility == string.Empty ? string.Empty : await ConvertRehireEligibilityToCode(rehireableStatusEligibility, bypassCache);
            if (!string.IsNullOrEmpty(rehireableStatusType))
            {
                newRehireType = ConvertGuidToCode(await GetRehireTypesAsync(bypassCache), rehireableStatusType);
                if (string.IsNullOrEmpty(newRehireType))
                     return new Tuple<IEnumerable<Dtos.Employee>, int>(new List<Dtos.Employee>(), 0);
            }

            employeeEntities = await employeeRepository.GetEmployeesAsync(offset, limit, newPerson, newCampus, status, newStartOn, newEndOn, newRehireStatus, newRehireType);
            if (employeeEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.Employee>, int>(new List<Dtos.Employee>(), 0);
            }
            employeeCount = employeeEntities.Item2;

            foreach (var employeeEntity in employeeEntities.Item1)
            {
                var employeeDto = await ConvertEmployeeEntityToDtoAsync(employeeEntity, bypassCache);
                if (employeeDto != null)
                {
                    employeeDtos.Add(employeeDto);
                }
            }

            return new Tuple<IEnumerable<Dtos.Employee>, int>(employeeDtos, employeeCount);
        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatus">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractType">Contract type filter.</param>
        /// <param name="contractDetail">Contract detail filter.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee2"/> and count for paging.</returns>
        public async Task<Tuple<IEnumerable<Dtos.Employee2>, int>> GetEmployees2Async(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
            string contractType = "", string contractDetailId = "")
        {
            var employeeDtos = new List<Dtos.Employee2>();
            int employeeCount = 0;
            var employeePersonIds = new List<string>();

            if (!HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                throw new PermissionsException("User does not have permission to view Employees.");
            }

            var employeeEntities = new Tuple<IEnumerable<Domain.HumanResources.Entities.Employee>, int>(new List<Domain.HumanResources.Entities.Employee>(), 0);
            string newPerson = string.Empty, newCampus = string.Empty, newStartOn = string.Empty, newEndOn = string.Empty, 
                newRehireStatus = string.Empty, newRehireType = string.Empty, newContractDetailTypeCode = string.Empty;
            var newContractTypeCodes = new List<string>();

            if (!string.IsNullOrEmpty(person))
            {
                newPerson = await personRepository.GetPersonIdFromGuidAsync(person);
                if (string.IsNullOrEmpty(newPerson))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(campus))
            {
                newCampus = ConvertGuidToCode(await GetLocationsAsync(bypassCache), campus);
                if (string.IsNullOrEmpty(newCampus))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if ((!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Leave.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter status '" + status + "'");
                }
            }
            newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            newRehireStatus = rehireableStatusEligibility == string.Empty ? string.Empty : await ConvertRehireEligibilityToCode(rehireableStatusEligibility, bypassCache);
            if (!string.IsNullOrEmpty(rehireableStatusType))
            {
                newRehireType = ConvertGuidToCode(await GetRehireTypesAsync(bypassCache), rehireableStatusType);
                if (string.IsNullOrEmpty(newRehireType))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(contractType))
            {
                if ((!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.Contractual.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.FullTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.PartTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter contract.type '" + contractType + "'");
                }
                var contractTypes = await GetPersonStatusesAsync(bypassCache);
                if (contractTypes == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                // Get all codes that match the incoming enumeration
                var categories = contractTypes.Where(h => h.Category.ToString() == contractType);
                if (categories == null)
                {
                    throw new ArgumentException(string.Concat("Invalid contract type enumeration '", contractType, "' not found in mappings for HR.STATUSES"));
                }
                foreach (var category in categories)
                {
                    newContractTypeCodes.Add(category.Code);
                }
            }
            if (!string.IsNullOrEmpty(contractDetailId))
            {
                newContractDetailTypeCode = ConvertGuidToCode(await GetContractTypesAsync(bypassCache), contractDetailId);
                if (string.IsNullOrEmpty(newContractDetailTypeCode))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);

                var categories = await GetPersonStatusesAsync(bypassCache);
                if (categories == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                var category = categories.FirstOrDefault(h => h.Code == newContractDetailTypeCode);

                if ((!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.Contractual.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.FullTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.PartTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter contract.detail '" + contractDetailId + "' for type '" + newContractDetailTypeCode + "'");
                }
                //
                // If user provided both a contract.type and contract.detail.id, then the code from the detail ID must be in the codes from the type;
                // otherwise return empty set
                //
                if (!string.IsNullOrEmpty(contractType))
                {
                    if(!newContractTypeCodes.Contains(newContractDetailTypeCode))
                    {
                        return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
                    }
                }
            }

            employeeEntities = await employeeRepository.GetEmployeesAsync(offset, limit, newPerson, newCampus, status, newStartOn, newEndOn, newRehireStatus, newRehireType, 
                newContractTypeCodes, newContractDetailTypeCode);
            if (employeeEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            employeeCount = employeeEntities.Item2;

            foreach (var employeeEntity in employeeEntities.Item1)
            {
                var employeeDto = await ConvertEmployee2EntityToDtoAsync(employeeEntity, bypassCache);
                if (employeeDto != null)
                {
                    employeeDtos.Add(employeeDto);
                }
            }

            return new Tuple<IEnumerable<Dtos.Employee2>, int>(employeeDtos, employeeCount);
        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache and read directly from disk.</param>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatus">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractType">Contract type filter.</param>
        /// <param name="contractDetail">Contract detail filter.</param>
        /// <returns>Tuple of employee objects <see cref="Dtos.Employee2"/> and count for paging.</returns>
        public async Task<Tuple<IEnumerable<Dtos.Employee2>, int>> GetEmployees3Async(int offset, int limit, bool bypassCache, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "", 
            string contractType = "", string contractDetailId = "")
        {
            var employeeDtos = new List<Dtos.Employee2>();
            int employeeCount = 0;
            var employeePersonIds = new List<string>();

            if (!HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                throw new PermissionsException("User does not have permission to view Employees.");
            }

            var employeeEntities = new Tuple<IEnumerable<Domain.HumanResources.Entities.Employee>, int>(new List<Domain.HumanResources.Entities.Employee>(), 0);
            string newPerson = string.Empty, newCampus = string.Empty, newStartOn = string.Empty, 
               newEndOn = string.Empty, newRehireStatus = string.Empty, newRehireType = string.Empty, newContractDetailTypeCode = string.Empty; 
            var newContractTypeCodes = new List<string>();

            if (!string.IsNullOrEmpty(person))
            {
                newPerson = await personRepository.GetPersonIdFromGuidAsync(person);
                if (string.IsNullOrEmpty(newPerson))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(campus))
            {
                newCampus = ConvertGuidToCode(await GetLocationsAsync(bypassCache), campus);
                if (string.IsNullOrEmpty(newCampus))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if ((!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Leave.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter status '" + status + "'");
                }
            }
            newStartOn = (startOn == string.Empty ? string.Empty : await ConvertDateArgument(startOn));
            newEndOn = (endOn == string.Empty ? string.Empty : await ConvertDateArgument(endOn));
            newRehireStatus = rehireableStatusEligibility == string.Empty ? string.Empty : await ConvertRehireEligibilityToCode(rehireableStatusEligibility, bypassCache);
            if (!string.IsNullOrEmpty(rehireableStatusType))
            {
                newRehireType = ConvertGuidToCode(await GetRehireTypesAsync(bypassCache), rehireableStatusType);
                if (string.IsNullOrEmpty(newRehireType))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }
            if (!string.IsNullOrEmpty(contractType))
            {
                if ((!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.Contractual.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.FullTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(contractType.ToUpper(), Dtos.EnumProperties.ContractType.PartTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter contract.type '" + contractType + "'");
                }      
                var contractTypes = await GetPersonStatusesAsync(bypassCache);
                if (contractTypes == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                // Get all codes that match the incoming enumeration
                var categories = contractTypes.Where(h => h.Category.ToString() == contractType);
                if (categories == null)
                {
                    throw new ArgumentException(string.Concat("Invalid contract type enumeration '", contractType, "' not found in mappings for HR.STATUSES"));
                }
                foreach (var category in categories)
                {
                    newContractTypeCodes.Add(category.Code);
                }
            }
            if (!string.IsNullOrEmpty(contractDetailId))
            {
                newContractDetailTypeCode = ConvertGuidToCode(await GetContractTypesAsync(bypassCache), contractDetailId);
                if (string.IsNullOrEmpty(newContractDetailTypeCode))
                    return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);

                var categories = await GetPersonStatusesAsync(bypassCache);
                if (categories == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                var category = categories.FirstOrDefault(h => h.Code == newContractDetailTypeCode);
                
                if ((!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.Contractual.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.FullTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase))
                    && (!string.Equals(category.Category.ToString().ToUpper(), Dtos.EnumProperties.ContractType.PartTime.ToString().ToUpper(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("Invalid filter contract.detail '" + contractDetailId + "' for type '" + newContractDetailTypeCode + "'");
                }
                //
                // If user provided both a contract.type and contract.detail.id, then the code from the detail ID must be in the codes from the type;
                // otherwise return empty set
                //
                if (!string.IsNullOrEmpty(contractType))
                {
                    if (!newContractTypeCodes.Contains(newContractDetailTypeCode))
                    {
                        return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
                    }
                }                                       
            }
            employeeEntities = await employeeRepository.GetEmployees2Async(offset, limit, newPerson, newCampus, status, newStartOn, newEndOn, newRehireStatus, newRehireType, 
                newContractTypeCodes, newContractDetailTypeCode);

            if (employeeEntities == null)
            {
                return new Tuple<IEnumerable<Dtos.Employee2>, int>(new List<Dtos.Employee2>(), 0);
            }

            employeeCount = employeeEntities.Item2;

            foreach (var employeeEntity in employeeEntities.Item1)
            {
                var employeeDto = await ConvertEmployee3EntityToDtoAsync(employeeEntity, bypassCache);
                if (employeeDto != null)
                {
                    employeeDtos.Add(employeeDto);
                }
            }

            return new Tuple<IEnumerable<Dtos.Employee2>, int>(employeeDtos, employeeCount);
        }

        //private async Task<string> GetContractTypeByCategoryAsync(string category, bool bypassCache)
        //{
        //    if (!string.IsNullOrEmpty(category))
        //    {
        //        var categories = await GetPersonStatusesAsync(bypassCache);
        //        if (categories == null)
        //        {
        //            throw new ArgumentException("Unable to retrieve all contract types.");
        //        }
        //        var contractType = categories.FirstOrDefault(h => h.Category.ToString() == category);
        //        if (contractType == null)
        //        {
        //            throw new ArgumentException(string.Concat("Invalid contract type enumeration '", category, "' not found in mappings for HR.STATUESS"));
        //        }
        //        else
        //        {
        //            return contractType.Code;
        //        }
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Contract type category is required to retrieve Contact Type.");
        //    }
        //}

        /// <returns>rehireEligibilityCodes</returns>
        private async Task<string> ConvertRehireEligibilityToCode(string category, bool bypassCache = false)
        {
            var rehireTypeCode = string.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                var rehireType = (await GetRehireTypesAsync(bypassCache)).Where(es => es.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase));
                if (rehireType.Any())
                {
                    foreach (var stat in rehireType)
                    {
                        rehireTypeCode += "'" + stat.Code + "' ";
                    }
                }
                else
                    throw new ArgumentException("Invalid filter rehireableStatus.eligibility of '" + category + "'");

            }
            return rehireTypeCode;
        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="guid">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee"./></returns>
        public async Task<Dtos.Employee> GetEmployeeByGuidAsync(string guid)
        {
            Dtos.Employee employeeDto = new Dtos.Employee();
            if (HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                try
                {
                    var ldmGuid = await referenceDataRepository.GetGuidLookupResultFromGuidAsync(guid);
                    if (ldmGuid == null)
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", guid));

                    if (ldmGuid.Entity.ToUpperInvariant() != "HRPER")
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", guid));

                }
                catch
                {
                    throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", guid));
                }
                var employeeEntity = await employeeRepository.GetEmployeeByGuidAsync(guid);
                employeeDto = await ConvertEmployeeEntityToDtoAsync(employeeEntity, false);
                return employeeDto;
            }
            else
                throw new PermissionsException("User does not have permission to view Employees.");

        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="id">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee2"./></returns>
        public async Task<Dtos.Employee2> GetEmployee2ByIdAsync(string id)
        {
            Dtos.Employee2 employeeDto = new Dtos.Employee2();
            if (HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                try
                {
                    var ldmGuid = await referenceDataRepository.GetGuidLookupResultFromGuidAsync(id);
                    if (ldmGuid == null)
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));

                    if (ldmGuid.Entity.ToUpperInvariant() != "HRPER")
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));

                }
                catch
                {
                    throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));
                }
                var employeeEntity = await employeeRepository.GetEmployeeByGuidAsync(id);
                employeeDto = await ConvertEmployee2EntityToDtoAsync(employeeEntity, false);
                return employeeDto;
            }
            else
                throw new PermissionsException("User does not have permission to view Employees.");

        }

        /// <summary>
        /// Get Employee Data based on the permissions of the current user
        /// </summary>
        /// <param name="id">Guid for the employee.</param>
        /// <returns>Employee object <see cref="Dtos.Employee2"./></returns>
        public async Task<Dtos.Employee2> GetEmployee3ByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id can't be null or empty.");
            }
            Dtos.Employee2 employeeDto = new Dtos.Employee2();
            if (HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData))
            {
                try
                {
                    var ldmGuid = await referenceDataRepository.GetGuidLookupResultFromGuidAsync(id);
                    if (ldmGuid == null)
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));

                    if (ldmGuid.Entity.ToUpperInvariant() != "HRPER")
                        throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));

                }
                catch
                {
                    throw new KeyNotFoundException(string.Format("No employee was found for guid '{0}'.", id));
                }
                var employeeEntity = await employeeRepository.GetEmployee2ByGuidAsync(id);
                employeeDto = await ConvertEmployee3EntityToDtoAsync(employeeEntity, false);
                return employeeDto;
            }
            else
                throw new PermissionsException("User does not have permission to view Employees.");

        }

        private async Task<Dtos.Employee> ConvertEmployeeEntityToDtoAsync(Domain.HumanResources.Entities.Employee employeeEntity, bool bypassCache)
        {
            var employeeDto = new Dtos.Employee();
            employeeDto.Id = employeeEntity.Guid;
            employeeDto.Person = new GuidObject2(await personRepository.GetPersonGuidFromIdAsync(employeeEntity.PersonId));
            if (!string.IsNullOrEmpty(employeeEntity.Location))
            {
                var campuses = await GetLocationsAsync(bypassCache);
                if (campuses == null)
                {
                    throw new ArgumentException("Unable to retrieve all locations.");
                }
                var campus = campuses.FirstOrDefault(loc => loc.Code == employeeEntity.Location);
                if (campus == null)
                {
                    throw new ArgumentException(string.Concat("Invalid Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
                // Campus or Location/Site
                if (!string.IsNullOrEmpty(campus.Guid))
                {
                    employeeDto.Campus = new GuidObject2(campus.Guid);
                }
                else
                {
                    throw new ArgumentException(string.Concat("Invalid Guid for Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
            }
            // Contract Type
            if (!string.IsNullOrEmpty(employeeEntity.StatusCode))
            {
                var categories = await GetPersonStatusesAsync(bypassCache);
                if (categories == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                var category = categories.FirstOrDefault(h => h.Code == employeeEntity.StatusCode);
                if (category == null)
                {
                    throw new ArgumentException(string.Concat("Invalid contract type code '", employeeEntity.StatusCode, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }

               // var category = (await GetPersonStatusesAsync(bypassCache)).FirstOrDefault(h => h.Code == employeeEntity.StatusCode).Category;
                if (category.Category == ContractType.PartTime)
                {
                    employeeDto.ContractType = Dtos.EnumProperties.ContractType.PartTime;
                }
                if (category.Category == ContractType.FullTime)
                {
                    employeeDto.ContractType = Dtos.EnumProperties.ContractType.FullTime;
                }
                if (category.Category == ContractType.Contractual)
                {
                    // "contractual" is not a valid enumeration for v7 employees.  Spec says
                    // translate/default it to "partTime".
                    employeeDto.ContractType = Dtos.EnumProperties.ContractType.PartTime;
                }
            }
            // Pay Status
            if (employeeEntity.PayStatus != null)
            {
                switch (employeeEntity.PayStatus)
                {
                    case PayStatus.PartialPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.PartialPay;
                            break;
                        }
                    case PayStatus.WithoutPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                            break;
                        }
                    case PayStatus.WithPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Benefits Status
            if (employeeEntity.BenefitsStatus != null)
            {
                switch (employeeEntity.BenefitsStatus)
                {
                    case BenefitsStatus.WithBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits;
                            break;
                        }
                    case BenefitsStatus.WithoutBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Hours Per Period
            if (employeeEntity.PayPeriodHours != null)
            {
                if (employeeEntity.PayPeriodHours.Any())
                {
                    var hoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>();
                    foreach (var payPeriodHours in employeeEntity.PayPeriodHours)
                    {
                        if (payPeriodHours > 0)
                        {
                            var period = new Dtos.DtoProperties.HoursPerPeriodDtoProperty()
                            {
                                Hours = payPeriodHours,
                                Period = Dtos.EnumProperties.PayPeriods.PayPeriod
                            };
                            hoursPerPeriod.Add(period);
                        }
                    }
                    if (hoursPerPeriod.Any())
                        employeeDto.HoursPerPeriod = hoursPerPeriod;
                }
            }
            // Employee Status
            if (employeeEntity.EmploymentStatus != null)
            {
                switch (employeeEntity.EmploymentStatus)
                {
                    case EmployeeStatus.Active:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Active;
                            break;
                        }
                    case EmployeeStatus.Leave:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                            break;
                        }
                    case EmployeeStatus.Terminated:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Terminated;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Start and End Dates
            if (employeeEntity.StartDate.HasValue)
            {
                employeeDto.StartOn = employeeEntity.StartDate.Value;
            }
            else // this is data error as starton is a required properly
            {
                throw new ArgumentException(string.Concat("Employee " , employeeEntity.PersonId, " is missing the startOn date found in HRP.EFFECT.EMPLOY.DATE. "), "employees.startOn");
            }
            if (employeeEntity.EndDate.HasValue) employeeDto.EndOn = employeeEntity.EndDate.Value;
            // Termination Reason
            if (!string.IsNullOrEmpty(employeeEntity.StatusEndReasonCode) && employeeEntity.EmploymentStatus == EmployeeStatus.Terminated)
            {
                var termReasonId = (await GetEmploymentStatusEndingReasonsAsync(bypassCache)).FirstOrDefault(sr => sr.Code == employeeEntity.StatusEndReasonCode).Guid;
                if (!string.IsNullOrEmpty(termReasonId))
                {
                    employeeDto.TerminationReason = new GuidObject2(termReasonId);
                }
            }
            if (!string.IsNullOrEmpty(employeeEntity.RehireEligibilityCode))
            {
                var rehireReason = (await GetRehireTypesAsync(bypassCache)).FirstOrDefault(rr => rr.Code == employeeEntity.RehireEligibilityCode);
                if (rehireReason != null)
                {
                    var eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Ineligible;
                    if (rehireReason.Category == Colleague.Domain.HumanResources.Entities.RehireTypeCategory.Eligible)
                    {
                        eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Eligible;
                    }
                    var rehireStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty()
                    {
                        Eligibility = eligibilityCategory,
                        Type = new GuidObject2(rehireReason.Guid)
                    };
                    employeeDto.RehireableStatus = rehireStatus;
                }
            }
            return employeeDto;
        }

        private async Task<Dtos.Employee2> ConvertEmployee2EntityToDtoAsync(Domain.HumanResources.Entities.Employee employeeEntity, bool bypassCache)
        {
            var employeeDto = new Dtos.Employee2();
            employeeDto.Id = employeeEntity.Guid;
            employeeDto.Person = new GuidObject2(await personRepository.GetPersonGuidFromIdAsync(employeeEntity.PersonId));

            if (!string.IsNullOrEmpty(employeeEntity.Location))
            {
                var campuses = await GetLocationsAsync(bypassCache);
                if (campuses == null)
                {
                    throw new ArgumentException("Unable to retrieve all locations.");
                }
                var campus = campuses.FirstOrDefault(loc => loc.Code == employeeEntity.Location);
                if (campus == null)
                {
                    throw new ArgumentException(string.Concat("Invalid Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
                // Campus or Location/Site
                if (!string.IsNullOrEmpty(campus.Guid))
                {
                    employeeDto.Campus = new GuidObject2(campus.Guid);
                }
                else
                {
                    throw new ArgumentException(string.Concat("Invalid Guid for Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
            }
            // Contract Type
            if (!string.IsNullOrEmpty(employeeEntity.StatusCode))
            {
                var categories = await GetPersonStatusesAsync(bypassCache);
                if (categories == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                var category = categories.FirstOrDefault(h => h.Code == employeeEntity.StatusCode);
                if (category == null)
                {
                    throw new ArgumentException(string.Concat("Invalid contract type code '", employeeEntity.StatusCode, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
                else
                {
                    employeeDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                    employeeDto.Contract.Detail = new GuidObject2(category.Guid);
                    employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.Contractual;
                    if (category.Category == ContractType.PartTime)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.PartTime;
                    }
                    if (category.Category == ContractType.FullTime)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.FullTime;
                    }
                    if (category.Category == ContractType.Contractual)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.Contractual;
                    }
                }
            }
            // Pay Status
            if (employeeEntity.PayStatus != null)
            {
                switch (employeeEntity.PayStatus)
                {
                    case PayStatus.PartialPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.PartialPay;
                            break;
                        }
                    case PayStatus.WithoutPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                            break;
                        }
                    case PayStatus.WithPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Benefits Status
            if (employeeEntity.BenefitsStatus != null)
            {
                switch (employeeEntity.BenefitsStatus)
                {
                    case BenefitsStatus.WithBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits;
                            break;
                        }
                    case BenefitsStatus.WithoutBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Hours Per Period
            if (employeeEntity.PayPeriodHours != null)
            {
                if (employeeEntity.PayPeriodHours.Any())
                {
                    var hoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>();
                    foreach (var payPeriodHours in employeeEntity.PayPeriodHours)
                    {
                        if (payPeriodHours > 0)
                        {
                            var period = new Dtos.DtoProperties.HoursPerPeriodDtoProperty()
                            {
                                Hours = payPeriodHours,
                                Period = Dtos.EnumProperties.PayPeriods.PayPeriod
                            };
                            hoursPerPeriod.Add(period);
                        }
                    }
                    if (hoursPerPeriod.Any())
                        employeeDto.HoursPerPeriod = hoursPerPeriod;
                }
            }
            // Employee Status
            if (employeeEntity.EmploymentStatus != null)
            {
                switch (employeeEntity.EmploymentStatus)
                {
                    case EmployeeStatus.Active:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Active;
                            break;
                        }
                    case EmployeeStatus.Leave:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                            break;
                        }
                    case EmployeeStatus.Terminated:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Terminated;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Start and End Dates
            if (employeeEntity.StartDate.HasValue)
            {
                employeeDto.StartOn = employeeEntity.StartDate.Value;
            }
            else // this is data error as starton is a required properly
            {
                throw new ArgumentException(string.Concat("Employee ", employeeEntity.PersonId, " is missing the startOn date found in HRP.EFFECT.EMPLOY.DATE. "), "employees.startOn");
            }
            if (employeeEntity.EndDate.HasValue) employeeDto.EndOn = employeeEntity.EndDate.Value;
            // Termination Reason
            if (!string.IsNullOrEmpty(employeeEntity.StatusEndReasonCode) && employeeEntity.EmploymentStatus == EmployeeStatus.Terminated)
            {
                var termReasonId = (await GetEmploymentStatusEndingReasonsAsync(bypassCache)).FirstOrDefault(sr => sr.Code == employeeEntity.StatusEndReasonCode).Guid;
                if (!string.IsNullOrEmpty(termReasonId))
                {
                    employeeDto.TerminationReason = new GuidObject2(termReasonId);
                }
            }
            if (!string.IsNullOrEmpty(employeeEntity.RehireEligibilityCode))
            {
                var rehireReason = (await GetRehireTypesAsync(bypassCache)).FirstOrDefault(rr => rr.Code == employeeEntity.RehireEligibilityCode);
                if (rehireReason != null)
                {
                    var eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Ineligible;
                    if (rehireReason.Category == Colleague.Domain.HumanResources.Entities.RehireTypeCategory.Eligible)
                    {
                        eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Eligible;
                    }
                    var rehireStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty()
                    {
                        Eligibility = eligibilityCategory,
                        Type = new GuidObject2(rehireReason.Guid)
                    };
                    employeeDto.RehireableStatus = rehireStatus;
                }
            }
            return employeeDto;
        }


        private async Task<Dtos.Employee2> ConvertEmployee3EntityToDtoAsync(Domain.HumanResources.Entities.Employee employeeEntity, bool bypassCache)
        {
            var employeeDto = new Dtos.Employee2();
            if (employeeEntity == null)
            {
                return employeeDto;
            }
            employeeDto.Id = employeeEntity.Guid;
            employeeDto.Person = new GuidObject2(await personRepository.GetPersonGuidFromIdAsync(employeeEntity.PersonId));

            if (!string.IsNullOrEmpty(employeeEntity.Location))
            {
                var campuses = await GetLocationsAsync(bypassCache);
                if (campuses == null)
                {
                    throw new ArgumentException("Unable to retrieve all locations.");
                }
                var campus = campuses.FirstOrDefault(loc => loc.Code == employeeEntity.Location);
                if (campus == null )
                {
                    throw new ArgumentException(string.Concat("Invalid Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
                // Campus or Location/Site
                if (!string.IsNullOrEmpty(campus.Guid))
                {
                    employeeDto.Campus = new GuidObject2(campus.Guid);
                }
                else
                {
                    throw new ArgumentException(string.Concat("Invalid Guid for Campus code '", employeeEntity.Location, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
            }
            
            // Contract Type
            if (!string.IsNullOrEmpty(employeeEntity.StatusCode))
            {
                var categories = await GetPersonStatusesAsync(bypassCache);
                if (categories == null)
                {
                    throw new ArgumentException("Unable to retrieve all contract types.");
                }
                var category = categories.FirstOrDefault(h => h.Code == employeeEntity.StatusCode);
                if (category == null)
                {
                    throw new ArgumentException(string.Concat("Invalid contract type code '", employeeEntity.StatusCode, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
               else
                { 
                    employeeDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                    employeeDto.Contract.Detail = new GuidObject2(category.Guid);
                    employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.Contractual;
                    if (category.Category == ContractType.PartTime)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.PartTime;
                    }
                    if (category.Category == ContractType.FullTime)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.FullTime;
                    }
                    if (category.Category == ContractType.Contractual)
                    {
                        employeeDto.Contract.Type = Dtos.EnumProperties.ContractType.Contractual;
                    }
                }
            }
            //Pay Class
            if (!string.IsNullOrEmpty(employeeEntity.PayClass))
            {
                var payClasses = (await GetPayClassesAsync(bypassCache));
                if (payClasses == null)
                {
                    throw new ArgumentException("Unable to retrieve all pay classes.");
                }
                var payClass = payClasses.FirstOrDefault(pc => pc.Code == employeeEntity.PayClass);
                if (payClass == null)
                {
                    throw new ArgumentException(string.Concat("Invalid payclass code '", employeeEntity.PayClass, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
                if (!string.IsNullOrEmpty(payClass.Guid))
                {
                    employeeDto.PayClass = new GuidObject2(payClass.Guid);
                }
                else
                {
                    throw new ArgumentException(string.Concat("Invalid Guid for payclass code '", employeeEntity.PayClass, "'. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
                }
            }
            // Pay Status
            if (employeeEntity.PayStatus != null)
            {
                switch (employeeEntity.PayStatus)
                {
                    case PayStatus.PartialPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.PartialPay;
                            break;
                        }
                    case PayStatus.WithoutPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                            break;
                        }
                    case PayStatus.WithPay:
                        {
                            employeeDto.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Benefits Status
            if (employeeEntity.BenefitsStatus != null)
            {
                switch (employeeEntity.BenefitsStatus)
                {
                    case BenefitsStatus.WithBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits;
                            break;
                        }
                    case BenefitsStatus.WithoutBenefits:
                        {
                            employeeDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                            break;
                        }
                    default:
                        break;
                }
            }
            //Hours Per Period
            var hoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>();
            if (employeeEntity.PpwgCycleWorkTimeAmt != null && employeeEntity.PpwgCycleWorkTimeAmt != 0)
            {
                var period = new Dtos.DtoProperties.HoursPerPeriodDtoProperty() 
                { 
                    Hours = employeeEntity.PpwgCycleWorkTimeAmt, 
                    Period = Dtos.EnumProperties.PayPeriods.PayPeriod 
                };
                hoursPerPeriod.Add(period);
            }
            if (employeeEntity.PpwgYearWorkTimeAmt != null && employeeEntity.PpwgYearWorkTimeAmt != 0)
            {
                var period = new Dtos.DtoProperties.HoursPerPeriodDtoProperty() 
                { 
                    Hours = employeeEntity.PpwgYearWorkTimeAmt, 
                    Period = Dtos.EnumProperties.PayPeriods.Year 
                };
                hoursPerPeriod.Add(period);
            }
            if (hoursPerPeriod.Any() && hoursPerPeriod.Count() > 0)
            {
                employeeDto.HoursPerPeriod = hoursPerPeriod;
            }
            // Employee Status
            if (employeeEntity.EmploymentStatus != null)
            {
                switch (employeeEntity.EmploymentStatus)
                {
                    case EmployeeStatus.Active:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Active;
                            break;
                        }
                    case EmployeeStatus.Leave:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                            break;
                        }
                    case EmployeeStatus.Terminated:
                        {
                            employeeDto.Status = Dtos.EnumProperties.EmployeeStatus.Terminated;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Start and End Dates
            if (employeeEntity.StartDate.HasValue)
            {
                employeeDto.StartOn = employeeEntity.StartDate.Value;
            }
            else // this is data error as starton is a required properly
            {
                throw new ArgumentException(string.Concat("Employee ", employeeEntity.PersonId, " is missing the startOn date found in HRP.EFFECT.EMPLOY.DATE. Entity: 'HRPER', Record ID: '", employeeEntity.Guid, "'"));
            }
            if (employeeEntity.EndDate.HasValue) employeeDto.EndOn = employeeEntity.EndDate.Value;
            // Termination Reason
            if (!string.IsNullOrEmpty(employeeEntity.StatusEndReasonCode) && employeeEntity.EmploymentStatus == EmployeeStatus.Terminated)
            {
                var termReasonId = (await GetEmploymentStatusEndingReasonsAsync(bypassCache)).FirstOrDefault(sr => sr.Code == employeeEntity.StatusEndReasonCode).Guid;
                if (!string.IsNullOrEmpty(termReasonId))
                {
                    employeeDto.TerminationReason = new GuidObject2(termReasonId);
                }
            }
            if (!string.IsNullOrEmpty(employeeEntity.RehireEligibilityCode))
            {
                var rehireReason = (await GetRehireTypesAsync(bypassCache)).FirstOrDefault(rr => rr.Code == employeeEntity.RehireEligibilityCode);
                if (rehireReason != null)
                {
                    var eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Ineligible;
                    if (rehireReason.Category == Colleague.Domain.HumanResources.Entities.RehireTypeCategory.Eligible)
                    {
                        eligibilityCategory = Dtos.EnumProperties.RehireEligibility.Eligible;
                    }
                    var rehireStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty()
                    {
                        Eligibility = eligibilityCategory,
                        Type = new GuidObject2(rehireReason.Guid)
                    };
                    employeeDto.RehireableStatus = rehireStatus;
                }
            }
            return employeeDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an Employee domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="employeeDto"><see cref="Dtos.Employee2">Employee</see></param>
        /// <returns><see cref="Dtos.Employee2">Employee</see></returns>
        public async Task<Dtos.Employee2> PutEmployee2Async(string guid, Dtos.Employee2 employeeDto, Dtos.Employee2 origEmployeeDto)
        {
            if (employeeDto == null)
                throw new ArgumentNullException("Employee", "Must provide a Employee for update");
            if (string.IsNullOrEmpty(employeeDto.Id))
                throw new ArgumentNullException("Employee", "Must provide a guid for Employee update");
            //make sure the guid is actually a GUID
            Guid guidOutput;
            if (!Guid.TryParse(guid, out guidOutput))
            {
                throw new ArgumentNullException("Employee", "Must provide a valid guid for Employee update");
            }

            // get the person ID associated with the incoming employee guid
            var employeeId = await employeeRepository.GetEmployeeIdFromGuidAsync(guid);

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(employeeId))
            {
                try
                {

                    // verify the user has the permission to update a Employee
                    CheckUpdateEmployeePermission();

                    employeeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    CompareEmployeesDto(employeeDto, origEmployeeDto);

                    await ValidateEmployee(employeeDto);

                    // map the DTO to entities
                    var employeeEntity
                    = await ConvertEmployee2DtoToEntityAsync(employeeDto);

                    // update the entity in the database
                    var updatedEmployeeEntity =
                        await employeeRepository.UpdateEmployee2Async(employeeEntity);

                    // convert the entity to a DTO
                    var dtoEmployee = await ConvertEmployee3EntityToDtoAsync(updatedEmployeeEntity, true);

                    // return the newly updated DTO
                    return dtoEmployee;

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await PostEmployee2Async(employeeDto);
        }

        private void CompareEmployeesDto(Dtos.Employee2 employeeDto, Dtos.Employee2 origDto)

        {
            if (origDto != null)
            {
                //status cannot be changed.
                if (origDto.Status != null && origDto.Status != Dtos.EnumProperties.EmployeeStatus.NotSet && employeeDto.Status != null && employeeDto.Status != Dtos.EnumProperties.EmployeeStatus.NotSet)
                {
                    if (origDto.Status != employeeDto.Status)
                    {
                        throw new ArgumentException("The status cannot be changed in a PUT request. ", "employees.status");
                    }
                }
                //person Id cannot be changed
                if (origDto.Person != null && !string.IsNullOrEmpty(origDto.Person.Id) && employeeDto.Person != null)
                {
                    if (!origDto.Person.Id.Equals(employeeDto.Person.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The person Id cannot be changed in a PUT request. ", "employees.person.id");
                    }
                }
                //campus location cannot be changed.
                if (origDto.Campus != null && !string.IsNullOrEmpty(origDto.Campus.Id) && employeeDto.Campus != null)
                {
                    if (!origDto.Campus.Id.Equals(employeeDto.Campus.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The campus Id cannot be changed in a PUT request. ", "employees.campus.id");
                    }
                }
                //the contract.type can't be changed
                if (origDto.Contract != null && origDto.Contract.Type != Dtos.EnumProperties.ContractType.NotSet && employeeDto.Contract != null)
                {
                    if (origDto.Contract.Type != employeeDto.Contract.Type)
                    {
                        throw new ArgumentException("The contract type cannot be changed in a PUT request. ", "employees.contract.type");
                    }
                }
                if (origDto.Contract != null && origDto.Contract.Detail != null && !string.IsNullOrEmpty(origDto.Contract.Detail.Id) && employeeDto.Contract != null && employeeDto.Contract.Detail != null)
                {
                    if (!origDto.Contract.Detail.Id.Equals(employeeDto.Contract.Detail.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The contract detail Id cannot be changed in a PUT request. ", "employees.contract.detail.id");
                    }
                }
                //payclass cannot be updated in a PUT
                if (origDto.PayClass != null && !string.IsNullOrEmpty(origDto.PayClass.Id) && employeeDto.PayClass != null)
                {
                    if (!origDto.PayClass.Id.Equals(employeeDto.PayClass.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The payclass Id cannot be changed in a PUT request. ", "employees.payclass.id");
                    }
                }
                if (origDto.PayClass == null && employeeDto.PayClass != null)
                {
                    throw new ArgumentException("The payclass Id cannot be changed in a PUT request. ", "employees.payclass.id");
                }

                //payStatus cannot be updated in a PUT
                if (origDto.PayStatus != null && origDto.PayStatus != Dtos.EnumProperties.PayStatus.NotSet && ((employeeDto.PayStatus != null && employeeDto.PayStatus != Dtos.EnumProperties.PayStatus.NotSet) || employeeDto.PayStatus == null))
                {
                    if (origDto.PayStatus != employeeDto.PayStatus)
                    {
                        throw new ArgumentException("The PayStatus cannot be changed in a PUT request. ", "employees.PayStatus");
                    }
                }
                //benefit status cannot be updated in a PUT
                if (origDto.BenefitsStatus != null && origDto.BenefitsStatus != Dtos.EnumProperties.BenefitsStatus.NotSet && ((employeeDto.BenefitsStatus != null && employeeDto.BenefitsStatus != Dtos.EnumProperties.BenefitsStatus.NotSet) || employeeDto.BenefitsStatus == null))
                {
                    if (origDto.BenefitsStatus != employeeDto.BenefitsStatus)
                    {
                        throw new ArgumentException("The benefitStatus cannot be changed in a PUT request. ", "employees.benefitStatus");
                    }
                }
                //end date cannot be updated in a PUT
                if ((origDto.EndOn.HasValue && employeeDto.EndOn.HasValue) || (origDto.EndOn == null && employeeDto.EndOn.HasValue))
                {
                    if (origDto.EndOn != employeeDto.EndOn)
                    {
                        throw new ArgumentException("The endOn cannot be changed in a PUT request. ", "employees.endOn");
                    }
                }
                //terminationReason cannot be changed.
                if (origDto.TerminationReason != null && !string.IsNullOrEmpty(origDto.TerminationReason.Id) )
                {
                    if ((employeeDto.TerminationReason != null && !origDto.TerminationReason.Id.Equals(employeeDto.TerminationReason.Id, StringComparison.OrdinalIgnoreCase) || employeeDto.TerminationReason == null))
                    {
                        throw new ArgumentException("The terminationReason Id cannot be changed in a PUT request. ", "employees.terminationReason.id");
                    }
                }
                if (origDto.TerminationReason == null && employeeDto.TerminationReason != null)
                {
                    throw new ArgumentException("The terminationReason Id cannot be changed in a PUT request. ", "employees.terminationReason.id");
                }
                //rehireableStatus cannot be changed.
                if (origDto.RehireableStatus == null && employeeDto.RehireableStatus != null)
                {
                    throw new ArgumentException("The rehireableStatus cannot be changed in a PUT request. ", "employees.rehireableStatus");
                }

                if (origDto.RehireableStatus != null && origDto.RehireableStatus.Type != null && !string.IsNullOrEmpty(origDto.RehireableStatus.Type.Id) && employeeDto.RehireableStatus != null && employeeDto.RehireableStatus.Type != null)
                {
                    if (!origDto.RehireableStatus.Type.Id.Equals(employeeDto.RehireableStatus.Type.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The rehireableStatus type Id cannot be changed in a PUT request. ", "employees.rehireableStatus.type.id");
                    }
                }
                if (origDto.RehireableStatus != null && origDto.RehireableStatus.Eligibility != null && origDto.RehireableStatus.Eligibility != Dtos.EnumProperties.RehireEligibility.NotSet && employeeDto.RehireableStatus != null && employeeDto.RehireableStatus.Eligibility != null && employeeDto.RehireableStatus.Eligibility != Dtos.EnumProperties.RehireEligibility.NotSet)
                {
                    if (origDto.RehireableStatus.Eligibility != employeeDto.RehireableStatus.Eligibility)
                    {
                        throw new ArgumentException("The rehireableStatus eligibility cannot be changed in a PUT request. ", "employees.rehireableStatus.eligibility");
                    }
                }
                //hoursPerPeriod cannot be changed.
                if(origDto.HoursPerPeriod != null && origDto.HoursPerPeriod.Any() && employeeDto.HoursPerPeriod != null)
                {
                    if (origDto.HoursPerPeriod.Count() != employeeDto.HoursPerPeriod.Count())
                    {
                        throw new ArgumentException("The hoursPerPeriod cannot be changed in a PUT request. ", "employees.hoursPerPeriod");
                    }
                    //if the number of elements in the array remains the same then we need check the content
                    foreach (var origHour in origDto.HoursPerPeriod)
                    {
                        var hour = employeeDto.HoursPerPeriod.Where(h => h.Hours == origHour.Hours && h.Period == origHour.Period);
                        if (hour == null || !hour.Any())
                        {
                            throw new ArgumentException("The hoursPerPeriod cannot be changed in a PUT request. ", "employees.hoursPerPeriod");
                        }
                    }
                }

            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Post (Create) an Accounts Payable Invoices doamin entity
        /// </summary>
        /// <param name="employeeDto"><see cref="Dtos.Employee2">Employee</see></param>
        /// <returns><see cref="Dtos.Employee2">Employee</see></returns>
        public async Task<Dtos.Employee2> PostEmployee2Async(Dtos.Employee2 employeeDto)
        {
            if (employeeDto == null)
                throw new ArgumentNullException("Employee", "Must provide a Employee for update");
            if (string.IsNullOrEmpty(employeeDto.Id))
                throw new ArgumentNullException("Employee", "Must provide a guid for Employee update");

            await ValidateEmployee(employeeDto);

            // verify the user has the permission to create a Employee
            CheckUpdateEmployeePermission();

            employeeRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var employeeEntity
                         = await ConvertEmployee2DtoToEntityAsync(employeeDto);

                // create a Employee entity in the database
                var createdEmployee =
                    await employeeRepository.CreateEmployee2Async(employeeEntity);

                var dtoEmployee = await ConvertEmployee3EntityToDtoAsync(createdEmployee, true);

                // return the newly created Employee
                return dtoEmployee;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        private async Task<Domain.HumanResources.Entities.Employee> ConvertEmployee2DtoToEntityAsync(Dtos.Employee2 employeeDto, bool bypassCache = true)
        {
            var guid = employeeDto.Id;
            var personId = await personRepository.GetPersonIdFromGuidAsync(employeeDto.Person.Id);
            var employeeEntity = new Domain.HumanResources.Entities.Employee(guid, personId);

            if (employeeDto.Campus != null)
            {
                if (!string.IsNullOrEmpty(employeeDto.Campus.Id))
                {
                    var campuses = await GetLocationsAsync(bypassCache);
                    if (campuses != null)
                    {

                        var campus = campuses.FirstOrDefault(loc => loc.Guid == employeeDto.Campus.Id);
                        if (campus == null)
                        {
                            throw new ArgumentNullException("Invalid Campus Id. ", "employees.campus");
                        }
                        // Campus or Location/Site
                        if (!string.IsNullOrEmpty(campus.Code))
                        {
                            employeeEntity.Location = campus.Code;
                        }
                    }
                }
            }

            //Pay Class
            if (employeeDto.PayClass != null && !string.IsNullOrEmpty(employeeDto.PayClass.Id))
            {
                var payClassCode = (await GetPayClassesAsync(bypassCache)).FirstOrDefault(pc => pc.Guid == employeeDto.PayClass.Id).Code;
                // Pay Class
                if (!string.IsNullOrEmpty(payClassCode))
                {
                    employeeEntity.PayClass = payClassCode;
                }
            }

            // Contract Type
            var contract = employeeDto.Contract;
            if (contract != null)
            {
                if (contract.Detail != null && !string.IsNullOrEmpty(contract.Detail.Id))
                {
                    var category = (await GetPersonStatusesAsync(bypassCache)).FirstOrDefault(h => h.Guid == contract.Detail.Id);
                    if (category != null && !string.IsNullOrEmpty(category.Code))
                    {
                        employeeEntity.StatusCode = category.Code;
                    }
                }
            }

            // Pay Status
            if (employeeDto.PayStatus != null)
            {
                switch (employeeDto.PayStatus)
                {
                    case Dtos.EnumProperties.PayStatus.PartialPay:
                        {
                            employeeEntity.PayStatus = PayStatus.PartialPay;
                            break;
                        }
                    case Dtos.EnumProperties.PayStatus.WithoutPay:
                        {
                            employeeEntity.PayStatus = PayStatus.WithoutPay;
                            break;
                        }
                    case Dtos.EnumProperties.PayStatus.WithPay:
                        {
                            employeeEntity.PayStatus = PayStatus.WithPay;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Benefits Status
            if (employeeDto.BenefitsStatus != null)
            {
                switch (employeeDto.BenefitsStatus)
                {
                    case Dtos.EnumProperties.BenefitsStatus.WithBenefits:
                        {
                            employeeEntity.BenefitsStatus = BenefitsStatus.WithBenefits;
                            break;
                        }
                    case Dtos.EnumProperties.BenefitsStatus.WithoutBenefits:
                        {
                            employeeEntity.BenefitsStatus = BenefitsStatus.WithoutBenefits;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Hours Per Period
            if (employeeDto.HoursPerPeriod != null)
            {
                if (employeeDto.HoursPerPeriod.Any())
                {
                    var hoursPerPeriod = new List<Decimal?>();
                    foreach (var payPeriod in employeeDto.HoursPerPeriod)
                    {
                        if (payPeriod.Hours > 0)
                        {
                            hoursPerPeriod.Add(payPeriod.Hours);
                        }
                    }
                    if (hoursPerPeriod.Any())
                        employeeEntity.PayPeriodHours = hoursPerPeriod;
                }
            }
            // Employee Status
            if (employeeDto.Status != null)
            {
                switch (employeeDto.Status)
                {
                    case Dtos.EnumProperties.EmployeeStatus.Active:
                        {
                            employeeEntity.EmploymentStatus = EmployeeStatus.Active;
                            break;
                        }
                    case Dtos.EnumProperties.EmployeeStatus.Leave:
                        {
                            employeeEntity.EmploymentStatus = EmployeeStatus.Leave;
                            break;
                        }
                    case Dtos.EnumProperties.EmployeeStatus.Terminated:
                        {
                            employeeEntity.EmploymentStatus = EmployeeStatus.Terminated;
                            break;
                        }
                    default:
                        break;
                }
            }
            // Start and End Dates
            employeeEntity.StartDate = employeeDto.StartOn;
            employeeEntity.EndDate = employeeDto.EndOn;

            // Termination Reason
            if (employeeDto.TerminationReason != null && !string.IsNullOrEmpty(employeeDto.TerminationReason.Id))
            {
                var termReasons = await GetEmploymentStatusEndingReasonsAsync(bypassCache);
                if (termReasons != null)
                {
                    var termReason = termReasons.FirstOrDefault(loc => loc.Guid == employeeDto.TerminationReason.Id);
                    if (termReason == null)
                    {
                        throw new ArgumentException("Invalid termination reason Id. ", "employees.termination");
                    }
                    if (!string.IsNullOrEmpty(termReason.Code))
                    {
                        employeeEntity.StatusEndReasonCode = termReason.Code;
                    }
                }

            }
            // Rehire Eligibility
            if (employeeDto.RehireableStatus != null)
            {
                var eligibilities = await GetRehireTypesAsync(bypassCache);
                if (eligibilities != null)
                {
                    if (employeeDto.RehireableStatus.Type != null && !string.IsNullOrEmpty(employeeDto.RehireableStatus.Type.Id))
                    {
                        var eligibility = eligibilities.FirstOrDefault(rr => rr.Guid == employeeDto.RehireableStatus.Type.Id);
                        if (eligibility == null)
                        {
                            throw new ArgumentException("Invalid rehireableStatus type Id. ", "employees.rehireableStatus.type.id");
                        }
                        if (!eligibility.Category.ToString().Equals(employeeDto.RehireableStatus.Eligibility.ToString()))
                        {
                            throw new ArgumentException("Invalid rehireableStatus eligibility. ", "employees.rehireableStatus.eligibility");
                        }
                        if (!string.IsNullOrEmpty(eligibility.Code))
                        {
                            employeeEntity.RehireEligibilityCode = eligibility.Code;
                        }
                    }
                    else
                    {
                        if (employeeDto.RehireableStatus.Eligibility.HasValue)
                        {
                            var eligibility = eligibilities.FirstOrDefault(rr => rr.Category.ToString().ToUpper() == employeeDto.RehireableStatus.Eligibility.ToString().ToUpper());
                            if (eligibility == null)
                            {
                                throw new ArgumentException("Invalid rehireableStatus eligibility . ", "employees.rehireableStatus.eligibility");
                            }
                            if (!string.IsNullOrEmpty(eligibility.Code))
                            {
                                employeeEntity.RehireEligibilityCode = eligibility.Code;
                            }
                        }
                    }
                }
            }

            return employeeEntity;
        }
        //#endregion

        /// <summary>
        /// Helper method to validate employee PUT/POST.
        /// </summary>
        /// <param name="employeeDto">employee DTO object of type <see cref="Dtos.Employee2"/></param>
        private async Task ValidateEmployee(Dtos.Employee2 employeeDto)
        {
            if (employeeDto.Person == null || string.IsNullOrEmpty(employeeDto.Person.Id))
            {
                throw new ArgumentNullException("The Person Id is required for a PUT or POST request. ", "employees.person.id");
            }
            var personId = await personRepository.GetPersonIdFromGuidAsync(employeeDto.Person.Id);
            if (personId == null)
            {
                throw new ArgumentException(string.Format("The person Id '{0}' is not a valid person. ", employeeDto.Person.Id));
            }
            var person = await personRepository.GetPersonByGuidNonCachedAsync(employeeDto.Person.Id);
            if (person == null)
            {
                throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", employeeDto.Person.Id));
            }
            if (person.PersonCorpIndicator == "Y")
            {
                throw new ArgumentException(string.Concat("An employee cannot be created for organization ", employeeDto.Person.Id));
            }
            if (employeeDto.HomeOrganization != null && !string.IsNullOrEmpty(employeeDto.HomeOrganization.Id))
            {
                throw new ArgumentNullException("The Home Organization Id is not allowed for a PUT or POST request. ", "employees.homeOrganization.id");
            }
            if (employeeDto.Status == null || employeeDto.Status == Dtos.EnumProperties.EmployeeStatus.NotSet)
            {
                throw new ArgumentException("Status is required for a PUT or POST request. ", "employees.status");
            }
            if (!string.IsNullOrEmpty(employeeDto.Id) && !employeeDto.Id.Equals(Guid.Empty.ToString()))
            {
                var employeeId = await employeeRepository.GetEmployeeIdFromGuidAsync(employeeDto.Id);
                if (employeeId != null && personId != employeeId)
                {
                    throw new ArgumentException(string.Format("The Person Id '{0}' doesn't match the Employee Id '{1}'. ", personId, employeeId), "employees.id");
                }
            }
            if (employeeDto.Id.Equals(Guid.Empty.ToString()))
            {
                if (employeeDto.Status.HasValue && employeeDto.Status != Dtos.EnumProperties.EmployeeStatus.Active)
                {
                    throw new ArgumentException("The status can only be set to 'active' for POST requests. ", "employees.status");
                }
            }
            if (employeeDto.Contract == null || (employeeDto.Contract.Type == Dtos.EnumProperties.ContractType.NotSet && (employeeDto.Contract.Detail == null || string.IsNullOrEmpty(employeeDto.Contract.Detail.Id))))
            {
                throw new ArgumentException("The contract object is a required field. ", "employee.contract");
            }

            if (employeeDto.Contract.Detail == null || string.IsNullOrWhiteSpace(employeeDto.Contract.Detail.Id ))
            {
                throw new ArgumentException("employee.contract.detail.id", "The contract detail id is a required field");
            }

            if (!employeeDto.StartOn.HasValue)
            {
                throw new ArgumentException("The start on is a required field for PUT and POST requests. ", "employees.startOn");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a UPDATE operation. This API will integrate information related to employees that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdateEmployeePermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.UpdateEmployee);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update Employees.");
            }
        }

        /// <summary>
        /// Gets a list of Employee names
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>Enumeration of Persons <see cref="Dtos.Base.Person"/> </returns>
        /// <exception><see cref="ArgumentNullException">ArgumentNullException</see></exception>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        public async Task<IEnumerable<Dtos.Base.Person>> QueryEmployeeNamesByPostAsync(Dtos.Base.EmployeeNameQueryCriteria criteria)
        {
            if (criteria == null)
            { 
                throw new ArgumentNullException("criteria", "criteria must be provided to search for persons.");
            }

            if (!HasPermission(HumanResourcesPermissionCodes.ViewAllEarningsStatements) &&
                !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeData) &&
                !HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData) &&
                !HasPermission(HumanResourcesPermissionCodes.ViewEmployeeW2) &&
                !HasPermission(HumanResourcesPermissionCodes.ViewEmployee1095C))
            {
                throw new PermissionsException("Current user is not authorized to view employee data");
            }

            var personBases = await personBaseRepository.SearchByIdsOrNamesAsync(criteria.Ids, criteria.QueryKeyword);

            var employeeKeys = await employeeRepository.GetEmployeeKeysAsync(includeNonEmployees: criteria.IncludeNonEmployees, activeOnly: criteria.ActiveOnly);

            var employeePersonBases = personBases.Where(pb => employeeKeys.Any(id => id == pb.Id));

            var results = new List<Dtos.Base.Person>();

            foreach (var personBase in employeePersonBases)
            {                
                var personDto = new Dtos.Base.Person()
                {
                    Id = personBase.Id,
                    FirstName = personBase.FirstName,
                    MiddleName = personBase.MiddleName,
                    LastName = personBase.LastName,
                    BirthNameFirst = personBase.BirthNameFirst,
                    BirthNameMiddle = personBase.BirthNameMiddle,
                    BirthNameLast = personBase.BirthNameLast,
                    PreferredName = personBase.PreferredName,
                    PrivacyStatusCode = personBase.PrivacyStatusCode
                };
                results.Add(personDto);
            }
            return results;
        }
    }
}
