// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the ILedgerActivityRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class LedgerActivityRepository : BaseColleagueRepository, ILedgerActivityRepository
    {
        private const string AllLedgerActivitiesCache = "AllLedgerActivities";
        private const int AllLedgerActivitiesCacheTimeout = 20; // Clear from cache every 20 minutes
        private string institutionName = string.Empty;
        private ApplValcodes sourceCodes = null;
        private string hostCountry = null;
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters = null;

        /// <summary>
        /// The constructor to instantiate a LedgerActivity repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public LedgerActivityRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        #region public methods

        /// <summary>
        /// Gets general ledger activities.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="fiscalYear"></param>
        /// <param name="fiscalPeriod"></param>
        /// <param name="reportingSegment"></param>
        /// <param name="transactionDate"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<GeneralLedgerActivity>, int>> GetGlaFyrAsync(int offset, int limit, string fiscalYear, string fiscalPeriod, string fiscalPeriodYear, string reportingSegment, string transactionDate)
        {
            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("Fiscal year is required.");
            }
            string glaFyrFileName = string.Format("GLA.{0}", fiscalYear);
            string glpFyrFileName = string.Format("GLP.{0}", fiscalYear);

            Collection<GlaFyr> glaDataContracts = null;
            Collection<GlpFyr> glpDataContracts = null;
            int totalCount = 0;

            string glCriteria = string.Empty;

            List<GlaFyr> glaFyrFiltered = null;

            if (!string.IsNullOrEmpty(reportingSegment))
            {
                var repSegment = await BuildReportingSegment();
                if (!repSegment.Equals(reportingSegment, StringComparison.OrdinalIgnoreCase))
                {
                    // throw new KeyNotFoundException(string.Format("Reporting segment not found for {0}.", reportingSegment));
                    return new Tuple<IEnumerable<GeneralLedgerActivity>, int>(new List<GeneralLedgerActivity>(), 0);
                }
            }

            string ledgerActivitiesCacheKey = CacheSupport.BuildCacheKey(AllLedgerActivitiesCache, fiscalYear, fiscalPeriod, fiscalPeriodYear, transactionDate, reportingSegment);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                  this,
                  ContainsKey,
                  GetOrAddToCacheAsync,
                  AddOrUpdateCacheAsync,
                  transactionInvoker,
                  ledgerActivitiesCacheKey,
                  glaFyrFileName,
                  offset,
                  limit,
                  AllLedgerActivitiesCacheTimeout,

                  async () =>
                  {
                      if (string.IsNullOrEmpty(fiscalPeriod) && string.IsNullOrEmpty(transactionDate))
                          glCriteria = string.Empty;
                      else if (!string.IsNullOrEmpty(fiscalPeriod) && string.IsNullOrEmpty(transactionDate))
                      {
                          var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(fiscalYear), Convert.ToInt32(fiscalPeriod));

                          var startOn = await GetUnidataFormattedDate(new DateTime(Convert.ToInt32(fiscalPeriodYear), Convert.ToInt32(fiscalPeriod), 1).ToString());
                          var endOn = await GetUnidataFormattedDate(new DateTime(Convert.ToInt32(fiscalPeriodYear), Convert.ToInt32(fiscalPeriod), daysInMonth).ToString());

                          glCriteria = string.Format("WITH GLA.TR.DATE GE '{0}' AND WITH GLA.TR.DATE LE '{1}'", startOn, endOn);
                      }
                      else if (!string.IsNullOrEmpty(transactionDate))
                          glCriteria = (string.Format("WITH GLA.TR.DATE EQ '{0}'", transactionDate));

                      return new CacheSupport.KeyCacheRequirements()
                      {
                          criteria = glCriteria,
                      };
                  }    
            );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<GeneralLedgerActivity>, int>(new List<GeneralLedgerActivity>(), 0);
            }

            var subListIds = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

            glaDataContracts = await DataReader.BulkReadRecordAsync<GlaFyr>(glaFyrFileName, subListIds);
            var glpFyrKeys = await DataReader.SelectAsync(glpFyrFileName, "WITH GLP.POOLEE.ACCTS.LIST = '?'", subListIds);
            if (glpFyrKeys.Any())
                glpDataContracts = await DataReader.BulkReadRecordAsync<GlpFyr>(glpFyrFileName, glpFyrKeys.ToArray(), true);
            glaFyrFiltered = glaDataContracts.ToList();

            var personIds = glaDataContracts.Where(gl => !string.IsNullOrEmpty(gl.GlaAcctId)).Select(gl => gl.GlaAcctId).Distinct().ToArray();
            Collection<Person> personContracts = null;
            Collection<Institutions> instContracts = null;
            Collection<DataContracts.Projects> projectContracts = null;
            Collection<DataContracts.ProjectsCf> grantsContracts = null;
            Collection<DataContracts.GlAccts> glAcctsContracts = null;

            if (personIds != null && personIds.Any())
            {
                personContracts = await DataReader.BulkReadRecordAsync<Person>("PERSON", personIds);          
                instContracts = await DataReader.BulkReadRecordAsync<Institutions>("INSTITUTIONS", personIds);
            }
            string[] projectIds = glaDataContracts.Where(repo => !string.IsNullOrWhiteSpace(repo.GlaProjectsIds)).Select(id => id.GlaProjectsIds).Distinct().ToArray();
            if (projectIds != null && projectIds.Any())
            {
                projectContracts = await DataReader.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", projectIds);
                grantsContracts = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsCf>("PROJECTS.CF", projectIds);
            }
            string[] acctIds = glaDataContracts.Select(gla => gla.Recordkey.Split('*')[0]).Distinct().ToArray();
            if (acctIds != null && acctIds.Any())
            {
                glAcctsContracts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", acctIds);
            }
            List<GeneralLedgerActivity> ledgerActivities = new List<GeneralLedgerActivity>();

            if (glaFyrFiltered != null && glaFyrFiltered.Any())
            {
                RepositoryException exception = null;

                foreach (var glaFyr in glaFyrFiltered)
                {
                    try
                    {
                        Person person = null;
                        Institutions inst = null;
                        if (personContracts != null)
                        {
                            person = personContracts.FirstOrDefault(p => p.Recordkey == glaFyr.GlaAcctId);
                        }
                        if (instContracts != null)
                        {
                            inst = instContracts.FirstOrDefault(i => i.Recordkey == glaFyr.GlaAcctId);
                        }
                        string personGuid = string.Empty;
                        string corpFlag = string.Empty;
                        string instFlag = string.Empty;
                        if (!string.IsNullOrEmpty(glaFyr.GlaAcctId))
                        {
                            if (person != null)
                            {
                                personGuid = person.RecordGuid;
                                if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
                                {
                                    corpFlag = "Y";
                                    if (inst != null)
                                    {
                                        instFlag = "Y";
                                    }
                                }
                            }
                        }

                        var description = glaFyr.GlaDescription;
                        if (string.IsNullOrEmpty(description)) description = glaFyr.GlaRefNo;
                        if (string.IsNullOrEmpty(description)) description = glaFyr.Recordkey;

                        var validationException = PreValidateLedgerActivity(glaFyr, description);
                        if (validationException != null && validationException.Errors != null && validationException.Errors.Any())
                        {
                            if (exception == null)
                                exception = validationException;
                            else
                                exception.AddErrors(validationException.Errors);
                            continue;
                        }
                        var ledgerActivity = new GeneralLedgerActivity(glaFyr.RecordGuid, glaFyr.Recordkey, description,
                            glaFyr.GlaSysDate, glaFyr.GlaTrDate, glaFyr.GlaDebit, glaFyr.GlaCredit);


                        ledgerActivity.GlaSource = await BuildGlaSourceCode(glaFyr.GlaSource);
                        ledgerActivity.ReportingSegment = await BuildReportingSegment();
                        ledgerActivity.GlaRefNumber = glaFyr.GlaRefNo;
                        ledgerActivity.GlaAccountId = personGuid;
                        ledgerActivity.GlaCorpFlag = corpFlag;
                        ledgerActivity.GlaInstFlag = instFlag;
                        ledgerActivity.HostCountry = await BuildHostCountry();
                        ledgerActivity.ProjectId = glaFyr.GlaProjectsIds;
                        if (!string.IsNullOrEmpty(glaFyr.GlaProjectsIds) && projectContracts != null)
                        {
                            var projContract = projectContracts.FirstOrDefault(pr => pr.Recordkey == glaFyr.GlaProjectsIds);
                            if (projContract != null)
                            {
                                ledgerActivity.ProjectRefNo = projContract.PrjRefNo;
                                ledgerActivity.ProjectGuid = projContract.RecordGuid;
                            }
                        }
                        if (!string.IsNullOrEmpty(glaFyr.GlaProjectsIds) && grantsContracts != null)
                        {
                            var grantContract = grantsContracts.FirstOrDefault(pr => pr.Recordkey == glaFyr.GlaProjectsIds);
                            if (grantContract != null) ledgerActivity.GrantsGuid = grantContract.RecordGuid;
                        }
                        if (glpDataContracts != null)
                        {
                            var glpContract = glpDataContracts.FirstOrDefault(glp => glp.GlpPooleeAcctsList.Contains(glaFyr.Recordkey));
                            if (glpContract != null) ledgerActivity.IsPooleeAcct = glpContract != null ? true : false;
                        }
                        if (glAcctsContracts != null)
                        {
                            var glAccountNumber = glaFyr.Recordkey.Split('*')[0];
                            var glAcctContract = glAcctsContracts.FirstOrDefault(gla => gla.Recordkey == glAccountNumber);
                            if (glAcctContract != null) ledgerActivity.AccountingStringGuid = glAcctContract.RecordGuid;
                        }

                        ledgerActivities.Add(ledgerActivity);
                    }

                    catch (Exception ex)
                    {
                        if (exception == null)
                        {
                            exception = new RepositoryException();
                        }
                        exception.AddError(new RepositoryError("Bad.Data", ex.Message) {
                            Id = glaFyr.RecordGuid,
                            SourceId = glaFyr.Recordkey
                        });
                    }
                }

                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
            }

            return (ledgerActivities != null && ledgerActivities.Any()) ? new Tuple<IEnumerable<GeneralLedgerActivity>, int>(ledgerActivities, totalCount) :
                new Tuple<IEnumerable<GeneralLedgerActivity>, int>(new List<GeneralLedgerActivity>(), 0);
        }

        /// <summary>
        /// Perform validation prior to calling the constructor and, if an error occurs, add error to the collection
        /// </summary>
        /// <param name="glaFyr"></param>
        /// <param name="description"></param>
        /// <returns>RepositoryException</returns>
        private RepositoryException PreValidateLedgerActivity(GlaFyr glaFyr, string description)
        {

            RepositoryException exception = null;

            if (string.IsNullOrEmpty(glaFyr.RecordGuid))
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "Guid is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }
            if (string.IsNullOrEmpty(glaFyr.Recordkey))
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "Accounting string is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }
            if (string.IsNullOrEmpty(description))
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "Description is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }
            if (!glaFyr.GlaTrDate.HasValue)
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "Transaction date is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }
            // If either credit or debit fields have a value of zero, then null it out
            // If they both have zero, then leave zero in the credit field and let the transaction go through
            // This is really bad data but we don't want to report on it as such because some Colleague
            // processes are causing the data to have a zero transaction and we can't remove them or do anything
            // to modify then, therefore, we don't want to issue an error response to something that they cannot fix or change.
            // SRM - 03/29/2021
            if (glaFyr.GlaCredit.HasValue && glaFyr.GlaCredit.Value == 0 && glaFyr.GlaDebit.HasValue && glaFyr.GlaDebit.Value != 0) glaFyr.GlaCredit = null;
            if (glaFyr.GlaDebit.HasValue && glaFyr.GlaDebit.Value == 0 && glaFyr.GlaCredit.HasValue && glaFyr.GlaCredit.Value != 0) glaFyr.GlaDebit = null;
            if (glaFyr.GlaDebit.HasValue && glaFyr.GlaDebit.Value == 0 && glaFyr.GlaCredit.HasValue && glaFyr.GlaCredit.Value == 0) glaFyr.GlaDebit = null;
            if ((!glaFyr.GlaDebit.HasValue && !glaFyr.GlaCredit.HasValue) || (glaFyr.GlaDebit == 0 && glaFyr.GlaCredit == 0))
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "Credit/Debit value is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }


            DateTime? enteredOn = null;
            if (glaFyr.GlaSysDate != null && glaFyr.GlaSysDate.HasValue)
            {
                enteredOn = glaFyr.GlaSysDate;
            }
            else if (glaFyr.GlaTrDate != null && glaFyr.GlaTrDate.HasValue)
            {
                enteredOn = glaFyr.GlaTrDate;
            }
            if (enteredOn == null || !enteredOn.HasValue)
            {
                if (exception == null)
                {
                    exception = new RepositoryException();
                }
                exception.AddError(new RepositoryError("Bad.Data", "EnteredOn date is required.")
                {
                    Id = !string.IsNullOrEmpty(glaFyr.RecordGuid) ? glaFyr.RecordGuid : null,
                    SourceId = !string.IsNullOrEmpty(glaFyr.Recordkey) ? glaFyr.Recordkey : null
                });
            }

            return exception;
        }

        /// <summary>
        /// Returns a single general ledger activity record.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<GeneralLedgerActivity> GetGlaFyrByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }
            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null)
            {
                throw new KeyNotFoundException("General ledger activity not found for GUID " + guid);
            }
            if (!recordInfo.Entity.StartsWith("GLA.", StringComparison.OrdinalIgnoreCase))
            {
                throw new KeyNotFoundException("General ledger activity not found for GUID " + guid);
            }
            var glaFyr = await DataReader.ReadRecordAsync<GlaFyr>(recordInfo.Entity, recordInfo.PrimaryKey, true);
            var year = recordInfo.Entity.Split('.')[1].ToString();
            var glpFyrKeys = await DataReader.SelectAsync(string.Format("GLP.{0}", year), string.Format("WITH GLP.POOLEE.ACCTS.LIST = '{0}'", recordInfo.PrimaryKey));
            var glpFyrContracts = await DataReader.BulkReadRecordAsync<GlpFyr>(recordInfo.Entity, glpFyrKeys.ToArray(), true);
            if (glaFyr == null)
            {
                throw new KeyNotFoundException("General ledger activity not found for GUID " + guid);
            }
            string personGuid = string.Empty;
            string corpFlag = string.Empty;
            string instFlag = string.Empty;
            if (!string.IsNullOrEmpty(glaFyr.GlaAcctId))
            {
                var person = await DataReader.ReadRecordAsync<Person>("PERSON", glaFyr.GlaAcctId);
                if (person != null)
                {
                    personGuid = person.RecordGuid;
                    if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        corpFlag = "Y";
                        var inst = await DataReader.ReadRecordAsync<Institutions>("INSTITUTIONS", glaFyr.GlaAcctId);
                        if (inst != null)
                        {
                            instFlag = "Y";
                        }
                    }
                }
            }

            var description = glaFyr.GlaDescription;
            if (string.IsNullOrEmpty(description)) description = glaFyr.GlaRefNo;
            if (string.IsNullOrEmpty(description)) description = glaFyr.Recordkey;

            var validationException = PreValidateLedgerActivity(glaFyr, description);
            if (validationException != null && validationException.Errors != null && validationException.Errors.Any())
            {
                throw validationException;
            }


            GeneralLedgerActivity ledgerActivity = null;

            try
            {
                ledgerActivity = new GeneralLedgerActivity(glaFyr.RecordGuid, glaFyr.Recordkey, description, glaFyr.GlaSysDate, glaFyr.GlaTrDate, glaFyr.GlaDebit, glaFyr.GlaCredit);
                ledgerActivity.GlaSource = await BuildGlaSourceCode(glaFyr.GlaSource);
                ledgerActivity.ReportingSegment = await BuildReportingSegment();
                ledgerActivity.GlaRefNumber = glaFyr.GlaRefNo;
                ledgerActivity.GlaAccountId = personGuid;
                ledgerActivity.GlaCorpFlag = corpFlag;
                ledgerActivity.GlaInstFlag = instFlag;
                ledgerActivity.HostCountry = await BuildHostCountry();
                ledgerActivity.ProjectId = glaFyr.GlaProjectsIds;
                if (!string.IsNullOrEmpty(glaFyr.GlaProjectsIds))
                {
                    var projContract = await DataReader.ReadRecordAsync<DataContracts.Projects>("PROJECTS", glaFyr.GlaProjectsIds);
                    if (projContract != null)
                    {
                        ledgerActivity.ProjectRefNo = projContract.PrjRefNo;
                        ledgerActivity.ProjectGuid = projContract.RecordGuid;
                    }
                    var grantsContract = await DataReader.ReadRecordAsync<DataContracts.ProjectsCf>("PROJECTS.CF", glaFyr.GlaProjectsIds);
                    if (grantsContract != null)
                    {
                        ledgerActivity.GrantsGuid = grantsContract.RecordGuid;
                    }
                }
                var glAcctsContract = await DataReader.ReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", glaFyr.Recordkey.Split('*')[0]);
                if (glAcctsContract != null) ledgerActivity.AccountingStringGuid = glAcctsContract.RecordGuid;
                ledgerActivity.IsPooleeAcct = glpFyrContracts != null ? true : false;
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();   
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                throw exception;
            }
            return ledgerActivity;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await InternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }
        #endregion  

        #region private build methods    
        /// <summary>
        /// Builds reporting segment.
        /// </summary>
        /// <returns></returns>        
        private async Task<string> BuildReportingSegment()
        {
            if (!string.IsNullOrEmpty(institutionName))
            {
                return institutionName;
            }
            var defaultCorpId = string.Empty;
            var defaults = await this.GetDefaults();
            if (defaults != null)
            {
                defaultCorpId = defaults.DefaultHostCorpId;
                var corpContract = await DataReader.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", defaultCorpId);
                if (corpContract.CorpName == null || !corpContract.CorpName.Any())
                {
                    throw new ApplicationException("Institution must have a name.");
                }
                institutionName = String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)));
            }
            return institutionName;
        }

        /// <summary>
        /// Builds gla source code.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<string> BuildGlaSourceCode(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("GL source is required.");
            }
            if (sourceCodes == null)
            {
                sourceCodes = await GetGlSourceCodesAsync();
            }

            var sourceCode = sourceCodes.ValsEntityAssociation.FirstOrDefault(item => item.ValInternalCodeAssocMember.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (sourceCode == null)
            {
                throw new KeyNotFoundException(string.Format("GL source code not found for {0}.", source));
            }
            string asterix = "*";
            return string.Concat(sourceCode.ValInternalCodeAssocMember + asterix + sourceCode.ValActionCode2AssocMember); ;
        }

        /// <summary>
        /// Builds host country.
        /// </summary>
        /// <returns></returns>
        private async Task<string> BuildHostCountry()
        {
            if (string.IsNullOrEmpty(hostCountry))
            {
                var internationalParameters = await InternationalParametersAsync();
                hostCountry = internationalParameters.HostCountry;
            }
            return hostCountry;
        }

        #endregion

        #region private helper methods

        /// <summary>
        /// Get the GL Source Codes from Colleague.
        /// </summary>
        /// <returns>ApplValcodes association of GL Source Codes data.</returns>
        private async Task<ApplValcodes> GetGlSourceCodesAsync()
        {
            var GlSourceCodesValidationTable = new ApplValcodes();
            try
            {
                // Verify that it is populated. If not, throw an error.
                GlSourceCodesValidationTable = await GetOrAddToCacheAsync<ApplValcodes>("GlSourceCodes",
                    async () =>
                    {
                        ApplValcodes GlSourceCodesValTable = await DataReader.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "GL.SOURCE.CODES");
                        if (GlSourceCodesValTable == null)
                            throw new ColleagueWebApiException("GL.SOURCE.CODES validation table data is null.");

                        return GlSourceCodesValTable;
                    }, Level1CacheTimeoutValue);

                return GlSourceCodesValidationTable;
            }
            catch (Exception ex)
            {
                LogDataError("CF.VALCODES", "GL.SOURCE.CODES", null, ex);
                throw new ColleagueWebApiException("Unable to retrieve GL.SOURCE.CODES validation table from Colleague.");
            }
        }

        /// <summary>
        /// Gets defaults.
        /// </summary>
        /// <returns></returns>
        private async Task<Base.DataContracts.Defaults> GetDefaults()
        {
            return await GetOrAddToCacheAsync<Data.Base.DataContracts.Defaults>("CoreDefaults",
                async () =>
                {
                    var coreDefaults = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Base.DataContracts.Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Gets international parameters.
        /// </summary>
        /// <returns></returns>
        private async Task<Ellucian.Data.Colleague.DataContracts.IntlParams> InternationalParametersAsync()
        {

            if (_internationalParameters == null)
            {
                _internationalParameters = await GetInternationalParametersAsync();
            }
            return _internationalParameters;
        }

        #endregion
    }
}