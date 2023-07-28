/*Copyright 2018-2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Dmi.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public class TestFinancialAidReferenceDataRepository : IFinancialAidReferenceDataRepository
    {
        #region Awards

        /// <summary>
        /// Array contains data that could have come from Colleague
        /// </summary>


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
                Explanation = "Explanation of"+DmiString.sVM+"Pell Grant",
                Category = "PELL",
                LoanType = "",
                ShoppingSheetGroup = "PL"
            },
            new AwardRecord()
            {
                Code = "UGTCH",
                Description = "Undergraduate Teach Grant",
                Explanation = "Explanation of"+DmiString.sVM+"Teach Grant",
                Category = "TEACH",
                LoanType = "",
                ShoppingSheetGroup = "ST"
            },
            new AwardRecord()
            {
                Code = "ZEBRA",
                Description = "Zebra Grant",
                Explanation = "Explanation of"+DmiString.sVM+"Zebra Grant",
                Category = "GRANT",
                LoanType = "",
                ShoppingSheetGroup = "SC"
            },
            new AwardRecord()
            {
                Code = "WOOFY",
                Description = "Woofy award",
                Explanation = "Explanation of"+DmiString.sVM+"Woofy Grant",
                Category = "GRANT",
                LoanType = "",
                ShoppingSheetGroup = "OT"
            },
            new AwardRecord()
            {
                Code = "PELL",
                Description = "Pell Grant",
                Explanation = "Explanation of"+DmiString.sVM+"Pell Grant",
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
                Explanation = "Explanation" + DmiString.sVM + DmiString.sVM + "of GPLUS1",
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
            }
        };

        public Task<IEnumerable<FinancialAidAward>> GetFinancialAidAwardsAsync()
        {
            var awardList = new List<FinancialAidAward>();

            foreach (var awardRecord in awardRecordData)
            {
                var category = GetFinancialAidAwardCategoriesAsync().Result.Where(ac => ac.Code == awardRecord.Category).FirstOrDefault();

                var award = new FinancialAidAward(awardRecord.Code, awardRecord.Description, category);

                awardList.Add(award);
            }

            return Task.FromResult(awardList.AsEnumerable());
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
            }
        };

        public Task<IEnumerable<FinancialAidAwardCategory>> GetFinancialAidAwardCategoriesAsync()
        {
            var categoryList = new List<FinancialAidAwardCategory>();
            foreach (var acRecord in awardCategoryData)
            {
                categoryList.Add(new FinancialAidAwardCategory(acRecord.Code, acRecord.Description));
            }
            return Task.FromResult(categoryList.AsEnumerable());
        }
        #endregion
    }
}
