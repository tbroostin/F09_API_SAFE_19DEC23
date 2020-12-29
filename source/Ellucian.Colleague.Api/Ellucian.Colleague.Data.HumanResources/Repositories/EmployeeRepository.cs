/* Copyright 2016-2019 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EmployeeRepository : BaseColleagueRepository, IEmployeeRepository
    {
        private readonly int bulkReadSize;
        private const string AllEmployeesCache = "AllEmployeesCache";
        private const int AllEmployeesCacheTimeout = 20; // Clear from cache every 20 minutes

        public EmployeeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        #region public methods
        /// <summary>
        /// Gets a list of employee keys
        /// </summary>
        /// <param name="ids">Optional: List of Ids to limit the query to</param>
        /// <param name="hasOnlineEarningsStatementConsentFilter">Optional: Online Earnings Statement Consent filter</param>
        /// <param name="includeNonEmployees">Boolean to include non-employees in query</param>
        /// <param name="activeOnly">Boolean to return only active users</param>
        /// <returns>List of keys for employees</returns>
        public async Task<IEnumerable<string>> GetEmployeeKeysAsync(IEnumerable<string> ids = null,
            bool? hasOnlineEarningsStatementConsentFilter = null, bool includeNonEmployees = false, bool activeOnly = false)
        {

            string fileName = string.Empty;
            string criteria = string.Empty;

            if (hasOnlineEarningsStatementConsentFilter.HasValue && includeNonEmployees)
            {
                var message = "hasOnlineEarningStatementConsent is an invalid parameter for a " + fileName + " lookup.";
                logger.Error(message);
                throw new ArgumentException(message);
            }

            try
            {

                // Selects all employees/non-employees
                if (includeNonEmployees)
                {

                    fileName = "HRPER";

                    //Selects only active employees/non-employees
                    if (activeOnly)
                    {
                        criteria = " WITH HRP.ACTIVE.STATUS NE '' ";
                    }

                }

                //Selects employees only
                else
                {
                    fileName = "EMPLOYES";

                    if (hasOnlineEarningsStatementConsentFilter.HasValue)
                    {
                        criteria = hasOnlineEarningsStatementConsentFilter.Value ?
                            " WITH EMP.VIEW.CHK.ADV.ONLINE EQ 'Y' " :
                            " WITH EMP.VIEW.CHK.ADV.ONLINE NE 'Y' ";
                    }

                    //Selects for active employees
                    if (activeOnly)
                    {
                        criteria += " WITH EMP.ACTIVE.STATUS NE '' ";
                    }
                }

                var limitingKeys = ids == null ? new string[0] : ids.ToArray();
                return await DataReader.SelectAsync(fileName, limitingKeys, criteria.Trim());

            }
            catch (Exception e)
            {
                var message = "Unable to get keys to" + fileName + " file";
                logger.Error(e, message);
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Get Employees objects for all employees bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractTypeCodes">Contract types filter.</param>
        /// <param name="contractDetailTypeCodes">Contract detail filter.</param>
        /// <returns>Tuple of Employee Entity objects <see cref="Employee"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>> GetEmployeesAsync(int offset, int limit, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
            IEnumerable<string> contractTypeCodes = null, string contractDetailTypeCode = "")
        {
            try
            {
                var employeeEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>();

                // Read Colleague-specified default values for:
                // - list of benefits to exclude for consideration of employee benefit status
                // - list of HR.STATUSES indication a leave status.
                var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
                // Get HR statuses that allow employees to be added to positions
                var hrStatuses = await GetHrStatusesAsync(true);
                string perStatValues = string.Empty;
                var perstatEmployeeValues = new List<string>();//list for merging with contract.type or contract.detail.id filter
                foreach (var hrStatus in hrStatuses)
                {
                    if (hrStatus != null && hrStatus.IsEmployeeStatus)
                    {
                        perstatEmployeeValues.Add(hrStatus.Code);
                        perStatValues = string.Concat(perStatValues, "'", hrStatus.Code, "'");
                    }
                }
                if (perstatEmployeeValues == null)
                {
                    return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                }

                var criteria = string.Empty;
                var perstatCriteria = string.Empty;
                var perstatContractTypeValues = new List<string>();
                string[] perstatFilteredKeys = null;
                string[] employeeKeys = null;

                if (!string.IsNullOrEmpty(person))
                {
                    // Get list of valid PERSTAT for person ID from filter
                    perstatFilteredKeys = await DataReader.SelectAsync("HRPER", new string[] { person }, "WITH ALL.STATUSES BY.EXP ALL.STATUSES SAVING ALL.STATUSES");
                    if (!perstatFilteredKeys.Any())
                    {
                        return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                    }
                }

                if (contractTypeCodes != null && contractTypeCodes.Any())
                {
                    // save status from contract.type filter for later comparison against all employee statuses
                    foreach (var contractTypeCode in contractTypeCodes)
                    {
                        perstatContractTypeValues.Add(contractTypeCode);
                    }
                }
                if (!string.IsNullOrWhiteSpace(contractDetailTypeCode))
                {
                    // incoming detail type code must be in the list of valid ones for an employee (special processing 1 = "Y" in HR.STATUSES)
                    if (perStatValues.Contains(contractDetailTypeCode))
                    {
                        // if user provided contract.type as well as contract.detail.id, we need to make sure
                        // contract.detail.id is in the translated list in persatContractTypeValues
                        if (perstatContractTypeValues != null && perstatContractTypeValues.Any())
                        {
                            if (perstatContractTypeValues.Contains(contractDetailTypeCode))
                            {
                                perstatContractTypeValues = new List<string>() { contractDetailTypeCode };
                            }
                            else
                            {
                                return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                            }
                        }
                        else
                        {
                            // user provided contract.detail.id without contract.type.  So we have a list of one value
                            perstatContractTypeValues = new List<string>() { contractDetailTypeCode };
                        }
                    }
                    else
                    {
                        // incoming detail type code is not valid for an employee (does not have special processing 1 = "Y" in HR.STATUSES)
                        return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                    }
                }
                if (perstatContractTypeValues != null && perstatContractTypeValues.Any())
                {
                    var perstatTypeValuesIntersect = perstatEmployeeValues.ToArray().Intersect(perstatContractTypeValues.ToArray().ToList());

                    perStatValues = string.Empty;
                    foreach (var perstatTypeValue in perstatTypeValuesIntersect)
                    {
                        perStatValues = string.Concat(perStatValues, "'", perstatTypeValue, "'");
                    }
                    if (perStatValues == null)
                    {
                        {
                            return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                        }
                    }
                    // select perstsat via HRPER because we need to limit to perstat based HRP.ACTIVE.STATUS   
                    if (!string.IsNullOrEmpty(person))
                    {
                        perstatFilteredKeys = await DataReader.SelectAsync("HRPER", new string[] { person }, "WITH HRP.ACTIVE.STATUS SAVING HRP.ACTIVE.STATUS");
                    }
                    else
                    {
                        perstatFilteredKeys = await DataReader.SelectAsync("HRPER", "WITH HRP.ACTIVE.STATUS SAVING HRP.ACTIVE.STATUS");
                    }

                    if (!perstatFilteredKeys.Any())
                    {
                        return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                    }

                }

                // select PERSTAT retreiving HRPER based on perStatValues (valid employee statuses possibly reduced by a contract filter)
                perstatCriteria = string.Concat(perstatCriteria, string.Format("WITH PERSTAT.HRP.ID NE '' AND WITH PERSTAT.STATUS = {0} SAVING UNIQUE PERSTAT.HRP.ID", perStatValues));
                employeeKeys = await DataReader.SelectAsync("PERSTAT", perstatFilteredKeys, perstatCriteria);

                var hrperCriteria = "";
                if (!string.IsNullOrEmpty(campus))
                {
                    hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.PRI.CAMPUS.LOCATION EQ '", campus, "'");
                }
                if (!string.IsNullOrEmpty(status))
                {
                    var today = await GetUnidataFormatDateAsync(DateTime.Now);
                    if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(hrperCriteria))
                            hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.TERM.DATE NE '' AND HRP.EFFECT.TERM.DATE LT '", today, "'");
                        else
                            hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.TERM.DATE NE '' AND HRP.EFFECT.TERM.DATE LT '", today, "'");
                    }

                    if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Leave.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        string leaveCodes = "";
                        if (ldmDefaults != null)
                        {
                            var leaveCodeIds = ldmDefaults.LdmdLeaveStatusCodes;

                            foreach (var leaveCodeId in leaveCodeIds)
                            {
                                leaveCodes = string.Concat(leaveCodes, "'", leaveCodeId, "'");
                            }
                        }
                        if (leaveCodes != "")
                        {
                            if (string.IsNullOrEmpty(hrperCriteria))
                                hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.CURRENT.STATUS EQ ", leaveCodes);
                            else
                                hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.CURRENT.STATUS EQ ", leaveCodes);
                        }
                    }

                    if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(hrperCriteria))
                            hrperCriteria = string.Concat(hrperCriteria, "WITH (HRP.EFFECT.TERM.DATE EQ '' OR HRP.EFFECT.TERM.DATE GE '", today, "')");
                        else
                            hrperCriteria = string.Concat(hrperCriteria, " AND WITH (HRP.EFFECT.TERM.DATE EQ '' OR HRP.EFFECT.TERM.DATE GE '", today, "')");

                        if (ldmDefaults != null)
                        {
                            var leaveCodeIds = ldmDefaults.LdmdLeaveStatusCodes;

                            foreach (var leaveCodeId in leaveCodeIds)
                            {
                                hrperCriteria = string.Concat(hrperCriteria, "AND WITH HRP.CURRENT.STATUS NE '", leaveCodeId, "'");
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(startOn))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.EMPLOY.DATE EQ '", startOn, "'");
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.EMPLOY.DATE EQ '", startOn, "'");
                }
                if (!string.IsNullOrEmpty(endOn))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.TERM.DATE EQ '", endOn, "'");
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.TERM.DATE EQ '", endOn, "'");
                }
                if (!string.IsNullOrEmpty(rehireableStatusEligibility))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.REHIRE.ELIGIBILITY EQ ", rehireableStatusEligibility);
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.REHIRE.ELIGIBILITY EQ ", rehireableStatusEligibility);
                }
                if (!string.IsNullOrEmpty(rehireableStatusType))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.REHIRE.ELIGIBILITY EQ '", rehireableStatusType, "'");
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.REHIRE.ELIGIBILITY EQ '", rehireableStatusType, "'");
                }

                if (employeeKeys != null && employeeKeys.Any())
                {
                    var hrperKeys = await DataReader.SelectAsync("HRPER", hrperCriteria);
                    if (hrperKeys.Any())
                    {
                        employeeKeys = employeeKeys.Intersect(hrperKeys).ToArray();
                    }
                    else
                    {
                        employeeKeys = null;
                    }
                }

                var employeeRecords = new List<Employes>();
                var totalCount = 0;
                var perposRecords = new List<Perpos>();
                var perposwgRecords = new List<Perposwg>();
                var perstatRecords = new List<Perstat>();
                var perbenRecords = new List<Perben>(); var hrperRecords = new List<Hrper>();

                if (employeeKeys != null && employeeKeys.Any())
                {
                    totalCount = employeeKeys.Count();

                    Array.Sort(employeeKeys);

                    var employeeSubList = employeeKeys.Skip(offset).Take(limit).ToArray();

                    // bulkread all HRPER and EMPLOYES records from Colleauge
                    for (int i = 0; i < employeeSubList.Count(); i += bulkReadSize)
                    {
                        var subList = employeeSubList.Skip(i).Take(bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                        if (records != null)
                        {
                            hrperRecords.AddRange(records);
                        }

                        var otherRecords = await DataReader.BulkReadRecordAsync<Employes>(subList.ToArray());
                        if (otherRecords != null)
                        {
                            employeeRecords.AddRange(otherRecords);
                        }
                    }

                    // select all the PERPOS ids with the HRP.ID equal to the input person id.
                    criteria = "WITH PERPOS.HRP.ID EQ ?";
                    var perposKeys = await DataReader.SelectAsync("PERPOS", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                    //bulkread the records for all the keys
                    for (int i = 0; i < perposKeys.Count(); i += bulkReadSize)
                    {
                        var subList = perposKeys.Skip(i).Take(bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                        if (records != null)
                        {
                            perposRecords.AddRange(records);
                        }
                    }

                    // select all the PERPOSWG ids with the HRP.ID equal to the input person id.
                    criteria = "WITH PPWG.HRP.ID EQ ?";
                    var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                    //bulkread the records for all the keys
                    for (int i = 0; i < perposwgKeys.Count(); i += bulkReadSize)
                    {
                        var subList = perposwgKeys.Skip(i).Take(bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                        if (records != null)
                        {
                            perposwgRecords.AddRange(records);
                        }
                    }

                    // select all the PERSTAT ids with the HRP.ID equal to the input person id.
                    criteria = "WITH PERSTAT.HRP.ID EQ ?";
                    var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                    for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
                    {
                        var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                        if (records != null)
                        {
                            perstatRecords.AddRange(records);
                        }
                    }

                    // select all the PERBEN ids with the HRP.ID equal to the input person id.
                    criteria = "WITH PERBEN.HRP.ID EQ ?";
                    var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                    for (int i = 0; i < perbenKeys.Count(); i += bulkReadSize)
                    {
                        var subList = perbenKeys.Skip(i).Take(bulkReadSize);
                        var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                        if (records != null)
                        {
                            perbenRecords.AddRange(records);
                        }
                    }
                }

                //build the Employee objects
                if (hrperRecords.Any())
                {
                    foreach (var hrPersonRecord in hrperRecords)
                    {
                        if (hrPersonRecord != null)
                        {
                            try
                            {
                                var employeeRecord = employeeRecords.FirstOrDefault(e => e.Recordkey == hrPersonRecord.Recordkey);
                                var personPositionRecords = perposRecords.Where(pp => pp.PerposHrpId == hrPersonRecord.Recordkey);
                                var personPositionWages = perposwgRecords.Where(ppw => ppw.PpwgHrpId == hrPersonRecord.Recordkey);
                                var personStatusRecords = perstatRecords.Where(ps => ps.PerstatHrpId == hrPersonRecord.Recordkey);
                                var personBenefitsRecords = perbenRecords.Where(pb => pb.PerbenHrpId == hrPersonRecord.Recordkey);
                                employeeEntities.Add(
                                    BuildEmployee(hrPersonRecord, employeeRecord, personPositionRecords,
                                    personPositionWages, personStatusRecords, personBenefitsRecords, ldmDefaults));
                            }
                            catch (Exception e)
                            {
                                LogDataError("Employees", hrPersonRecord.Recordkey, hrPersonRecord, e, e.Message);
                            }
                        }
                    }
                }

                return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, totalCount);
            }

            catch (RepositoryException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get Employees objects for all employees bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable status type filter.</param>
        /// <param name="contractTypeCodes">Contract type filters.</param>
        /// <param name="contractDetailTypeCode">Contract detail filter.</param>
        /// <returns>Tuple of Employee Entity objects <see cref="Employee"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Domain.HumanResources.Entities.Employee>, int>> GetEmployees2Async(int offset, int limit, string person = "",
            string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
            IEnumerable<string> contractTypeCodes = null, string contractDetailTypeCode = "")
        {
           
            var employeeEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>();
            int totalCount = 0;
            var criteria = string.Empty;
            //string[] subList = null;
            // Read Colleague-specified default values for:
            // - list of benefits to exclude for consideration of employee benefit status
            // - list of HR.STATUSES indication a leave status.
            var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            // Get HR statuses that allow employees to be added to positions
            string employeesCacheKey = CacheSupport.BuildCacheKey(AllEmployeesCache, person, campus, status, startOn, endOn, rehireableStatusEligibility,
                rehireableStatusType, contractTypeCodes, contractDetailTypeCode);
            try
            {

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    employeesCacheKey,
                    "",
                     offset,
                    limit,
                    AllEmployeesCacheTimeout,

                    async () =>
                    {
                        var keys = await GetEmployeesFiltersAsync(ldmDefaults, person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType, contractTypeCodes, contractDetailTypeCode);

                        if (keys == null || !keys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true} ;
                        }

                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = keys.Distinct().ToList(),
                            criteria = "",
                        };

                        return requirements;
                });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Domain.HumanResources.Entities.Employee>, int>(employeeEntities, 0);
                }

                var employeeSubList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;

                var employeeRecords = new List<Employes>();

                var perposRecords = new List<Perpos>();
                var perposwgRecords = new List<Perposwg>();
                var perstatRecords = new List<Perstat>();
                var perbenRecords = new List<Perben>(); var hrperRecords = new List<Hrper>();

                // bulkread all HRPER and EMPLOYES records from Colleauge
                for (int i = 0; i < employeeSubList.Count(); i += bulkReadSize)
                {
                    var subList = employeeSubList.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                    if (records != null)
                    {
                        hrperRecords.AddRange(records);
                    }

                    var otherRecords = await DataReader.BulkReadRecordAsync<Employes>(subList.ToArray());
                    if (otherRecords != null)
                    {
                        employeeRecords.AddRange(otherRecords);
                    }
                }

                // select all the PERPOS ids with the HRP.ID equal to the input person id.
                criteria = "WITH PERPOS.HRP.ID EQ ?";
                var perposKeys = await DataReader.SelectAsync("PERPOS", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                //bulkread the records for all the keys
                for (int i = 0; i < perposKeys.Count(); i += bulkReadSize)
                {
                    var subList = perposKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                    if (records != null)
                    {
                        perposRecords.AddRange(records);
                    }
                }

                // select all the PERPOSWG ids with the HRP.ID equal to the input person id.
                criteria = "WITH PPWG.HRP.ID EQ ?";
                var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                //bulkread the records for all the keys
                for (int i = 0; i < perposwgKeys.Count(); i += bulkReadSize)
                {
                    var subList = perposwgKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                    if (records != null)
                    {
                        perposwgRecords.AddRange(records);
                    }
                }

                // select all the PERSTAT ids with the HRP.ID equal to the input person id.
                criteria = "WITH PERSTAT.HRP.ID EQ ?";
                var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
                {
                    var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                    if (records != null)
                    {
                        perstatRecords.AddRange(records);
                    }
                }

                // select all the PERBEN ids with the HRP.ID equal to the input person id.
                criteria = "WITH PERBEN.HRP.ID EQ ?";
                var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria, employeeSubList.Select(id => string.Format("\"{0}\"", id)).ToArray());

                for (int i = 0; i < perbenKeys.Count(); i += bulkReadSize)
                {
                    var subList = perbenKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                    if (records != null)
                    {
                        perbenRecords.AddRange(records);
                    }
                }


                //build the Employee objects
                if (hrperRecords.Any())
                {
                    foreach (var hrPersonRecord in hrperRecords)
                    {
                        if (hrPersonRecord != null)
                        {
                            try
                            {
                                var employeeRecord = employeeRecords.FirstOrDefault(e => e.Recordkey == hrPersonRecord.Recordkey);
                                var personPositionRecords = perposRecords.Where(pp => pp.PerposHrpId == hrPersonRecord.Recordkey);
                                var personPositionWages = perposwgRecords.Where(ppw => ppw.PpwgHrpId == hrPersonRecord.Recordkey);
                                var personStatusRecords = perstatRecords.Where(ps => ps.PerstatHrpId == hrPersonRecord.Recordkey);
                                var personBenefitsRecords = perbenRecords.Where(pb => pb.PerbenHrpId == hrPersonRecord.Recordkey);
                                employeeEntities.Add(
                                    BuildEmployee2(hrPersonRecord, employeeRecord, personPositionRecords,
                                    personPositionWages, personStatusRecords, personBenefitsRecords, ldmDefaults));
                            }
                            catch (Exception e)
                            {
                                LogDataError("Employees", hrPersonRecord.Recordkey, hrPersonRecord, e, e.Message);
                            }
                        }
                    }
                }

                return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeeEntities, totalCount);
            }

            catch (RepositoryException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        /// <summary>
        /// Get Employee object for the specified GUID.
        /// </summary>   
        /// <param name="guid">guid of the employees record.</param>
        /// <returns>Employee Entity <see cref="Employee"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> GetEmployeeByGuidAsync(string guid)
        {
            // Get HR statuses that allow employees to be added to positions
            var hrStatuses = await GetHrStatusesAsync(true);

            var guidLookUp = new GuidLookup(guid);
            var hrPersonRecord = await DataReader.ReadRecordAsync<Hrper>(guidLookUp);
            if (hrPersonRecord == null)
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("employee.guid", string.Format("No employee was found for guid '{0}'.", guid)));
                throw exception;
            }

            var personId = hrPersonRecord.Recordkey;

            //get the employeeRecord
            var employeeRecord = await DataReader.ReadRecordAsync<Employes>(personId);

            // select all the PERSTAT ids with the HRP.ID equal to the input person id.
            var criteria = string.Format("WITH PERSTAT.HRP.ID EQ '{0}'", personId);
            var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria);

            var perstatRecords = new List<Perstat>();
            for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
            {
                var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                if (records != null)
                {
                    perstatRecords.AddRange(records);
                }
            }
            // If no qualifying records selected from PERSTAT than this is not an employee
            bool IsEmployeeCode = false;
            foreach (var ps in perstatRecords)
            {
                var status = hrStatuses.FirstOrDefault(hr => hr.Code == ps.PerstatStatus);
                if (status != null && status.IsEmployeeStatus)
                {
                    IsEmployeeCode = true;
                }
            }
            if (!IsEmployeeCode)
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("employee.guid", string.Format("No employee was found for guid '{0}'.", guid)));
                throw exception;
            }

            // select all the PERPOS ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PERPOS.HRP.ID EQ '{0}'", personId);
            var perposKeys = await DataReader.SelectAsync("PERPOS", criteria);

            //bulkread the records for all the keys
            var perposRecords = new List<Perpos>();
            for (int i = 0; i < perposKeys.Count(); i += bulkReadSize)
            {
                var subList = perposKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (records != null)
                {
                    perposRecords.AddRange(records);
                }
            }

            // select all the PERPOSWG ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PPWG.HRP.ID EQ '{0}'", personId);
            var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria);

            //bulkread the records for all the keys
            var perposwgRecords = new List<Perposwg>();
            for (int i = 0; i < perposwgKeys.Count(); i += bulkReadSize)
            {
                var subList = perposwgKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                if (records != null)
                {
                    perposwgRecords.AddRange(records);
                }
            }

            // select all the PERBEN ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PERBEN.HRP.ID EQ '{0}'", personId);
            var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria);

            var perbenRecords = new List<Perben>();
            for (int i = 0; i < perbenKeys.Count(); i += bulkReadSize)
            {
                var subList = perbenKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                if (records != null)
                {
                    perbenRecords.AddRange(records);
                }
            }

            // Get list of benefits to exclude for consideration of employee benefit status
            var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            //build the Employee objects

            return BuildEmployee(hrPersonRecord, employeeRecord, perposRecords,
                    perposwgRecords, perstatRecords, perbenRecords, ldmDefaults);
        }

        /// <summary>
        /// Get Employee object for the specified GUID.
        /// </summary>   
        /// <param name="guid">guid of the employees record.</param>
        /// <returns>Employee Entity <see cref="Employee"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> GetEmployee2ByGuidAsync(string guid)
        {
            // Get HR statuses that allow employees to be added to positions
            var hrStatuses = await GetHrStatusesAsync(true);

            var guidLookUp = new GuidLookup(guid);
            var hrPersonRecord = await DataReader.ReadRecordAsync<Hrper>(guidLookUp);
            if (hrPersonRecord == null)
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("employee.guid", string.Format("No employee was found for guid '{0}'.", guid)));
                throw exception;
            }

            var personId = hrPersonRecord.Recordkey;

            //get the employeeRecord
            var employeeRecord = await DataReader.ReadRecordAsync<Employes>(personId);

            // select all the PERSTAT ids with the HRP.ID equal to the input person id.
            var criteria = string.Format("WITH PERSTAT.HRP.ID EQ '{0}'", personId);
            var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria);

            var perstatRecords = new List<Perstat>();
            for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
            {
                var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                if (records != null)
                {
                    perstatRecords.AddRange(records);
                }
            }
            // If no qualifying records selected from PERSTAT than this is not an employee
            bool IsEmployeeCode = false;
            foreach (var ps in perstatRecords)
            {
                var status = hrStatuses.FirstOrDefault(hr => hr.Code == ps.PerstatStatus);
                if (status != null && status.IsEmployeeStatus)
                {
                    IsEmployeeCode = true;
                }
            }
            if (!IsEmployeeCode)
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("employee.guid", string.Format("No employee was found for guid '{0}'.", guid)));
                throw exception;
            }

            // select all the PERPOS ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PERPOS.HRP.ID EQ '{0}'", personId);
            var perposKeys = await DataReader.SelectAsync("PERPOS", criteria);

            //bulkread the records for all the keys
            var perposRecords = new List<Perpos>();
            for (int i = 0; i < perposKeys.Count(); i += bulkReadSize)
            {
                var subList = perposKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (records != null)
                {
                    perposRecords.AddRange(records);
                }
            }

            // select all the PERPOSWG ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PPWG.HRP.ID EQ '{0}'", personId);
            var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria);

            //bulkread the records for all the keys
            var perposwgRecords = new List<Perposwg>();
            for (int i = 0; i < perposwgKeys.Count(); i += bulkReadSize)
            {
                var subList = perposwgKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                if (records != null)
                {
                    perposwgRecords.AddRange(records);
                }
            }

            // select all the PERBEN ids with the HRP.ID equal to the input person id.
            criteria = string.Format("WITH PERBEN.HRP.ID EQ '{0}'", personId);
            var perbenKeys = await DataReader.SelectAsync("PERBEN", criteria);

            var perbenRecords = new List<Perben>();
            for (int i = 0; i < perbenKeys.Count(); i += bulkReadSize)
            {
                var subList = perbenKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perben>(subList.ToArray());
                if (records != null)
                {
                    perbenRecords.AddRange(records);
                }
            }

            // Get list of benefits to exclude for consideration of employee benefit status
            var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            //build the Employee objects

            return BuildEmployee2(hrPersonRecord, employeeRecord, perposRecords,
                    perposwgRecords, perstatRecords, perbenRecords, ldmDefaults);
        }

        /// <summary>
        /// Update an existing employee v11
        /// </summary>
        /// <param name="employeeEntity">Employee entity to be updated</param>
        /// <returns>A employee entity object <see cref="Employee"/> in EEDM format</returns>
        public async Task<Domain.HumanResources.Entities.Employee> UpdateEmployee2Async(Domain.HumanResources.Entities.Employee employeeEntity)
        {
            if (employeeEntity == null)
                throw new ArgumentNullException("employeeEntity", "Must provide a employeeEntity to update.");
            if (string.IsNullOrEmpty(employeeEntity.Guid))
                throw new ArgumentNullException("employeeEntity", "Must provide the guid of the employeeEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var employeeId = await this.GetEmployeeIdFromGuidAsync(employeeEntity.Guid);

            if (!string.IsNullOrEmpty(employeeId))
            {
                var extendedDataTuple = GetEthosExtendedDataLists();
                var updateRequest = BuildEmployeeUpdateRequest(employeeEntity);
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)

                {

                    updateRequest.ExtendedNames = extendedDataTuple.Item1;

                    updateRequest.ExtendedValues = extendedDataTuple.Item2;

                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateEmployeeRequest, UpdateEmployeeResponse>(updateRequest);

                if (updateResponse.UpdateEmployeeErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating employee '{0}':", employeeEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.UpdateEmployeeErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetEmployee2ByGuidAsync(employeeEntity.Guid);
            }

            // perform a create instead
            return await CreateEmployee2Async(employeeEntity);
        }

        /// <summary>
        /// Create a new employee record v12
        /// </summary>
        /// <param name="employeeEntity">Employee entity to be updated</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        public async Task<Domain.HumanResources.Entities.Employee> CreateEmployee2Async(Domain.HumanResources.Entities.Employee employeeEntity)
        {
            if (employeeEntity == null)
                throw new ArgumentNullException("employeeEntity", "Must provide a employeeEntity to create.");

            var extendedDataTuple = GetEthosExtendedDataLists();

            var createRequest = BuildEmployeeUpdateRequest(employeeEntity);

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;

            }
            //createRequest.EmployeeId = null;
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateEmployeeRequest, UpdateEmployeeResponse>(createRequest);

            if (createResponse == null || string.IsNullOrEmpty(createResponse.EmployeeGuid))
            {
                var errorMessage = string.Format("Error(s) occurred creating employeesEntity '{0}':", employeeEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("EmploymentPerformanceReview", errorMessage));
                logger.Error(errorMessage);
                throw exception;
            }

            if (createResponse.UpdateEmployeeErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating new employee for person '{0}':", employeeEntity.PersonId);
                var exception = new RepositoryException(errorMessage);
                createResponse.UpdateEmployeeErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created entity from the database
            return await GetEmployee2ByGuidAsync(createResponse.EmployeeGuid);
        }

        /// <summary>
        /// Get the employee ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The employee ID</returns>
        public async Task<string> GetEmployeeIdFromGuidAsync(string guid)
        {
            var guidInfo = await GetRecordInfoFromGuidAsync(guid);
            if (guidInfo != null && guidInfo.Entity.ToUpperInvariant() == "HRPER")
            {
                return guidInfo.PrimaryKey;
            }
            return null;
        }
        #endregion

        #region private methods
        /// <summary>
        /// GetEmployeesFiltersAsync
        /// </summary>
        /// <param name="ldmDefaults"></param>
        /// <param name="person"></param>
        /// <param name="campus"></param>
        /// <param name="status"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="rehireableStatusEligibility"></param>
        /// <param name="rehireableStatusType"></param>
        /// <param name="contractTypeCodes"></param>
        /// <param name="contractDetailTypeCode"></param>
        /// <returns>collection of valid keys</returns>
        private async Task<List<string>> GetEmployeesFiltersAsync(LdmDefaults ldmDefaults, string person = "",
           string campus = "", string status = "", string startOn = "", string endOn = "", string rehireableStatusEligibility = "", string rehireableStatusType = "",
           IEnumerable<string> contractTypeCodes = null, string contractDetailTypeCode = "")
        {
            var employeeKeys = new List<string>();

            var hrStatuses = await GetHrStatusesAsync(true);
            string perStatValues = string.Empty;
            var perstatEmployeeValues = new List<string>();//list for merging with contract.type or contract.detail.id filter
            foreach (var hrStatus in hrStatuses)
            {
                if (hrStatus != null && hrStatus.IsEmployeeStatus)
                {
                    perstatEmployeeValues.Add(hrStatus.Code);
                    perStatValues = string.Concat(perStatValues, "'", hrStatus.Code, "'");
                }
            }
            if (perstatEmployeeValues == null)
            {
                return employeeKeys;
            }

            var perstatCriteria = string.Empty;
            var perstatContractTypeValues = new List<string>();
            string[] perstatFilteredKeys = null;

            if (!string.IsNullOrEmpty(person))
            {
                // Get list of valid PERSTAT for person ID from filter
                perstatFilteredKeys = await DataReader.SelectAsync("HRPER", new string[] { person }, "WITH ALL.STATUSES BY.EXP ALL.STATUSES SAVING ALL.STATUSES");
                if (!perstatFilteredKeys.Any())
                {
                    return employeeKeys;
                }
            }

            if (contractTypeCodes != null && contractTypeCodes.Any())
            {
                foreach (var contractTypeCode in contractTypeCodes)
                {
                    perstatContractTypeValues.Add(contractTypeCode);
                }
            }
            if (!string.IsNullOrWhiteSpace(contractDetailTypeCode))
            {
                // incoming detail type code must be in the list of valid ones for an employee (special processing 1 = "Y" in HR.STATUSES)
                if (perStatValues.Contains(contractDetailTypeCode))
                {
                    // if user provided contract.type as well as contract.detail.id, we need to make sure
                    // contract.detail.id is in the translated list in persatContractTypeValues
                    if (perstatContractTypeValues != null && perstatContractTypeValues.Any())
                    {
                        if (perstatContractTypeValues.Contains(contractDetailTypeCode))
                        {
                            perstatContractTypeValues = new List<string>() { contractDetailTypeCode };
                        }
                        else
                        {
                            return employeeKeys;
                        }
                    }
                    else
                    {
                        // user provided contract.detail.id without contract.type.  So we have a list of one value
                        perstatContractTypeValues = new List<string>() { contractDetailTypeCode };
                    }
                }
                else
                {
                    // incoming detail type code is not valid for an employee (does not have special processing 1 = "Y" in HR.STATUSES)
                    return employeeKeys;
                }
            }

            if (perstatContractTypeValues != null && perstatContractTypeValues.Any())
            {
                var perstatTypeValuesIntersect = perstatEmployeeValues.ToArray().Intersect(perstatContractTypeValues.ToArray().ToList());
                perStatValues = string.Empty;
                foreach (var perstatTypeValue in perstatTypeValuesIntersect)
                {
                    perStatValues = string.Concat(perStatValues, "'", perstatTypeValue, "'");
                }
                if (perStatValues == null)
                {
                    return employeeKeys;
                }
                // select perstsat via HRPER because we need to limit to perstat based HRP.ACTIVE.STATUS   
                if (!string.IsNullOrEmpty(person))
                {
                    perstatFilteredKeys = await DataReader.SelectAsync("HRPER", new string[] { person }, "WITH HRP.ACTIVE.STATUS SAVING HRP.ACTIVE.STATUS");
                }
                else
                {
                    perstatFilteredKeys = await DataReader.SelectAsync("HRPER", "WITH HRP.ACTIVE.STATUS SAVING HRP.ACTIVE.STATUS");
                }

                if (!perstatFilteredKeys.Any())
                {
                    return employeeKeys;
                }
            }

            // select PERSTAT based on perStatValues (valid employee statuses possibly reduced by a contract filter)
            perstatCriteria = string.Concat(perstatCriteria, string.Format("WITH PERSTAT.HRP.ID NE '' AND WITH PERSTAT.STATUS = {0} SAVING UNIQUE PERSTAT.HRP.ID", perStatValues));
            var employeeKeysArray = await DataReader.SelectAsync("PERSTAT", perstatFilteredKeys, perstatCriteria);

            if (employeeKeysArray != null && employeeKeysArray.Any())
            {
                employeeKeys = employeeKeysArray.ToList();
            }

            var hrperCriteria = "";
            if (!string.IsNullOrEmpty(campus))
            {
                hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.PRI.CAMPUS.LOCATION EQ '", campus, "'");
            }
            if (!string.IsNullOrEmpty(status))
            {
                var today = await GetUnidataFormatDateAsync(DateTime.Now);
                if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Terminated.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.TERM.DATE NE '' AND HRP.EFFECT.TERM.DATE LT '", today, "'");
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.TERM.DATE NE '' AND HRP.EFFECT.TERM.DATE LT '", today, "'");
                }

                if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Leave.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    string leaveCodes = "";
                    if (ldmDefaults != null)
                    {
                        var leaveCodeIds = ldmDefaults.LdmdLeaveStatusCodes;

                        foreach (var leaveCodeId in leaveCodeIds)
                        {
                            leaveCodes = string.Concat(leaveCodes, "'", leaveCodeId, "'");
                        }
                    }
                    if (leaveCodes != "")
                    {
                        if (string.IsNullOrEmpty(hrperCriteria))
                            hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.CURRENT.STATUS EQ ", leaveCodes);
                        else
                            hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.CURRENT.STATUS EQ ", leaveCodes);
                    }
                }

                if (string.Equals(status, Dtos.EnumProperties.EmployeeStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(hrperCriteria))
                        hrperCriteria = string.Concat(hrperCriteria, "WITH (HRP.EFFECT.TERM.DATE EQ '' OR HRP.EFFECT.TERM.DATE GE '", today, "')");
                    else
                        hrperCriteria = string.Concat(hrperCriteria, " AND WITH (HRP.EFFECT.TERM.DATE EQ '' OR HRP.EFFECT.TERM.DATE GE '", today, "')");

                    if (ldmDefaults != null)
                    {
                        var leaveCodeIds = ldmDefaults.LdmdLeaveStatusCodes;

                        foreach (var leaveCodeId in leaveCodeIds)
                        {
                            hrperCriteria = string.Concat(hrperCriteria, "AND WITH HRP.CURRENT.STATUS NE '", leaveCodeId, "'");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(startOn))
            {
                if (string.IsNullOrEmpty(hrperCriteria))
                    hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.EMPLOY.DATE EQ '", startOn, "'");
                else
                    hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.EMPLOY.DATE EQ '", startOn, "'");
            }
            if (!string.IsNullOrEmpty(endOn))
            {
                if (string.IsNullOrEmpty(hrperCriteria))
                    hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.EFFECT.TERM.DATE EQ '", endOn, "'");
                else
                    hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.EFFECT.TERM.DATE EQ '", endOn, "'");
            }
            if (!string.IsNullOrEmpty(rehireableStatusEligibility))
            {
                if (string.IsNullOrEmpty(hrperCriteria))
                    hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.REHIRE.ELIGIBILITY EQ ", rehireableStatusEligibility);
                else
                    hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.REHIRE.ELIGIBILITY EQ ", rehireableStatusEligibility);
            }
            if (!string.IsNullOrEmpty(rehireableStatusType))
            {
                if (string.IsNullOrEmpty(hrperCriteria))
                    hrperCriteria = string.Concat(hrperCriteria, "WITH HRP.REHIRE.ELIGIBILITY EQ '", rehireableStatusType, "'");
                else
                    hrperCriteria = string.Concat(hrperCriteria, " AND WITH HRP.REHIRE.ELIGIBILITY EQ '", rehireableStatusType, "'");
            }


            if (employeeKeys != null && employeeKeys.Any())
            {
                var hrperKeys = await DataReader.SelectAsync("HRPER", hrperCriteria);
                if (hrperKeys != null && hrperKeys.Any())
                {
                    employeeKeys = (employeeKeys.Intersect(hrperKeys)).ToList(); ;
                }
                else
                {
                    employeeKeys = null;
                }
            }

            return employeeKeys;
        }

        /// <summary>
        /// Helper to build PersonPosition objects
        /// </summary>
        /// <param name="employRecord">the Perpos db record</param>
        /// <returns></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.Employee BuildEmployee(Hrper hrPersonRecord,
            Employes employeeRecord,
            IEnumerable<Perpos> personPositionRecords,
            IEnumerable<Perposwg> personPositionWages,
            IEnumerable<Perstat> personStatusRecords,
            IEnumerable<Perben> personBenefitsRecords,
            LdmDefaults ldmDefaults
          )
        {
            Domain.HumanResources.Entities.Employee employeeEntity = null;
            var guid = hrPersonRecord.RecordGuid;
            var personId = hrPersonRecord.Recordkey;
            if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(personId))
            {
                // Build the Employees Entity from the gathered data.
                employeeEntity = new Domain.HumanResources.Entities.Employee(guid, personId);

                if (employeeRecord != null)
                {
                    employeeEntity.HasOnlineEarningsStatementConsent = employeeRecord.EmpViewChkAdvOnline != null && employeeRecord.EmpViewChkAdvOnline.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
                }

                // HRPER values for employee
                employeeEntity.Location = hrPersonRecord.HrpPriCampusLocation;
                employeeEntity.StartDate = hrPersonRecord.HrpEffectEmployDate;
                employeeEntity.EndDate = hrPersonRecord.HrpEffectTermDate;
                employeeEntity.RehireEligibilityCode = hrPersonRecord.HrpRehireEligibility;
                employeeEntity.EmploymentStatus = EmployeeStatus.Active;
                if (hrPersonRecord.HrpEffectTermDate != null && hrPersonRecord.HrpEffectTermDate <= DateTime.Today)
                {
                    employeeEntity.EmploymentStatus = EmployeeStatus.Terminated;
                }

                List<string> leaveCodes = new List<string>();
                if (ldmDefaults != null)
                {
                    leaveCodes = ldmDefaults.LdmdLeaveStatusCodes;
                }

                // PERSTAT records
                if (personStatusRecords.Any())
                {
                    foreach (var perstat in personStatusRecords)
                    {
                        if (perstat.PerstatEndDate == null || perstat.PerstatEndDate > DateTime.Today)
                        {
                            if (perstat.PerstatStartDate <= DateTime.Today)
                            {
                                employeeEntity.StatusCode = perstat.PerstatStatus;
                                if (employeeEntity.EmploymentStatus != EmployeeStatus.Terminated)
                                {
                                    if (leaveCodes.Contains(employeeEntity.StatusCode))
                                    {
                                        employeeEntity.EmploymentStatus = EmployeeStatus.Leave;
                                        break;
                                    }
                                }
                            }
                        }
                        if (hrPersonRecord.HrpEffectTermDate != null && hrPersonRecord.HrpEffectTermDate <= DateTime.Today)
                        {
                            employeeEntity.StatusEndReasonCode = perstat.PerstatEndReason;
                        }
                    }
                    if (string.IsNullOrEmpty(employeeEntity.StatusCode))
                    {
                        // If we didn't find an active PERSTAT record than
                        // Take the most recent status.
                        var perstat = personStatusRecords.OrderByDescending(i => i.PerstatEndDate).FirstOrDefault();
                        employeeEntity.StatusCode = perstat.PerstatStatus;
                    }
                }

                // PERPOSWG records
                employeeEntity.PayStatus = PayStatus.WithoutPay;
                var payPeriodHours = new List<decimal?>();
                if (personPositionWages.Any())
                {
                    foreach (var perwage in personPositionWages)
                    {
                        if (perwage.PpwgEndDate == null || perwage.PpwgEndDate > DateTime.Today)
                        {
                            employeeEntity.PayStatus = PayStatus.WithPay;
                            payPeriodHours.Add(perwage.PpwgCycleWorkTimeAmt);
                        }
                    }
                }
                employeeEntity.PayPeriodHours = payPeriodHours;
                // PERBEN records
                employeeEntity.BenefitsStatus = BenefitsStatus.WithoutBenefits;
                if (personBenefitsRecords.Any())
                {
                    List<string> excludeBenefits = new List<string>();
                    if (ldmDefaults != null)
                    {
                        excludeBenefits = ldmDefaults.LdmdExcludeBenefits;
                    }
                    foreach (var perben in personBenefitsRecords)
                    {
                        var benefit = perben.PerbenBdId;
                        // Make sure this benefit is not on the list for exclusion before using it to determine
                        // employee benefit status
                        if (excludeBenefits != null && !excludeBenefits.Contains(benefit))
                        {
                            if (perben.PerbenCancelDate == null || perben.PerbenCancelDate > DateTime.Today)
                            {
                                employeeEntity.BenefitsStatus = BenefitsStatus.WithBenefits;
                                break;
                            }
                        }
                    }
                }
            }
            return employeeEntity;
        }

        /// <summary>
        /// Helper to build PersonPosition objects
        /// </summary>
        /// <param name="employRecord">the Perpos db record</param>
        /// <returns></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.Employee BuildEmployee2(Hrper hrPersonRecord,
            Employes employeeRecord,
            IEnumerable<Perpos> personPositionRecords,
            IEnumerable<Perposwg> personPositionWages,
            IEnumerable<Perstat> personStatusRecords,
            IEnumerable<Perben> personBenefitsRecords,
            LdmDefaults ldmDefaults
          )
        {
            Domain.HumanResources.Entities.Employee employeeEntity = null;
            var guid = hrPersonRecord.RecordGuid;
            var personId = hrPersonRecord.Recordkey;
            if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(personId))
            {
                // Build the Employees Entity from the gathered data.
                employeeEntity = new Domain.HumanResources.Entities.Employee(guid, personId);

                if (employeeRecord != null)
                {
                    employeeEntity.HasOnlineEarningsStatementConsent = employeeRecord.EmpViewChkAdvOnline != null && employeeRecord.EmpViewChkAdvOnline.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
                }

                // HRPER values for employee
                employeeEntity.Location = hrPersonRecord.HrpPriCampusLocation;
                employeeEntity.StartDate = hrPersonRecord.HrpEffectEmployDate;
                employeeEntity.EndDate = hrPersonRecord.HrpEffectTermDate;
                employeeEntity.RehireEligibilityCode = hrPersonRecord.HrpRehireEligibility;
                employeeEntity.EmploymentStatus = EmployeeStatus.Active;
                if (hrPersonRecord.HrpEffectTermDate != null && hrPersonRecord.HrpEffectTermDate <= DateTime.Today)
                {
                    employeeEntity.EmploymentStatus = EmployeeStatus.Terminated;
                }

                List<string> leaveCodes = new List<string>();
                if (ldmDefaults != null)
                {
                    leaveCodes = ldmDefaults.LdmdLeaveStatusCodes;
                }

                // PERSTAT records
                if (personStatusRecords.Any())
                {
                    foreach (var perstat in personStatusRecords)
                    {
                        if (perstat.PerstatEndDate == null || perstat.PerstatEndDate > DateTime.Today)
                        {
                            if (perstat.PerstatStartDate <= DateTime.Today)
                            {
                                employeeEntity.StatusCode = perstat.PerstatStatus;
                                if (employeeEntity.EmploymentStatus != EmployeeStatus.Terminated)
                                {
                                    if (leaveCodes.Contains(employeeEntity.StatusCode))
                                    {
                                        employeeEntity.EmploymentStatus = EmployeeStatus.Leave;
                                        break;
                                    }
                                }
                                //v12 addition: Department
                                employeeEntity.PrimaryPosition = perstat.PerstatPrimaryPosId;
                            }
                        }
                        if (hrPersonRecord.HrpEffectTermDate != null && hrPersonRecord.HrpEffectTermDate <= DateTime.Today)
                        {
                            employeeEntity.StatusEndReasonCode = perstat.PerstatEndReason;
                        }
                    }
                    if (string.IsNullOrEmpty(employeeEntity.StatusCode))
                    {
                        // If we didn't find an active PERSTAT record than
                        // Take the most recent status.
                        var perstat = personStatusRecords.OrderByDescending(i => i.PerstatEndDate).FirstOrDefault();
                        employeeEntity.StatusCode = perstat.PerstatStatus;
                    }
                }

                // PERPOSWG records
                employeeEntity.PayStatus = PayStatus.WithoutPay;
                ////var payPeriodHours = new List<decimal?>();
                if (personPositionWages.Any())
                {
                    foreach (var perwage in personPositionWages)
                    {
                        if (perwage.PpwgEndDate == null || perwage.PpwgEndDate > DateTime.Today)
                        {
                            employeeEntity.PayStatus = PayStatus.WithPay;
                            //v12 NEW LOGIC FOR hoursPerPeriod property
                            if (!string.IsNullOrEmpty(perwage.PpwgCycleWorkTimeUnits) && perwage.PpwgCycleWorkTimeUnits.Equals("HRS"))
                            {
                                employeeEntity.PpwgCycleWorkTimeAmt = perwage.PpwgCycleWorkTimeAmt;
                            }
                            if (!string.IsNullOrEmpty(perwage.PpwgYearWorkTimeUnits) && perwage.PpwgYearWorkTimeUnits.Equals("HRS"))
                            {
                                employeeEntity.PpwgYearWorkTimeAmt = perwage.PpwgYearWorkTimeAmt;
                            }
                            //v12 addition: PayClass is just for primary positiohn
                            if (perwage.PpwgStartDate <= DateTime.Today && perwage.PpwgPositionId.Equals(employeeEntity.PrimaryPosition, StringComparison.OrdinalIgnoreCase))
                            {
                                employeeEntity.PayClass = perwage.PpwgPayclassId;
                            }
                        }
                    }
                }
                ////employeeEntity.PayPeriodHours = payPeriodHours;
                // PERBEN records
                employeeEntity.BenefitsStatus = BenefitsStatus.WithoutBenefits;
                if (personBenefitsRecords.Any())
                {
                    List<string> excludeBenefits = new List<string>();
                    if (ldmDefaults != null)
                    {
                        excludeBenefits = ldmDefaults.LdmdExcludeBenefits;
                    }
                    foreach (var perben in personBenefitsRecords)
                    {
                        var benefit = perben.PerbenBdId;
                        // Make sure this benefit is not on the list for exclusion before using it to determine
                        // employee benefit status
                        if (excludeBenefits == null || (excludeBenefits != null && !excludeBenefits.Contains(benefit)))
                        {
                            if (perben.PerbenCancelDate == null || perben.PerbenCancelDate > DateTime.Today)
                            {
                                employeeEntity.BenefitsStatus = BenefitsStatus.WithBenefits;
                                break;
                            }
                        }
                    }
                }
            }
            return employeeEntity;
        }
    

        /// <summary>
        /// Create an UpdateVouchersIntegrationRequest from an Employee domain entity
        /// </summary>
        /// <param name="employeeEntity">Employee domain entity</param>
        /// <returns>UpdateVouchersIntegrationRequest transaction object</returns>
        private UpdateEmployeeRequest BuildEmployeeUpdateRequest(Domain.HumanResources.Entities.Employee employeeEntity)
        {
            var request = new UpdateEmployeeRequest()
            {
                EmployeeGuid = employeeEntity.Guid,
                EmployeeId = employeeEntity.PersonId,
                EmployeePersonId = employeeEntity.PersonId,
                EmployeeStatus = employeeEntity.StatusCode,
                EmployeeLocation = employeeEntity.Location,
                EmployeePayClass = employeeEntity.PayClass,
                EmployeeStartDate = employeeEntity.StartDate,
                EmployeeEndDate = employeeEntity.EndDate,
                EmployeeRehireCode = employeeEntity.RehireEligibilityCode,
                EmployeeStatusEndReason = employeeEntity.StatusEndReasonCode
            };

            if (employeeEntity.BenefitsStatus.HasValue)
            {
                request.EmployeeBenefitStatus = employeeEntity.BenefitsStatus.ToString();
            }
            if (employeeEntity.PayStatus.HasValue)
            {
                request.EmployeePayStatus = employeeEntity.PayStatus.ToString();
            }
            if (employeeEntity.PayPeriodHours != null && employeeEntity.PayPeriodHours.Any())
            {
                request.EmployeePayPeriodHours = employeeEntity.PayPeriodHours.ToList();
            }

            return request;
        }

        /// <summary>
        /// Get a collection of HrStatuses with special processing 1 equal to Y.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of HrStatuses</returns>
        private async Task<IEnumerable<HrStatuses>> GetHrStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<HrStatuses>("HR", "HR.STATUSES",
                (cl, g) => new HrStatuses(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }

        #endregion
    }
}