// Copyright 2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Implement the IJournalEntryRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GrantsRepository : BaseColleagueRepository, IGrantsRepository
    {

        /// <summary>
        /// Constructor to instantiate a general ledger transaction repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public GrantsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get all with fiscalYear filter.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="fiscalYearId"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<ProjectCF>, int>> GetGrantsAsync(int offset, int limit, string reportingSegment = "", string fiscalYearId = "", bool bypassCache = false)
        {
            List<ProjectCF> projectEntities = new List<ProjectCF>();
            int totalCount = 0;
            string criteria = string.Empty;

            if(!string.IsNullOrEmpty(fiscalYearId))
            {
                var recordKey = await GetRecordKeyFromGuidAsync(fiscalYearId);
                if(string.IsNullOrEmpty(recordKey))
                {
                    return new Tuple<IEnumerable<ProjectCF>, int>(new List<ProjectCF>(), 0);
                }
                criteria = string.Format("WITH PRJCF.FISCAL.YEARS EQ '{0}'", recordKey);
            }

            if (!string.IsNullOrEmpty(reportingSegment))
            {
                var repSegment = await BuildReportingSegmentAsync();
                if (!repSegment.Equals(reportingSegment, StringComparison.OrdinalIgnoreCase))
                {
                    return new Tuple<IEnumerable<ProjectCF>, int>(new List<ProjectCF>(), 0);
                }
            }

            //Id's for ProjectCF
            var projectCFIds = await DataReader.SelectAsync("PROJECTS.CF", criteria);

            if(projectCFIds != null && !projectCFIds.Any())
            {
                return new Tuple<IEnumerable<ProjectCF>, int>(new List<ProjectCF>(), 0);
            }

            totalCount = projectCFIds.Count();

            Array.Sort(projectCFIds);

            var subIdList = projectCFIds.Skip(offset).Take(limit);

            //ProjectCF
            var projCFDCs = await DataReader.BulkReadRecordAsync<ProjectsCf>(subIdList.ToArray());
            //Project
            var projDCs = await DataReader.BulkReadRecordAsync<Projects>(subIdList.ToArray());
            //ProjectsLineItems
            var prjLineItemIds = projCFDCs.SelectMany(r => r.PrjcfLineItems).Distinct();
            var projLineItemDCs = await DataReader.BulkReadRecordAsync<ProjectsLineItems>("PROJECTS.LINE.ITEMS", prjLineItemIds.ToArray());

            if (projCFDCs != null && projCFDCs.Any())
            {
                foreach (var projCFDC in projCFDCs)
                {
                    var prjDC = projDCs.FirstOrDefault(rec => rec.Recordkey.Equals(projCFDC.Recordkey, StringComparison.OrdinalIgnoreCase));
                    if (prjDC == null)
                    {
                        throw new KeyNotFoundException(string.Format("Project record not found for guid: {0}", projCFDC.RecordGuid));
                    }

                    var projectEntity = await BuildProjectCFEntityAsync(projCFDC, prjDC, projLineItemDCs);
                    projectEntities.Add(projectEntity);
                }
            }

            return projectEntities.Any() ? new Tuple<IEnumerable<ProjectCF>, int>(projectEntities, totalCount) :
                new Tuple<IEnumerable<ProjectCF>, int>(new List<ProjectCF>(), 0);
        }

        /// <summary>
        /// Get grant by Id.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<ProjectCF> GetProjectsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            // Read the PROJECTS.CF record
            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "PROJECTS.CF")
            {
                throw new KeyNotFoundException(string.Format("Grants record {0} does not exist.", guid));
            }

            //Project CF
            var projCFDC = await DataReader.ReadRecordAsync<ProjectsCf>(recordInfo.PrimaryKey);
            if (projCFDC == null)
            {
                throw new KeyNotFoundException(string.Format("Project CF record not found for guid: {0}", guid));
            }

            //Project
            var projDC = await DataReader.ReadRecordAsync<Projects>(recordInfo.PrimaryKey);
            if (projDC == null)
            {
                throw new KeyNotFoundException(string.Format("Project record not found for guid: {0}", guid));
            }

            //Project Line Items
            var prjLineItemIds = projCFDC.PrjcfLineItems.Distinct();
            var projLineItemDCs = await DataReader.BulkReadRecordAsync<ProjectsLineItems>("PROJECTS.LINE.ITEMS", prjLineItemIds.ToArray());

            var projectEntity = await BuildProjectCFEntityAsync(projCFDC, projDC, projLineItemDCs);

            return projectEntity;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="prjCF"></param>
        /// <param name="project"></param>
        /// <param name="prjLineItems"></param>
        /// <returns></returns>
        private async Task<ProjectCF> BuildProjectCFEntityAsync(ProjectsCf prjCF, Projects project, Collection<ProjectsLineItems> prjLineItems)
        {
            ProjectCF projectCf = new ProjectCF(prjCF.RecordGuid, prjCF.Recordkey, project.PrjRefNo, project.PrjStartDate)
            {
                Title = project.PrjTitle,
                EndOn = project.PrjEndDate,
                SponsorReferenceCode = project.PrjAgencyRefNo,
                ReportingSegment = await BuildReportingSegmentAsync(),
                CurrentStatus = project.PrjCurrentStatus,
                AccountingStrings = BuildAccountingStrings(project.PrjRefNo, prjCF, prjLineItems),
                BudgetAmount = BuildBudgetAmount(prjCF, prjLineItems),
                ReportingPeriods = BuildReportingPeriods(prjCF),
                ProjectType = project.PrjType,
                ProjectContactPerson = await BuildContactPersonIdAsync(project.PrjContactsEntityAssociation)
            };
            return projectCf;
        }

        string institutionName = string.Empty;
        /// <summary>
        /// Builds reporting segment.
        /// </summary>
        /// <returns></returns>        
        private async Task<string> BuildReportingSegmentAsync()
        {
            if (!string.IsNullOrEmpty(institutionName))
            {
                return institutionName;
            }
            var defaultCorpId = string.Empty;
            var defaults = await this.GetDefaultsAsync();
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
        /// Builds accounting strings.
        /// </summary>
        /// <param name="prjRefNo"></param>
        /// <param name="prjCF"></param>
        /// <param name="prjLineItems"></param>
        /// <returns></returns>
        private IEnumerable<string> BuildAccountingStrings(string prjRefNo, ProjectsCf prjCF, Collection<ProjectsLineItems> prjLineItems)
        {
            List<string> accountingStrings = new List<string>();
            if (prjLineItems != null && prjLineItems.Any())
            {
                var acctStrings = prjLineItems
                    .Where(id => id.PrjlnProjectsCf.Equals(prjCF.Recordkey))
                    .SelectMany(astr => astr.PrjlnGlEntityAssociation)
                    .Where(i => !string.IsNullOrWhiteSpace(i.PrjlnGlInactiveAssocMember) && !i.PrjlnGlInactiveAssocMember.Equals("I", StringComparison.OrdinalIgnoreCase))
                    .Select(gl => gl.PrjlnGlAcctsAssocMember);

                if (acctStrings != null && acctStrings.Any())
                {
                    bool refEmpty = string.IsNullOrEmpty(prjRefNo);
                    acctStrings.ToList().ForEach(i =>
                    {
                        if (!refEmpty)
                        {
                            var acctStr = i.Replace("_", "-");
                            accountingStrings.Add(string.Concat(acctStr, "*", prjRefNo));
                        }
                        else
                        {
                            var acctStr = i.Replace("_", "-");
                            accountingStrings.Add(acctStr);
                        }

                    });
                    return accountingStrings;
                }
            }
            return null;
        }

        /// <summary>
        /// Builds budget amount.
        /// </summary>
        /// <param name="prjCF"></param>
        /// <param name="prjLineItems"></param>
        /// <returns></returns>
        private decimal? BuildBudgetAmount(ProjectsCf prjCF, Collection<ProjectsLineItems> prjLineItems)
        {
            decimal? totalBgtAmount = null;

            if (prjLineItems != null && prjLineItems.Any() && prjCF.PrjcfPeriodsEntityAssociation != null && prjCF.PrjcfPeriodsEntityAssociation.Any())
            {
                totalBgtAmount = prjLineItems
                .Where(li => li.PrjlnProjectsCf.Equals(prjCF.Recordkey) && li.PrjlnGlClassType.Equals("E", StringComparison.OrdinalIgnoreCase))
                .SelectMany(amt => amt.PrjlnBudgetAmts).Sum();
            }
            return totalBgtAmount;
        }

        /// <summary>
        /// Build reporting periods.
        /// </summary>
        /// <param name="prjCF"></param>
        /// <returns></returns>
        private IEnumerable<ReportingPeriod> BuildReportingPeriods(ProjectsCf prjCF)
        {
            List<ReportingPeriod> reportingPeriods = new List<ReportingPeriod>();

            if (prjCF.PrjcfPeriodsEntityAssociation != null && prjCF.PrjcfPeriodsEntityAssociation.Any())
            {
                foreach (var PrjcfPeriodEntity in prjCF.PrjcfPeriodsEntityAssociation)
                {
                    ReportingPeriod reportingPeriod = new ReportingPeriod()
                    {
                        StartDate = PrjcfPeriodEntity.PrjcfPeriodStartDatesAssocMember,
                        EndDate = PrjcfPeriodEntity.PrjcfPeriodEndDatesAssocMember
                    };
                    reportingPeriods.Add(reportingPeriod);
                }
                return reportingPeriods;
            }
            return null;
        }

        /// <summary>
        /// Builds contact person.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<List<string>> BuildContactPersonIdAsync(List<ProjectsPrjContacts> source)
        {
            if (source != null && source.Any())
            {
                var defaults = await this.GetLdmDefaultsAsync();
                var personIds = new List<string>();
                var prinInvestigatorRole = defaults.LdmdPrinInvestigatorRole;
                //we need to check the corp indicator to return a person. 
                var prjContacts = source.Where(i => i.PrjContactRolesAssocMember.Equals(prinInvestigatorRole, StringComparison.OrdinalIgnoreCase)).ToList();
                if (prjContacts != null && prjContacts.Any())
                {
                    foreach (var prj in prjContacts)
                        personIds.Add(prj.PrjContactPersonIdsAssocMember);
                }
                return personIds;
            }
            return null;
        }

        /// <summary>
        /// Gets defaults.
        /// </summary>
        /// <returns></returns>
        private async Task<Base.DataContracts.Defaults> GetDefaultsAsync()
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

        private async Task<Ellucian.Colleague.Data.Base.DataContracts.LdmDefaults> GetLdmDefaultsAsync()
        {
            var ldmDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            if (ldmDefaults == null)
            {
                throw new ConfigurationException("CDM configuration setup not complete.");
            }
            return ldmDefaults;
        }

        private Ellucian.Data.Colleague.DataContracts.IntlParams internationalParameters;

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
        /// Gets projectcf guids.
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>

        public async Task<IDictionary<string, string>> GetProjectCFGuidsAsync(string[] projectIds)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            var projects = await DataReader.BulkReadRecordAsync<DataContracts.ProjectsCf>(projectIds);

            foreach (var project in projects)
            {
                if (!dict.ContainsKey(project.Recordkey))
                {
                    dict.Add(project.Recordkey, project.RecordGuid);
                }
            }
            return dict;
        }

        /// <summary>
        /// Gets project ids.
        /// </summary>
        /// <param name="projectGuids"></param>
        /// <returns></returns>
        public async Task<List<string>> GetProjectCFIdsAsync(string[] projectGuids)
        {
            if(projectGuids == null || !projectGuids.Any())
            {
                return new List<string>();
            }
            List<string> ids = new List<string>();

            foreach (var guid in projectGuids)
            {
                var recordInfo = await GetRecordInfoFromGuidAsync(guid);
                if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "PROJECTS.CF")
                {
                    return new List<string>();
                }
                ids.Add(recordInfo.PrimaryKey);
            }
            return ids;
        }
    }
}
