/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayrollDeductionArrangementRepository : BaseColleagueRepository, IPayrollDeductionArrangementRepository
    {
        private Ellucian.Data.Colleague.DataContracts.IntlParams internationalParameters;
        private readonly int bulkReadSize;

        public PayrollDeductionArrangementRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        private async new Task<Ellucian.Data.Colleague.DataContracts.IntlParams> GetInternationalParametersAsync()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = await GetOrAddToCacheAsync<Ellucian.Data.Colleague.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Ellucian.Data.Colleague.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Ellucian.Data.Colleague.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new Exception(errorMessage);
                        Ellucian.Data.Colleague.DataContracts.IntlParams newIntlParams = new Ellucian.Data.Colleague.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        newIntlParams.HostCountry = "USA";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        /// <summary>
        /// Get the Host Country code from the INTL form parameter
        /// </summary>
        /// <returns>Returns a string with the host couuntry of USA or CANADA</returns>
        public async Task<string> GetHostCountryAsync()
        {
            var intlParams = await GetInternationalParametersAsync();
            return intlParams.HostCountry;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                return string.Empty;
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Format("No Payroll Deduction Arrangements was found for id '{0}'. ", guid));
            }

            if (foundEntry.Value.Entity != "PERBEN")
            {
                var errorMessage = string.Format("Id '{0}' has different entity, '{1}', than expected, PERBEN", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get PayrollDeductionArrangement objects for all payroll deduction arrangements.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="bypassCache">Bypass the cache and read directly from disk for all reads.</param>
        /// <param name="person">Employee for which this deduction applies.</param>
        /// <param name="contribution">Contribution reference from other system.</param>
        /// <param name="deductionType">Deduction Code used to identify the paroll deduction arrangement</param>
        /// <param name="status"></param>
        /// <returns>Tuple of PayrollDeductionArrangement Entity objects <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>> GetAsync(int offset, int limit, bool bypassCache = false, 
            string person = "", string contribution = "", string deductionType = "", string status = "")
        {
            try
            {
                var criteria = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria = string.Concat(criteria, " AND ");
                    }
                    criteria = string.Concat(criteria, "WITH PERBEN.HRP.ID EQ '", person, "'");
                }
                if (!string.IsNullOrEmpty(contribution))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria = string.Concat(criteria, " AND ");
                    }
                    criteria = string.Concat(criteria, "WITH PERBEN.INTG.CONTRIBUTION EQ '", contribution, "'");
                }
                if (!string.IsNullOrEmpty(deductionType))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria = string.Concat(criteria, " AND ");
                    }
                    criteria = string.Concat(criteria, "WITH PERBEN.BD.ID EQ '", deductionType, "'");
                }
                if (!string.IsNullOrEmpty(status))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria = string.Concat(criteria, " AND ");
                    }
                    var todaysDate = await GetUnidataFormatDateAsync(DateTime.Today);
                    if (status.ToLower() == "cancelled")
                    {
                        criteria = string.Concat(criteria, string.Format("WITH PERBEN.CANCEL.DATE NE '' AND WITH PERBEN.CANCEL.DATE LT '{0}'", todaysDate));
                    }
                    else
                    {
                        criteria = string.Concat(criteria, string.Format("WITH PERBEN.CANCEL.DATE EQ '' OR WITH PERBEN.CANCEL.DATE GE '{0}'", todaysDate));
                    }
                }
                
                string[] perbenKeys = null;
                string[] perbenCsKeys = null;
                var primaryKeys = await DataReader.SelectAsync("PERBEN", "WITH PERBEN.INTG.INTERVAL NE '' OR WITH PERBEN.INTG.MON.PAY.PERIODS NE ''");
                if (!string.IsNullOrEmpty(criteria))
                {
                    perbenKeys = await DataReader.SelectAsync("PERBEN", primaryKeys, criteria);
                } 
                else
                {
                    perbenKeys = primaryKeys;
                }

                var perbenRecords = new List<Perben>();
                var perbenCsRecords = new Collection<Perbencs>();
                var totalCount = 0;
                if (perbenKeys != null)
                {
                    totalCount = perbenKeys.Count();

                    Array.Sort(perbenKeys);

                    var perbenSubList = perbenKeys.Skip(offset).Take(limit).ToArray();

                    //bulkread the records for all the keys
                    var bulkRecords = await DataReader.BulkReadRecordAsync<Perben>(perbenSubList);
                    if (bulkRecords == null)
                    {
                        logger.Error("Unexpected null from bulk read of PERBEN records");
                    }
                    else
                    {
                        perbenRecords.AddRange(bulkRecords);
                    }
                    perbenCsKeys = perbenRecords.SelectMany(pb => pb.AllBenefitCosts).ToArray();
                    perbenCsRecords = await DataReader.BulkReadRecordAsync<Perbencs>(perbenCsKeys);
                }
                
                //build the Employee objects
                var payrollDeductionArrangementEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>();
                if (perbenRecords.Any())
                {
                    foreach (var personBenefitsRecord in perbenRecords)
                    {
                        if (personBenefitsRecord != null)
                        {
                            try
                            {
                                var perbenCsSubset = perbenCsRecords.Where(pb => pb.PbcBdId == personBenefitsRecord.PerbenBdId && pb.PbcHrpId == personBenefitsRecord.PerbenHrpId);
                                Perbencs activePerbenCs = perbenCsSubset.FirstOrDefault();
                                foreach (var perbenCs in perbenCsSubset)
                                {
                                    // Find the Active PERBENCS record
                                    var endDate = perbenCs.PbcEndDate;
                                    if (!endDate.HasValue || endDate.Value <= DateTime.Today)
                                    {
                                        activePerbenCs = perbenCs;
                                    }
                                }
                                payrollDeductionArrangementEntities.Add(
                                    BuildPayrollDeductionArrangement(personBenefitsRecord, activePerbenCs));
                            }
                            catch (Exception e)
                            {
                                LogDataError("personBenefitsRecords", personBenefitsRecord.Recordkey, personBenefitsRecord, e, e.Message);
                                throw e;
                            }
                        }
                    }
                }
                return new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, totalCount);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get PayrollDeductionArrangement objects for a single id.
        /// </summary>   
        /// <param name="id">guid of the employees record.</param>
        /// <returns>PayrollDeductionArrangement Entity <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements> GetByIdAsync(string id)
        {
            var perbenId = await GetIdFromGuidAsync(id); 
            if (string.IsNullOrEmpty(perbenId))
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("id", string.Format("No Payroll Deduction Arrangement was found for id '{0}'. ", id)));
                throw exception;
            }
            var perbenRecord = await DataReader.ReadRecordAsync<Perben>(perbenId);
            if (perbenRecord == null)
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("id", string.Format("The Payroll Deduction Arrangement record for id '{0}' is not valid.", id)));
                throw exception;
            }
            if (!perbenRecord.PerbenIntgInterval.HasValue && !perbenRecord.PerbenIntgMonPayPeriods.Any())
            {
                var exception = new RepositoryException();
                exception.AddError(new Domain.Entities.RepositoryError("id", string.Format("The record for id '{0}' is not a valid Payroll Deduction Arrangement.", id)));
                throw exception;
            }

            var perbenCsKeys = perbenRecord.AllBenefitCosts.ToArray();
            var perbenCsRecords = await DataReader.BulkReadRecordAsync<Perbencs>(perbenCsKeys);
            Perbencs activePerbenCs = perbenCsRecords.FirstOrDefault();
            foreach (var perbenCs in perbenCsRecords)
            {
                // Find the Active PERBENCS record
                var endDate = perbenCs.PbcEndDate;
                if (!endDate.HasValue || endDate.Value <= DateTime.Today)
                {
                    activePerbenCs = perbenCs;
                }
            }

            //build the PayrollDeductionArrangement object
            return BuildPayrollDeductionArrangement(perbenRecord, activePerbenCs);
        }

        /// <summary>
        /// Update an existing PERBEN record for an employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payrollDeductionArrangement">Payroll Deduction Arrangement object</param>
        /// <returns>PayrollDeductionArrangement object <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/></returns>
        public async Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> UpdateAsync(string id, Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "The id is required when updating a Payroll Deduction Arrangment. ");
            }
            return await CreatePayrollDeductionArrangement(payrollDeductionArrangement);
        }

        /// <summary>
        /// Create a new PERBEN record for an employee
        /// </summary>
        /// <param name="payrollDeductionArrangement">Payroll Deduction Arrangement object</param>
        /// <returns>PayrollDeductionArrangement object <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/></returns>
        public async Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> CreateAsync(Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            if (!string.IsNullOrEmpty(payrollDeductionArrangement.Guid))
            {
                ////Guid reqdness HEDM-2628, 00000000-0000-0000-0000-000000000000 should not be validated
                if (!payrollDeductionArrangement.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(payrollDeductionArrangement.Guid);
                    if (recordInfo != null)
                    {
                        throw new InvalidOperationException(string.Format("Payroll Deduction Arrangement record {0} already exists.", payrollDeductionArrangement.Guid));
                    }
                }
            }
            return await CreatePayrollDeductionArrangement(payrollDeductionArrangement);
        }

        /// <summary>
        /// Helper to build PayrollDeductionArrangement object
        /// </summary>
        /// <param name="personBenefitsRecord">the PERBEN record from the database</param>
        /// <returns>PayrollDeductionArrangement Object <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements BuildPayrollDeductionArrangement(Perben personBenefitsRecord, Perbencs perbenCsRecord)
        {
            Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangementEntity = null;
            var guid = personBenefitsRecord.RecordGuid;
            var id = personBenefitsRecord.Recordkey;
            var personId = personBenefitsRecord.PerbenHrpId;
            if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(personId))
            {
                // Build the PayrollDeductionArrangement Entity from the gathered data.
                payrollDeductionArrangementEntity = new Domain.HumanResources.Entities.PayrollDeductionArrangements(guid, personId);
                payrollDeductionArrangementEntity.Id = id;
                payrollDeductionArrangementEntity.CommitmentContributionId = personBenefitsRecord.PerbenIntgContribution;
                payrollDeductionArrangementEntity.CommitmentType = personBenefitsRecord.PerbenIntgCommitmentType;
                payrollDeductionArrangementEntity.DeductionTypeCode = personBenefitsRecord.PerbenBdId;
                payrollDeductionArrangementEntity.Status = (personBenefitsRecord.PerbenCancelDate != null && personBenefitsRecord.PerbenCancelDate.HasValue && personBenefitsRecord.PerbenCancelDate.Value.Date < DateTime.Today.Date ? "Cancelled" : "Active");
                payrollDeductionArrangementEntity.StartDate = personBenefitsRecord.PerbenEnrollDate;
                payrollDeductionArrangementEntity.EndDate = personBenefitsRecord.PerbenCancelDate;
                payrollDeductionArrangementEntity.Interval = personBenefitsRecord.PerbenIntgInterval;
                payrollDeductionArrangementEntity.MonthlyPayPeriods = personBenefitsRecord.PerbenIntgMonPayPeriods;
                payrollDeductionArrangementEntity.ChangeReason = personBenefitsRecord.PerbenChangeReasons != null &&
                    personBenefitsRecord.PerbenChangeReasons.Any() ? personBenefitsRecord.PerbenChangeReasons.ElementAt(0) : string.Empty;


                if (perbenCsRecord != null)
                {
                    payrollDeductionArrangementEntity.AmountPerPayment = perbenCsRecord.PbcEmplyePayCost;
                    payrollDeductionArrangementEntity.TotalAmount = perbenCsRecord.PbcEmplyeLimitAmt;
                }
            }

            return payrollDeductionArrangementEntity;
        }

        private async Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> CreatePayrollDeductionArrangement(Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangement)
        {
            var request = new CreateUpdatePerbenRequest()
            {   
                Guid = payrollDeductionArrangement.Guid,
                PerbenId = payrollDeductionArrangement.Id,
                PersonId = payrollDeductionArrangement.PersonId,
                ContributionId = payrollDeductionArrangement.CommitmentContributionId,
                CommitmentType = payrollDeductionArrangement.CommitmentType,
                DeductionCode = payrollDeductionArrangement.DeductionTypeCode,
                Status = payrollDeductionArrangement.Status,
                AmountPerPayment = payrollDeductionArrangement.AmountPerPayment,
                TotalAmount = payrollDeductionArrangement.TotalAmount,
                StartDate = payrollDeductionArrangement.StartDate.HasValue ? payrollDeductionArrangement.StartDate : null,
                EndDate = payrollDeductionArrangement.EndDate.HasValue ? payrollDeductionArrangement.EndDate : null,
                Interval = payrollDeductionArrangement.Interval,
                MonthlyPayPeriods = payrollDeductionArrangement.MonthlyPayPeriods,
                ChangeReason = payrollDeductionArrangement.ChangeReason
            };

            ////Guid reqdness HEDM-2628, since transaction doesn't support 00000000-0000-0000-0000-000000000000, we have to assign empty string
            if (request.Guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                request.Guid = string.Empty;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdatePerbenRequest, CreateUpdatePerbenResponse>(request);

            // If there is any error message - throw an exception 
            if (!string.IsNullOrEmpty(updateResponse.ErrorOccurred))
            {
                var errorMessage = string.Format("Error(s) occurred updating payroll-deduction-arrangements for id: '{0}'.", request.Guid);
                var exception = new RepositoryException(errorMessage);
                foreach (var errMsg in updateResponse.PerbenUpdateErrors)
                {
                    exception.AddError(new RepositoryError(errMsg.ErrorCodes, errMsg.ErrorMessages));
                    errorMessage += string.Join(Environment.NewLine, errMsg.ErrorMessages);
                }
                logger.Error(errorMessage.ToString());
                throw exception;
            }

            return await GetByIdAsync(updateResponse.Guid);
        }
    }
}
