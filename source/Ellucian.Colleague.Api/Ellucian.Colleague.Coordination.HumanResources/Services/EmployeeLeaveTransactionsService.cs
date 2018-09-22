//Copyright 2017 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmployeeLeaveTransactionsService : BaseCoordinationService, IEmployeeLeaveTransactionsService
    {
        private readonly IEmployeeLeaveTransactionsRepository _employeeLeaveTransactionsRepository;
        private readonly IEmployeeLeavePlansRepository _empLeavePlansRepository;
        public EmployeeLeaveTransactionsService(

            IEmployeeLeaveTransactionsRepository employeeLeaveTransactionsRepository,
            IEmployeeLeavePlansRepository empLeavePlansRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _empLeavePlansRepository = empLeavePlansRepository;
            _employeeLeaveTransactionsRepository = employeeLeaveTransactionsRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all employee-leave-transactions
        /// </summary>
        /// <returns>Collection of EmployeeLeaveTransactions DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions>, int>> GetEmployeeLeaveTransactionsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewEmployeeLeaveTransactionsPermission();
            var empLeaveTransCollection = new List<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions>();
            int empLeaveTransCount = 0;
            try
            {
                var empLeaveTransEntities = await _employeeLeaveTransactionsRepository.GetEmployeeLeaveTransactionsAsync(offset, limit, bypassCache);
                if (empLeaveTransEntities != null)
                {
                    empLeaveTransCount = empLeaveTransEntities.Item2;
                    foreach (var trans in empLeaveTransEntities.Item1)
                    {
                        var leaveTransDto = await ConvertEmployeeLeaveTransactionsEntityToDto(trans, bypassCache);
                        if (leaveTransDto != null)
                        {
                            empLeaveTransCollection.Add(leaveTransDto);
                        }

                    }
                    return new Tuple<IEnumerable<EmployeeLeaveTransactions>, int>(empLeaveTransCollection, empLeaveTransCount);

                }
                else
                {
                    return new Tuple<IEnumerable<EmployeeLeaveTransactions>, int>(new List<Dtos.EmployeeLeaveTransactions>(), 0);

                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmployeeLeaveTransactions from its GUID
        /// </summary>
        /// <returns>EmployeeLeaveTransactions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions> GetEmployeeLeaveTransactionsByGuidAsync(string guid, bool bypassCache = false)
        {
            CheckViewEmployeeLeaveTransactionsPermission();
            try
            {
                return await ConvertEmployeeLeaveTransactionsEntityToDto(await _employeeLeaveTransactionsRepository.GetEmployeeLeaveTransactionsByIdAsync(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("employee-leave-transactions not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("employee-leave-transactions not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Perleave domain entity to its corresponding EmployeeLeaveTransactions DTO
        /// </summary>
        /// <param name="source">Perleave domain entity</param>
        /// <returns>EmployeeLeaveTransactions DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions> ConvertEmployeeLeaveTransactionsEntityToDto(PerleaveDetails source, bool bypassCache)
        {
            var employeeLeaveTransactions = new Ellucian.Colleague.Dtos.EmployeeLeaveTransactions();
            employeeLeaveTransactions.Id = source.Guid;
            //get employee leave
            if (!string.IsNullOrEmpty(source.EmployeeLeaveId))
            {
                var employeeLeave = await _empLeavePlansRepository.GetEmployeeLeavePlansByIdAsync(source.EmployeeLeaveId);
                if (employeeLeave == null)
                {
                    throw new ArgumentException(string.Concat("Invalid employee leave transaction record '", source.EmployeeLeaveId, "'. Entity: ‘PERLVDTL’, Record ID: '", source.Id, "'"));
                }
                employeeLeaveTransactions.EmployeeLeave = new GuidObject2(employeeLeave.Guid);


            }

            //get transaction date
            employeeLeaveTransactions.TransactionDate = source.TransactionDate;
            //get accrued & taken hours
            if (source.LeaveHours.HasValue)
            {
                if (source.LeaveHours >= 0)
                {
                    employeeLeaveTransactions.Accrued = source.LeaveHours;
                }
                else
                {
                    //get taken hours
                    employeeLeaveTransactions.Taken = Math.Abs(source.LeaveHours.Value);
                }
            }
            // get available hours
            employeeLeaveTransactions.Available = source.AvailableHours;
            return employeeLeaveTransactions;
        }

        /// <summary>
        /// Permissions code that allows an external system to do view employee leave plans
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewEmployeeLeaveTransactionsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewEmployeeLeaveTransactions);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view employee leave transactions.");
            }
        }
    }
}