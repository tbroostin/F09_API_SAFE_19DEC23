//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
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
        private string fiscalPeriodGuid = string.Empty;

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
        /// <returns>Collection of LedgerActivities DTO objects</returns>
        public async Task<Tuple<IEnumerable<LedgerActivity>, int>> GetLedgerActivitiesAsync(int offset, int limit, string fiscalYear, string fiscalPeriod, string reportingSegment,
            string transactionDate, bool bypassCache = false)
        {
            if (!await CheckViewLedgerActivitiesPermission())
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view ledger activities.");
            }

            string newFiscalYear = string.Empty;
            string newFiscalPeriod = string.Empty;
            string fiscalPeriodYear = string.Empty;
            string newTransactionDate = null;

            if (string.IsNullOrEmpty(string.Concat(fiscalYear, fiscalPeriod, transactionDate)))
            {              
                var fiscalYearLookup = await GetFiscalYear(DateTime.Now.Date, bypassCache);
                if (fiscalYearLookup != null)
                {
                    newFiscalYear = fiscalYearLookup.Id;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(fiscalYear))
                {
                    var fiscalYearLookup = (await FiscalYearsAsync(bypassCache)).FirstOrDefault(x => x.Guid.Equals(fiscalYear, StringComparison.OrdinalIgnoreCase));
                    if (fiscalYearLookup == null)
                    {
                        return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
                    }
                    newFiscalYear = fiscalYearLookup.Id;
                }

                if (!string.IsNullOrEmpty(fiscalPeriod))
                {
                    var fiscalPeriodLookup = (await FiscalPeriodsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(fiscalPeriod, StringComparison.OrdinalIgnoreCase));
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
                            FiscalYear fiscalYearLookup = await GetFiscalYear(outDate, bypassCache);
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
            var glConfiguration = await _generalLedgerConfigurationRepository.GetAccountStructureAsync();

            var ledgerActivitiesCollection = new List<Ellucian.Colleague.Dtos.LedgerActivity>();

            Tuple<IEnumerable<GeneralLedgerActivity>, int> ledgerActivitiesEntities = null;
            try
            {
                ledgerActivitiesEntities = await _ledgerActivityRepository.GetGlaFyrAsync(offset, limit, newFiscalYear, newFiscalPeriod, fiscalPeriodYear, reportingSegment, newTransactionDate);
            }
            catch (KeyNotFoundException)
            {
                return new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
            }

            if (ledgerActivitiesEntities != null && ledgerActivitiesEntities.Item1.Any())
            {
                foreach (var ledgerActivityEntity in ledgerActivitiesEntities.Item1)
                {
                    ledgerActivitiesCollection.Add(await ConvertLedgerActivitiesEntityToDto(ledgerActivityEntity, glConfiguration, bypassCache));
                }
            }
            return ledgerActivitiesCollection.Any()? new Tuple<IEnumerable<LedgerActivity>, int>(ledgerActivitiesCollection, ledgerActivitiesEntities.Item2) :
                new Tuple<IEnumerable<LedgerActivity>, int>(new List<Ellucian.Colleague.Dtos.LedgerActivity>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a general ledger activity record.
        /// </summary>
        /// <returns>LedgerActivities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LedgerActivity> GetLedgerActivityByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                if (!await CheckViewLedgerActivitiesPermission())
                {
                    throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view ledger activities.");
                }
                var glConfiguration = await _generalLedgerConfigurationRepository.GetAccountStructureAsync();
                var entity = await _ledgerActivityRepository.GetGlaFyrByIdAsync(guid);

                return await ConvertLedgerActivitiesEntityToDto(entity, glConfiguration, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No ledger activity was found for guid: " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No ledger activity was found for guid: " + guid, ex);
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to delete Student Aptitude Assessments.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task<bool> CheckViewLedgerActivitiesPermission()
        {
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(ColleagueFinancePermissionCodes.ViewLedgerActivities))
            {
                return true;
            }
            return false;
        }

        #region Convert methods

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a GlaFyr domain entity to its corresponding LedgerActivities DTO
        /// </summary>
        /// <param name="source">GlaFyr domain entity</param>
        /// <returns>LedgerActivities DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.LedgerActivity> ConvertLedgerActivitiesEntityToDto(GeneralLedgerActivity source, GeneralLedgerAccountStructure GlConfig, bool bypassCache)
        {
            var ledgerActivity = new Ellucian.Colleague.Dtos.LedgerActivity();

            ledgerActivity.Id = source.RecordGuid;
            ledgerActivity.Title = source.Description;
            ledgerActivity.ReportingSegment = source.ReportingSegment;
            ledgerActivity.LedgerCategory = ConvertEntityToLedgerActivitiesLedgerCategoryDtoEnumAsync(source);
            ledgerActivity.DocumentType = await ConvertEntityToDocumentTypeDtoAsync(source.GlaSource, bypassCache);
            ledgerActivity.AdjustmentType = ConvertEntityToAdjustmentTypeDto(source.GlaSource);
            ledgerActivity.Period = await ConvertEntityToPeriodDtoAsync(source.TransactionDate, bypassCache);
            ledgerActivity.TransactionDate = source.TransactionDate.Value;
            ledgerActivity.EnteredOn = source.EnteredOn.Value;
            ledgerActivity.AccountingString = ConvertEntityToDtoAccountingString(source.AccountingString, source.ProjectRefNo, GlConfig);
            ledgerActivity.AccountingStringComponentValues = ConvertEntityToAccountingStringComponentValues(source.AccountingStringGuid, source.ProjectGuid);
            if (!string.IsNullOrEmpty(source.GlaRefNumber)) ledgerActivity.ReferenceDocumentNumber = source.GlaRefNumber;
            ledgerActivity.Reference = ConvertEntityToReferenceDtoAsync(source.GlaAccountId, source.GlaCorpFlag, source.GlaInstFlag);
            ledgerActivity.Type = ConvertEntityToCreditDebitTypeDto(source.Credit, source.Debit);
            ledgerActivity.Amount = ConvertEntityToAmountDto(source.Credit, source.Debit, source.HostCountry);
            ledgerActivity.Status = LedgerActivityStatus.Posted;
            ledgerActivity.Grant = ConvertEntityToGrantDto(source.GrantsGuid);
     
            return ledgerActivity;
        }

        private string ConvertEntityToDtoAccountingString(string source, string refIdSource, GeneralLedgerAccountStructure GlConfig)
        {
            string accountNumber = string.Empty;
            if (source.Contains("*"))
            {
                accountNumber = source.Split('*')[0];
            }
            else
            {
                accountNumber = source;
            }
            var glAccount = new GlAccount(accountNumber);
            string acctNumber = glAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
            acctNumber = GetFormattedGlAccount(acctNumber, GlConfig);

            if (!string.IsNullOrEmpty(refIdSource))
            {
                return string.Concat(acctNumber, "*", refIdSource);
            }
            return acctNumber;
        }

        private string GetFormattedGlAccount(string accountNumber, GeneralLedgerAccountStructure GlConfig)
        {
            string formattedGlAccount = string.Empty;
            string tempGlNo = string.Empty;
            formattedGlAccount = Regex.Replace(accountNumber, "[^0-9a-zA-Z]", "");

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
                    throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", accountNumber));
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

            if(!string.IsNullOrEmpty(acctGuid))
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
        private async Task<GuidObject2> ConvertEntityToPeriodDtoAsync(DateTime? source, bool bypassCache)
        {
            if(!source.HasValue)
            {
                throw new ArgumentNullException("Gl transaction date is required.");
            }

            if(!string.IsNullOrEmpty(fiscalPeriodGuid))
            {
                //var periodIntg = (await FiscalPeriodsAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(fiscalPeriodGuid, StringComparison.OrdinalIgnoreCase));
                //if (periodIntg == null)
                //{
                //    throw new KeyNotFoundException(string.Format("Fiscal period not found for guid {0}.", fiscalPeriodGuid));
                //}
                return new GuidObject2(fiscalPeriodGuid);
            }

            var month = source.Value.Month;
            var year = source.Value.Year;

            var period = (await FiscalPeriodsAsync(bypassCache)).FirstOrDefault(i => year <= i.Year && month >= i.Month);
            if(period == null)
            {
                throw new KeyNotFoundException(string.Format("Fiscal period not found for {0}.", source.ToString()));
            }
            return new GuidObject2(period.Guid);
        }

        /// <summary>
        /// Converts entity to credit debit values.
        /// </summary>
        /// <param name="credit"></param>
        /// <param name="debit"></param>
        /// <returns></returns>
        private LedgerActivityType ConvertEntityToCreditDebitTypeDto(decimal? credit, decimal? debit)
        {
            if(credit.HasValue && !debit.HasValue)
            {
                return LedgerActivityType.Credit;
            }

            if(!credit.HasValue && debit.HasValue)
            {
                return LedgerActivityType.Debit;
            }

            return LedgerActivityType.NotSet;
        }        
       
        /// <summary>
        /// Converts entity to document type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Guid Object for GL source code.</returns>
        private async Task<GuidObject2> ConvertEntityToDocumentTypeDtoAsync(string source, bool bypassCache)
        {
            if(string.IsNullOrEmpty(source)) return null;

            var adjType = source.Split('*')[0];
            if (adjType != null)
            {
                var docType = (await GetGlaSourceCodesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(adjType, StringComparison.OrdinalIgnoreCase));
                if (docType == null)
                {
                    throw new KeyNotFoundException(string.Format("GL source not found for code: {0}.", source));
                }
                return new GuidObject2(docType.Guid);
            }
            else
            {
                return null;
            }

                
        }

        /// <summary>
        /// Converts entity to amount.
        /// </summary>
        /// <param name="credit">Credit Amount</param>
        /// <param name="debit">Debit Amount</param>
        /// <param name="hostCountry">Host Country from the INTL form.</param>
        /// <returns>LegerACtivityAmount DTO with amount and currency code.</returns>
        private LedgerActivityAmount ConvertEntityToAmountDto(decimal? credit, decimal? debit, string hostCountry)
        {
            LedgerActivityAmount laa = null;
            if (credit.HasValue && credit != 0)
            {
                laa = new LedgerActivityAmount() { Value = credit, Currency = ConvertEntityToHostCountryEnum(hostCountry) };
            }

            if(debit.HasValue && debit != 0)
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
                    throw new ArgumentNullException(string.Format("GL source code is required for guid '{0}'.", source.RecordGuid));
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
        private async Task<FiscalYear> GetFiscalYear(DateTime outDate, bool bypassCache)
        {
            FiscalYear fiscalYearLookup = null;

            var fiscalYears = await FiscalYearsAsync(bypassCache);

            foreach (var year in fiscalYears)
            {
                if (!year.FiscalStartMonth.HasValue)
                {
                    throw new InvalidOperationException("Fiscal year lookup is missing fiscal start month.");
                }
                var startMonth = year.FiscalStartMonth.Value;
                if (!year.CurrentFiscalYear.HasValue)
                {
                    throw new InvalidOperationException("Fiscal year lookup is missing fiscal year.");
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

        IEnumerable<FiscalPeriodsIntg> _fiscalperriods = null;
        /// <summary>
        /// Gets fiscal periods
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<FiscalPeriodsIntg>> FiscalPeriodsAsync(bool bypassCache)
        {
            return _fiscalperriods ?? (_fiscalperriods = await _referenceDataRepository.GetFiscalPeriodsIntgAsync(bypassCache));
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