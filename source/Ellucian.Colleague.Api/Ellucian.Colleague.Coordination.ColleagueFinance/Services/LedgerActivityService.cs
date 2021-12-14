//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class LedgerActivityService : BaseCoordinationService, ILedgerActivityService
    {

        private readonly ILedgerActivityRepository _ledgerActivityRepository;
        private readonly IGeneralLedgerConfigurationRepository _generalLedgerConfigurationRepository;
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="ledgerActivityRepository"></param>
        /// <param name="referenceDataRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="staffRepository"></param>
        /// <param name="configurationRepository"></param>
        public LedgerActivityService
        (
            ILedgerActivityRepository ledgerActivityRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStaffRepository staffRepository = null,
            IConfigurationRepository configurationRepository = null
        ) : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            _ledgerActivityRepository = ledgerActivityRepository;
            _generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all ledger-activities
        /// </summary>
        /// <returns>Collection of LedgerActivity DTO objects</returns>
        public async Task<Tuple<IEnumerable<LedgerActivity>, int>> GetLedgerActivitiesAsync(int offset, int limit, string fiscalYear, string fiscalPeriod, string reportingSegment,
            string transactionDate, bool bypassCache = false)
        {
        

            string newFiscalYear = string.Empty;
            string newFiscalPeriod = string.Empty;
            string fiscalPeriodYear = string.Empty;
            string newTransactionDate = null;
            string fiscalPeriodGuid = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(string.Concat(fiscalYear, fiscalPeriod, transactionDate)))
                {
                    FiscalYear fiscalYearLookup = null;
                    try
                    {
                        fiscalYearLookup = await GetFiscalYearAsync(DateTime.Now.Date, bypassCache);
                    }
                    catch (Exception)
                    {
                        return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                    }
                    if (fiscalYearLookup != null)
                    {
                        newFiscalYear = fiscalYearLookup.Id;
                    }
                }
                else
                {

                    if (!string.IsNullOrEmpty(fiscalYear))
                    {
                        IEnumerable<FiscalYear> fiscalYears = null;
                        try
                        {
                            fiscalYears = await FiscalYearsAsync(bypassCache);
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        if (fiscalYears == null)
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        var fiscalYearLookup = fiscalYears.FirstOrDefault(x => x.Guid.Equals(fiscalYear, StringComparison.OrdinalIgnoreCase));
                        if (fiscalYearLookup == null)
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        newFiscalYear = fiscalYearLookup.Id;
                    }

                    if (!string.IsNullOrEmpty(fiscalPeriod))
                    {
                        var fiscalPeriods = await FiscalPeriodsAsync(bypassCache);
                        if (fiscalPeriods == null)
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        var fiscalPeriodLookup = fiscalPeriods.FirstOrDefault(i => i.Guid.Equals(fiscalPeriod, StringComparison.OrdinalIgnoreCase));
                        if (fiscalPeriodLookup == null)
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        if (!string.IsNullOrEmpty(newFiscalYear) && !newFiscalYear.Equals(fiscalPeriodLookup.FiscalYear.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                        if (string.IsNullOrEmpty(newFiscalYear) && !string.IsNullOrEmpty(fiscalPeriodLookup.FiscalYear.ToString()))
                        {
                            newFiscalYear = fiscalPeriodLookup.FiscalYear.ToString();
                        }
                        fiscalPeriodYear = fiscalPeriodLookup.Year.ToString();
                        newFiscalPeriod = fiscalPeriodLookup.Month.HasValue ? fiscalPeriodLookup.Month.Value.ToString() : null;
                        fiscalPeriodGuid = fiscalPeriod;
                    }

                    if (!string.IsNullOrEmpty(transactionDate))
                    {
                        DateTime outDate;
                        if (DateTime.TryParse(transactionDate, out outDate))
                        {
                            if (string.IsNullOrEmpty(newFiscalYear))
                            {
                                FiscalYear fiscalYearLookup = null;
                                try
                                {
                                    fiscalYearLookup = await GetFiscalYearAsync(outDate, bypassCache);
                                }
                                catch (Exception)
                                {
                                    return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                                }
                                if (fiscalYearLookup == null)
                                {
                                    return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                                }
                                newFiscalYear = fiscalYearLookup.Id;
                            }

                            if (!string.IsNullOrEmpty(newFiscalPeriod) && !string.IsNullOrEmpty(newFiscalYear))
                            {
                                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(newFiscalYear), Convert.ToInt32(newFiscalPeriod));

                                var startOn = new DateTime(Convert.ToInt32(fiscalPeriodYear), Convert.ToInt32(newFiscalPeriod), 1);
                                var endOn = new DateTime(Convert.ToInt32(fiscalPeriodYear), Convert.ToInt32(newFiscalPeriod), daysInMonth);

                                if (!(outDate >= startOn && outDate <= endOn))
                                {
                                    return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                                }
                            }

                            newTransactionDate = await _ledgerActivityRepository.GetUnidataFormattedDate(outDate.Date.ToString());
                        }
                        else
                        {
                            return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            var ledgerActivitiesCollection = new List<Ellucian.Colleague.Dtos.LedgerActivity>();

            Tuple<IEnumerable<GeneralLedgerActivity>, int> ledgerActivitiesEntities = null;
            GeneralLedgerAccountStructure glConfiguration = null;
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            try
            {
                ledgerActivitiesEntities = await _ledgerActivityRepository.GetGlaFyrAsync(offset, limit, newFiscalYear, newFiscalPeriod, fiscalPeriodYear, reportingSegment, newTransactionDate);
                glConfiguration = await _generalLedgerConfigurationRepository.GetAccountStructureAsync();

                glClassConfiguration = await _generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            if (ledgerActivitiesEntities == null || !ledgerActivitiesEntities.Item1.Any())
            {
                return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
            }

            foreach (var ledgerActivityEntity in ledgerActivitiesEntities.Item1)
            {
                try
                {
                    ledgerActivitiesCollection.Add(await ConvertLedgerActivitiesEntityToDto(ledgerActivityEntity, glConfiguration, glClassConfiguration,
                        fiscalPeriodGuid, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data", ledgerActivityEntity.RecordGuid, ledgerActivityEntity.RecordKey);
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return ledgerActivitiesCollection.Any() ? new Tuple<IEnumerable<LedgerActivity>, int>(ledgerActivitiesCollection, ledgerActivitiesEntities.Item2) :
                new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a general ledger activity record.
        /// </summary>
        /// <returns>LedgerActivity DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LedgerActivity> GetLedgerActivityByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                IntegrationApiExceptionAddError("Must provide a ledger-activity GUID for retrieval.", "Missing.GUID",
                   "", "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

         
            GeneralLedgerAccountStructure glConfiguration = null;
            GeneralLedgerActivity entity = null;
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            try
            {
                glConfiguration = await _generalLedgerConfigurationRepository.GetAccountStructureAsync();
                entity = await _ledgerActivityRepository.GetGlaFyrByIdAsync(guid);

                glClassConfiguration = await _generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                IntegrationApiExceptionAddError("No ledger activity was found for guid: " + guid, "GUID.Not.Found", guid, null, HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            LedgerActivity ledgerActivity = null;
            try
            {
                ledgerActivity = await ConvertLedgerActivitiesEntityToDto(entity, glConfiguration, glClassConfiguration, string.Empty, bypassCache);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", entity.RecordGuid, entity.RecordKey);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return ledgerActivity;
        }


        #region Convert methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a GlaFyr domain entity to its corresponding LedgerActivities DTO
        /// </summary>
        /// <param name="source">GlaFyr domain entity</param>
        /// <returns>LedgerActivities DTO.  Errors are added to IntegrationApiException collection.</returns>
        private async Task<Ellucian.Colleague.Dtos.LedgerActivity> ConvertLedgerActivitiesEntityToDto(GeneralLedgerActivity source,
            GeneralLedgerAccountStructure GlConfig, GeneralLedgerClassConfiguration glClassConfiguration, string fiscalPeriodGuid = "", bool bypassCache = false)
        {
            var ledgerActivity = new Ellucian.Colleague.Dtos.LedgerActivity()
            {
                Id = source.RecordGuid,
                Title = source.Description,
                ReportingSegment = source.ReportingSegment,
                TransactionDate = source.TransactionDate.Value,
                EnteredOn = source.EnteredOn.Value,
                Status = LedgerActivityStatus.Posted
            };

            if (source == null)
            {
                IntegrationApiExceptionAddError("GeneralLedgerActivity domain entity is required.  An unexpected error occurred retrieving data.", "Bad.Data");
                return ledgerActivity;
            }

            var recordGuidRecordKeyTuple = new Tuple<string, string>(source.RecordGuid, source.RecordKey);

            ledgerActivity.LedgerCategory = ConvertEntityToLedgerActivitiesLedgerCategoryDtoEnumAsync(source);

            ledgerActivity.DocumentType = await ConvertEntityToDocumentTypeDtoAsync(source.GlaSource, recordGuidRecordKeyTuple, bypassCache);

            ledgerActivity.AdjustmentType = ConvertEntityToAdjustmentTypeDto(source.GlaSource);

            ledgerActivity.Period = await ConvertEntityToPeriodDtoAsync(source.TransactionDate, recordGuidRecordKeyTuple,
                    fiscalPeriodGuid, bypassCache);

            ledgerActivity.AccountingString = ConvertEntityToDtoAccountingString(source.AccountingString, source.ProjectRefNo, GlConfig,
                recordGuidRecordKeyTuple);

            ledgerActivity.LedgerType = GetLedgerActivityLedgerType(source, glClassConfiguration, recordGuidRecordKeyTuple);

            ledgerActivity.AccountingStringComponentValues = ConvertEntityToAccountingStringComponentValues(source.AccountingStringGuid, source.ProjectGuid);

            if (!string.IsNullOrEmpty(source.GlaRefNumber))
                ledgerActivity.ReferenceDocumentNumber = source.GlaRefNumber;

            ledgerActivity.Reference = ConvertEntityToReferenceDtoAsync(source.GlaAccountId, source.GlaCorpFlag, source.GlaInstFlag);

            ledgerActivity.Type = ConvertEntityToCreditDebitTypeDto(source.Credit, source.Debit, recordGuidRecordKeyTuple);

            ledgerActivity.Amount = ConvertEntityToAmountDto(source.Credit, source.Debit, source.HostCountry);

            ledgerActivity.Grant = ConvertEntityToGrantDto(source.GrantsGuid);

            return ledgerActivity;
        }

        private LedgerActivityLedgerType? GetLedgerActivityLedgerType(GeneralLedgerActivity source, GeneralLedgerClassConfiguration glClassConfiguration,
             Tuple<string, string> recordGuidRecordKeyTuple)
        {

            var retval = LedgerActivityLedgerType.NotSet;

            if (source == null || glClassConfiguration == null)
            {
                return retval;
            }

            //The GL Class Definition (GLCD)form shows where to get the "GL Class" component of
            // a general ledger number.This is stored in record GL.CLASS.DEF in file 
            // ACCOUNT.PARAMETERS.Field 1(GL.CLASS.LOCATION) value 1 is the starting character 
            // of the GL class and field 1 value 2 is its length.Extract the GL class from the 
            // GL account being examined
            var glClassLength = glClassConfiguration.GlClassLength;
            var glStartPosition = glClassConfiguration.GlClassStartPosition;
            var glClass = string.Empty;

            if (!string.IsNullOrEmpty(source.AccountingString))
            {
                try
                {
                    // In Colleague, if the GL number is greater than 15 characters, then it gets stored with an underscore ("_")
                    // in place of the delimiter and therefore needs to have the delimiter included before we extract the GL class.
                    // If the length of the GL number is less than or equal to 15, then we strip out the delimiter completly before
                    // we extract the GL class.  This is because the GL structure setup has the starting position for the class
                    // based on delimiters when the number is 16 or greater in length.
                    var glNumberWithProject = source.AccountingString.Split('*');
                    var glNumber = glNumberWithProject.Count() > 0 ? glNumberWithProject[0] : source.AccountingString;
                    var unFormattedGlAccount = Regex.Replace(glNumber, "[^0-9a-zA-Z]", "");
                    glClass = (unFormattedGlAccount.Length < 16) ?
                            unFormattedGlAccount.Substring(glStartPosition, glClassLength) :
                            source.AccountingString.Substring(glStartPosition, glClassLength);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("Ledger-Activities.  An exception occurred extracting GL.CLASS. {0} . GUID: '{1}'. Id: '{2}'",
                        ex.Message,
                        recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : "",
                        recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : ""));
                }

                if (!string.IsNullOrEmpty(glClass))
                {
                    if (glClassConfiguration.AssetClassValues.Contains(glClass) || glClassConfiguration.LiabilityClassValues.Contains(glClass) ||
                        glClassConfiguration.FundBalanceClassValues.Contains(glClass))
                    {
                        retval = LedgerActivityLedgerType.General;
                    }
                    if (glClassConfiguration.ExpenseClassValues.Contains(glClass) || glClassConfiguration.RevenueClassValues.Contains(glClass))
                    {
                        retval = LedgerActivityLedgerType.Operating;
                    }
                    else
                    {
                        logger.Error(string.Format("Ledger-Activities. Unable to extact map glClass to LedgerType: '{0}. GUID: '{1}'. Id: '{2}' ",
                            glClass,
                            recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : "",
                            recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : ""));

                    }
                }
            }

            return retval;
        }

        private string ConvertEntityToDtoAccountingString(string source, string refIdSource, GeneralLedgerAccountStructure GlConfig,
                Tuple<string, string> recordGuidRecordKeyTuple)
        {
            if (source == null || GlConfig == null)
            {
                return string.Empty;
            }

            var acctNumber = string.Empty;
            try
            {
                var accountNumber = source.Contains("*") ? source.Split('*')[0] : source;

                var glAccount = new GlAccount(accountNumber);
                if (glAccount != null)
                {
                    acctNumber = glAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                    acctNumber = GetFormattedGlAccount(acctNumber, GlConfig, recordGuidRecordKeyTuple);
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("An unexpected error occurred extracting accounting string: {0}", ex.Message),
                        "Bad.Data",
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
            }
            return !string.IsNullOrEmpty(refIdSource) ? string.Concat(acctNumber, "*", refIdSource) : acctNumber;
        }

        private string GetFormattedGlAccount(string accountNumber, GeneralLedgerAccountStructure GlConfig, Tuple<string, string> recordGuidRecordKeyTuple)
        {

            var tempGlNo = string.Empty;
            var formattedGlAccount = Regex.Replace(accountNumber, "[^0-9a-zA-Z]", "");

            int startLoc = 0;
            int x = 0, glCount = GlConfig.MajorComponents.Count;

            foreach (var glMajor in GlConfig.MajorComponents)
            {
                try
                {
                    x++;
                    if (x < glCount) { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength) + GlConfig.glDelimiter; }
                    else { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength); }
                    startLoc += glMajor.ComponentLength;
                }
                catch (ArgumentOutOfRangeException)
                {
                    IntegrationApiExceptionAddError(string.Format("Invalid GL account number: {0}", accountNumber), "Bad.Data",
                      recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                    return null;
                }
            }
            formattedGlAccount = tempGlNo;

            return formattedGlAccount;
        }

        /// <summary>
        /// Converts entity to accounting string dto values.
        /// </summary>
        /// <param name="acctGuid">Guid for gl account component</param>
        /// <param name="projectGuid">Guid for Project component</param>
        /// <returns></returns>
        private List<GuidObject2> ConvertEntityToAccountingStringComponentValues(string acctGuid, string projectGuid)
        {
            List<GuidObject2> acctCompStrValues = new List<GuidObject2>();

            if (!string.IsNullOrEmpty(acctGuid))
            {
                acctCompStrValues.Add(new GuidObject2(acctGuid));
            }
            if (!string.IsNullOrEmpty(projectGuid))
            {
                acctCompStrValues.Add(new GuidObject2(projectGuid));
            }
            return acctCompStrValues;
        }

        /// <summary>
        /// Converts entity to reference dto.
        /// </summary>
        /// <param name="personGuid">Guid for Person/Corporation/Institution.</param>
        /// <param name="corpFlag">Flag set to "Y" if this is a corporation.</param>
        /// <param name="instFlag">Flag set to "Y" if this is an institution.</param>
        /// <returns></returns>
        private LedgerActivityReference ConvertEntityToReferenceDtoAsync(string personGuid, string corpFlag, string instFlag)
        {
            if (string.IsNullOrEmpty(personGuid))
            {
                return null;
            }

            LedgerActivityReference reference = null;

            if (string.IsNullOrEmpty(corpFlag) || !corpFlag.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                reference = new LedgerActivityReference()
                {
                    Person = new GuidObject2(personGuid)
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(instFlag) && instFlag.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    reference = new LedgerActivityReference()
                    {
                        Institution = new GuidObject2(personGuid)
                    };
                }
                else
                {
                    reference = new LedgerActivityReference()
                    {
                        Organization = new GuidObject2(personGuid)
                    };
                }
            }
            return reference;
        }

        /// <summary>
        /// Convert to fiscal period.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToPeriodDtoAsync(DateTime? source, Tuple<string, string> recordGuidRecordKeyTuple,
            string fiscalPeriodGuid, bool bypassCache)
        {
            if (!source.HasValue)
            {
                //throw new ArgumentNullException("Gl transaction date is required.");
                IntegrationApiExceptionAddError("Gl transaction date is required.", "Bad.Data",
                   recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;
            }

            //Fiscal period guid will be set if a filter is provided,
            if (!string.IsNullOrEmpty(fiscalPeriodGuid))
            {
                return new GuidObject2(fiscalPeriodGuid);
            }

            var month = source.Value.Month;
            var year = source.Value.Year;

            IEnumerable<FiscalPeriodsIntg> fiscalPeriods = null;
            try
            {
                fiscalPeriods = await FiscalPeriodsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to retrieve Fiscal periods. " + ex.Message, source.ToString()), "Bad.Data",
                 recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;
            }
            if (fiscalPeriods == null)
            {
                IntegrationApiExceptionAddError(string.Format("Fiscal period not found for '{0}'.", source.ToString()), "Bad.Data",
                   recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;
            }

            var period = fiscalPeriods.FirstOrDefault(i => year == i.Year && month == i.Month);
            if (period == null)
            {
                IntegrationApiExceptionAddError(string.Format("Fiscal period not found for '{0}'.", source.ToString()), "Bad.Data",
                   recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;

            }
            if (string.IsNullOrEmpty(period.Guid))
            {
                IntegrationApiExceptionAddError(string.Format("Fiscal period GUID not found for '{0}'.", source.ToString()), "Bad.Data",
                   recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;

            }

            return new GuidObject2(period.Guid);
        }

        /// <summary>
        /// Converts entity to credit debit values.
        /// </summary>
        /// <param name="credit"></param>
        /// <param name="debit"></param>
        /// <returns></returns>
        private LedgerActivityType ConvertEntityToCreditDebitTypeDto(decimal? credit, decimal? debit,
            Tuple<string, string> recordGuidRecordKeyTuple)
        {
            // If either credit or debit fields have a value of zero, then null it out
            // If they both have zero, then leave zero in the credit field and let the transaction go through
            // This is really bad data but we don't want to report on it as such because some Colleague
            // processes are causing the data to have a zero transaction and we can't remove them or do anything
            // to modify then, therefore, we don't want to issue an error response to something that they cannot fix or change.
            // SRM - 03/29/2021
            if (credit.HasValue && credit.Value == 0 && debit.HasValue && debit.Value != 0) credit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value != 0) debit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value == 0) debit = null;
            if (credit.HasValue && !debit.HasValue)
            {
                return LedgerActivityType.Credit;
            }

            else if (!credit.HasValue && debit.HasValue)
            {
                return LedgerActivityType.Debit;
            }
            else if (!credit.HasValue && !debit.HasValue)
            {
                IntegrationApiExceptionAddError("Missing a credit or debit.", "Bad.Data",
                      recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                           recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
            }
            else if (credit.HasValue && debit.HasValue)
            {
                IntegrationApiExceptionAddError("Record has both a credit and a debit.", "Bad.Data",
                      recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                           recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
            }
            return LedgerActivityType.NotSet;
        }

        /// <summary>
        /// Converts entity to document type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Guid Object for GL source code.</returns>
        private async Task<GuidObject2> ConvertEntityToDocumentTypeDtoAsync(string source, Tuple<string, string> recordGuidRecordKeyTuple, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            var adjType = source.Split('*')[0];
            if (adjType == null)
            {
                return null;
            }
            var docType2 = await GetGlaSourceCodesAsync(bypassCache);
            if (docType2 == null)
            {
                return null;
            }
            var docType = docType2.FirstOrDefault(i => i.Code.Equals(adjType, StringComparison.OrdinalIgnoreCase));
            if (docType == null)
            {
                //throw new KeyNotFoundException(string.Format("GL source not found for code: {0}.", source));
                IntegrationApiExceptionAddError(string.Format("GL source not found for code: {0}.", source), "Bad.Data",
                    recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item1 : null,
                         recordGuidRecordKeyTuple != null ? recordGuidRecordKeyTuple.Item2 : null);
                return null;
            }
            return new GuidObject2(docType.Guid);

        }

        /// <summary>
        /// Converts entity to amount.
        /// </summary>
        /// <param name="credit">Credit Amount</param>
        /// <param name="debit">Debit Amount</param>
        /// <param name="hostCountry">Host Country from the INTL form.</param>
        /// <returns>LegerActivityAmount DTO with amount and currency code.</returns>
        private LedgerActivityAmount ConvertEntityToAmountDto(decimal? credit, decimal? debit, string hostCountry)
        {
            // If either credit or debit fields have a value of zero, then null it out
            // If they both have zero, then leave zero in the credit field and let the transaction go through
            // This is really bad data but we don't want to report on it as such because some Colleague
            // processes are causing the data to have a zero transaction and we can't remove them or do anything
            // to modify then, therefore, we don't want to issue an error response to something that they cannot fix or change.
            // SRM - 03/29/2021
            if (credit.HasValue && credit.Value == 0 && debit.HasValue && debit.Value != 0) credit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value != 0) debit = null;
            if (debit.HasValue && debit.Value == 0 && credit.HasValue && credit.Value == 0) debit = null;
            LedgerActivityAmount laa = null;
            if (credit.HasValue)
            {
                laa = new LedgerActivityAmount() { Value = credit, Currency = ConvertEntityToHostCountryEnum(hostCountry) };
            }

            if (debit.HasValue)
            {
                laa = new LedgerActivityAmount() { Value = debit, Currency = ConvertEntityToHostCountryEnum(hostCountry) };
            }
            return laa;
        }

        /// <summary>
        /// Converts entity to host country.
        /// </summary>
        /// <param name="hostCountry">Host country of either USA or CANADA</param>
        /// <returns>LedgerActivitiesCurrency Enumeration for Host Country</returns>
        private LedgerActivitiesCurrency ConvertEntityToHostCountryEnum(string hostCountry)
        {
            LedgerActivitiesCurrency outValue;
            if (hostCountry.ToUpper().Equals("USA", StringComparison.OrdinalIgnoreCase) && Enum.TryParse("USD", true, out outValue))
            {
                return outValue;
            }
            if (hostCountry.ToUpper().Equals("CANADA", StringComparison.OrdinalIgnoreCase) && Enum.TryParse("CAD", true, out outValue))
            {
                return outValue;
            }
            return LedgerActivitiesCurrency.NotSet;
        }

        /// <summary>
        /// Converts entity to adjustment type.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private LedgerActivityAdjustmentType? ConvertEntityToAdjustmentTypeDto(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            var adjType = source.Split('*')[0];
            if (adjType != null)
            {
                switch (adjType.ToUpper())
                {
                    case "YE":
                        return LedgerActivityAdjustmentType.Yearendadjustment;
                    case "AA":
                    case "AB":
                    case "AE":
                        return LedgerActivityAdjustmentType.Openingbalance;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a LedgerActivitiesLedgerCategory domain enumeration value to its corresponding LedgerActivitiesLedgerCategory DTO enumeration value
        /// </summary>
        /// <param name="source">LedgerActivitiesLedgerCategory domain enumeration value</param>
        /// <returns>LedgerActivitiesLedgerCategory DTO enumeration value</returns>
        private LedgerActivityLedgerCategory ConvertEntityToLedgerActivitiesLedgerCategoryDtoEnumAsync(GeneralLedgerActivity source)
        {
            string[] vals = source.GlaSource.Split('*');

            switch (vals[1].ToUpper())
            {
                case "1":
                    return LedgerActivityLedgerCategory.Actuals;
                case "2":
                    if (source.IsPooleeAcct)
                    {
                        return LedgerActivityLedgerCategory.Accountedbudget;
                    }
                    else
                    {
                        if (vals[0].ToUpperInvariant().Equals("AB"))
                        {
                            return LedgerActivityLedgerCategory.Originalbudget;
                        }
                        return LedgerActivityLedgerCategory.Budgetadjustment;
                    }
                case "3":
                    return LedgerActivityLedgerCategory.Encumbrance;
                case "4":
                    return LedgerActivityLedgerCategory.Reservation;
                default:
                    //throw new ArgumentNullException(string.Format("GL source code is required for guid '{0}'.", source.RecordGuid));
                    IntegrationApiExceptionAddError("GL source code is required", "Bad.Data", source.RecordGuid, source.RecordKey);
                    return LedgerActivityLedgerCategory.NotSet;
            }
        }

        /// <summary>
        /// Gets grant guid object.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>      
        private GuidObject2 ConvertEntityToGrantDto(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return new GuidObject2(source);
            }
            return null;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Gets a fiscal year.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="outDate"></param>
        /// <returns></returns>
        private async Task<FiscalYear> GetFiscalYearAsync(DateTime outDate, bool bypassCache)
        {
            FiscalYear fiscalYearLookup = null;

            IEnumerable<FiscalYear> fiscalYears = null;
            try
            {
                fiscalYears = await FiscalYearsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Unable to extract fiscal years." + ex.Message, "Bad.Data");

            }
            foreach (var year in fiscalYears)
            {
                if (!year.FiscalStartMonth.HasValue)
                {
                    //throw new InvalidOperationException("Fiscal year lookup is missing fiscal start month.  Fiscal year id '" + year.Id + "'.");
                    IntegrationApiExceptionAddError("Fiscal year lookup is missing fiscal start month.  Fiscal year id '" + year.Id + "'.", "Bad.Data");
                }
                var startMonth = year.FiscalStartMonth.Value;
                if (!year.CurrentFiscalYear.HasValue)
                {
                    //throw new InvalidOperationException("Fiscal year lookup is missing fiscal year. Fiscal year id '" + year.Id + "'.");
                    IntegrationApiExceptionAddError("Fiscal year lookup is missing fiscal year. Fiscal year id '" + year.Id + "'.", "Bad.Data");
                }

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }
                var fiscalYear = year.CurrentFiscalYear.Value;

                if (outDate.Month >= startMonth && outDate.Year == Convert.ToInt32(year.Id) - 1)
                {
                    fiscalYearLookup = year;
                    break;
                }
                else if (outDate.Month < startMonth && outDate.Year == Convert.ToInt32(year.Id))
                {
                    fiscalYearLookup = year;
                    break;
                }
            }

            return fiscalYearLookup;
        }
        IEnumerable<FiscalYear> _fiscalYears = null;
        /// <summary>
        /// Gets fiscal years
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<FiscalYear>> FiscalYearsAsync(bool bypassCache)
        {
            return _fiscalYears ?? (_fiscalYears = await _referenceDataRepository.GetFiscalYearsAsync(bypassCache));
        }

        IEnumerable<FiscalPeriodsIntg> _fiscalperiods = null;
        /// <summary>
        /// Gets fiscal periods
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<FiscalPeriodsIntg>> FiscalPeriodsAsync(bool bypassCache)
        {
            return _fiscalperiods ?? (_fiscalperiods = await _referenceDataRepository.GetFiscalPeriodsIntgAsync(bypassCache));
        }

        IEnumerable<GlSourceCodes> _glaSourceCodes = null;
        /// <summary>
        /// Gets gla source codes.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GlSourceCodes>> GetGlaSourceCodesAsync(bool bypassCache)
        {
            return _glaSourceCodes ?? (_glaSourceCodes = await _referenceDataRepository.GetGlSourceCodesValcodeAsync(bypassCache));
        }
        #endregion
    }
}