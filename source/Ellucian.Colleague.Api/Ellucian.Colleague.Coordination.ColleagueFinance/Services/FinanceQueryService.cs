// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using dtoDomain = Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IFinanceQueryService interface.
    /// </summary>
    [RegisterType]
    public class FinanceQueryService : BaseCoordinationService, IFinanceQueryService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;
        private IFinanceQueryRepository financeQueryRepository;

        // This constructor initializes the private attributes.
        public FinanceQueryService(IFinanceQueryRepository financeQueryRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.financeQueryRepository = financeQueryRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
        }


        /// <summary>
        /// Returns the finance query summary object DTOs that are associated with the user logged into self-service for the 
        /// specified fiscal year.
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>List of finance query summary object for the fiscal year.</returns>
        public async Task<IEnumerable<dtoDomain.FinanceQuery>> QueryFinanceQuerySelectionByPostAsync(dtoDomain.FinanceQueryCriteria criteria)
        {
            // If the user does not have any gl accounts assigned, return an empty list of DTOs.
            List<dtoDomain.FinanceQuery> financeQueryDtos = new List<dtoDomain.FinanceQuery>();

            // The query criteria can be empty, but it cannot be null.
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Filter criteria must be specified.");
            }

            // in case fiscal year in criteria not sent we need to default the fiscal year to
            // the one for today's date because the user does not yet get a change to select one from the list.
            // Get the fiscal year configuration data to get the fiscal year for today's date.
            var fiscalYear = criteria.FiscalYear;
            if (string.IsNullOrEmpty(fiscalYear))
            {
                var fiscalYearConfiguration = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
                criteria.FiscalYear = fiscalYearConfiguration.FiscalYearForToday.ToString();
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            if (glAccountStructure.MajorComponents == null && glAccountStructure.Subcomponents == null)
            {
                throw new ConfigurationException("GL account structure - major / sub-components set up is missing.");
            }

            if (!ValidateSortCriteria(glAccountStructure, criteria))
            {
                throw new InvalidOperationException("Invalid sort/subtotal component criteria specified.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("GL class configuration is not set up.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ApplicationException("No GL user definition available.");
            }

            // If the user does not have any GL accounts assigned, return an empty list of finance query object.
            if ((generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any()))
            {
                var errorMessage = string.Format("user '{0}' does not have any GL accounts assigned.", CurrentUser.PersonId);
                logger.Error(errorMessage);
                return financeQueryDtos;
            }

            // If the user has GL accounts assigned, convert the filter criteria DTO
            // into a domain entity, and pass it into the finance query repository.
            var financeQueryCriteriaAdapter = new FinanceQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
            var queryCriteriaEntity = financeQueryCriteriaAdapter.MapToType(criteria);

            var filteredFinanceQueryGlAccountLineItems = await financeQueryRepository.GetGLAccountsListAsync(generalLedgerUser, glAccountStructure, glClassConfiguration, queryCriteriaEntity, CurrentUser.PersonId);

            if (filteredFinanceQueryGlAccountLineItems == null || !filteredFinanceQueryGlAccountLineItems.Any())
            {
                return financeQueryDtos;
            }

            var financeQueryGlAccountLineItems = filteredFinanceQueryGlAccountLineItems.ToList();

            #region Sort & Subtotal
            var glAccountNumbers = financeQueryGlAccountLineItems.Select(x => x.GlAccountNumber).ToList().Union(financeQueryGlAccountLineItems.SelectMany(x => x.Poolees).Select(x => x.GlAccountNumber).ToList());
            var glAccountDescriptionsDictionary = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(glAccountNumbers, glAccountStructure);
            var glSubComponentStructureDictionary = BuildGlComponentStructureDictionary(glAccountStructure);

            //list of unique sort criteria - remove duplicates or if duplicate found get criteria with subtotal checked if any.
            var sortCriteria = GetUniqueSortCriteria(queryCriteriaEntity.SortCriteria);

            foreach (var item in financeQueryGlAccountLineItems)
            {
                item.SortKey = sortCriteria.Any() ? BuildSortKeyForGlAccount(item.GlAccountNumber, sortCriteria, glSubComponentStructureDictionary) : string.Empty;
                AssignGlAccountDescription(item, glAccountDescriptionsDictionary);
                item.Poolees = item.Poolees != null && item.Poolees.Any() ? item.Poolees.OrderBy(x => x.GlAccountNumber, StringComparer.Ordinal).ToList() : item.Poolees;
            }

            var lineItemsForSubtotal = sortCriteria.Any() ? financeQueryGlAccountLineItems.OrderBy(x => x.SortKey, StringComparer.Ordinal).ThenBy(x => x.GlAccountNumber, StringComparer.Ordinal).ToList()
                : financeQueryGlAccountLineItems.OrderBy(x => x.GlAccountNumber, StringComparer.Ordinal).ToList();

            //dictionary stores the 'subcomponent name' as key and 'list of subcomponent values' as value 
            Dictionary<string, List<string>> subtotalComponentValuesDictionary = new Dictionary<string, List<string>>();
            //build the finance query entity and fill the 'subtotalComponentValuesDictionary'
            //for e.g.- 'subtotalComponentValuesDictionary' looks like
            //    <Fund.Group, [1,2,3]> 
            //    <Fund , [11,12,21,34]>
            //    <Category, [123,124]> 
            //    <SubCategory, [1234,1243]>
            // Note: Fund.Group, Fund belongs to 'FUND' Major Component Type
            // Category, SubCategory belongs to 'OBJECT' Major Component Type
            var financeQueryEntity = BuildFinanceQueryWithSubtotalEntities(lineItemsForSubtotal, sortCriteria, glAccountStructure, glSubComponentStructureDictionary, subtotalComponentValuesDictionary);
            #endregion
            // filter the subtotal criteria to be displayed
            var onlySubTotalCriteria = sortCriteria.Where(x => x.IsDisplaySubTotal.HasValue && x.IsDisplaySubTotal.Value).OrderBy(x => x.Order);
            if (onlySubTotalCriteria.Any())
            {
                // create component description dictionary where 'major component type' as key and 'a dictionary of subcomponent values and respective subcomponent description' as value 
                Dictionary<GeneralLedgerComponentType, Dictionary<string, string>> componentDescriptionDictionary = await CreateComponentTypeDescriptionDictionaryAsync(glAccountStructure, subtotalComponentValuesDictionary, onlySubTotalCriteria);
                AssignComponentDescriptionToFinanceQueryEntity(financeQueryEntity, componentDescriptionDictionary, glAccountStructure);
            }

            // Create the adapter to convert finance query domain entities to DTOs.
            var financeQueryAdapter = new FinanceQueryEntityToDtoAdapter(_adapterRegistry, logger);

            // Convert the domain entities into DTOs
            if (financeQueryEntity != null)
            {
                var financeQueryDto = financeQueryAdapter.MapToType(financeQueryEntity, glAccountStructure.MajorComponentStartPositions);
                financeQueryDtos.Add(financeQueryDto);
            }
            
            return financeQueryDtos;
        }

        #region private methods
        /// <summary>
        /// if any one of sort/subtotal criteria is not valid, return false
        /// if more than 3 sort/subtotal criteria found, return false
        /// </summary>
        /// <param name="glAccountStructure">gl account structure</param>
        /// <param name="criteria">finance query criteria</param>
        /// <returns></returns>
        private bool ValidateSortCriteria(GeneralLedgerAccountStructure glAccountStructure, dtoDomain.FinanceQueryCriteria criteria)
        {
            if (criteria.ComponentSortCriteria != null && criteria.ComponentSortCriteria.Any())
            {
                foreach (var component in criteria.ComponentSortCriteria)
                {
                    if (glAccountStructure.MajorComponents.All(x => x.ComponentName != component.ComponentName) && glAccountStructure.Subcomponents.All(x => x.ComponentName != component.ComponentName))
                        return false;
                }

                if (criteria.ComponentSortCriteria.Count > 3)
                    return false;
            }
            return true;
        }

        //GL Sub-component dictionary to fetch start position and length of each sub components in GL account structure.
        private Dictionary<string, Tuple<int, int>> BuildGlComponentStructureDictionary(GeneralLedgerAccountStructure generalLedgerAccountStructure)
        {
            Dictionary<string, Tuple<int, int>> glSubComponentStructureDictionary = new Dictionary<string, Tuple<int, int>>();

            foreach (var subComponent in generalLedgerAccountStructure.Subcomponents)
            {
                var key = subComponent.ComponentName;
                if (!glSubComponentStructureDictionary.ContainsKey(key))
                {
                    glSubComponentStructureDictionary.Add(key, new Tuple<int, int>(subComponent.StartPosition, subComponent.ComponentLength));
                }
            }

            return glSubComponentStructureDictionary;
        }

        private string GetComponentNameValue(string componentName, string glAccountNumber, Dictionary<string, Tuple<int, int>> glSubComponentStructureDictionary)
        {
            string componentValue;
            Tuple<int, int> subComponentValue;
            glSubComponentStructureDictionary.TryGetValue(componentName, out subComponentValue);
            if (subComponentValue == null)
            {
                throw new ApplicationException("gl component used for sort/subtotal is not found in gl account structure");
            }
            else
            {
                componentValue = glAccountNumber.Substring(subComponentValue.Item1, subComponentValue.Item2);
                componentName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(componentName.ToLower());
                componentValue = componentName + "-" + componentValue;
            }

            return componentValue;
        }

        private void AssignGlAccountDescription(FinanceQueryGlAccountLineItem glAccountItem, Dictionary<string, string> glAccountDescriptionsDictionary)
        {
            glAccountItem.GlAccount.GlAccountDescription = GetGlAccountDescription(glAccountItem.GlAccountNumber, glAccountDescriptionsDictionary);

            if (!glAccountItem.IsUmbrellaAccount || !glAccountItem.Poolees.Any()) return;
            foreach (var poolee in glAccountItem.Poolees)
            {
                poolee.GlAccountDescription = GetGlAccountDescription(poolee.GlAccountNumber, glAccountDescriptionsDictionary);
            }
        }

        private string GetGlAccountDescription(string glAccountNumber, Dictionary<string, string> glAccountDescriptionsDictionary)
        {
            string description = "";
            if (!string.IsNullOrEmpty(glAccountNumber))
            {
                glAccountDescriptionsDictionary.TryGetValue(glAccountNumber, out description);
            }
            return description ?? string.Empty;
        }


        private string BuildSortKeyForGlAccount(string glAccountNumber, List<FinanceQueryComponentSortCriteria> sortCriteria, Dictionary<string, Tuple<int, int>> glSubComponentStructureDictionary)
        {
            string componentValue = string.Empty;
            foreach (var item in sortCriteria)
            {
                Tuple<int, int> subComponentValue;
                glSubComponentStructureDictionary.TryGetValue(item.ComponentName, out subComponentValue);
                if (subComponentValue == null)
                    throw new ApplicationException("gl component used for sort/subtotal is not found in gl account structure");
                else
                {
                    var value = glAccountNumber.Substring(subComponentValue.Item1, subComponentValue.Item2);
                    if (string.IsNullOrEmpty(componentValue))
                        componentValue = value;
                    else
                        componentValue = componentValue + "-" + value;
                }
            }

            return componentValue;
        }

        private FinanceQuerySubtotal AddSubtotal(string breakValue, List<FinanceQueryGlAccountLineItem> subtotalGlAccounts)
        {
            FinanceQuerySubtotal subtotal = new FinanceQuerySubtotal();
            var subtotalComponents = new List<FinanceQuerySubtotalComponent> { AddSubtotalComponent(breakValue, subtotalGlAccounts) };
            subtotal.FinanceQueryGlAccountLineItems = subtotalGlAccounts;
            subtotal.FinanceQuerySubtotalComponents = subtotalComponents; 
            return subtotal;
        }

        private FinanceQuerySubtotalComponent AddSubtotalComponent(string breakValue, List<FinanceQueryGlAccountLineItem> subtotalGlAccounts)
        {
            string[] nameValueStrings = breakValue.Split(new[] { "-" }, StringSplitOptions.None);
            if (nameValueStrings != null && nameValueStrings.Length != 2)
                throw new ApplicationException("invalid subtotal component name & values for subtotal.");

            return new FinanceQuerySubtotalComponent(nameValueStrings[0], nameValueStrings[1], subtotalGlAccounts);
        }

        /// <summary>
        /// assigns the subcomponent description from the dictionary to finance query entity
        /// </summary>
        /// <param name="financeQueryEntity"></param>
        /// <param name="componentDescriptionDictionary"></param>
        /// <param name="glAccountStructure"></param>
        private void AssignComponentDescriptionToFinanceQueryEntity(FinanceQuery financeQueryEntity, Dictionary<GeneralLedgerComponentType, Dictionary<string, string>> componentDescriptionDictionary, GeneralLedgerAccountStructure glAccountStructure)
        {
            //iterate through the entity and assign the component descriptions for the subtotal items
            foreach (var item in financeQueryEntity.FinanceQuerySubtotals)
            {
                foreach (var subtotalItem in item.FinanceQuerySubtotalComponents)
                {
                    // try to get the subcomponent of current subtotal item
                    var subtotalComponent = glAccountStructure.Subcomponents.FirstOrDefault(x => x.ComponentName.ToLower() == subtotalItem.SubtotalComponentName.ToLower());
                    if (subtotalComponent != null)
                    {
                        Dictionary<string, string> subcomponentDescriptionDictionary;
                        if (componentDescriptionDictionary.TryGetValue(subtotalComponent.ComponentType, out subcomponentDescriptionDictionary))
                        {
                            string description = string.Empty;
                            // from the subcomponent description dictionary get the description for the current subtotal component value
                            if(subcomponentDescriptionDictionary.TryGetValue(subtotalItem.SubtotalComponentValue,out description)){
                                subtotalItem.SubtotalComponentDescription = description;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// this method will return component description dictionary where 'major component type' as key and 'a dictionary of subcomponent values 
        /// and respective subcomponent description' as value 
        /// </summary>
        /// <param name="glAccountStructure"></param>
        /// <param name="subtotalCriteriaComponentValuesDictionary"></param>
        /// <param name="onlySubTotalCriteria"></param>
        /// <returns></returns>
        private async Task<Dictionary<GeneralLedgerComponentType, Dictionary<string, string>>> CreateComponentTypeDescriptionDictionaryAsync(GeneralLedgerAccountStructure glAccountStructure, Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary, IEnumerable<FinanceQueryComponentSortCriteria> onlySubTotalCriteria)
        {
            // extract all the subtotal components with its major component types information
            var subComponentObjects = glAccountStructure.Subcomponents.Join(onlySubTotalCriteria, ct => ct.ComponentName.ToLower(), sc => sc.ComponentName.ToLower(),
                                    (ct, sc) => new { MajorComponentType = ct.ComponentType, ComponentName = ct.ComponentName }).Distinct();
            // select all distinct component types from the extracted list
            var distinctComponentTypes = subComponentObjects.Select(x => x.MajorComponentType).Distinct();

            //declare a wrapper dictionary with 'component type' as key and 'dictionary of component values and its respective description'  as value
            Dictionary<GeneralLedgerComponentType, Dictionary<string, string>> componentTypeToValueDescriptionDictionary = new Dictionary<GeneralLedgerComponentType, Dictionary<string, string>>();

            // iterate through each of the component types and consolidate all the component values available from its subcomponents
            foreach (var currentComponentType in distinctComponentTypes)
            {
                // the subcomponent values from each component type will be consolidated in this list inorder to call repository
                List<string> glComponentIds = new List<string>();

                //get all subcomponent for the current component type
                var subcomponents = subComponentObjects.Where(x => x.MajorComponentType == currentComponentType);
                string componentKey = string.Empty;
                //iterate through all subcomponents for the current component type
                foreach (var currentSubcomponent in subcomponents)
                {
                    if (subtotalCriteriaComponentValuesDictionary.ContainsKey(currentSubcomponent.ComponentName))
                    {
                        // the current subcomponent values are added to a list of consolidated subcomponent values for current component type
                        glComponentIds.AddRange(subtotalCriteriaComponentValuesDictionary[currentSubcomponent.ComponentName]);
                    }
                }
                // calling the repository with major component type and list of subcomponent values and expected to get the dictionary with  'component value' as key and 'component descriptions' as value
                // for e.g., we will get a dictionary with all subcomponent values and corresponding descriptions according to the current component type 
                // if type is 'Fund', dictionary returned would look like   <1,Abc>, <2,Def>, ..... ,<34,Xyz> or 
                // and if type is 'Object', dictionary returned would look like  <123,pqr>,<1234,stu>,...,<12345,rst>
                var currentComponentTypeDescriptionDictionary = await generalLedgerAccountRepository.GetGlComponentDescriptionsByIdsAndComponentTypeAsync(glComponentIds, currentComponentType);
                if (currentComponentTypeDescriptionDictionary.Any())
                {
                    // add the returned dictionary into a wrapper dictionary with 'current component type' as key and  'currentDescriptionDictionary' as value
                    // for e.g., the wrapper dictionary would look like
                    // <'Fund' ,  <1,Abc>, <2,Def>, ..... , <34,Xyz>>  ,   <'Object' ,   <123,pqr>,  <1234,stu>, ......  <12345,rst>>
                    componentTypeToValueDescriptionDictionary.Add(currentComponentType, currentComponentTypeDescriptionDictionary);
                }
            }
            // return the wrapper dictionary 
            return componentTypeToValueDescriptionDictionary;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Fetch list of unique sort criteria , if any duplicates found get the one with subtotal checked.
        /// </summary>
        /// <param name="sortCriteriaList">list of sort criteria</param>
        /// <returns></returns>
        public List<FinanceQueryComponentSortCriteria> GetUniqueSortCriteria(List<FinanceQueryComponentSortCriteria> sortCriteriaList)
        {
            List<FinanceQueryComponentSortCriteria> uniqueList = new List<FinanceQueryComponentSortCriteria>();
            if (sortCriteriaList != null && sortCriteriaList.Any())
            {
                uniqueList = sortCriteriaList.Where(x => !string.IsNullOrEmpty(x.ComponentName))
                    .GroupBy(x => x.ComponentName)
                    .Select(x => x.OrderBy(y => y.Order))
                    .Select(x => x.OrderBy(y => y.IsDisplaySubTotal.HasValue && y.IsDisplaySubTotal.Value ? 0 : 1).First())
                    .ToList().OrderBy(x => x.Order).ToList();
            }
            return uniqueList;
        }

        /// <summary>
        /// Create a list of subtotals, each subtotal entity will have list of GL accounts and respective subtotal components.
        /// </summary>
        /// <param name="filteredFinanceQueryGlAccountLineItems">list of finance query gl account line items</param>
        /// <param name="subtotalCriteriaList">list of subtotal criterias</param>
        /// <param name="glAccountStructure">gl account structure</param>
        /// <param name="glSubComponentStructureDictionary">gl subcomponent dictionary contains component name as key & value would be start index & length</param>
        /// <returns>finance query object</returns>
        public FinanceQuery BuildFinanceQueryWithSubtotalEntities(List<FinanceQueryGlAccountLineItem> filteredFinanceQueryGlAccountLineItems, List<FinanceQueryComponentSortCriteria> subtotalCriteriaList, GeneralLedgerAccountStructure glAccountStructure, Dictionary<string, Tuple<int, int>> glSubComponentStructureDictionary, Dictionary<string, List<string>> subtotalCriteriaComponentValuesDictionary)
        {
            FinanceQuery financeQuery;
            
            var onlySubTotalCriteria = subtotalCriteriaList.Where(x => x.IsDisplaySubTotal.HasValue && x.IsDisplaySubTotal.Value).OrderBy(x => x.Order).ToList();
            //if no subtotal criteria/s, assign list of gl accounts to subtotal entity & return finance query entity.
            if (!onlySubTotalCriteria.Any())
            {
                financeQuery = new FinanceQuery { FinanceQuerySubtotals = new List<FinanceQuerySubtotal>() };
                var subtotal = new FinanceQuerySubtotal
                {
                    FinanceQuerySubtotalComponents = new List<FinanceQuerySubtotalComponent>(),
                    FinanceQueryGlAccountLineItems = filteredFinanceQueryGlAccountLineItems
                };
                financeQuery.FinanceQuerySubtotals.Add(subtotal);
                return financeQuery;
            }
            else
            {
                financeQuery = new FinanceQuery { FinanceQuerySubtotals = new List<FinanceQuerySubtotal>() };
                //List to store values  for each of the subtotal level component
                List<string> subtotalCriteriaLevel1Values = new List<string>();
                List<string> subtotalCriteriaLevel2Values = new List<string>();
                List<string> subtotalCriteriaLevel3Values = new List<string>();
                List<FinanceQueryGlAccountLineItem> subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                List<FinanceQueryGlAccountLineItem> subtotalGlAccountLevel2 = new List<FinanceQueryGlAccountLineItem>();
                List<FinanceQueryGlAccountLineItem> subtotalGlAccountLevel3 = new List<FinanceQueryGlAccountLineItem>();
                string[] currentBreak = new string[onlySubTotalCriteria.Count];
                string[] nextBreak = new string[onlySubTotalCriteria.Count];

                for (int index = 0; index < filteredFinanceQueryGlAccountLineItems.Count; index++)
                {
                    var currentGlAccount = filteredFinanceQueryGlAccountLineItems[index];
                    bool lastItem = index == filteredFinanceQueryGlAccountLineItems.Count - 1;

                    //fetch current gl account break component values
                    for (var j = 0; j < onlySubTotalCriteria.Count; j++)
                    {
                        currentBreak[j] = GetComponentNameValue(onlySubTotalCriteria[j].ComponentName, currentGlAccount.GlAccountNumber, glSubComponentStructureDictionary);
                    }

                    //fetch next gl account break component values
                    if (!lastItem)
                    {
                        var nextGlAccount = filteredFinanceQueryGlAccountLineItems[index + 1];
                        for (var j = 0; j < onlySubTotalCriteria.Count; j++)
                        {
                            nextBreak[j] = GetComponentNameValue(onlySubTotalCriteria[j].ComponentName, nextGlAccount.GlAccountNumber, glSubComponentStructureDictionary);
                        }
                    }


                    switch (onlySubTotalCriteria.Count)
                    {
                        case 1:
                            {                            
                                subtotalGlAccounts.Add(currentGlAccount);
                                //create a level 1 value list with the iteration
                                subtotalCriteriaLevel1Values.Add(currentBreak[0].Split('-')[1]);
                                //when current & next break component value doesn't match, add subtotal component for current break value & assign list of gl accounts.
                                if (string.Compare(currentBreak[0], nextBreak[0], StringComparison.OrdinalIgnoreCase) != 0 || lastItem)
                                {
                                    financeQuery.FinanceQuerySubtotals.Add(AddSubtotal(currentBreak[0], subtotalGlAccounts));
                                    subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                }
                                break;
                            }
                        case 2:
                            {
                                subtotalGlAccounts.Add(currentGlAccount);
                                subtotalGlAccountLevel2.Add(currentGlAccount);
                                //create a level 1 value list with the iteration
                                subtotalCriteriaLevel1Values.Add(currentBreak[0].Split('-')[1]);
                                //create a level 2 value list with the iteration
                                subtotalCriteriaLevel2Values.Add(currentBreak[1].Split('-')[1]);
                                //when first current & next break component value doesn't match, add subtotal component for first & second current break value in reverse & assign list of gl accounts.
                                if (string.Compare(currentBreak[0], nextBreak[0], StringComparison.OrdinalIgnoreCase) != 0 || lastItem)
                                {
                                    FinanceQuerySubtotal subtotal = AddSubtotal(currentBreak[1], subtotalGlAccounts);
                                    subtotal.FinanceQuerySubtotalComponents.Add(AddSubtotalComponent(currentBreak[0], subtotalGlAccountLevel2));
                                    financeQuery.FinanceQuerySubtotals.Add(subtotal);
                                    subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                    subtotalGlAccountLevel2 = new List<FinanceQueryGlAccountLineItem>();
                                }

                                //when second current & next break component value doesn't match, add subtotal component for second current break value & assign list of gl accounts.
                                else
                                {
                                    if (string.Compare(currentBreak[1], nextBreak[1], StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        financeQuery.FinanceQuerySubtotals.Add(AddSubtotal(currentBreak[1], subtotalGlAccounts));
                                        subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                    }
                                }

                                break;
                            }

                        case 3:
                            {
                                subtotalGlAccounts.Add(currentGlAccount);
                                subtotalGlAccountLevel2.Add(currentGlAccount);
                                subtotalGlAccountLevel3.Add(currentGlAccount);
                                //create a level 1 value list with the iteration
                                subtotalCriteriaLevel1Values.Add(currentBreak[0].Split('-')[1]);
                                //create a level 2 value list with the iteration
                                subtotalCriteriaLevel2Values.Add(currentBreak[1].Split('-')[1]);
                                //create a level 3 value list with the iteration
                                subtotalCriteriaLevel3Values.Add(currentBreak[2].Split('-')[1]);
                                //when first current & next break component value doesn't match, add subtotal component for first, second & third current break value in reverse & assign list of gl accounts.
                                if (string.Compare(currentBreak[0], nextBreak[0], StringComparison.OrdinalIgnoreCase) != 0 || lastItem)
                                {
                                    FinanceQuerySubtotal subtotal = AddSubtotal(currentBreak[2], subtotalGlAccounts);
                                    subtotal.FinanceQuerySubtotalComponents.Add(AddSubtotalComponent(currentBreak[1], subtotalGlAccountLevel2));
                                    subtotal.FinanceQuerySubtotalComponents.Add(AddSubtotalComponent(currentBreak[0], subtotalGlAccountLevel3));
                                    financeQuery.FinanceQuerySubtotals.Add(subtotal);
                                    subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                    subtotalGlAccountLevel2 = new List<FinanceQueryGlAccountLineItem>();
                                    subtotalGlAccountLevel3 = new List<FinanceQueryGlAccountLineItem>();
                                }
                                else
                                {
                                    //when second current & next break component value doesn't match, add subtotal component for second, last/third current break value & assign list of gl accounts.
                                    if (string.Compare(currentBreak[1], nextBreak[1], StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        FinanceQuerySubtotal subtotal = AddSubtotal(currentBreak[2], subtotalGlAccounts);
                                        subtotal.FinanceQuerySubtotalComponents.Add(AddSubtotalComponent(currentBreak[1], subtotalGlAccountLevel2));
                                        financeQuery.FinanceQuerySubtotals.Add(subtotal);
                                        subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                        subtotalGlAccountLevel2 = new List<FinanceQueryGlAccountLineItem>();
                                    }
                                    //when last current & next break component value doesn't match, add subtotal component for last/third current break value & assign list of gl accounts.
                                    else
                                    {
                                        if (string.Compare(currentBreak[2], nextBreak[2], StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            financeQuery.FinanceQuerySubtotals.Add(AddSubtotal(currentBreak[2], subtotalGlAccounts));
                                            subtotalGlAccounts = new List<FinanceQueryGlAccountLineItem>();
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
                //create 3 levels of dictionary for subtotal components with component name as key and list of values as value
                for (int i = 0; i < onlySubTotalCriteria.Count; i++)
                {
                    if (i == 0)
                        subtotalCriteriaComponentValuesDictionary.Add(onlySubTotalCriteria[i].ComponentName, subtotalCriteriaLevel1Values.Distinct().ToList());
                    else
                    {
                        if (i == 1)
                            subtotalCriteriaComponentValuesDictionary.Add(onlySubTotalCriteria[i].ComponentName, subtotalCriteriaLevel2Values.Distinct().ToList());
                        else
                            subtotalCriteriaComponentValuesDictionary.Add(onlySubTotalCriteria[i].ComponentName, subtotalCriteriaLevel3Values.Distinct().ToList());
                    }
                }
            }

            return financeQuery;

        }
        #endregion

    }
}
