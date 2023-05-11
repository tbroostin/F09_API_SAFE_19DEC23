// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using System.Linq;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using Ellucian.Web.Dependency;
using Ellucian.Data.Colleague;
using System.Collections.Generic;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Dmi.Runtime;
using System.Diagnostics;
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Collections.ObjectModel;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RetentionAlertRepository : PersonBaseRepository, IRetentionAlertRepository
    {
        private string colleagueTimeZone;
        private const string retentionCaseUpdateEmailAction = "EMAIL";
        private const string retentionCaseUpdateAddNote = "NOTE";
        private const string retentionCaseUpdateCommCode = "COMM";
        private const string retentionCaseUpdateAddType = "TYPE";
        private const string retentionCaseUpdateChangePriority = "PRI";
        private const string retentionCaseUpdateReassign = "REASSIGN";
        private const string retentionCaseUpdateCloseCase = "CLOSE";
        private const string retentionCaseUpdateSetReminder = "SETREMIND";
        private const string retentionCaseUpdateManageReminders = "MANREMIND";
        private const string retentionCaseFollowUp = "FOLLOWUP";

        /// <summary>
        /// Constructor for Retention Alert Repository
        /// </summary>
        /// <param name="cacheProvider">Cache Provider</param>
        /// <param name="transactionFactory">Colleague TX Factory</param>
        /// <param name="logger">Logger</param>
        public RetentionAlertRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger, settings)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Retrieves retention alert open cases
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <returns> A list of retention alert open cases </returns>
        public async Task<List<RetentionAlertOpenCase>> GetRetentionAlertOpenCasesAsync(string advisorId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "Advisor ID must be specified");
            }
            List<RetentionAlertOpenCase> openCases = new List<RetentionAlertOpenCase>();
            var getRaOpenCasesForReportingRequest = new GetRaOpenCasesForReportingRequest()
            {
                APersonId = advisorId
            };

            try
            {
                var getResponse = await transactionInvoker.ExecuteAsync<GetRaOpenCasesForReportingRequest, GetRaOpenCasesForReportingResponse>(getRaOpenCasesForReportingRequest);

                if (getResponse == null)
                {
                    var message = "Could not get retention alert open cases. Unexpected null response from CTX GetRaOpenCasesForReporting";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                if (getResponse.AlErrorMessages != null && getResponse.AlErrorMessages.Any())
                {
                    string errorMessage = "";
                    foreach (var error in getResponse.AlErrorMessages)
                    {
                        logger.Error(error);
                        errorMessage += error + Environment.NewLine;
                    }
                    throw new RepositoryException(errorMessage);
                }

                foreach (OpenCases item in getResponse.OpenCases)
                {
                    RetentionAlertOpenCase openCase = new RetentionAlertOpenCase()
                    {
                        Category = item.AlCategory,
                        CategoryId = item.AlCategoryId,
                        ThirtyDaysOld = (item.AlThirtyDaysOld != null && item.AlThirtyDaysOld.Any()) ? item.AlThirtyDaysOld.Split(SubValueMark).ToList() : new List<string>(),
                        SixtyDaysOld = (item.AlSixtyDaysOld != null && item.AlSixtyDaysOld.Any()) ? item.AlSixtyDaysOld.Split(SubValueMark).ToList() : new List<string>(),
                        NinetyDaysOld = (item.AlNinetyDaysOld != null && item.AlNinetyDaysOld.Any()) ? item.AlNinetyDaysOld.Split(SubValueMark).ToList() : new List<string>(),
                        OverNinetyDaysOld = (item.AlOverNinetyDaysOld != null && item.AlOverNinetyDaysOld.Any()) ? item.AlOverNinetyDaysOld.Split(SubValueMark).ToList() : new List<string>(),
                        TotalOpenCases = (item.AlTotalOpenCases != null && item.AlTotalOpenCases.Any()) ? item.AlTotalOpenCases.Split(SubValueMark).ToList() : new List<string>()
                    };

                    openCases.Add(openCase);
                }

                return openCases;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting open cases");
                throw;
            }
            catch (RepositoryException re)
            {
                logger.Error(re, string.Format("Unable to get the open cases for the advisor: {0}", advisorId));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the open cases for the advisor: {0}", advisorId));
                throw;
            }
        }

        /// <summary>
        /// Retrieves retention alert closed cases grouped by closure reason
        /// </summary>
        /// <param name="categoryId">Retention Alert Case Category</param>
        /// <returns>A list of Retention Alert Cases grouped by a Closure Reason</returns>
        public async Task<List<RetentionAlertClosedCasesByReason>> GetRetentionAlertClosedCasesByReasonAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                throw new ArgumentNullException(categoryId, "Case Category ID must be specified");
            }

            List<RetentionAlertClosedCasesByReason> closedCasesByReason = new List<RetentionAlertClosedCasesByReason>();
            var request = new GetRaClosedCasesRequest()
            {
                ACategory = categoryId
            };

            try
            {
                var getResponse = await transactionInvoker.ExecuteAsync<GetRaClosedCasesRequest, GetRaClosedCasesResponse>(request);
                if (getResponse == null)
                {
                    var message = "Could not get retention alert closed cases. Unexpected null response from CTX GetRaClosedCases";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                foreach (var item in getResponse.RAClosureReasons)
                {
                    var caseIds = item.AlCases.Split(SubValueMark).ToList();
                    var lastActionDates = item.AlLastActionDates.Split(SubValueMark).ToList();
                    var cases = new List<RetentionAlertClosedCase>();

                    for (var i = 0; i < caseIds.Count; i++)
                    {
                        cases.Add(new RetentionAlertClosedCase()
                        {
                            CasesId = caseIds[i],
                            LastActionDate = DmiString.PickDateToDateTime(Int32.Parse(lastActionDates[i]))
                        }) ;
                    }
                    
                    var closedCases = new RetentionAlertClosedCasesByReason()
                    {
                        ClosureReasonId = item.AlClosureReasons,
                        Cases = cases
                    };

                    closedCasesByReason.Add(closedCases);
                }

                return closedCasesByReason;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting Closed Cases grouped by Closure Reason");
                throw;
            }
            catch (RepositoryException re)
            {
                logger.Error(re, "Unable to get Closed Cases grouped by Closure Reason.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get Closed Cases grouped by Closure Reason.");
                throw;
            }
        }

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="caseCategoryId">Retention Alert Case Category Id</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that category</returns> 
        public async Task<RetentionAlertGroupOfCasesSummary> GetRetentionAlertCaseOwnerSummaryAsync(string caseCategoryId)
        {
            if (string.IsNullOrEmpty(caseCategoryId))
            {
                throw new ArgumentNullException("caseCategoryIds", "caseCategoryIds are required to get a Category Summary.");
            }

            GetRaCasesCategorySummaryRequest request = new GetRaCasesCategorySummaryRequest()
            {
                ACategoryId = caseCategoryId
            };

            try
            {
                var response = await transactionInvoker.ExecuteAsync<GetRaCasesCategorySummaryRequest, GetRaCasesCategorySummaryResponse>(request);

                RetentionAlertGroupOfCasesSummary cases = new RetentionAlertGroupOfCasesSummary()
                {
                    Summary = response.ACategory
                };

                foreach (var item in response.RAEntitiesByOwner)
                {
                    if (item.AlOrgEntityCases != null && item.AlOrgEntityCases.Any())
                    {
                        cases.AddEntityCase(new RetentionAlertGroupOfCases()
                        {
                            Id = item.AlOrgEntityIds,
                            Name = item.AlOrgEntities,
                            CaseIds = item.AlOrgEntityCases.Split(SubValueMark).ToList()
                        });
                    }
                }

                foreach (var item in response.RARolesByOwner)
                {
                    if (item.AlOrgRoleCases != null && item.AlOrgRoleCases.Any())
                    {
                        cases.AddRoleCase(new RetentionAlertGroupOfCases()
                        {
                            Id = item.AlOrgRoleIds,
                            Name = item.AlOrgRoleTitles,
                            CaseIds = item.AlOrgRoleCases.Split(SubValueMark).ToList()
                        });
                    }
                }

                return cases;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting open cases");
                throw;
            }
            catch (RepositoryException re)
            {
                logger.Error(re, string.Format("Unable to get Open Cases for {0}.", string.Join(",", caseCategoryId)));
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get Open Cases for {0}.", string.Join(",", caseCategoryId) ));
                throw;
            }
        }

        /// <summary>
        /// Get the Org Role settings for Case Categories.
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids</param>
        /// <returns>List of Retention Alert Case Category Org Roles.</returns>
        public async Task<IEnumerable<RetentionAlertCaseCategoryOrgRoles>> GetRetentionAlertCaseCategoryOrgRolesAsync(IEnumerable<string> caseCategoryIds)
        {
            if (caseCategoryIds == null || !caseCategoryIds.Any())
            {
                throw new ArgumentNullException("caseCategoryIds", "Case Categories are required to retrieve the Case Category Details.");
            }

            List<RetentionAlertCaseCategoryOrgRoles> caseCategoryOrgRolesList = new List<RetentionAlertCaseCategoryOrgRoles>();
            try
            {
                var request = new GetRaCaseCategoryOrgRolesRequest()
                {
                    AlCaseCategoryIdsIn = caseCategoryIds.ToList()
                };

                var response = await transactionInvoker.ExecuteAsync<GetRaCaseCategoryOrgRolesRequest, GetRaCaseCategoryOrgRolesResponse>(request);

                if (response == null)
                {
                    var message = "Could not get Case Category Org Roles.  Unexpected null response from CTX GetRACaseCategories";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                foreach (var item in response.CaseCategoryOrgRoles)
                {
                    List<RetentionAlertCaseCategoryOrgRole> caseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();

                    var Ids = item.AlOrgRoleIds.Split(SubValueMark).ToList();
                    var Names = item.AlOrgRoleNames.Split(SubValueMark).ToList();
                    var InitialAssignment = item.AlInitialAssignment.Split(SubValueMark).ToList();
                    var AvailableForReassignment = item.AlReassignment.Split(SubValueMark).ToList();
                    var ReportingAndAdministrative = item.AlReporting.Split(SubValueMark).ToList();

                    for (int i =0; i < Ids.Count(); i++)
                    {
                        caseCategoryOrgRoles.Add(new RetentionAlertCaseCategoryOrgRole()
                        {
                            OrgRoleId = Ids[i],
                            OrgRoleName = Names[i],
                            IsAssignedInitially = InitialAssignment[i],
                            IsAvailableForReassignment = AvailableForReassignment[i],
                            IsReportingAndAdministrative = ReportingAndAdministrative[i]
                        }) ;
                    }

                    caseCategoryOrgRolesList.Add(new RetentionAlertCaseCategoryOrgRoles()
                    {
                        CaseCategoryId = item.AlCaseCategoryIds,
                        CaseCategoryOrgRoles = caseCategoryOrgRoles
                    });
                }
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting Org Role settings");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get Org Role settings for {0}.", string.Join(",", caseCategoryIds)));
                throw;
            }
            return caseCategoryOrgRolesList;
        }

        /// <summary>
        /// Retrieves retention alert contributions
        /// </summary>
        /// <param name="advisorId">Advisor ID</param>
        /// <param name="contributionsQueryCriteria">Query Criteria</param>
        /// <returns>Retention alert contributions list</returns>
        public async Task<List<RetentionAlertWorkCase>> GetRetentionAlertContributionsAsync(string advisorId, ContributionsQueryCriteria contributionsQueryCriteria)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "Advisor ID must be specified");
            }

            List<RetentionAlertWorkCase> workItems = new List<RetentionAlertWorkCase>();

            string excludeClosedCases = "I"; // Default, Include Closed cases
            string excludeOwnedCases = "E";  // Default, Exclude Owned cases
            string excludeOldCases = "E";   // Default, Exclude Old cases

            if (contributionsQueryCriteria != null)
            {
                if (!contributionsQueryCriteria.IncludeClosedCases)
                    excludeClosedCases = "E";
                if (contributionsQueryCriteria.IncludeOwnedCases)
                    excludeOwnedCases = "I";
                if (contributionsQueryCriteria.IncludeCasesOverOneYear)
                    excludeOldCases = "I";
            }

            var getMyCaseContributionsRequest = new GetMyCaseContributionsRequest()
            {
                AId = advisorId,
                AExclClosed = excludeClosedCases,
                AExclOwned = excludeOwnedCases,
                AExclOld = excludeOldCases
            };

            try
            {
                var createResponse = await transactionInvoker.ExecuteAsync<GetMyCaseContributionsRequest, GetMyCaseContributionsResponse>(getMyCaseContributionsRequest);

                if (createResponse == null)
                {
                    var message = "Could not get retention alert contributions. Unexpected null response from CTX GetMyCaseContributions";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                if (createResponse.AlErrorMessages != null && createResponse.AlErrorMessages.Any())
                {
                    string message = "Error(s) occurred while getting retention alert contributions";
                    logger.Error(message);
                    throw new RepositoryException(message);
                }

                foreach (MyCaseContributions item in createResponse.MyCaseContributions)
                {
                    RetentionAlertWorkCase workItem = new RetentionAlertWorkCase()
                    {
                        CaseId = item.AlCaseIds,
                        CaseItemIds = item.AlCaseItemIds,
                        Category = item.AlCategories,
                        CaseOwner = item.AlCaseOwners,
                        StudentId = item.AlStudents,
                        Status = item.AlCaseStatuses,
                        CategoryDescription = item.AlCategoryDescriptions,
                        DateCreated = item.AlCaseItemDates,
                        Summary = item.AlCaseSummaries,
                        CaseType = item.AlCaseTypes
                    };
                    workItems.Add(workItem);
                }

                return workItems;
            }
            catch(RepositoryException re)
            {
                logger.Error(re, string.Format("Unable to get contributions for Advisor: {0}.", advisorId));
                throw;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving contributions for Advisor: {0}.", advisorId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get contributions for Advisor: {0}.", advisorId));
                throw;
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="advisorId"></param>
        /// <param name="StudentIds"></param>
        /// <param name="CaseIds"></param>
        /// <returns>Retention Alert Work Cases list</returns>
        public async Task<List<RetentionAlertWorkCase>> GetRetentionAlertCasesAsync(string advisorId, IEnumerable<string> studentIds, IEnumerable<string> caseIds)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "Advisor ID must be specified");
            }
            var getRaCaseInformationRequest = new GetRaCaseInformationRequest();

            List<string> PaddedstudentIds = new List<string>();
            if (studentIds != null && studentIds.Any())
            {
                foreach (var studentId in studentIds) {
                    PaddedstudentIds.Add(await PadIdPerPid2ParamsAsync(studentId));
                }
                getRaCaseInformationRequest.AlStudentIds = PaddedstudentIds;
            }
            else if (caseIds != null && caseIds.Any())
            {
                getRaCaseInformationRequest.AlCaseIdsIn = caseIds.ToList();
            }
            else
            {
                getRaCaseInformationRequest.AAdvisorId = advisorId;
            }
            try
            {
                List<RetentionAlertWorkCase> workItems = new List<RetentionAlertWorkCase>();

                var createResponse = await transactionInvoker.ExecuteAsync<GetRaCaseInformationRequest, GetRaCaseInformationResponse>(getRaCaseInformationRequest);
                if (createResponse.AlErrorMessages != null && createResponse.AlErrorMessages.Count > 0)
                {
                    string errorMessage = "";
                    foreach (var error in createResponse.AlErrorMessages)
                    {
                        logger.Error(error);
                        errorMessage += error + Environment.NewLine;
                    }
                    throw new RepositoryException(errorMessage);
                }
                else
                {
                    if (createResponse == null)
                    {
                        var message = "Could not get Retention Alert Cases. Unexpected null response from CTX GetRACaseInformation";
                        logger.Error(message);
                        throw new RepositoryException(message);
                    }

                    foreach (CaseInformation item in createResponse.CaseInformation)
                    {
                        RetentionAlertWorkCase workItem = new RetentionAlertWorkCase(item.AlCaseIds, item.AlStatuses, item.AlCaseClosedBy, item.AlCaseClosedDate)
                        {                            
                            Category = item.AlCategories,
                            CaseOwner = item.AlCaseOwners,
                            StudentId = item.AlStudents,
                            Priority = item.AlPriorities,
                            CategoryDescription = item.AlCategoryDescriptions,
                            DateCreated = item.AlDatesCreated,
                            CaseOwnerIds = item.AlCaseOwnerIds,
                            MethodOfContact = item.AlCaseMethodOfContacts,
                            ActionCount = item.AlCaseActionCounts,
                            CaseTypeIds = (item.AlCaseTypes != null && item.AlCaseTypes.Any()) ? item.AlCaseTypes.Split(SubValueMark).ToList() : new List<string>(),
                            DaysOpen = item.AlCaseDaysOpen,
                            LastActionDate = item.AlCaseLastActionDates,
                            ReminderDate = item.AlReminderDates
                        };
                        workItems.Add(workItem);
                    }
                }
                return workItems;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get Retention Alert Cases for Advisor ") );
                throw;
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="advisorId"></param>
        /// <param name="studentIds"></param>
        /// <param name="searchCaseIds"></param>
        /// <param name="roleIds"></param>
        /// <param name="IsIncludeClosedCases"></param>
        /// <returns>Retention Alert Work Cases 2 list</returns>
        public async Task<List<RetentionAlertWorkCase2>> GetRetentionAlertCases2Async(string advisorId, IEnumerable<string> studentIds, IEnumerable<string> searchCaseIds, IEnumerable<string> roleIds, bool IsIncludeClosedCases)
        {
            List<RetentionAlertWorkCase2> workItems = new List<RetentionAlertWorkCase2>();
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException(advisorId, "Advisor ID must be specified");
            }

            try
            {
                var caseCategoriesTask = GetCaseCategoriesAsync();
                var caseTypesTask = GetCaseTypesAsync();
                var caseStatusesTask = GetCaseStatusAsync();
                var casePrioritiesTask = GetCasePrioritiesAsync();
                await Task.WhenAll(caseCategoriesTask, caseTypesTask, caseStatusesTask, casePrioritiesTask);

                IEnumerable<CaseCategory> caseCategories = caseCategoriesTask.Result;
                IEnumerable<CaseType> caseTypes = caseTypesTask.Result;
                IEnumerable<CaseStatus> caseStatuses = caseStatusesTask.Result;
                IEnumerable<CasePriority> casePriorities = casePrioritiesTask.Result;

                string[] caseIds;
                string[] workflowIds;
                string workflowWhere = "";
                string workListAddrWhere = "";
                IEnumerable<string> worklistAddrIds;

                Collection<Cases> cases = new Collection<Cases>();
                Collection<Worklist> worklistItems = new Collection<Worklist>();
                Collection<WorklistAddr> worklistAddr = new Collection<WorklistAddr>();
                //string beginningStartDate = "10/01/2020";
                string closedCaseStatus = "C";
                string casesCriteria = "";

                if ((studentIds != null && studentIds.Any()) || (searchCaseIds != null && searchCaseIds.Any()))
                {
                    // If searching for any particular student, filter the cases
                    if (studentIds != null && studentIds.Any())
                    {
                        List<string> paddedStudentIds = new List<string>();
                        foreach (var studentId in studentIds)
                        {
                            paddedStudentIds.Add(await PadIdPerPid2ParamsAsync(studentId));
                        }

                        if (IsIncludeClosedCases)
                        {
                            casesCriteria = "WITH CASES.PERSON.ID EQ '?' AND CASES.STATUS EQ '" + closedCaseStatus + "'";
                        }
                        else
                        {
                            casesCriteria = "WITH CASES.PERSON.ID EQ '?' ";
                        }
                        caseIds = (await DataReader.SelectAsync("CASES", casesCriteria, paddedStudentIds.ToArray()));
                        cases = await DataReader.BulkReadRecordAsync<Cases>(caseIds);
                    }
                    // If searching for any particular case id, filter the cases
                    else if (searchCaseIds != null && searchCaseIds.Any())
                    {
                        if (IsIncludeClosedCases)
                        {
                            casesCriteria = "WITH CASES.ID EQ '?' AND CASES.STATUS EQ '" + closedCaseStatus + "'";
                        }
                        else
                        {
                            casesCriteria = "WITH CASES.ID EQ '?' ";
                        }
                        caseIds = (await DataReader.SelectAsync("CASES", casesCriteria, searchCaseIds.ToArray()));
                        cases = await DataReader.BulkReadRecordAsync<Cases>(caseIds);
                    }

                    if (cases != null && cases.Any())
                    {
                        List<string> casesWorkFlowIds = cases.Select(x => x.CasesWorkflowId).ToList();
                        workflowIds = (await DataReader.SelectAsync("WORKLIST", "WITH WKL.WORKFLOW EQ '?' SAVING WORKLIST.ID", casesWorkFlowIds.ToArray()));
                        worklistItems = await DataReader.BulkReadRecordAsync<Worklist>(workflowIds.ToArray());

                        workListAddrWhere = "WITH WKLAD.WORKLIST EQ '?'";
                        worklistAddrIds = (await DataReader.SelectAsync("WORKLIST.ADDR", workListAddrWhere, workflowIds.ToArray())).Distinct();
                        worklistAddr = await DataReader.BulkReadRecordAsync<WorklistAddr>(worklistAddrIds.ToArray());
                    }
                }
                else
                {
                    // Get the WKLAD.WORKLIST from WORKLIST.ADDR for the logged in user and roles
                    workListAddrWhere = String.Format(@"WITH WKLAD.ORG.ENTITY = '{0}' OR WKLAD.ORG.ROLE EQ '?' SAVING WKLAD.WORKLIST", advisorId);
                    worklistAddrIds = (await DataReader.SelectAsync("WORKLIST.ADDR", workListAddrWhere, roleIds.ToArray())).Distinct();

                    // Get the record key from WORKLIST.ADDR 
                    string workloadListWhere = "WITH WKLAD.WORKLIST EQ '?'";
                    IEnumerable<string> worklistRecordIds = (await DataReader.SelectAsync("WORKLIST.ADDR", workloadListWhere, worklistAddrIds.ToArray()));
                    // Retrieve worklistaddr collection for WKLAD.ORG.ENTITY and WKLAD.ORG.ROLE
                    worklistAddr = await DataReader.BulkReadRecordAsync<WorklistAddr>(worklistRecordIds.ToArray());

                    workflowWhere = "WITH WORKLIST.ID EQ '?'";
                    string[] worklistIds = (await DataReader.SelectAsync("WORKLIST", workflowWhere, worklistAddrIds.ToArray()));
                    worklistItems = await DataReader.BulkReadRecordAsync<Worklist>(worklistIds.ToArray());
                    List<string> worklistWrkFlow = worklistItems.Select(x => x.WklWorkflow.ToString()).ToList();

                    if (IsIncludeClosedCases)
                    {
                        casesCriteria = "WITH CASES.WORKFLOW.ID EQ '?' AND CASES.STATUS EQ '" + closedCaseStatus + "'";
                    }
                    else
                    {
                        casesCriteria = "WITH CASES.WORKFLOW.ID EQ '?' AND CASES.STATUS NE '" + closedCaseStatus + "'";
                    }
                    caseIds = (await DataReader.SelectAsync("CASES", casesCriteria, worklistWrkFlow.ToArray()));
                    cases = await DataReader.BulkReadRecordAsync<Cases>(caseIds);
                }

                // Course.Sections
                IEnumerable<string> distictCaseItemIds = cases.SelectMany(c => c.CasesItems).Distinct();
                Collection<CaseItems> allCaseItems = await DataReader.BulkReadRecordAsync<CaseItems>(distictCaseItemIds.ToArray());
                //convert collection of case items in lookup as case  Id with case item details. This will speed up to look for case item from case
                ILookup<string, CaseItems> groupedCaseItems = allCaseItems.ToLookup(c => c.CitCase, c => c);

                foreach (var racase in cases)
                {
                    //pick first case type
                    String caseType = racase.CasesTypes.FirstOrDefault();
                    var pri = string.Empty;
                    if (!string.IsNullOrEmpty(racase.CasesPriority))
                    {
                        pri = casePriorities.FirstOrDefault(vals => vals.Code.Equals(racase.CasesPriority, StringComparison.CurrentCultureIgnoreCase)).Description;
                    }
                    var status = string.Empty;
                    var statusAction = string.Empty;
                    if (!string.IsNullOrEmpty(racase.CasesStatus))
                    {
                        var caseStatus = caseStatuses.FirstOrDefault(vals => vals.Code.Equals(racase.CasesStatus, StringComparison.CurrentCultureIgnoreCase));
                        status = caseStatus.Description;
                        statusAction = caseStatus.ActionCode;
                    }
                    //use lookup to find all the case items for the case
                   IEnumerable<CaseItems> caseItems= groupedCaseItems[racase.Recordkey];
                    
                    //find earliest reminder date
                    var reminderDate = caseItems.Where(x => x.CitReminderDate.HasValue).Select(x => x.CitReminderDate).OrderBy(x => x).FirstOrDefault();

                    // Get Case Owners
                    Worklist caseWorkListItems = worklistItems.FirstOrDefault(x => x.WklWorkflow.ToString() == racase.CasesWorkflowId);
                    List<WorklistAddr> lstWorkListAddr = new List<WorklistAddr>();
                    List<string> caseOwnerIds = new List<string>();
                    List<int> caseOrgRoleIds = new List<int>();
                    string caseOwner = "";

                    DateTime? closedDate = null;
                    string closedBy = string.Empty;
                    //if case is closed then consider the latest case item like when was it added and added by whom
                    if (statusAction.Equals("3"))
                    {
                        var closed = caseItems.OrderByDescending(x => x.CaseItemsAdddate).FirstOrDefault();
                        closedDate = closed.CaseItemsAdddate;
                        closedBy = closed.CaseItemsAddopr;
                        caseOwner = "No Owner, Case Closed";
                    }
                    else if(caseWorkListItems != null)
                    {
                        lstWorkListAddr = worklistAddr.Where(x => x.WkladWorklist == caseWorkListItems.Recordkey).ToList();

                        foreach (var wla in lstWorkListAddr)
                        {
                            if(!string.IsNullOrEmpty(wla.WkladOrgEntity))
                            {
                                caseOwnerIds.Add(wla.WkladOrgEntity);
                            }

                            if (!string.IsNullOrEmpty(wla.WkladOrgRole))
                            {
                                caseOrgRoleIds.Add(int.Parse(wla.WkladOrgRole));
                            }
                        }
                    }

                    RetentionAlertWorkCase2 item = new RetentionAlertWorkCase2(racase.Recordkey, racase.CasesPersonId)
                    {
                        Category = caseCategories.Where(x => x.CategoryId.Equals(racase.CasesCategory))
                                                            .Select(x => x.Code)
                                                            .FirstOrDefault(),
                        CategoryDescription = caseCategories.Where(x => x.CategoryId.Equals(racase.CasesCategory))
                                                            .Select(x => x.Description)
                                                            .FirstOrDefault(),
                        CaseTypeIds = racase.CasesTypes,
                        CaseType = caseTypes.Where(x => x.CaseTypeId.Equals(caseType))
                                            .Select(x => x.Description)
                                            .FirstOrDefault(),
                        Priority = pri,
                        CaseOwner = caseOwner,
                        CaseOwnerIds = caseOwnerIds,
                        CaseRoleIds = caseOrgRoleIds,
                        LastActionDate = racase.CasesLastActionDate,
                        DateCreated = racase.CasesAdddate,
                        Status = status,
                        ReminderDate = reminderDate,
                        ClosedDate = closedDate,
                        ClosedBy = closedBy,
                        ActionCount = caseItems.Count().ToString()
                    };
                    workItems.Add(item);
                }
                return workItems;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving retention alert cases 2 for Advisor");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get retention alert cases 2 for Advisor "));
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of Case priorities
        /// </summary>
        /// <returns>Collection of Case priorities</returns>
        public async Task<IEnumerable<CaseStatus>> GetCaseStatusAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<Domain.Base.Entities.CaseStatus>>("AllCaseStatus", async () =>
            {
                return await GetValcodeAsync<CaseStatus>("CORE", "CASE.STATUSES",
                e => new CaseStatus(e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember, e.ValActionCode1AssocMember));
            });
        }

        /// <summary>
        /// Gets a collection of Case priorities
        /// </summary>
        /// <returns>Collection of Case priorities</returns>
        public async Task<IEnumerable<CasePriority>> GetCasePrioritiesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<Domain.Base.Entities.CasePriority>>("AllCasePriorities", async () =>
            {
                return await GetValcodeAsync<CasePriority>("CORE", "CASE.PRIORITIES",
                e => new CasePriority(e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember));
            });
        }

        /// <summary>
        /// Gets a collection of case categories
        /// By default the results are cached with the name AllCaseCategories
        /// </summary>
        /// <returns>Collection of Case Categories</returns>
        public async Task<IEnumerable<CaseCategory>> GetCaseCategoriesAsync()
        {
            IEnumerable<CaseCategory> CaseCategories = await GetCodeItemAsync<CaseCategories, Domain.Base.Entities.CaseCategory>("AllCaseCategories", "CASE.CATEGORIES",
             c => new CaseCategory(c.CcatName, c.CcatDescription, c.Recordkey, c.CcatTypes, c.CcatClosureReasons, c.CcatWorkerEmailHier));
            return CaseCategories;
        }

        /// <summary>
        /// Gets a collection of case type
        /// By default the results are cached with the name AllCaseType
        /// </summary>
        /// <returns>Collection of Case Type</returns>
        public async Task<IEnumerable<CaseType>> GetCaseTypesAsync()
        {
            return await GetCodeItemAsync<CaseTypes, Domain.Base.Entities.CaseType>("AllCaseTypes", "CASE.TYPES",
             c => new CaseType(c.CtypName, c.CtypDescription, c.Recordkey, c.CtypCategory, c.CtypDefaultPriority, c.CtypActiveFlag.ToUpperInvariant() == "Y", c.CtypAllowCaseContrib.ToUpperInvariant() == "Y", c.CtypAvailCommCodes));

        }

        /// <summary>
        /// Gets the retention alert case detail.
        /// </summary>
        /// <param name="caseId">The case identifier.</param>
        /// <returns>Retention alert case detail</returns>
        /// <exception cref="ArgumentNullException">Case ID must be specified</exception>
        /// <exception cref="Exception"></exception>
        public async Task<RetentionAlertCaseDetail> GetRetentionAlertCaseDetailAsync(string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                throw new ArgumentNullException(caseId, "Case ID must be specified");
            }
            try
            {
                var getRaCaseDetailRequest = new GetRaCaseDetailsRequest()
                {
                    ACaseId = caseId
                };

                var getResponse = await transactionInvoker.ExecuteAsync<GetRaCaseDetailsRequest, GetRaCaseDetailsResponse>(getRaCaseDetailRequest);
                if (getResponse.AlErrorMessages != null && getResponse.AlErrorMessages.Any())
                {
                    string errorMessage = "";
                    foreach (var error in getResponse.AlErrorMessages)
                    {
                        logger.Error(error);
                        errorMessage += error + Environment.NewLine;
                    }
                    throw new ColleagueWebApiException(errorMessage);
                }

                List<RetentionAlertCaseRecipEmail> caseRecipEmails = new List<RetentionAlertCaseRecipEmail>();
                foreach (CaseRecipEmailAddresses emailRecip in getResponse.CaseRecipEmailAddresses)
                {
                    RetentionAlertCaseRecipEmail email = new RetentionAlertCaseRecipEmail()
                    {
                        Name = emailRecip.AlEmailNames,
                        Relationship = emailRecip.AlEmailRelations,
                        EmailAddress = emailRecip.AlEmailAddresses
                    };
                    caseRecipEmails.Add(email);
                }
                List<RetentionAlertCaseReassignmentDetail> caseReassignmentList = new List<RetentionAlertCaseReassignmentDetail>();
                foreach (ReassignmentList reassignment in getResponse.ReassignmentList)
                {
                    bool isSelected = false;
                    if(reassignment.AlAvailSelected != null)
                    {
                        isSelected = reassignment.AlAvailSelected.ToUpperInvariant() == "Y" ? true : false;
                    }
                    RetentionAlertCaseReassignmentDetail reassignmentData = new RetentionAlertCaseReassignmentDetail()
                    {
                        AssignmentCode = reassignment.AlAvailToAssign,
                        Title = reassignment.AlAvailTitles,
                        Role = reassignment.AlAvailIsRole,
                        IsSelected = isSelected
                    };
                    caseReassignmentList.Add(reassignmentData);
                }


                List<RetentionAlertCaseHistory> caseHistory = new List<RetentionAlertCaseHistory>();
                foreach (CaseHistory item in getResponse.CaseHistory)
                {
                    DateTimeOffset? dateCreated = item.AlTimes.HasValue && item.AlDates.HasValue ? item.AlTimes.ToPointInTimeDateTimeOffset(item.AlDates, colleagueTimeZone) : null;                 
                    RetentionAlertCaseHistory history = new RetentionAlertCaseHistory()
                    {
                        CaseItemType = item.AlCaseItemTypes,
                        CaseItemId = item.AlCaseItemId,
                        ContactMethod = item.AlContactMethods,
                        DateCreated = dateCreated.HasValue ? dateCreated.ToLocalDateTime(colleagueTimeZone): null,
                        TimeCreated = item.AlTimes,
                        DetailedNote = item.AlDetailedNotes.Split(SubValueMark).ToList(), // Split the list of @SM-delimited detailed notes
                        Summary = item.AlSummaries,
                        UpdatedBy = item.AlUpdatedBy,
                        CaseType = item.AlCaseItemContributionTypes,
                        CaseClosureReason = item.AlCaseClosureReason,
                        ReminderDate = item.AlReminderDates
                    };
                    caseHistory.Add(history);
                }

                RetentionAlertCaseDetail caseDetail = new RetentionAlertCaseDetail(caseId)
                {
                    Status = getResponse.AStatus,
                    CaseOwner = getResponse.ACaseOwner,
                    CaseType = getResponse.ACaseType,
                    CaseTypeCodes = getResponse.ACaseTypeCode,
                    CategoryId = getResponse.ACategoryId,
                    CategoryName = getResponse.ACategoryName,
                    Priority = getResponse.ACasePriority,
                    CasePriorityCode = getResponse.ACasePriorityCode,
                    StudentId = getResponse.AStudentId,
                    CreatedBy = getResponse.ACreatedBy,
                    CaseHistory = caseHistory,
                    CaseRecipEmails = caseRecipEmails,
                    CaseReassignmentList = caseReassignmentList
                };
                return caseDetail;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving retention alert case detail for the case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get retention alert case detail for the case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Creates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase"></param>
        /// <returns>Retention Alert Case Create Response</returns>
        public async Task<RetentionAlertCaseCreateResponse> AddRetentionAlertCaseAsync(RetentionAlertCase retentionAlertCase)
        {
            var addRetentionAlertCaseRequest = new AddOrUpdtRaCaseNoteRequest()
            {
                APersonId = retentionAlertCase.StudentId,
                AType = retentionAlertCase.CaseType,
                ASummary = retentionAlertCase.Summary,
                AlNotes = retentionAlertCase.Notes,
                AlMethodOfContact = retentionAlertCase.MethodOfContact
            };

            try
            {
                RetentionAlertCaseCreateResponse retentionAlertCaseCreateResponse = new RetentionAlertCaseCreateResponse();

                var createResponse = await transactionInvoker.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(addRetentionAlertCaseRequest);
                if (createResponse.AlErrorMessages != null && createResponse.AlErrorMessages.Count > 0)
                {
                    logger.Info("Unable to add retention alert case for student");
                    throw new ColleagueWebApiException();
                }
                else
                {
                    retentionAlertCaseCreateResponse = new RetentionAlertCaseCreateResponse()
                    {
                        CaseId = createResponse.ACaseId,
                        CaseItemsId = createResponse.ACaseItemsId,
                        CaseStatus = createResponse.ACaseStatus,
                        OwnerIds = createResponse.AlOwnerIds,
                        OwnerNames = createResponse.AlOwnerNames,
                        OwnerRoles = createResponse.AlOwnerRoles,
                        OwnerRoleTitles = createResponse.AlOwnerRoleTitles,
                        ErrorMessages = createResponse.AlErrorMessages
                    };
                }

                return retentionAlertCaseCreateResponse;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding retention alert case for student: '{0}'.", retentionAlertCase.StudentId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to add retention alert case for student: '{0}'.", retentionAlertCase.StudentId));
                throw;
            }
        }

        /// <summary>
        /// Updates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase"></param>
        /// <returns>Retention Alert Case update Response</returns>
        public async Task<RetentionAlertCaseCreateResponse> UpdateRetentionAlertCaseAsync(string caseId, RetentionAlertCase retentionAlertCase)
        {
            var addRetentionAlertCaseRequest = new AddOrUpdtRaCaseNoteRequest()
            {
                APersonId = retentionAlertCase.StudentId,
                AType = retentionAlertCase.CaseType,
                ASummary = retentionAlertCase.Summary,
                AlNotes = retentionAlertCase.Notes,
                AlMethodOfContact = retentionAlertCase.MethodOfContact
            };

            try
            {
                RetentionAlertCaseCreateResponse retentionAlertCaseCreateResponse = new RetentionAlertCaseCreateResponse();

                var createResponse = await transactionInvoker.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(addRetentionAlertCaseRequest);
                if (createResponse.AlErrorMessages != null && createResponse.AlErrorMessages.Count > 0)
                {
                    logger.Info("Unable to update retention alert case for student");
                    throw new ColleagueWebApiException();
                }
                else
                {
                    retentionAlertCaseCreateResponse = new RetentionAlertCaseCreateResponse()
                    {
                        CaseId = createResponse.ACaseId,
                        CaseItemsId = createResponse.ACaseItemsId,
                        CaseStatus = createResponse.ACaseStatus,
                        OwnerIds = createResponse.AlOwnerIds,
                        OwnerRoles = createResponse.AlOwnerRoles,
                        ErrorMessages = createResponse.AlErrorMessages
                    };
                }

                return retentionAlertCaseCreateResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to update retention alert case for student: '{0}'.", retentionAlertCase.StudentId));
                throw;
            }
        }

        /// <summary>
        /// Add a note to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseNote"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseNoteAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertWorkCaseNote)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case note");
            }
            if (retentionAlertWorkCaseNote == null)
            {
                throw new ArgumentNullException("retentionAlertCase", "retentionAlertCase must be specified to add a case note");
            }
            if (retentionAlertWorkCaseNote.Notes == null)
            {
                retentionAlertWorkCaseNote.Notes = new List<string>();
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertWorkCaseNote.UpdatedBy,
                ASummary = retentionAlertWorkCaseNote.Summary,
                AlNotes = retentionAlertWorkCaseNote.Notes.ToList(),
                AAction = retentionCaseUpdateAddNote
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding note to the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to add note to the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Add a FollowUp to a Retention Alert Case, this will not updat the Case Owner; only add a Note to the Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseNote"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseFollowUpAsync(string caseId, RetentionAlertWorkCaseNote retentionAlertWorkCaseNote)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case followup");
            }
            if (retentionAlertWorkCaseNote == null)
            {
                throw new ArgumentNullException("retentionAlertCase", "retentionAlertCase must be specified to add a case followup");
            }
            if (retentionAlertWorkCaseNote.Notes == null)
            {
                retentionAlertWorkCaseNote.Notes = new List<string>();
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertWorkCaseNote.UpdatedBy,
                ASummary = retentionAlertWorkCaseNote.Summary,
                AlNotes = retentionAlertWorkCaseNote.Notes.ToList(),
                AAction = retentionCaseFollowUp
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding followup note to the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to add followup note to the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Add a Communication Code to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCode"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseCommCodeAsync(string caseId, RetentionAlertWorkCaseCommCode retentionAlertCaseCode)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a communications code");
            }
            if (retentionAlertCaseCode == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to add a communications code");
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertCaseCode.UpdatedBy,
                ACommCode = retentionAlertCaseCode.CommunicationCode,
                AAction = retentionCaseUpdateCommCode
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding a communication code to the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to add a communication code to the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Add a Case Type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseTypeAsync(string caseId, RetentionAlertWorkCaseType retentionAlertCaseType)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case type");
            }
            if (retentionAlertCaseType == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to add a case type");
            }

            if (retentionAlertCaseType.Notes == null)
            {
                retentionAlertCaseType.Notes = new List<string>();
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertCaseType.UpdatedBy,
                ACaseType = retentionAlertCaseType.CaseType,
                AlNotes = retentionAlertCaseType.Notes.ToList(),
                AAction = retentionCaseUpdateAddType
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding a case type to the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to add a case type to the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Set a reminder date for a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="reminder"></param>
        /// <returns>Retention alert work case action response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseReminderAsync(string caseId, RetentionAlertWorkCaseSetReminder reminder)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case type");
            }
            if (reminder == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to add a case type");
            }

            if (reminder.Notes == null)
            {
                reminder.Notes = new List<string>();
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = reminder.UpdatedBy,
                AReminderDate = reminder.ReminderDate,
                ASummary = reminder.Summary,
                AlNotes = reminder.Notes.ToList(),
                AAction = retentionCaseUpdateSetReminder
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while setting reminder for the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to set reminder for the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Manage Retention Alert Case Reminders
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="reminders"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> ManageRetentionAlertCaseRemindersAsync(string caseId, RetentionAlertWorkCaseManageReminders reminders)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case type");
            }
            if (reminders == null)
            {
                throw new ArgumentNullException("reminders", "reminders must be specified to add a case type");
            }

            var caseItemReminders = new List<ManageReminderDates>();
            foreach (var item in reminders.Reminders)
            {
                caseItemReminders.Add(
                    new ManageReminderDates()
                    {
                        AlCaseItemsId = item.CaseItemsId,
                        AlClearReminderDates = item.ClearReminderDateFlag
                    });
            }

            var request = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = reminders.UpdatedBy,
                AAction = retentionCaseUpdateManageReminders,
                ManageReminderDates = caseItemReminders
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(request, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while setting reminder for the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to set reminder for the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Change the priority of a case
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="retentionAlertCasePriority"></param>
        /// <returns>Retention alert work case action response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> ChangeRetentionAlertCasePriorityAsync(string caseId, RetentionAlertWorkCasePriority retentionAlertCasePriority)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to change the priority");
            }
            if (retentionAlertCasePriority == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to change the priority");
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertCasePriority.UpdatedBy,
                APriority = retentionAlertCasePriority.Priority,
                AAction = retentionCaseUpdateChangePriority
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while changing the priority of the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to change the priority of the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Close a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case Id</param>
        /// <param name="retentionAlertCaseClose"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> CloseRetentionAlertCaseAsync(string caseId, RetentionAlertWorkCaseClose retentionAlertCaseClose)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to close a case");
            }
            if (retentionAlertCaseClose == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to close a case");
            }

            if (retentionAlertCaseClose.Notes == null)
            {
                retentionAlertCaseClose.Notes = new List<string>();
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertCaseClose.UpdatedBy,
                AClosureReason = retentionAlertCaseClose.ClosureReason,
                ASummary = retentionAlertCaseClose.Summary,
                AlNotes = retentionAlertCaseClose.Notes.ToList(),
                AAction = retentionCaseUpdateCloseCase
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while closing the retention alert case: '{0}'.", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to close the retention alert case: '{0}'.", caseId));
                throw;
            }
        }

        /// <summary>
        /// Process the response from the UpdtRaCase.
        /// </summary>
        /// <param name="request">UpdtRaCaseRequest</param>
        /// <param name="caseId">Case ID</param>
        /// <returns></returns>
        private async Task<RetentionAlertWorkCaseActionResponse> ProcessUpdtRaCaseResponse(UpdtRaCaseRequest request, string caseId)
        {
            RetentionAlertWorkCaseActionResponse retentionAlertCaseCreateResponse;

            var response = await transactionInvoker.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(request);
            if (response.AlErrorMessages != null && response.AlErrorMessages.Count > 0)
            {
                string errorMessage = "";
                foreach (var error in response.AlErrorMessages)
                {
                    logger.Error(error);
                    errorMessage += error + Environment.NewLine;
                }
                throw new ColleagueWebApiException(errorMessage);
            }
            else
            {
                retentionAlertCaseCreateResponse = new RetentionAlertWorkCaseActionResponse()
                {
                    CaseId = caseId
                };
            }

            return retentionAlertCaseCreateResponse;
        }

        /// <summary>
        /// Sends a mail for the Retention Alert Case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail"></param>
        /// <returns></returns>
        public async Task<RetentionAlertWorkCaseActionResponse> SendRetentionAlertWorkCaseMailAsync(string caseId, RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to add a case type");
            }
            if (retentionAlertWorkCaseSendMail == null)
            {
                throw new ArgumentNullException("retentionAlertWorkCaseSendMail", "retentionAlertWorkCaseSendMail must be specified to send mail for the case");
            }

            if (retentionAlertWorkCaseSendMail.MailNames == null)
            {
                retentionAlertWorkCaseSendMail.MailNames = new List<string>();
            }
            if (retentionAlertWorkCaseSendMail.MailAddresses == null)
            {
                retentionAlertWorkCaseSendMail.MailAddresses = new List<string>();
            }
            if (retentionAlertWorkCaseSendMail.MailTypes == null)
            {
                retentionAlertWorkCaseSendMail.MailTypes = new List<string>();
            }

            List<EmailAddresses> EmailAddresses = new List<EmailAddresses>();
            if (retentionAlertWorkCaseSendMail.MailNames.Any())
            {
                var MailNames = retentionAlertWorkCaseSendMail.MailNames.ToList();
                var MailAddresses = retentionAlertWorkCaseSendMail.MailAddresses.ToList();
                var MailTypes = retentionAlertWorkCaseSendMail.MailTypes.ToList();

                for (int i = 0; i < retentionAlertWorkCaseSendMail.MailNames.Count(); i++)
                {
                    EmailAddresses.Add(new EmailAddresses()
                    {
                        AlEmailNames = MailNames[i],
                        AlEmailAddresses = MailAddresses[i],
                        AlEmailTypes = MailTypes[i]
                    });
                }
            }

            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertWorkCaseSendMail.UpdatedBy,
                AEmailSubject = retentionAlertWorkCaseSendMail.MailSubject,
                AEmailBody = retentionAlertWorkCaseSendMail.MailBody,
                EmailAddresses = EmailAddresses,
                AAction = retentionCaseUpdateEmailAction
            };

            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while sending mail for the retention alert case: '{0}'. ", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to send mail for the retention alert case: '{0}'. ", caseId));
                throw;
            }
        }

        /// <summary>
        /// Reassign the retention alert case 
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCode"></param>
        /// <returns>Retention Alert Work Case Action Response</returns>
        public async Task<RetentionAlertWorkCaseActionResponse> ReassignRetentionAlertWorkCaseAsync(string caseId, RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign)
        {
            if (caseId == null)
            {
                throw new ArgumentNullException("caseId", "caseId must be specified to reassign the case");
            }
            if (retentionAlertWorkCaseReassign == null)
            {
                throw new ArgumentNullException("retentionAlertCaseAction", "retentionAlertCaseAction must be specified to reassign a case");
            }

            List<ReassignCase> reassignOwners = new List<ReassignCase>();
            foreach (RetentionAlertCaseReassignmentDetail caseOwnerDetail in retentionAlertWorkCaseReassign.ReassignOwners)
            {
                ReassignCase retentionCaseOwner = new ReassignCase()
                {
                    AlReassignTo = caseOwnerDetail.AssignmentCode,
                    AlReassignToIsRole = caseOwnerDetail.Role
                };
                reassignOwners.Add(retentionCaseOwner);
            }
            var updtRaCaseRequest = new UpdtRaCaseRequest()
            {
                ACasesId = caseId,
                AUpdatedBy = retentionAlertWorkCaseReassign.UpdatedBy,
                ReassignCase = reassignOwners,
                AlNotes = retentionAlertWorkCaseReassign.Notes,
                AAction = retentionCaseUpdateReassign
            };
            try
            {
                return await ProcessUpdtRaCaseResponse(updtRaCaseRequest, caseId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while reassigning the retention alert case: '{0}'. ", caseId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to reassign the retention alert case: '{0}'. ", caseId));
                throw;
            }
        }

        /// <summary>
        /// Set the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <param name="sendEmailPreference"></param>
        /// <returns>Retention Alert Send Email Preference</returns>
        public async Task<RetentionAlertSendEmailPreference> SetRetentionAlertEmailPreferenceAsync(string orgEntityId,  RetentionAlertSendEmailPreference sendEmailPreference)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                throw new ArgumentNullException("orgEntityId", "orgEntityId is required.");
            }
            if (sendEmailPreference == null)
            {
                throw new ArgumentNullException("sendEmailPreference", "sendEmailPreference is required.");
            }
            try
            {
                ManageRaEmailPreferenceRequest request = new ManageRaEmailPreferenceRequest()
                {
                    AAction = "SET",
                    AOrgEntityId = orgEntityId,
                    ASendPref = sendEmailPreference.HasSendEmailFlag
                };

                var response = await transactionInvoker.ExecuteAsync<ManageRaEmailPreferenceRequest, ManageRaEmailPreferenceResponse>(request);
                return new RetentionAlertSendEmailPreference(response.ASendPref, response.AMessage);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while setting the retention alert case worker email preference for {0}", orgEntityId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to set the retention alert case worker email preference for {0}", orgEntityId));
                throw;
            }
        }

        /// <summary>
        /// Get the Send Email Preference
        /// </summary>
        /// <param name="orgEntityId"></param>
        /// <returns>Retention Alert Send Email Preference</returns>
        public async Task<RetentionAlertSendEmailPreference> GetRetentionAlertEmailPreferenceAsync(string orgEntityId)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                throw new ArgumentNullException("orgEntityId", "orgEntityId is required.");
            }

            try
            {
                ManageRaEmailPreferenceRequest request = new ManageRaEmailPreferenceRequest()
                {
                    AAction = "GET",
                    AOrgEntityId = orgEntityId
                };

                var response = await transactionInvoker.ExecuteAsync<ManageRaEmailPreferenceRequest, ManageRaEmailPreferenceResponse>(request);
                return new RetentionAlertSendEmailPreference(response.ASendPref, response.AMessage);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving retention alert case worker email preference for {0}", orgEntityId);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to get the retention alert case worker email preference for {0}", orgEntityId));
                throw;
            }
        }


        /// <summary>
        /// Finds students given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against STUDENTS.
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Student Ids</returns>
        public async Task<IEnumerable<string>> SearchStudentsByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<string>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var studentIds = await base.SearchByNameForExactMatchAsync(lastName, firstName, middleName);

            watch.Stop();
            logger.Info("  STEP5.1 SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();
            // Filter to only return students
            studentIds = await base.FilterByEntityAsync("STUDENTS", studentIds);
            watch.Stop();
            logger.Info("  STEP5.2 FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            if (studentIds != null)
            {
                logger.Info("  STEP5.2 Filtered PERSONS to " + studentIds.Count() + " STUDENTS.");
            }

            return studentIds;
        }
    }
}
