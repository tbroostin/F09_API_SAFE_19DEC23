//Copyright 2013-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Provides read-only access to fundamental FinancialAid data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinancialAidReferenceDataRepository : BaseColleagueRepository, IFinancialAidReferenceDataRepository
    {

        private Data.Base.DataContracts.IntlParams internationalParameters;

        /// <summary>
        /// Constructor for the FinancialAidReferenceDataRepository. 
        /// CacheTimeout value is set for Level1
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public FinancialAidReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }
        
        /// <summary>
        /// Get all SAP.APPEALS.CODES from Colleague. This accessor caches all the categories.
        /// </summary>
        public async Task<IEnumerable<AcademicProgressAppealCode>> GetAcademicProgressAppealCodesAsync()
        {
            return await GetCodeItemAsync<SapAppealsCodes, AcademicProgressAppealCode>("AllAcademicProgressAppealCodes", "SAP.APPEALS.CODES",
                    apac =>
                    {
                        return new AcademicProgressAppealCode(apac.Recordkey, apac.SapacDesc);
                    }
                );

        }

        /// <summary>
        /// Public Accessor for Financial Aid Awards. Retrieves and caches all awards defined
        /// in Colleague. 
        /// This code is also used in the StudentAwardRepository. Any changes here should also
        /// be copied to that class.
        /// </summary>
        public IEnumerable<Award> Awards
        {
            get
            {
                return GetOrAddToCache<IEnumerable<Award>>("AllAwards",
                    () =>
                    {
                        var awardList = new List<Award>();
                        var awardRecords = DataReader.BulkReadRecord<Awards>("", false);
                        foreach (var awardRecord in awardRecords)
                        {
                            var explanation = awardRecord.AwExplanationText;
                            char _VM = Convert.ToChar(DynamicArray.VM);

                            if (!string.IsNullOrEmpty(explanation))
                            {
                                explanation = FormatString(explanation, _VM);
                            }
                            

                            ShoppingSheetAwardGroup? shoppingSheetGroup = null;
                            if (!string.IsNullOrEmpty(awardRecord.AwShopsheetGroup))
                            {
                                switch (awardRecord.AwShopsheetGroup.ToUpper())
                                {
                                    case "SC":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.SchoolGrants;
                                        break;
                                    case "PL":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.PellGrants;
                                        break;
                                    case "ST":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.StateGrants;
                                        break;
                                    case "OT":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.OtherGrants;
                                        break;
                                    case "WS":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.WorkStudy;
                                        break;
                                    case "PK":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.PerkinsLoans;
                                        break;
                                    case "DS":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.SubsidizedLoans;
                                        break;
                                    case "DU":
                                        shoppingSheetGroup = ShoppingSheetAwardGroup.UnsubsidizedLoans;
                                        break;
                                };
                            }

                            //TODO: Change this to a real Async when Awards method is converted. This is a temporary workaround.
                            var awardCategory = Task.Run(async () => 
                                {
                                    return await GetAwardCategoriesAsync();
                                }).GetAwaiter().GetResult()
                                .Where(c => c.Code == awardRecord.AwCategory).FirstOrDefault();
                            try
                            {
                                var award = new Award(awardRecord.Recordkey, awardRecord.AwDescription, awardCategory, explanation)
                                    {
                                        IsFederalDirectLoan = (!string.IsNullOrEmpty(awardRecord.AwDlLoanType)),
                                        Type = awardRecord.AwType,
                                        ShoppingSheetGroup = shoppingSheetGroup,
                                        AwRenewableFlag = awardRecord.AwRenewableFlag,
                                        AwRenewableText = awardRecord.AwRenewableText
                                    };
                                awardList.Add(award);
                            }
                            catch (Exception e)
                            {                                
                                LogDataError("AWARDS", awardRecord.Recordkey, awardRecord, e, string.Format("Failed to add award {0}", awardRecord.Recordkey));
                            }
                        }
                        return awardList;
                    });
            }
        }

        /// <summary>
        /// Formats a string: replaces any value marks with new lines and spaces;
        /// removes any accidental spaces or new lines from included links if any exist
        /// </summary>
        /// <param name="stringToFormat">string to format</param>
        /// <param name="_VM">value mark</param>
        /// <returns>formatted string</returns>
        private static string FormatString(string stringToFormat, char _VM)
        {
            // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
            stringToFormat = stringToFormat.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
            // If there is a single-VM, replace it with a space.
            stringToFormat = stringToFormat.Replace(_VM, ' ');

            //Remove spaces from urls (if any)
            if (stringToFormat.Contains("<a href="))
            {
                int strLen = stringToFormat.Length;
                int formattedStrLen = strLen;

                //Difference in length between the initial string length and each time it is formatted
                int difference = 0;

                //Indices before and after a url
                int urlStartIndex = 0;
                int indexAfterUrlEnd = 0;

                var urls = Regex.Matches(stringToFormat, @"href=[""']([^""'])+[""']");

                foreach (Match url in urls)
                {
                    foreach (Capture capture in url.Captures)
                    {
                        var formattedUrl = capture.Value.ToString().Replace(" ", "").Replace(Environment.NewLine, "");

                        urlStartIndex = capture.Index - difference;
                        indexAfterUrlEnd = capture.Index + capture.Length - difference;

                        if ((urlStartIndex >= 0 && urlStartIndex < formattedStrLen) && (indexAfterUrlEnd > 0 && indexAfterUrlEnd < formattedStrLen) && (urlStartIndex < indexAfterUrlEnd))
                        {
                            stringToFormat = stringToFormat.Substring(0, urlStartIndex) + formattedUrl + stringToFormat.Substring(indexAfterUrlEnd);
                            formattedStrLen = stringToFormat.Length;
                            difference = strLen - formattedStrLen;
                        }

                    }
                }
            }
            return stringToFormat;
        }

        /// <summary>
        /// Get all AwardCategories from Colleague. This accessor caches all the categories.
        /// </summary>
        [Obsolete("Obsolete as of Api version 1.14, use GetAwardCategoriesAsync")]
        public IEnumerable<AwardCategory> AwardCategories
        {
            get
            {
                return GetCodeItem<AwardCategories, AwardCategory>("AllAwardCategories", "AWARD.CATEGORIES",
                    ac =>
                    {
                        AwardCategoryType? type = null;
                        var typeArray = new string[4] { ac.AcLoanFlag, ac.AcGrantFlag, ac.AcScholarshipFlag, ac.AcWorkFlag };

                        //is exactly one of the flags equal to Yes?
                        if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                        {
                            if (!string.IsNullOrEmpty(ac.AcLoanFlag) && ac.AcLoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                            else if (!string.IsNullOrEmpty(ac.AcGrantFlag) && ac.AcGrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                            else if (!string.IsNullOrEmpty(ac.AcScholarshipFlag) && ac.AcScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                            else if (!string.IsNullOrEmpty(ac.AcWorkFlag) && ac.AcWorkFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                        }

                        return new AwardCategory(ac.Recordkey, ac.AcDescription, type);
                    }
                );
            }
        }

        public async Task<IEnumerable<AwardCategory>> GetAwardCategoriesAsync()
        {
            
                return await GetCodeItemAsync<AwardCategories, AwardCategory>("AllAwardCategories", "AWARD.CATEGORIES",
                    ac =>
                    {
                        AwardCategoryType? type = null;
                        var typeArray = new string[4] { ac.AcLoanFlag, ac.AcGrantFlag, ac.AcScholarshipFlag, ac.AcWorkFlag };

                        //is exactly one of the flags equal to Yes?
                        if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                        {
                            if (!string.IsNullOrEmpty(ac.AcLoanFlag) && ac.AcLoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                            else if (!string.IsNullOrEmpty(ac.AcGrantFlag) && ac.AcGrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                            else if (!string.IsNullOrEmpty(ac.AcScholarshipFlag) && ac.AcScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                            else if (!string.IsNullOrEmpty(ac.AcWorkFlag) && ac.AcWorkFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                        }

                        return new AwardCategory(ac.Recordkey, ac.AcDescription, type);
                    }
                );
            
        }


        /// <summary>
        /// Public Accessor for Financial Aid AwardStatuses. Retrieves and caches all award statuses 
        /// defined in Colleague.
        /// Each status category in Colleague maps to one of three categories in the API. There are 5:
        /// Colleague Accepted = Accepted
        /// Colleague Pending = Pending
        /// Colleague Estimated = Pending
        /// Colleague Rejected = Rejected
        /// Colleague Denied = Rejected
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException">Thrown when Colleague data contains an unexpected category.</exception>
        public IEnumerable<AwardStatus> AwardStatuses
        {
            get
            {
                return GetCodeItem<AwardActions, AwardStatus>("AllAwardStatuses", "AWARD.ACTIONS",
                    a =>
                    {
                        AwardStatusCategory cat;
                        switch (a.AaCategory)
                        {
                            case "A":
                                cat = AwardStatusCategory.Accepted;
                                break;
                            case "P":
                                cat = AwardStatusCategory.Pending;
                                break;
                            case "E":
                                cat = AwardStatusCategory.Estimated;
                                break;
                            case "R":
                                cat = AwardStatusCategory.Rejected;
                                break;
                            case "D":
                                cat = AwardStatusCategory.Denied;
                                break;
                            default:
                                cat = AwardStatusCategory.Rejected;
                                break;
                        }

                        return new AwardStatus(a.Recordkey, a.AaDescription, cat);
                    });
            }
        }

        /// <summary>
        /// Public accessor for the Award Types validation table.
        /// </summary>
        public IEnumerable<AwardType> AwardTypes
        {
            get
            {
                var awardTypes = GetValcode<AwardType>("ST", "AWARD.TYPES",
                    at =>
                    {
                        try
                        {
                            return new AwardType(at.ValInternalCodeAssocMember, at.ValExternalRepresentationAssocMember);
                        }
                        catch (Exception e)
                        {
                            // Log and return null for codes without a description.
                            LogDataError("AWARD.TYPES", at.ValInternalCodeAssocMember, null, e, string.Format("Failed to add award type {0}", at.ValInternalCodeAssocMember));
                            return null;
                        }
                    }
                );
                // Exclude nulls from codes without a description.
                return awardTypes.Where(at => at != null).ToList(); ;
            }
        }

        /// <summary>
        /// Public Accessor for Financial Aid Award Years. Retrieves and caches all award years
        /// defined for Financial Aid in Colleague. 
        /// List contains all years defined in the FA.SUITES entity.
        /// </summary>
        public IEnumerable<AwardYear> AwardYears
        {
            get
            {
                var awardYears = GetOrAddToCache<IEnumerable<AwardYear>>("AllAwardYears",
                    () =>
                    {
                        //list of awardyear objects to cache
                        var awardYearList = new List<AwardYear>();

                        //list of years from FA.SUITES
                        //exclude deleted suites?
                        var awardYearData = DataReader.BulkReadRecord<FaSuites>("");

                        if (awardYearData == null)
                        {
                            return awardYearList;
                        }

                        var invalidYearStatuses = new string[] {"D", "O", "A"};
                        var validYears = awardYearData.Where(y => !invalidYearStatuses.Contains(y.FaSuitesStatus));
                        foreach (var year in validYears)
                        {
                            try
                            {
                                awardYearList.Add(new AwardYear(year.Recordkey, year.Recordkey));
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, e.Message);
                            }
                        }

                        // Cached data must be serializable, so convert the sorted object to a list
                        return awardYearList.OrderBy(ay => ay.Code).ToList();
                    }
                );

                return awardYears;
            }
        }

        /// <summary>
        /// Create the public accessor for the FA Award Periods. Get all award periods from Colleague.
        /// </summary>
        public IEnumerable<AwardPeriod> AwardPeriods
        {
            get
            {
                return GetCodeItem<AwardPeriods, AwardPeriod>("AllAwardPeriods", "AWARD.PERIODS",
                    m =>
                    {
                        var startDate = m.AwdpStartDate.Value.Date;
                        return new AwardPeriod(m.Recordkey, m.AwdpDesc, startDate);
                    }
                );
            }
        }

        /// <summary>
        /// Public accessor to get the list of external links that a student might need
        /// to research or complete financial aid tasks.
        /// </summary>
        public IEnumerable<Link> Links
        {
            get
            {
                var links = GetOrAddToCache<IEnumerable<Link>>("AllLinks",
                    () =>
                    {
                        //Get the links sorted by the link position in the list
                        string criteria = "BY FAHUB.LINK.POS";
                        var linkData = DataReader.BulkReadRecord<FahubLinks>(criteria);

                        if (linkData == null)
                        {
                            return new List<Link>();
                        }

                        var linksList = new List<Link>();
                        foreach (var link in linkData)
                        {
                            string title = link.FahubLinkTitle;

                            LinkTypes type;
                            switch (link.FahubLinkType.ToUpper())
                            {
                                case "FAFSA":
                                    type = LinkTypes.FAFSA;
                                    break;
                                case "PROFILE":
                                    type = LinkTypes.PROFILE;
                                    break;
                                case "FORECASTER":
                                    type = LinkTypes.Forecaster;
                                    break;
                                case "ENTRINT":
                                    type = LinkTypes.EntranceInterview;
                                    break;
                                case "MPN":
                                    type = LinkTypes.MPN;
                                    break;
                                case "NSLDS":
                                    type = LinkTypes.NSLDS;
                                    break;
                                case "PLUS":
                                    type = LinkTypes.PLUS;
                                    break;
                                case "FORM":
                                    type = LinkTypes.Form;
                                    break;
                                case "USER":
                                    type = LinkTypes.User;
                                    break;
                                case "SAP":
                                    type = LinkTypes.SatisfactoryAcademicProgress;
                                    break;
                                default:
                                    type = LinkTypes.User;
                                    break;
                            }

                            string url = link.FahubLinkUrl;
                            var linkRecord = new Link(title, type, url);                            
                            
                            linksList.Add(linkRecord);
                        }
                        return linksList;
                    }
                );
                return links;
            }
        }


        /// <summary>
        /// Public accessor to get a list of BudgetComponents
        /// Note that we retrieve only budget components from 2011 and later because 
        /// only the Financial Aid Shopping Sheet uses budget components at this point, and it
        /// was only implemented in 2012.
        /// When we need budget components for earlier years, change the year value used below
        /// </summary>
        public IEnumerable<BudgetComponent> BudgetComponents
        {
            get
            {
                return GetOrAddToCache<IEnumerable<BudgetComponent>>("AllBudgetComponents",
                    () =>
                    {
                        var allBudgetComponents = new List<BudgetComponent>();

                        var budgetYears = AwardYears.Where(y => y.YearAsNumber >= 2011).ToList();
                        foreach (var awardYear in budgetYears)
                        {
                            var acyrFile = "FBC." + awardYear.Code;
                            try
                            {
                                var budgetRecords = DataReader.BulkReadRecord<FbcAcyr>(acyrFile, "");
                                if (budgetRecords != null && budgetRecords.Count() > 0)
                                {
                                    foreach (var budgetRecord in budgetRecords)
                                    {
                                        ShoppingSheetBudgetGroup? shoppingSheetGroup = null;
                                        if (!string.IsNullOrEmpty(budgetRecord.FbcShopsheetGroup))
                                        {
                                            switch (budgetRecord.FbcShopsheetGroup.ToUpper())
                                            {
                                                case "TF":
                                                    shoppingSheetGroup = ShoppingSheetBudgetGroup.TuitionAndFees;
                                                    break;
                                                case "HM":
                                                    shoppingSheetGroup = ShoppingSheetBudgetGroup.HousingAndMeals;
                                                    break;
                                                case "BS":
                                                    shoppingSheetGroup = ShoppingSheetBudgetGroup.BooksAndSupplies;
                                                    break;
                                                case "TP":
                                                    shoppingSheetGroup = ShoppingSheetBudgetGroup.Transportation;
                                                    break;
                                                case "OC":
                                                    shoppingSheetGroup = ShoppingSheetBudgetGroup.OtherCosts;
                                                    break;
                                            }
                                        }
                                        try
                                        {
                                            allBudgetComponents.Add(
                                                new BudgetComponent(awardYear.Code, budgetRecord.Recordkey, budgetRecord.FbcDesc)
                                                {
                                                    ShoppingSheetGroup = shoppingSheetGroup,
                                                    CostType = CalculateCostType(budgetRecord)
                                                });
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Error(e, "Error creating budget object for year " + awardYear.Code + " id " + budgetRecord.Recordkey);
                                        }
                                    }
                                }
                            }
                            catch(ColleagueDataReaderException cdre)
                            {
                                string message = string.Format("There was an error reading {0} file.", acyrFile);
                                LogDataError("FBCAcyr", acyrFile, null, cdre, message);
                            }
                        }
                        return allBudgetComponents;
                    });
            }
        }

        /// <summary>
        /// Calulates budget component cost type
        /// </summary>
        /// <param name="budgetRecord">budget record to calculate the type for</param>
        /// <returns>BudgetComponentCostType or null</returns>
        private BudgetComponentCostType? CalculateCostType(FbcAcyr budgetRecord)
        {
            if (budgetRecord != null && !string.IsNullOrEmpty(budgetRecord.FbcCostIndicator)) {
                if (budgetRecord.FbcCostIndicator.ToUpper() == "D")
                {
                    return BudgetComponentCostType.Direct;
                }
                else if (budgetRecord.FbcCostIndicator.ToUpper() == "I")
                {
                    return BudgetComponentCostType.Indirect;
                }
                else return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Public accessor to get a list of Checklist Items that could be assigned to a student
        /// </summary>
        public IEnumerable<ChecklistItem> ChecklistItems
        {
            get
            {
                var items = GetOrAddToCache<IEnumerable<ChecklistItem>>("AllChecklistItems",
                    () =>
                    {
                        var itemData = DataReader.BulkReadRecord<FaChecklistItems>("");

                        if (itemData == null || !itemData.Any())
                        {
                            return new List<ChecklistItem>();
                        }

                        var itemsList = new List<ChecklistItem>();
                        foreach (var item in itemData)
                        {
                            ChecklistItemType? itemtype = null;

                            switch (item.Recordkey.ToUpper())
                            {
                                case "FAFSA":
                                    itemtype = ChecklistItemType.FAFSA;
                                    break;
                                case "PROFILE":
                                    itemtype = ChecklistItemType.PROFILE;
                                    break;
                                case "ACCAWDPKG":
                                    itemtype = ChecklistItemType.ReviewAwardPackage;
                                    break;
                                case "APPLRVW":
                                    itemtype = ChecklistItemType.ApplicationReview;
                                    break;
                                case "CMPLREQDOC":
                                    itemtype = ChecklistItemType.CompletedDocuments;
                                    break;
                                case "SIGNAWDLTR":
                                    itemtype = ChecklistItemType.ReviewAwardLetter;
                                    break;

                            }

                            if (!itemtype.HasValue)
                            {
                                LogDataError("FA.CHECKLIST.ITEMS", item.Recordkey, item, null, "Unknown Checklist item");
                            }
                            else if (!item.FciDisplayPosition.HasValue)
                            {
                                LogDataError("FA.CHECKLIST.ITEMS", item.Recordkey, item, null, "Checklist item does not have a display position.");
                            }

                            else
                            {
                                var itemRecord = new ChecklistItem(item.Recordkey, item.FciDisplayPosition.Value, item.FciSelfServiceDescription);

                                itemRecord.ChecklistItemType = itemtype.Value;

                                itemsList.Add(itemRecord);
                            }
                        }
                        return itemsList;
                    }
                );
                return items;
            }
        }

        /// <summary>
        /// Gets a list of Academic Progress Statuses and their respective
        /// descriptions as well as additional information such as category and explanation. The
        /// Status and Description values come from the SAP.STATUSES code table and the category
        /// and explanation come from a sequentially keyed file called FA.SAP.STATUS.INFO.
        /// Category defines the Academic Process Status as 'Satisfactory', 'Unsatisfactory' or
        /// 'Warning'.  Explanation further describes the Academic Progress status.
        /// </summary>
        public async Task<IEnumerable<AcademicProgressStatus>> GetAcademicProgressStatusesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<AcademicProgressStatus>>("AllAcademicProgressStatuses",
                    async() =>
                    {
                        var sapStatusesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SAP.STATUSES", true);

                        if (sapStatusesValcode == null)
                        {
                            logger.Info("Read Record on the Sap Status Valcode returned null");
                            return new List<AcademicProgressStatus>();
                        }

                        // Read all records from FA.SAP.STATUS.INFO
                        var faSapStatusInfoData = await DataReader.BulkReadRecordAsync<FaSapStatusInfo>("");

                        if (faSapStatusInfoData == null)
                        {
                            faSapStatusInfoData = new Collection<FaSapStatusInfo>();
                        }

                        var academicProgressStatusList = new List<AcademicProgressStatus>();

                        foreach (var sapStatusValue in sapStatusesValcode.ValsEntityAssociation)
                        {
                            try
                            {

                                var academicProgressStatus = new AcademicProgressStatus(sapStatusValue.ValInternalCodeAssocMember, sapStatusValue.ValExternalRepresentationAssocMember);

                                // Find a matching record in FA.SAP.STATUS.INFO using the SAP status 
                                var faSapStatusInfoRec = faSapStatusInfoData.FirstOrDefault(fc => fc.FssiStatus == sapStatusValue.ValInternalCodeAssocMember);

                                if (faSapStatusInfoRec != null)
                                {
                                    AcademicProgressStatusCategory? itemtype = null;

                                    if (faSapStatusInfoRec.FssiCategory != null)
                                    {
                                        switch (faSapStatusInfoRec.FssiCategory.ToUpper())
                                        {
                                            case "S":
                                                itemtype = AcademicProgressStatusCategory.Satisfactory;
                                                break;
                                            case "U":
                                                itemtype = AcademicProgressStatusCategory.Unsatisfactory;
                                                break;
                                            case "W":
                                                itemtype = AcademicProgressStatusCategory.Warning;
                                                break;
                                            case "D":
                                                itemtype = AcademicProgressStatusCategory.DoNotDisplay;
                                                break;
                                        }
                                    }

                                    // Add Category and Explanation properties
                                    academicProgressStatus.Category = itemtype;
                                    
                                    var explanation = faSapStatusInfoRec.FssiExplained;
                                    char _VM = Convert.ToChar(DynamicArray.VM);

                                    if (!string.IsNullOrEmpty(explanation))
                                    {
                                        // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
                                        explanation = explanation.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
                                        // If there is a single-VM, replace it with a space.
                                        explanation = explanation.Replace(_VM, ' ');
                                    }

                                    if (!string.IsNullOrEmpty(explanation))
                                    {
                                        academicProgressStatus.Explanation = explanation;
                                    }

                                }
                                academicProgressStatusList.Add(academicProgressStatus);
                            }
                            catch (Exception e)
                            {
                                LogDataError("SapStatuses.ValsEntityAssociation", sapStatusValue.ValInternalCodeAssocMember, sapStatusValue, e, "Also check FA.SAP.STATUS.INFO record and SAP.STATUSES valcode for data corruption");
                            }
                        }

                        return academicProgressStatusList;
                    }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Public accessor to get all AwardLetter configurations
        /// </summary>
        /// <returns>Collection of AwardLetterConfiguration entities</returns>
        public async Task<IEnumerable<AwardLetterConfiguration>> GetAwardLetterConfigurationsAsync()
        {            
            return  await GetOrAddToCacheAsync<IEnumerable<AwardLetterConfiguration>>("AwardLetterParameters",
                async() =>
                {
                    var awardLetterParametersRecords = new List<AwardLetterConfiguration>();
                    var awardLetterParameters = await DataReader.BulkReadRecordAsync<AltrParameters>("", false);

                    if (awardLetterParameters != null && awardLetterParameters.Any())
                    {
                        foreach (var record in awardLetterParameters)
                        {
                            var awardLetterConfiguration = new AwardLetterConfiguration(record.Recordkey)
                            {
                                IsContactBlockActive = !string.IsNullOrEmpty(record.AltrOfficeBlock) && record.AltrOfficeBlock.ToUpper() == "Y",
                                IsHousingBlockActive = !string.IsNullOrEmpty(record.AltrHousingCode) && record.AltrHousingCode.ToUpper() == "Y",
                                IsNeedBlockActive = !string.IsNullOrEmpty(record.AltrNeedBlock) && record.AltrNeedBlock.ToUpper() == "Y",
                                IsEfcActive = !string.IsNullOrEmpty(record.AltrEfcFlag) && record.AltrEfcFlag.ToUpper() == "Y",
                                IsBudgetActive = !string.IsNullOrEmpty(record.AltrBudgetFlag) && record.AltrBudgetFlag.ToUpper() == "Y",
                                IsDirectCostActive = !string.IsNullOrEmpty(record.AltrDirectCostFlag) && record.AltrDirectCostFlag.ToUpper() == "Y",
                                IsIndirectCostActive = !string.IsNullOrEmpty(record.AltrIndirectCostFlag) && record.AltrIndirectCostFlag.ToUpper() == "Y",
                                IsEnrollmentActive = !string.IsNullOrEmpty(record.AltrEnrollmentFlag) && record.AltrEnrollmentFlag.ToUpper() == "Y",
                                IsPellEntitlementActive = !string.IsNullOrEmpty(record.AltrPellEntitlementFlag) && record.AltrPellEntitlementFlag.ToUpper() == "Y",
                                IsPreAwardTextActive = !string.IsNullOrEmpty(record.AltrPreAwardsText) && record.AltrPreAwardsText.ToUpper() == "Y",
                                IsPostAwardTextActive = !string.IsNullOrEmpty(record.AltrPostAwardsText) && record.AltrPostAwardsText.ToUpper() == "Y",
                                IsPostClosingTextActive = !string.IsNullOrEmpty(record.AltrPostClosingText) && record.AltrPostClosingText.ToUpper() == "Y",
                                IsRenewalActive = !string.IsNullOrEmpty(record.AltrRenewalFlag) && record.AltrRenewalFlag.ToUpper() == "Y",
                                ParagraphSpacing = !string.IsNullOrEmpty(record.AltrParaSpacing) ? record.AltrParaSpacing : "1",
                                AwardTableTitle = !string.IsNullOrEmpty(record.AltrTitleAwdName) ? record.AltrTitleAwdName : "Awards",
                                AwardTotalTitle = !string.IsNullOrEmpty(record.AltrTitleAwdTotal) ? record.AltrTitleAwdTotal : "Total"
                            };
                            //Add award categories groups to the configuration
                            awardLetterConfiguration.AddAwardCategoryGroup(record.AltrTitleGroup1, 1, GroupType.AwardCategories);
                            awardLetterConfiguration.AddAwardCategoryGroup(record.AltrTitleGroup2, 2, GroupType.AwardCategories);
                            awardLetterConfiguration.AddAwardCategoryGroup(record.AltrTitleGroup3, 3, GroupType.AwardCategories);

                            //Add award period column groups to the configuration
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn1, 1, GroupType.AwardPeriodColumn);
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn2, 2, GroupType.AwardPeriodColumn);
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn3, 3, GroupType.AwardPeriodColumn);
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn4, 4, GroupType.AwardPeriodColumn);
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn5, 5, GroupType.AwardPeriodColumn);
                            awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AltrTitleColumn6, 6, GroupType.AwardPeriodColumn);                                

                            awardLetterParametersRecords.Add(awardLetterConfiguration);
                        }
                        return awardLetterParametersRecords;
                    }
                    else
                    {
                        logger.Info("Null AltrParameters returned from database");
                        return awardLetterParametersRecords;
                    }
                });
            
        }
        /// <summary>
        /// Get a collection of financial aid award periods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid award periods</returns>
        public async Task<IEnumerable<FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AwardPeriods, FinancialAidAwardPeriod>("AllFinancialAidAwardPeriods", "AWARD.PERIODS",
                (fa, g) => new FinancialAidAwardPeriod(g, fa.Recordkey, fa.AwdpDesc, "active") { StartDate = fa.AwdpStartDate, EndDate = fa.AwdpEndDate, AwardTerms = fa.AwdpAcadTerms }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of financial aid fund categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund categories</returns>
        public async Task<IEnumerable<FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AwardCategories, FinancialAidFundCategory>("AllFinancialAidFundCategories", "AWARD.CATEGORIES",
                (fa, g) =>
                    {
                        AwardCategoryType? type = null;
                        var typeArray = new string[4] { fa.AcLoanFlag, fa.AcGrantFlag, fa.AcScholarshipFlag, fa.AcWorkFlag };

                        //is exactly one of the flags equal to Yes?
                        if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                        {
                            if (!string.IsNullOrEmpty(fa.AcLoanFlag) && fa.AcLoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                            else if (!string.IsNullOrEmpty(fa.AcGrantFlag) && fa.AcGrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                            else if (!string.IsNullOrEmpty(fa.AcScholarshipFlag) && fa.AcScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                            else if (!string.IsNullOrEmpty(fa.AcWorkFlag) && fa.AcWorkFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                        }

                        var restrictedFlag = fa.AcIntgRestricted.ToUpper() == "Y" ? true : false;
                        var categoryName = ConvertCategoryName(fa.AcIntgName);

                        return new FinancialAidFundCategory(g, fa.Recordkey, String.IsNullOrEmpty(fa.AcDescription) ? fa.Recordkey : fa.AcDescription, type, categoryName, restrictedFlag);
                    }, bypassCache: ignoreCache);
          }

        /// <summary>
        /// Get a collection of financial aid fund classifications
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund classifications</returns>
        public async Task<IEnumerable<FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<ReportFundTypes, FinancialAidFundClassification>("AllFinancialAidFundClassifications", "REPORT.FUND.TYPES",
                (fa, g) => new FinancialAidFundClassification(g, fa.RftFundTypeCode, String.IsNullOrEmpty(fa.RftTitle) ? fa.RftFundTypeCode : fa.RftTitle) 
                { Description2 = fa.RftDesc, FundingTypeCode = fa.Recordkey }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of financial aid years
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        public async Task<IEnumerable<FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<FaSuites, FinancialAidYear>("AllFinancialAidYears", "FA.SUITES",
                (fa, g) => new FinancialAidYear(g, fa.Recordkey, fa.Recordkey, fa.FaSuitesStatus) { HostCountry = GetHostCountryAsync().Result }, bypassCache: ignoreCache);
        }     

        /// <summary>
        /// Gets all financial aid explanations
        /// </summary>
        /// <returns>a set of FinanciaAidExplanation entities</returns>
        public async Task<IEnumerable<FinancialAidExplanation>> GetFinancialAidExplanationsAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<FinancialAidExplanation>>("FinancialAidExplanations",
                    async () =>
                    {
                        var explanations = await DataReader.ReadRecordAsync<FaExplanations>("FA.EXPLANATIONS", "FA.SS.TEXT");
                        if(explanations == null)
                        {
                            logger.Info("No financial aid explanations records found.");
                            return new List<FinancialAidExplanation>();
                        }

                        var faExplanationEntities = new List<FinancialAidExplanation>();
                        char vm = Convert.ToChar(DynamicArray.VM);
                        if (!string.IsNullOrEmpty(explanations.FePellLeuExpl))
                        {
                            faExplanationEntities.Add(new FinancialAidExplanation(FormatString(explanations.FePellLeuExpl, vm), FinancialAidExplanationType.PellLEU));                            
                        }
                        return faExplanationEntities;
                    });
        }

        ///// <summary>
        ///// Read the international parameters records to extract date format used
        ///// locally and setup in the INTL parameters.
        ///// </summary>
        ///// <returns>International Parameters with date properties</returns>
        //private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
        //{
        //    if (internationalParameters != null)
        //    {
        //        return internationalParameters;
        //    }
        //    // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
        //    internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
        //        async () =>
        //        {
        //            Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
        //            if (intlParams == null)
        //            {
        //                var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
        //                logger.Info(errorMessage);
        //                // If we cannot read the international parameters default to US with a / delimiter.
        //                // throw new ColleagueWebApiException(errorMessage);
        //                Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
        //                newIntlParams.HostShortDateFormat = "MDY";
        //                newIntlParams.HostDateDelimiter = "/";
        //                newIntlParams.HostCountry = "USA";
        //                intlParams = newIntlParams;
        //            }
        //            return intlParams;
        //        }, Level1CacheTimeoutValue);
        //    return internationalParameters;
        //}

        public async Task<string> GetHostCountryAsync()
        {
            var intlParams = await GetInternationalParametersAsync();
            return intlParams.HostCountry;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
        /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
        private FinancialAidFundAidCategoryType ConvertCategoryName(string source)
        {
            switch (source)
            {

                case "1":
                    return FinancialAidFundAidCategoryType.pellGrant;
                case "2":
                    return FinancialAidFundAidCategoryType.federalUnsubsidizedLoan;
                case "3":
                    return FinancialAidFundAidCategoryType.federalSubsidizedLoan;
                case "4":
                    return FinancialAidFundAidCategoryType.graduateTeachingGrant;
                case "5":
                    return FinancialAidFundAidCategoryType.undergraduateTeachingGrant;
                case "6":
                    return FinancialAidFundAidCategoryType.parentPlusLoan;
                case "7":
                    return FinancialAidFundAidCategoryType.graduatePlusLoan;
                case "8":
                    return FinancialAidFundAidCategoryType.federalWorkStudyProgram;
                case "9":
                    return FinancialAidFundAidCategoryType.iraqAfghanistanServiceGrant;
                case "10":
                    return FinancialAidFundAidCategoryType.academicCompetitivenessGrant;
                case "11":
                    return FinancialAidFundAidCategoryType.bureauOfIndianAffairsFederalGrant;
                case "12":
                    return FinancialAidFundAidCategoryType.robertCByrdScholarshipProgram;
                case "13":
                    return FinancialAidFundAidCategoryType.paulDouglasTeacherScholarship;
                case "14":
                    return FinancialAidFundAidCategoryType.generalTitleIVloan;
                case "15":
                    return FinancialAidFundAidCategoryType.healthEducationAssistanceLoan;

                    return FinancialAidFundAidCategoryType.federalPerkinsLoan;
                case "16":
                    return FinancialAidFundAidCategoryType.healthProfessionalStudentLoan;
                case "17":
                    return FinancialAidFundAidCategoryType.incomeContingentLoan;
                case "18":
                    return FinancialAidFundAidCategoryType.loanForDisadvantagesStudent;
                case "19":
                    return FinancialAidFundAidCategoryType.leveragingEducationalAssistancePartnership;
                case "20":
                    return FinancialAidFundAidCategoryType.nationalHealthServicesCorpsScholarship;
                case "21":
                    return FinancialAidFundAidCategoryType.nursingStudentLoan;
                case "22":
                    return FinancialAidFundAidCategoryType.primaryCareLoan;
                case "23":
                    return FinancialAidFundAidCategoryType.rotcScholarship;
                case "24":
                    return FinancialAidFundAidCategoryType.federalSupplementaryEducationalOpportunityGrant;
                case "25":
                    return FinancialAidFundAidCategoryType.stayInSchoolProgram;
                case "26":
                    return FinancialAidFundAidCategoryType.federalSupplementaryLoanForParent;
                case "27":
                    return FinancialAidFundAidCategoryType.nationalSmartGrant;
                case "28":
                    return FinancialAidFundAidCategoryType.stateStudentIncentiveGrant;
                case "29":
                    return FinancialAidFundAidCategoryType.vaHealthProfessionsScholarship;
                case "30":
                    return FinancialAidFundAidCategoryType.nonGovernmental;
                default:
                    return FinancialAidFundAidCategoryType.nonGovernmental;
            }
        }
   
        /// <summary>
        /// Public Accessor for Financial Aid Awards. Retrieves and caches all awards defined
        /// in Colleague. 
        /// This code is also used in the StudentAwardRepository. Any changes here should also
        /// be copied to that class.
        /// </summary>
        //public IEnumerable<SatisfactoryAcademicProgressType> SatisfactoryAcademicProgressTypes
        //{
        //    get
        //    {
        //        return GetOrAddToCache<IEnumerable<SatisfactoryAcademicProgressType>>("AllSAPTypes",
        //            () =>
        //            {
        //                var typeList = new List<SatisfactoryAcademicProgressType>();
        //                var typeRecords = DataReader.BulkReadRecord<SapType>("", false);
        //                foreach (var typeRecord in typeRecords)
        //                {
        //                   // I expect an explanation field to be coming up in the near future. pbw 06/17/15
        //                   // var explanation = typeRecord.AwExplanationText;
        //                   // char _VM = Convert.ToChar(DynamicArray.VM);

        //                   // if (!string.IsNullOrEmpty(explanation))
        //                   // {
        //                        // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
        //                   //     explanation = explanation.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
        //                        // If there is a single-VM, replace it with a space.
        //                   //     explanation = explanation.Replace(_VM, ' ');
        //                   // }

                            
        //                    }
        //                    // Award Categrory is no longer required
        //                    var awardCategory = AwardCategories.FirstOrDefault(ac => ac.Code == typeRecord.AwCategory);
        //                    var award = new Award(typeRecord.Recordkey, typeRecord.AwDescription, awardCategory, explanation)
        //                    {
        //                        IsFederalDirectLoan = (!string.IsNullOrEmpty(typeRecord.AwDlLoanType)),
        //                        Type = typeRecord.AwType,
                                
        //                    };

        //                    awardList.Add(award);
        //                }
        //                return awardList;
        //            });
        //    }
        //}
    }
}