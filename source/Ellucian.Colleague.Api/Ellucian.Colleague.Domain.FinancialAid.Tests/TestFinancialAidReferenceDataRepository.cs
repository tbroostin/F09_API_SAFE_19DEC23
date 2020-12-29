//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Dmi.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestFinancialAidReferenceDataRepository : IFinancialAidReferenceDataRepository
    {
        #region Awards

        /// <summary>
        /// Array contains data that could have come from Colleague
        /// </summary>

        private static char _VM = Convert.ToChar(DynamicArray.VM);

        public class AwardRecord
        {
            public string Code;
            public string Description;
            public string Explanation;
            public string Category;
            public string LoanType;
            public string ShoppingSheetGroup;
        }

        public List<AwardRecord> awardRecordData = new List<AwardRecord>()
        {
            new AwardRecord()
            {
                Code = "PPLUS1",
                Description = "Parent Plus 1 Description",
                Explanation = "Explanation of PLUS Loan",
                Category = "PLUS",
                LoanType = "P",
                ShoppingSheetGroup = "PK"
            },
            new AwardRecord()
            {
                Code = "SUB3",
                Description = "Subsidized Loan",
                Explanation = "Explanation of Sub Loan",
                Category = "GSL",
                LoanType = "S",
                ShoppingSheetGroup = "DS"
            },
            new AwardRecord()
            {
                Code = "SUB3DL",
                Description = "Subsidized Loan 3 Description",
                Explanation = "Explanation of Sub Loan",
                Category = "GSL",
                LoanType = "",
                ShoppingSheetGroup = "DS"
            },
            new AwardRecord()
            {
                Code = "UNSDL",
                Description = "Unsubsidized Loan",
                Explanation = "Explanation of Unsub Loan",
                Category = "USTF",
                LoanType = "U",
                ShoppingSheetGroup = "DU"
            },
            new AwardRecord()
            {
                Code = "SUBDL",
                Description = "Subsidized Loan",
                Explanation = "Explanation of Sub Loan",
                Category = "GSL",
                LoanType = "S",
                ShoppingSheetGroup = "DS"
            },
            new AwardRecord()
            {
                Code = "PELL",
                Description = "Pell Grant",
                Explanation = "Explanation of"+_VM.ToString()+"Pell Grant",
                Category = "PELL",
                LoanType = "",
                ShoppingSheetGroup = "PL"
            },
            new AwardRecord()
            {
                Code = "UGTCH",
                Description = "Undergraduate Teach Grant",
                Explanation = "Explanation of"+_VM.ToString()+"Teach Grant",
                Category = "TEACH",
                LoanType = "",
                ShoppingSheetGroup = "ST"
            },
            new AwardRecord()
            {
                Code = "ZEBRA",
                Description = "Zebra Grant",
                Explanation = "Explanation of"+_VM.ToString()+"Zebra Grant",
                Category = "GRANT",
                LoanType = "",
                ShoppingSheetGroup = "SC"
            },
            new AwardRecord()
            {
                Code = "WOOFY",
                Description = "Woofy award",
                Explanation = "Explanation of"+_VM.ToString()+"Woofy Grant",
                Category = "GRANT",
                LoanType = "",
                ShoppingSheetGroup = "OT"
            },
            new AwardRecord()
            {
                Code = "PELL",
                Description = "Pell Grant",
                Explanation = "Explanation of"+_VM.ToString()+"Pell Grant",
                Category = "PELL",
                LoanType = "",                
                ShoppingSheetGroup = "PL"
            },
            new AwardRecord()
            {
                Code = "SUB1",
                Description = "Sub1 Description",
                Explanation = "Explanation",
                Category = "GSL",
                LoanType = "S",
                ShoppingSheetGroup = "DS"
            },
            new AwardRecord()
            {
                Code = "SUB2",
                Description = "Sub2 Description",
                Explanation = "Explanation",
                Category = "GSL",
                LoanType = "",
                ShoppingSheetGroup = "DS"
            },
            new AwardRecord()
            {
                Code = "UNSUB1",
                Description = "Unsub1 Description",
                Explanation = "Explanation",
                Category = "USTF",
                LoanType = "U",
                ShoppingSheetGroup = "DU"
            },
            new AwardRecord()
            {
                Code = "UNSUB2",
                Description = "Unsub2 Description",
                Explanation = "",
                Category = "USTF",
                LoanType = "",
                ShoppingSheetGroup = "DU"
            },
            new AwardRecord()
            {
                Code = "GPLUS1",
                Description = "GPLUS1 Description",
                Explanation = "Explanation" + _VM.ToString()+_VM.ToString() + "of GPLUS1",
                Category = "GPLUS",
                LoanType = "P",
                ShoppingSheetGroup = "PK"
            },
            new AwardRecord()
            {
                Code = "GPLUS2",
                Description = "GPLUS2 Description",
                Explanation = "Explanation",
                Category = "GPLUS",
                LoanType = "",
                ShoppingSheetGroup = "pk"
            },
            new AwardRecord()
            {
                Code = "PLUS1",
                Description = "PLUS1 Description",
                Explanation = "Explanation",
                Category = "PLUS",
                LoanType = "",
                ShoppingSheetGroup = "PK"
            },
            new AwardRecord()
            {
                Code = "CLLOAN",
                Description = "CommonLine Loan Description",
                Explanation = "Explanation",
                Category = "CL",
                LoanType = "",
                ShoppingSheetGroup = ""
            },
            new AwardRecord()
            {
                Code = "GRTCH",
                Description = "CommonLine Loan Description",
                Explanation = "Explanation",
                Category = "TEACH",
                LoanType = "",
                ShoppingSheetGroup = ""
            },
            new AwardRecord()
            {
                Code = "Goofy",
                Description = "Goofy award Description",
                Explanation = "Explanation",
                Category = "GRANT",
                LoanType = "",
                ShoppingSheetGroup = ""
            },
            new AwardRecord()
            {
                Code = "SNEEZY",
                Description = "SNEEZY award Description",
                Explanation = "Explanation",
                Category = "Loan",
                LoanType = "",
                ShoppingSheetGroup = ""
            }
        };


        /// <summary>
        /// Method returns data from awards array as if it was retreiving data from Colleague.
        /// </summary>
        public IEnumerable<Award> Awards
        {
            get
            {
                var awardList = new List<Award>();

                foreach (var awardRecord in awardRecordData)
                {
                    var explanation = awardRecord.Explanation;

                    if (!string.IsNullOrEmpty(explanation))
                    {
                        explanation = explanation.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
                        explanation = explanation.Replace(_VM, ' ');
                    }

                    ShoppingSheetAwardGroup? shoppingSheetGroup = null;
                    switch (awardRecord.ShoppingSheetGroup.ToUpper())
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

                    //var category = AwardCategories.FirstOrDefault(ac => ac.Code == awardRecord.Category);
                    var category = GetAwardCategoriesAsync().Result.Where(ac => ac.Code == awardRecord.Category).FirstOrDefault();

                    var award = new Award(awardRecord.Code, awardRecord.Description, category, explanation);

                    award.IsFederalDirectLoan = (!string.IsNullOrEmpty(awardRecord.LoanType));
                    award.ShoppingSheetGroup = shoppingSheetGroup;
                    awardList.Add(award);
                }

                return awardList;
            }
        }
        #endregion

        #region AwardPeriods
        /// <summary>
        /// Define an AwardPeriod structure
        /// </summary>
        private string[,] awardPeriods = {
                                       //CODE    DESCRIPTION
                                       {"14/FA", "2014/2015 Fall"},
                                       {"15/WI", "2014/2015 Winter"},
                                       {"15/SP", "2014/2015 Spring"},
                                       {"15/S1", "2014/2015 Summer 1"},
                                       {"17/FA", "2017/2018 Fall"},
                                       {"18/SP", "2017/2018 Spring"}
                                   };

        private DateTime startDt;

        /// <summary>
        /// Define that we will have multiple AwardPeriods
        /// </summary>
        public IEnumerable<AwardPeriod> AwardPeriods
        {
            get
            {
                var awardPeriodList = new List<AwardPeriod>();

                //There are 2 fields for each award period in the array
                var items = awardPeriods.Length / 2;

                startDt = DateTime.Parse("08/15/2015");

                for (int i = 0; i < items; i++)
                {
                    awardPeriodList.Add(new AwardPeriod(awardPeriods[i, 0], awardPeriods[i, 1], startDt));
                }

                return awardPeriodList;

            }
        }

        #endregion

        #region AwardStatuses

        public string[,] AwardActionData = {
                                            {"A", "Accepted", "A"},
                                            {"D", "Denied", "D"},
                                            {"E", "Estimated", "E"},
                                            {"P", "Pending", "P"},
                                            {"R", "Rejected", "R"},
                                            {"X", "Foobar", "E"}
                                          };

        public IEnumerable<AwardStatus> AwardStatuses
        {
            get
            {
                var awardStatusList = new List<AwardStatus>();

                //There are 3 fields for each award status in the array
                var items = AwardActionData.Length / 3;

                for (int i = 0; i < items; i++)
                {
                    AwardStatusCategory cat;
                    switch (AwardActionData[i, 2])
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
                            throw new InvalidEnumArgumentException("category");
                    }

                    awardStatusList.Add(new AwardStatus(AwardActionData[i, 0], AwardActionData[i, 1], cat));
                }

                return awardStatusList;
            }
        }
        #endregion

        #region AwardYears

        public List<string> SuiteYears = new List<string>() { "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2017" };

        public IEnumerable<AwardYear> AwardYears
        {
            get
            {
                var awardYearList = new List<AwardYear>();

                var numSuiteItems = SuiteYears.Count;
                for (int i = 0; i < numSuiteItems; i++)
                {
                    awardYearList.Add(new AwardYear(SuiteYears[i], SuiteYears[i]));
                }

                return awardYearList.OrderBy(ay => ay.Code);

            }
        }
        #endregion

        #region AwardCategories

        public class AwardCategoryRecord
        {
            public string Code;
            public string Description;
            public string LoanFlag;
            public string GrantFlag;
            public string ScholarshipFlag;
            public string WorkStudyFlag;
        }

        public List<AwardCategoryRecord> awardCategoryData = new List<AwardCategoryRecord>()
        {
            new AwardCategoryRecord() 
            {
                Code = "PELL", 
                Description = "Pell Category",
                GrantFlag = "Y"
            },
            new AwardCategoryRecord()
            {
                Code = "DOG",
                Description = "Canine Category",
                ScholarshipFlag = "Y"
            },
            new AwardCategoryRecord()
            {
                Code = "WORK",
                Description = "Work Study Category",
                WorkStudyFlag = "Y"
            },
            new AwardCategoryRecord() 
            {
                Code = "GSL", 
                Description = "Subsidized Loans",
                LoanFlag = "Y"
            },
            new AwardCategoryRecord() 
            {
                Code = "USTF", 
                Description = "Unsubsidized Loans",
                LoanFlag = "Y"
            },
            new AwardCategoryRecord() 
            {
                Code = "GPLUS", 
                Description = "Graduate Plus Loans",
                LoanFlag = "Y"
            },
            new AwardCategoryRecord()             
            {
                Code = "PLUS", 
                Description = "Parent Plus Loans",
                LoanFlag = "Y"
            },
            new AwardCategoryRecord() 
            {
                Code = "CL", 
                Description = "Commonline Loans",
                LoanFlag = "Y"
            },
            new AwardCategoryRecord()
            {
                Code = "GRANT",
                Description = "Grants",
                LoanFlag = "N"
            }
        };

        public IEnumerable<AwardCategory> AwardCategories
        {
            get
            {
                var categoryList = new List<AwardCategory>();
                foreach (var acRecord in awardCategoryData)
                {
                    AwardCategoryType? type = null;
                    var typeArray = new string[4] { acRecord.LoanFlag, acRecord.GrantFlag, acRecord.ScholarshipFlag, acRecord.WorkStudyFlag };

                    //is exactly one of the flags equal to Yes?
                    if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                    {
                        if (!string.IsNullOrEmpty(acRecord.LoanFlag) && acRecord.LoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                        else if (!string.IsNullOrEmpty(acRecord.GrantFlag) && acRecord.GrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                        else if (!string.IsNullOrEmpty(acRecord.ScholarshipFlag) && acRecord.ScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                        else if (!string.IsNullOrEmpty(acRecord.WorkStudyFlag) && acRecord.WorkStudyFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                    }

                    categoryList.Add(new AwardCategory(acRecord.Code, acRecord.Description, type));
                }
                return categoryList;
            }
        }

        public Task<IEnumerable<AwardCategory>> GetAwardCategoriesAsync()
        {
            
            
                var categoryList = new List<AwardCategory>();
                foreach (var acRecord in awardCategoryData)
                {
                    AwardCategoryType? type = null;
                    var typeArray = new string[4] { acRecord.LoanFlag, acRecord.GrantFlag, acRecord.ScholarshipFlag, acRecord.WorkStudyFlag };

                    //is exactly one of the flags equal to Yes?
                    if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                    {
                        if (!string.IsNullOrEmpty(acRecord.LoanFlag) && acRecord.LoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                        else if (!string.IsNullOrEmpty(acRecord.GrantFlag) && acRecord.GrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                        else if (!string.IsNullOrEmpty(acRecord.ScholarshipFlag) && acRecord.ScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                        else if (!string.IsNullOrEmpty(acRecord.WorkStudyFlag) && acRecord.WorkStudyFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                    }

                    categoryList.Add(new AwardCategory(acRecord.Code, acRecord.Description, type));
                }
                return Task.FromResult(categoryList.AsEnumerable());
            
        }


        #endregion

        #region Links


        public class LinkRecord
        {
            public string Id;
            public string Description;
            public string Type;
            public string Url;
            public int Position;
        }

        public List<LinkRecord> LinkRecordData = new List<LinkRecord>()
        {
            new LinkRecord()
            {
                Id = "1",
                Description = "FAFSA On The Web",
                Type = "FAFSA",
                Url = "www.fafsa.ed.gov",
                Position = 1
            },
            new LinkRecord()
            {
                Id = "2",
                Description = "PROFILE On The Web",
                Type = "PROFILE",
                Url = "www.thecollegeboard.com",
                Position = 3
            },
            new LinkRecord()
            {
                Id = "3",
                Description = "Master Promissory Note",
                Type = "MPN",
                Url = "www.cod.ed.gov",
                Position = 5
            },
            new LinkRecord()
            {
                Id = "4",
                Description = "SAP Policy",
                Type = "SAP",
                Url = "www.dallascowboys.com",
                Position = 2
            },
            new LinkRecord()
            {
                Id = "5",
                Description = "FAFSA Forecaster",
                Type = "FORECASTER",
                Url = "www.dallascowboys.com",
                Position = 4
            },
            new LinkRecord()
            {
                Id = "6",
                Description = "Entrance Interviews",
                Type = "ENTRINT",
                Url = "www.cod.ed.gov",
                Position = 7
            },
            new LinkRecord()
            {
                Id = "7",
                Description = "National Student Loan Data System",
                Type = "NSLDS",
                Url = "www.nslds.ed.gov",
                Position = 9
            },
            new LinkRecord()
            {
                Id = "8",
                Description = "Parent Loans",
                Type = "PLUS",
                Url = "www.cod.ed.gov",
                Position = 11
            },
            new LinkRecord()
            {
                Id = "9",
                Description = "User Forms",
                Type = "USER",
                Url = "www.anyschoollink.gov",
                Position = 6
            },
            new LinkRecord()
            {
                Id = "10",
                Description = "Miscellaneous Forms",
                Type = "FORM",
                Url = "www.anylink.com",
                Position = 8
            }

        };

        public IEnumerable<Link> Links
        {
            get
            {
                var linkList = new List<Link>();
                foreach (var linkRecord in LinkRecordData)
                {
                    LinkTypes type;
                    switch (linkRecord.Type.ToUpper())
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

                    linkList.Add(new Link(linkRecord.Description, type, linkRecord.Url));
                }

                return linkList;
            }
        }
        #endregion

        #region AwardTypes

        public class AwardTypeValcode
        {
            public string Code;
            public string Description;
        }

        public List<AwardTypeValcode> awardTypeValcodData = new List<AwardTypeValcode>()
        {
            new AwardTypeValcode() { Code = "F", Description = "Federal"},
            new AwardTypeValcode() { Code = "S", Description = "State"},
            new AwardTypeValcode() { Code = "I", Description = "Institutional"},
            new AwardTypeValcode() { Code = "O", Description = "Other"}
        };

        public IEnumerable<AwardType> AwardTypes
        {
            get
            {
                var awardTypes = new List<AwardType>();
                foreach (var awardTypeRecord in awardTypeValcodData)
                {
                    awardTypes.Add(new AwardType(awardTypeRecord.Code, awardTypeRecord.Description));
                }
                return awardTypes;
            }
        }

        #endregion

        #region BudgetComponents

        public class BudgetComponentRecord
        {
            public string AwardYear;
            public string Code;
            public string Description;
            public string ShoppingSheetGroupCode;
            public string IsDirectCost;
        }

        public List<BudgetComponentRecord> BudgetComponentData = new List<BudgetComponentRecord>()
        {

            new BudgetComponentRecord()
            {
                AwardYear = "2014",
                Code = "TUITION",
                Description = "Tuition Budget",
                ShoppingSheetGroupCode = "TF",
                IsDirectCost = "I"
            },
            new BudgetComponentRecord()
            {
                AwardYear = "2014",
                Code = "HOUSING",
                Description = "Housing Budget",
                ShoppingSheetGroupCode = "HM",
                IsDirectCost = "I"
            },
            new BudgetComponentRecord()
            {
                AwardYear = "2014",
                Code = "FUEL",
                Description = "Commuter Fuel Budget",
                ShoppingSheetGroupCode = "TP",
                IsDirectCost = "I"
            },
            new BudgetComponentRecord()
            {
                AwardYear = "2015",
                Code = "TUITION",
                Description = "Tuition Budget",
                ShoppingSheetGroupCode = "TF",
                IsDirectCost = "I"
            },
            new BudgetComponentRecord()
            {
                AwardYear = "2015",
                Code = "HOUSING",
                Description = "Housing Budget",
                ShoppingSheetGroupCode = "HM",
                IsDirectCost = "I"
            },
            new BudgetComponentRecord()
            {
                AwardYear = "2015",
                Code = "FUEL",
                Description = "Commuter Fuel Budget",
                ShoppingSheetGroupCode = "TP",
                IsDirectCost = "I"
            }
        };

        public IEnumerable<BudgetComponent> BudgetComponents
        {
            get
            {
                return BudgetComponentData.Select(budgetRecord =>
                    {
                        ShoppingSheetBudgetGroup? shoppingSheetGroup = null;
                        switch (budgetRecord.ShoppingSheetGroupCode.ToUpper())
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
                        return new BudgetComponent(budgetRecord.AwardYear, budgetRecord.Code, budgetRecord.Description)
                        {
                            ShoppingSheetGroup = shoppingSheetGroup
                        };
                    });
            }
        }

        #endregion

        #region ChecklistItems

        public class FAChecklistItem
        {
            public string Code;
            public string ItemDescription;
            public int? SortNumber;
        }

        public List<FAChecklistItem> checklistItemData = new List<FAChecklistItem>()
        {
            new FAChecklistItem()
            {
                Code = "PROFILE",
                ItemDescription = "PROFILE Application",
                SortNumber = 1,
            },

            new FAChecklistItem()
            {
                Code = "FAFSA",
                ItemDescription = "FAFSA Application",
                SortNumber = 2,
            },
            new FAChecklistItem()
            {
                Code = "ACCAWDPKG",
                ItemDescription = "Review your awards",
                SortNumber = 5,
            },
            new FAChecklistItem()
            {
                Code = "APPLRVW",
                ItemDescription = "Application is under review",
                SortNumber = 4,
            },
            new FAChecklistItem()
            {
                Code = "CMPLREQDOC",
                ItemDescription = "Complete required documents",
                SortNumber = 3,
            },
            new FAChecklistItem()
            {
                Code = "SIGNAWDLTR",
                ItemDescription = "Sign the Award Letter",
                SortNumber = 6,
            },
        };

        public IEnumerable<ChecklistItem> ChecklistItems
        {
            get
            {
                if (checklistItemData == null) return null;
                var itemList = new List<ChecklistItem>();
                foreach (var item in checklistItemData)
                {
                    ChecklistItemType? Itemtype = null;
                    switch (item.Code.ToUpper())
                    {
                        case "FAFSA":
                            Itemtype = ChecklistItemType.FAFSA;
                            break;
                        case "PROFILE":
                            Itemtype = ChecklistItemType.PROFILE;
                            break;
                        case "ACCAWDPKG":
                            Itemtype = ChecklistItemType.ReviewAwardPackage;
                            break;
                        case "APPLRVW":
                            Itemtype = ChecklistItemType.ApplicationReview;
                            break;
                        case "CMPLREQDOC":
                            Itemtype = ChecklistItemType.CompletedDocuments;
                            break;
                        case "SIGNAWDLTR":
                            Itemtype = ChecklistItemType.ReviewAwardLetter;
                            break;
                    }

                    if (Itemtype.HasValue && item.SortNumber.HasValue)
                    {
                        var itemRecord = new ChecklistItem(item.Code, item.SortNumber.Value, item.ItemDescription)
                            {
                                ChecklistItemType = Itemtype.Value
                            };
                        itemList.Add(itemRecord);
                    }
                }

                return itemList;
            }
        }
        #endregion

        #region AcademicProgressStatuses

        public class SapStatusValcodeVals
        {
            public string Code;
            public string Description;

            public string Explanation { get; set; }
        }

        public List<SapStatusValcodeVals> SapStatusValcodeData = new List<SapStatusValcodeVals>()
        {
           
            new SapStatusValcodeVals() { Code = "U", Description = "Unsatisfactory"},
            new SapStatusValcodeVals() { Code = "S", Description = "Satisfactory"},
            new SapStatusValcodeVals() { Code = "W", Description = "Foobar"}, 
            new SapStatusValcodeVals() { Code = "J", Description = "No Category"},
            new SapStatusValcodeVals() { Code = "H", Description = "No Explanation"},
            new SapStatusValcodeVals() { Code = "Z", Description = "Cannot Calculate Status"},
        };

        public class SapStatusInfoRecord
        {
            public string Code;
            public string Category;
            public string Explanation;
            public string Explained;
        }

        public List<SapStatusInfoRecord> SapStatusInfoData = new List<SapStatusInfoRecord>()
        {

           new SapStatusInfoRecord() { Code = "U", Category = "U", Explanation = "This is unsatisfactory",Explained = "This is unsatisfactory too"},
           new SapStatusInfoRecord() { Code = "S", Category = "S", Explanation = "This is satisfactory",Explained = "This is satisfactory too"},
           new SapStatusInfoRecord() { Code = "W", Category = "W", Explanation = "This is warning",Explained = "This is warning too"},
           new SapStatusInfoRecord() { Code = "J", Category = null,Explanation = "Status with No Category",Explained = "Status with no Category too"},
           new SapStatusInfoRecord() { Code = "H", Category = "S", Explanation = null,Explained = null},
           new SapStatusInfoRecord() { Code = "Z", Category = "D", Explanation = "Do Not Display This Status",Explained = "Do Not Display This Status Too"},
        };

        public Task<IEnumerable<AcademicProgressStatus>> GetAcademicProgressStatusesAsync()
        {
            var academicProgressStatusList = new List<AcademicProgressStatus>();

            foreach (var sapStatusValue in SapStatusValcodeData)
            {
                try
                {

                    var academicProgressStatus = new AcademicProgressStatus(sapStatusValue.Code, sapStatusValue.Description);

                    // Find a matching record in FA.SAP.STATUS.INFO using the SAP status 
                    var faSapStatusInfoRec = SapStatusInfoData.FirstOrDefault(fc => fc.Code == sapStatusValue.Code);

                    if (faSapStatusInfoRec != null)
                    {
                        AcademicProgressStatusCategory? itemtype = null;

                        if (faSapStatusInfoRec.Category != null)
                        {
                            switch (faSapStatusInfoRec.Category.ToUpper())
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
                        //academicProgressStatus.Explanation = faSapStatusInfoRec.Explanation;
                        academicProgressStatus.Explanation = faSapStatusInfoRec.Explained;
                    }
                    academicProgressStatusList.Add(academicProgressStatus);
                }
                catch (Exception)
                {

                }

            }
            return Task.FromResult(academicProgressStatusList.AsEnumerable());
        }
        #endregion

        #region AcademicProgressAppealCodes

        public class SapAppealCodesRecords
        {
            public string Code;
            public string Description;
        }

        public List<SapAppealCodesRecords> SapAppealInfoRecords = new List<SapAppealCodesRecords>()
        {
            new SapAppealCodesRecords() { Code = "Pending", Description = "Appeal Pending"},
            new SapAppealCodesRecords() { Code = "Granted", Description = "Appeal Granted"},
            new SapAppealCodesRecords() { Code = "Submitted", Description = "Appeal Submitted"},
        };


        public Task<IEnumerable<AcademicProgressAppealCode>> GetAcademicProgressAppealCodesAsync()
        {
            
                if (SapAppealInfoRecords == null) return null;
                var appealList = new List<AcademicProgressAppealCode>();
                foreach (var appealRecord in SapAppealInfoRecords)
                {
                    var appeal = new AcademicProgressAppealCode(appealRecord.Code, appealRecord.Description);
                    appeal.Code = appealRecord.Code;
                    appeal.Description = appealRecord.Description;
                    appealList.Add(appeal);
                }
                return Task.FromResult(appealList.AsEnumerable());
           
        }
        #endregion

        #region AwardLetterConfigurations 
       
        public class awardLetterParamRecord
        {
            public string Id { get; set; }
            public bool IsOfficeBlockActive { get; set; }
            public bool IsNeedBlockActive { get; set; }
            public bool IsHousingCodeActive { get; set; }
            public string AwardCategoryGroup1Title { get; set; }
            public string AwardCategoryGroup2Title { get; set; }
            public string AwardCategoryGroup3Title { get; set; }
            public string AwardPeriodGroup1Title { get; set; }
            public string AwardPeriodGroup2Title { get; set; }
            public string AwardPeriodGroup3Title { get; set; }
            public string AwardPeriodGroup4Title { get; set; }
            public string AwardPeriodGroup5Title { get; set; }
            public string AwardPeriodGroup6Title { get; set; }           

            public string AwardColumnTitle { get; set; }
            public string TotalColumnTitle { get; set; }
            public string ParagraphSpacing { get; set; }
        }

        public List<awardLetterParamRecord> awardLetterParameterData = new List<awardLetterParamRecord>()
        {
            new awardLetterParamRecord() 
            {
                Id = "UNDERGRAD",
                IsOfficeBlockActive = true,
                IsNeedBlockActive = true,
                IsHousingCodeActive = false,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",                
                AwardCategoryGroup2Title = "Group 2 Title",                
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",                
                AwardPeriodGroup2Title = "Period Group 2",                
                AwardPeriodGroup3Title = "Period Group 3",                
                AwardPeriodGroup4Title = "Period Group 4",                
                AwardPeriodGroup5Title = "Period Group 5",                
                AwardPeriodGroup6Title = "Period Group 6",
                ParagraphSpacing = "2"
            },
            new awardLetterParamRecord() 
            {
                Id = "GRADUATE",                
                IsOfficeBlockActive = false,
                IsNeedBlockActive = true,
                IsHousingCodeActive = true,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",                
                AwardCategoryGroup2Title = "Group 2 Title",                
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",                
                AwardPeriodGroup2Title = "Period Group 2",                
                AwardPeriodGroup3Title = "Period Group 3",                
                AwardPeriodGroup4Title = "Period Group 4",                
                AwardPeriodGroup5Title = "Period Group 5",                
                AwardPeriodGroup6Title = "Period Group 6"                
            },
            new awardLetterParamRecord() 
            {
                Id = "EDUCATION",                
                IsOfficeBlockActive = true,
                IsNeedBlockActive = false,
                IsHousingCodeActive = false,
                AwardColumnTitle = "Awards",
                TotalColumnTitle = "Total",
                AwardCategoryGroup1Title = "Group 1 Title",                
                AwardCategoryGroup2Title = "Group 2 Title",                
                AwardCategoryGroup3Title = "Group 3 Title",
                AwardPeriodGroup1Title = "Period Group 1",                
                AwardPeriodGroup2Title = "Period Group 2",                
                AwardPeriodGroup3Title = "Period Group 3",                
                AwardPeriodGroup4Title = "Period Group 4",                
                AwardPeriodGroup5Title = "Period Group 5",                
                AwardPeriodGroup6Title = "Period Group 6",
                ParagraphSpacing = "1"
            }
        };

        public async Task<IEnumerable<AwardLetterConfiguration>> GetAwardLetterConfigurationsAsync()
        {             
            return await Task.Run(() => getAwardLetterConfigurations());            
        }

        private IEnumerable<AwardLetterConfiguration> getAwardLetterConfigurations(){
            var awardLetterConfigurations = new List<AwardLetterConfiguration>();
            if (awardLetterParameterData != null)
            {
                foreach (var record in awardLetterParameterData)
                {
                    var awardLetterConfiguration = new AwardLetterConfiguration(record.Id)
                    {
                        IsContactBlockActive = record.IsOfficeBlockActive,
                        IsHousingBlockActive = record.IsHousingCodeActive,
                        IsNeedBlockActive = record.IsNeedBlockActive,
                        ParagraphSpacing = !string.IsNullOrEmpty(record.ParagraphSpacing) ? record.ParagraphSpacing : "1",
                        AwardTableTitle = !string.IsNullOrEmpty(record.AwardColumnTitle) ? record.AwardColumnTitle : "Awards",
                        AwardTotalTitle = !string.IsNullOrEmpty(record.TotalColumnTitle) ? record.TotalColumnTitle : "Total"
                    };
                    //Add award categories groups to the configuration
                    awardLetterConfiguration.AddAwardCategoryGroup(record.AwardCategoryGroup1Title, 0, GroupType.AwardCategories);
                    awardLetterConfiguration.AddAwardCategoryGroup(record.AwardCategoryGroup2Title, 1, GroupType.AwardCategories);
                    awardLetterConfiguration.AddAwardCategoryGroup(record.AwardCategoryGroup3Title, 2, GroupType.AwardCategories);

                    //Add award period column groups to the configuration
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup1Title, 0, GroupType.AwardPeriodColumn);
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup2Title, 1, GroupType.AwardPeriodColumn);
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup3Title, 2, GroupType.AwardPeriodColumn);
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup4Title, 3, GroupType.AwardPeriodColumn);
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup5Title, 4, GroupType.AwardPeriodColumn);
                    awardLetterConfiguration.AddAwardPeriodColumnGroup(record.AwardPeriodGroup6Title, 5, GroupType.AwardPeriodColumn);

                    awardLetterConfigurations.Add(awardLetterConfiguration);
                }
            }
            return awardLetterConfigurations;
        }

        #endregion

        #region FinancialAidExplanations

        public class faExplanationsContract
        {
            public string pellLeuExpl;
            public string recordKey;
        }

        public faExplanationsContract faExplanations = new faExplanationsContract()
        {
            recordKey = "FA",
            pellLeuExpl = "This is pell leu explantion for students who want to know what pell leu is."
        };


        #endregion

        public Task<IEnumerable<FinancialAid.Entities.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<FinancialAid.Entities.FinancialAidAwardPeriod>>(new List<FinancialAid.Entities.FinancialAidAwardPeriod>()
                {
                    new FinancialAid.Entities.FinancialAidAwardPeriod("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1", "STATUS1"),
                    new FinancialAid.Entities.FinancialAidAwardPeriod("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2", "STATUS2"),
                    new FinancialAid.Entities.FinancialAidAwardPeriod("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3", "STATUS3")
                });
        }

        public Task<IEnumerable<FinancialAid.Entities.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<FinancialAid.Entities.FinancialAidFundCategory>>(new List<FinancialAid.Entities.FinancialAidFundCategory>()
                {
                    new FinancialAid.Entities.FinancialAidFundCategory("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new FinancialAid.Entities.FinancialAidFundCategory("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new FinancialAid.Entities.FinancialAidFundCategory("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<IEnumerable<FinancialAid.Entities.FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<FinancialAid.Entities.FinancialAidYear>>(new List<FinancialAid.Entities.FinancialAidYear>()
                {
                    new FinancialAid.Entities.FinancialAidYear("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1", "STATUS1"),
                    new FinancialAid.Entities.FinancialAidYear("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2", "STATUS2"),
                    new FinancialAid.Entities.FinancialAidYear("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3", "STATUS3")
                });
        }

        public Task<IEnumerable<FinancialAid.Entities.FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache)
        {
            return Task.FromResult<IEnumerable<FinancialAid.Entities.FinancialAidFundClassification>>(new List<FinancialAid.Entities.FinancialAidFundClassification>()
                {
                    new FinancialAid.Entities.FinancialAidFundClassification("bb66b971-3ee0-4477-9bb7-539721f93434", "CODE1", "DESC1"),
                    new FinancialAid.Entities.FinancialAidFundClassification("5aeebc5c-c973-4f83-be4b-f64c95002124", "CODE2", "DESC2"),
                    new FinancialAid.Entities.FinancialAidFundClassification("27178aab-a6e8-4d1e-ae27-eca1f7b33363", "CODE3", "DESC3")
                });
        }

        public Task<string> GetHostCountryAsync()
        {
            return Task.FromResult<string>("USA");
        }

        public Task<IEnumerable<FinancialAidExplanation>> GetFinancialAidExplanationsAsync()
        {
            var explanations = new List<FinancialAidExplanation>();
            if (!string.IsNullOrEmpty(faExplanations.pellLeuExpl))
            {
                explanations.Add(new FinancialAidExplanation(faExplanations.pellLeuExpl, FinancialAidExplanationType.PellLEU));
                
            }
            return Task.FromResult(explanations.AsEnumerable());
        }
    }
}
