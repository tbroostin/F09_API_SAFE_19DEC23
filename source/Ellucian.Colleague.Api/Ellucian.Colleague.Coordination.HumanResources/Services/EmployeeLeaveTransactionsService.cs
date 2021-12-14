//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
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
using Ellucian.Colleague.Domain.Exceptions;
using System.Linq;

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
            var empLeaveTransCollection = new List<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions>();
    
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails>, int> empLeaveTransEntities = null;
            try
            {
                empLeaveTransEntities = await _employeeLeaveTransactionsRepository.GetEmployeeLeaveTransactionsAsync(offset, limit, bypassCache);
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

            if (empLeaveTransEntities == null || empLeaveTransEntities.Item1 == null)
            {
                return new Tuple<IEnumerable<EmployeeLeaveTransactions>, int>(new List<Dtos.EmployeeLeaveTransactions>(), 0);
            }

           
            var employeeLeaveIds = empLeaveTransEntities.Item1
                .Where(x => (!string.IsNullOrEmpty(x.EmployeeLeaveId)))
                .Select(x => x.EmployeeLeaveId).Distinct().ToList();

            var perLeaveGuidCollection = await _empLeavePlansRepository.GetPerleaveGuidsCollectionAsync(employeeLeaveIds);

            foreach (var trans in empLeaveTransEntities.Item1)
            {
                try
                {
                    empLeaveTransCollection.Add(await ConvertEmployeeLeaveTransactionsEntityToDto(trans, perLeaveGuidCollection));
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
            return new Tuple<IEnumerable<EmployeeLeaveTransactions>, int>(empLeaveTransCollection, empLeaveTransEntities.Item2);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmployeeLeaveTransactions from its GUID
        /// </summary>
        /// <returns>EmployeeLeaveTransactions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions> GetEmployeeLeaveTransactionsByGuidAsync(string guid, bool bypassCache = false)
        {
            PerleaveDetails perLeaveDetailsDomianEntity = null;
            try
            {
                perLeaveDetailsDomianEntity = await _employeeLeaveTransactionsRepository.GetEmployeeLeaveTransactionsByIdAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No employee-leave-transactions was found for GUID '" + guid + "'", ex);
            }

            try
            {
              
                var perLeaveGuidCollection = await _empLeavePlansRepository.GetPerleaveGuidsCollectionAsync(new List<string> { perLeaveDetailsDomianEntity.EmployeeLeaveId });

                var retval = await ConvertEmployeeLeaveTransactionsEntityToDto(perLeaveDetailsDomianEntity, perLeaveGuidCollection);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return retval;

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
        /// <param name="source">PerleaveDetails domain entity</param>
        /// <param name="source">Perleave guid collection</param>
        /// <returns>EmployeeLeaveTransactions DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions> ConvertEmployeeLeaveTransactionsEntityToDto(PerleaveDetails source,
            Dictionary<string, string> perLeaveGuidCollection)
        {
            var employeeLeaveTransactions = new Ellucian.Colleague.Dtos.EmployeeLeaveTransactions();
            employeeLeaveTransactions.Id = source.Guid;
            //get employee leave
            //if (!string.IsNullOrEmpty(source.EmployeeLeaveId))
            //{
            //  var employeeLeave = await _empLeavePlansRepository.GetEmployeeLeavePlansByIdAsync(source.EmployeeLeaveId);
            //    if (employeeLeave == null)
            //    {
            //        throw new ArgumentException(string.Concat("Invalid employee leave transaction record '", source.EmployeeLeaveId, "'. Entity: ‘PERLVDTL’, Record ID: '", source.Id, "'"));
            //    }
            //    employeeLeaveTransactions.EmployeeLeave = new GuidObject2(employeeLeave.Guid);
            //}

            if (!string.IsNullOrEmpty(source.EmployeeLeaveId))
            {
                if (perLeaveGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("PERLEAVE GUID not found for employeeLeaveId: '", source.EmployeeLeaveId, "'"), "GUID.Not.Found"
                        , source.Id, source.Guid);
                }
                else
                {
                    var employeeLeaveGuid = string.Empty;
                    perLeaveGuidCollection.TryGetValue(source.EmployeeLeaveId, out employeeLeaveGuid);
                    if (string.IsNullOrEmpty(employeeLeaveGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("PERLEAVE GUID not found for arrangement: '", source.EmployeeLeaveId, "'"), "GUID.Not.Found"
                            , source.Id, source.Guid);
                    }
                    else
                    {
                        employeeLeaveTransactions.EmployeeLeave = new GuidObject2(employeeLeaveGuid);
                    }
                }
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
    }
}