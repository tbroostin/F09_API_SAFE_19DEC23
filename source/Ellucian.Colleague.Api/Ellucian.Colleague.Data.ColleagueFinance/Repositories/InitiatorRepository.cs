// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    ///  This class implements the IInitiatorRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class InitiatorRepository : PersonBaseRepository, IInitiatorRepository
    {
        private int bulkReadSize;
        /// <summary>
        /// The constructor to instantiate a initiator repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public InitiatorRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger, settings)
        {
            bulkReadSize = settings.BulkReadSize;
        }

        /// <summary>
        /// Get the list of initiator based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for initiator search.</param>
        /// <returns> The staff search results</returns> 
        public async Task<IEnumerable<Initiator>> QueryInitiatorByKeywordAsync(string searchCriteria)
        {
            List<Initiator> initiatorEntities = new List<Initiator>();
            if (string.IsNullOrEmpty(searchCriteria))
                throw new ArgumentNullException("searchCriteria", "search criteria required to query");

            // Remove extra blank spaces
            var tempString = searchCriteria.Trim();
            Regex regEx = new Regex(@"\s+");
            searchCriteria = regEx.Replace(tempString, @" ");

            List<string> filteredInitiator = new List<string>();

            filteredInitiator = await ApplyFilterCriteria(searchCriteria);

            if (!filteredInitiator.Any() || filteredInitiator.Count == 0 || filteredInitiator == null)
                return null;

            filteredInitiator = filteredInitiator.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            // bulk read staff records
            var staffData = await DataReader.BulkReadRecordAsync<Staff>("STAFF", filteredInitiator.ToArray());

            if (staffData != null && staffData.Any())
            {
                // Get initiator preferred Names from person ids
                Dictionary<string, string> hierarchyNameDictionary = GetPersonHierarchyNamesDictionary(filteredInitiator);

                foreach (var id in filteredInitiator)
                {
                    Staff staffDataContact = staffData.FirstOrDefault(sd => sd.Recordkey == id);

                    try
                    {
                        string initiatorName = string.Empty;
                        // Get preferred name for person id from dictionary 
                        hierarchyNameDictionary.TryGetValue(staffDataContact.Recordkey, out initiatorName);
                        Initiator initiator = new Initiator(staffDataContact.Recordkey, initiatorName, staffDataContact.StaffInitials);
                        initiatorEntities.Add(initiator);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                }
            }

            return initiatorEntities.AsEnumerable();
        }

        private async Task<List<string>> ApplyFilterCriteria(string searchKey)
        {
            List<string> filteredInitiator = new List<string>();
            long personId = 0;
            string staffIdQuery = "";

            // serach criteria is number
            if (long.TryParse(searchKey, out personId))
            {
                if (searchKey.Count() > 1)
                {
                    staffIdQuery = string.Format("WITH STAFF.ID LIKE ..." + searchKey + "...");
                    filteredInitiator = await ExecuteQueryStatement("STAFF", filteredInitiator, staffIdQuery);
                }
            }
            else
            {
                // try to fetch initiator from initiator code if search key does not have "," or whitespace
                if (!(searchKey.Contains(",") || searchKey.Contains(" ")))
                {
                    // As staff initials are always in Uppercase , make searchkey to upper case
                    staffIdQuery = string.Format("WITH STAFF.INITIALS EQ '" + searchKey.ToUpper() + "'");
                    filteredInitiator = await ExecuteQueryStatement("STAFF", filteredInitiator, staffIdQuery);
                }

                // If there is no result in staff then checck in person file
                if (filteredInitiator == null || !(filteredInitiator.Any()))
                {
                    // Otherwise, we are doing a name search of initiator - parse the search string into name parts
                    List<string> names = CommentsUtility.FormatStringToNames(searchKey);
                    filteredInitiator = await GetInitiatorIdsFromPerson(names[0], names[1], names[2]);
                }
            }

            return filteredInitiator;
        }

        private async Task<List<string>> GetInitiatorIdsFromPerson(string lastName, string firstName, string middleName)
        {

            List<string> filteredInitiator = new List<string>();
            // Search for the name and returns person ids
            var filteredPersonIds = await SearchByNameAsync(lastName, firstName, middleName);

            // filter the resulted person records with below criteria
            string personQuery = "WITH WHERE.USED EQ 'STAFF' ";
            if (filteredPersonIds != null && filteredPersonIds.Any())
                filteredInitiator = await ExecuteQueryStatement("PERSON", filteredPersonIds.ToList(), personQuery);
            return filteredInitiator;
        }

        private async Task<List<string>> ExecuteQueryStatement(string FileName, List<string> filteredInitiators, string queryCriteria)
        {
            string[] filteredByQueryCriteria = null;
            if (string.IsNullOrEmpty(queryCriteria))
                return null;
            if (filteredInitiators != null && filteredInitiators.Any())
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(FileName, filteredInitiators.ToArray(), queryCriteria);
            }
            else
            {
                filteredByQueryCriteria = await DataReader.SelectAsync(FileName, queryCriteria);
            }
            return filteredByQueryCriteria.ToList();
        }

        /// <summary>
        /// Get a collection of hierarchy names for initiator.
        /// </summary>
        /// <param name="initiatorIds">person Id collection</param>        
        /// <returns>collection of hierarchy names for initiator</returns>
        private Dictionary<string, string> GetPersonHierarchyNamesDictionary(List<string> initiatorIds)
        {
            #region Get Hierarchy Names

            // Use a colleague transaction to get all names at once. 
            List<string> personIds = new List<string>();
            List<string> hierarchies = new List<string>();
            Dictionary<string, string> hierarchyNameDictionary = new Dictionary<string, string>();

            GetHierarchyNamesForIdsResponse response = null;

            //Get all unique requestor & initiator personIds
            personIds = initiatorIds;

            // Call a colleague transaction to get the person names based on their hierarchies, if necessary
            if ((personIds != null) && (personIds.Count > 0))
            {
                hierarchies = Enumerable.Repeat("PREFERRED", personIds.Count).ToList();
                GetHierarchyNamesForIdsRequest request = new GetHierarchyNamesForIdsRequest()
                {
                    IoPersonIds = personIds,
                    IoHierarchies = hierarchies
                };
                response = transactionInvoker.Execute<GetHierarchyNamesForIdsRequest, GetHierarchyNamesForIdsResponse>(request);

                // The transaction returns the hierarchy names. If the name is multivalued,
                // the transaction only returns the first value of the name.
                if (response != null)
                {
                    for (int i = 0; i < response.IoPersonIds.Count; i++)
                    {
                        string key = response.IoPersonIds[i];
                        string value = response.OutPersonNames[i];
                        if (!hierarchyNameDictionary.ContainsKey(key))
                        {
                            hierarchyNameDictionary.Add(key, value);
                        }
                    }
                }
            }

            #endregion
            return hierarchyNameDictionary;
        }
    }
}
