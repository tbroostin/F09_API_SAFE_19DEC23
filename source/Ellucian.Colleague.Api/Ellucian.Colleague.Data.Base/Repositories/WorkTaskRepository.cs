// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class WorkTaskRepository : BaseColleagueRepository, IWorkTaskRepository
    {

        private ApplValcodes categories;
        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkTaskRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public WorkTaskRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        private async Task<ApplValcodes> GetCategoriesAsync()
        {
            if (categories != null)
            {
                return categories;
            }
            categories = await GetOrAddToCacheAsync<ApplValcodes>("WF.CATEGORIES",
                    async () =>
                    {
                        ApplValcodes categoriesTable = await DataReader.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "WF.CATEGORIES");
                        if (categoriesTable == null)
                        {
                            var errorMessage = "Unable to access WF.CATEGORIES valcode table.";
                            logger.Info(errorMessage);
                            categoriesTable = new ApplValcodes();
                        }
                        return categoriesTable;
                    }, Level1CacheTimeoutValue);
            return categories;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private ExecutionState? ConvertCodeToExecutionState(string executionCode)
        {
            switch (executionCode)
            {
                case null:
                    return null;
                case "NS":
                    return ExecutionState.NS;
                case "ON":
                    return ExecutionState.ON;
                case "C":
                    return ExecutionState.C;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the list of work tasks for the specified user and roles.
        /// </summary>
        /// <param name="personId">The ID of the persons</param>
        /// <param name="roleIds">The IDs (not titles) of the roles</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">At least one personId or roleId must be provided</exception>
        public async Task<List<WorkTask>> GetAsync(string personId, List<string> roleIds)
        {
            if (string.IsNullOrEmpty(personId) && (roleIds == null || roleIds.Count() == 0))
            {
                throw new ArgumentException("Must specify a person or at least one role to retrieve work tasks");
            }
            List<WorkTask> taskList = new List<WorkTask>();

            // Build the query string based on the provided arguments
            string[] worklistAddrIds = null;
            string personCriteria = string.Empty;
            string roleCriteria = string.Empty;

            if (!string.IsNullOrEmpty(personId))
            {
                personCriteria = "WITH WKLAD.ORG.ENTITY EQ '" + personId + "'";
            }
            if (roleIds != null && roleIds.Count() > 0)
            {
                // If we have roles, this will require a substitution query
                roleCriteria = "WITH WKLAD.ORG.ROLE EQ '?'";
                string criteria = roleCriteria;
                if (!string.IsNullOrEmpty(personCriteria))
                {
                    // If we have person and roles, combine into a single either/or query
                    criteria = personCriteria + " OR " + roleCriteria;
                }
                if (!string.IsNullOrEmpty(criteria))
                {
                    worklistAddrIds = await DataReader.SelectAsync("WORKLIST.ADDR", criteria, roleIds.ToArray());
                }
            }
            else
            {
                // If we don't have roles, just select with the single person criteria, no substitution needed
                worklistAddrIds = await DataReader.SelectAsync("WORKLIST.ADDR", personCriteria);
            }

            // Bulkread worklist data
            if (worklistAddrIds != null && worklistAddrIds.Count() > 0)
            {
                worklistAddrIds = worklistAddrIds.Distinct().ToArray();

                Collection<WorklistAddr> worklistAddrRecords = new Collection<WorklistAddr>();
                Collection<Worklist> worklistRecords = new Collection<Worklist>();

                List<string> worklistIds = new List<string>();
                worklistAddrRecords = await DataReader.BulkReadRecordAsync<WorklistAddr>(worklistAddrIds.ToArray());
                if (worklistAddrRecords != null && worklistAddrRecords.Count() > 0)
                {
                    worklistIds = worklistAddrRecords.Where(wa => !string.IsNullOrEmpty(wa.WkladWorklist)).Select(wa => wa.WkladWorklist).Distinct().ToList();
                }
                if (worklistIds != null && worklistIds.Count() > 0)
                {
                    worklistRecords = await DataReader.BulkReadRecordAsync<Worklist>(worklistIds.ToArray());
                }

                // Build tasks
                if (worklistRecords != null && worklistRecords.Count() > 0)
                {
                    foreach (var item in worklistRecords)
                    {
                      
                        if (IsOpen(item.WklExecState))
                        {
                            var categoryAssociation = await GetCategoryValcodeAssociation(item.WklCategory);
                            // Use the category description from the valcode if available, otherwise use the code
                            string categoryName;
                            if (categoryAssociation != null)
                            {
                                categoryName = categoryAssociation.ValExternalRepresentationAssocMember;
                            }
                            else if (!string.IsNullOrEmpty(item.WklCategory))
                            {
                                categoryName = item.WklCategory;
                            }
                            else
                            {
                                categoryName = string.Empty;
                            }
                            // The process code for which application to route this type of task to is stored in the second action code
                            var processCode = categoryAssociation != null ? categoryAssociation.ValActionCode2AssocMember : string.Empty;

                            DateTimeOffset itemDateTime = item.WklStartTime.ToPointInTimeDateTimeOffset(item.WklStartDate, _colleagueTimeZone).GetValueOrDefault();

                            var task = new WorkTask(item.Recordkey, categoryName, item.WklDescription, processCode, itemDateTime, ConvertCodeToExecutionState(item.WklExecState));
                            taskList.Add(task);
                        }
                    }
                }
            }

            return taskList;
        }

        /// <summary>
        /// Examines the execution state and returns true if one we want to show to the user
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool IsOpen(string state)
        {
            if (string.IsNullOrEmpty(state)) return false;
            state = state.ToLower();
            if (state == "ns" || state == "s" || state == "r" || state == "on" || state == "o")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the <see cref="ApplValcodesVals"></see> for the given category code
        /// </summary>
        /// <param name="categoryCode"></param>
        /// <returns></returns>
        private async Task<ApplValcodesVals> GetCategoryValcodeAssociation(string categoryCode)
        {
            try
            {
                var categoriesValcode = await GetCategoriesAsync();
                if (string.IsNullOrEmpty(categoryCode)) return null;
                if (categoriesValcode == null || categoriesValcode.ValsEntityAssociation == null) return null;
                var categoryEntry = categoriesValcode.ValsEntityAssociation.FirstOrDefault(i => i.ValInternalCodeAssocMember == categoryCode);
                return categoryEntry;
            }
            catch
            {
                logger.Info("Error occurred while trying to retrieve category association for " + categoryCode);
                return null;
            }
        }
    }
}
