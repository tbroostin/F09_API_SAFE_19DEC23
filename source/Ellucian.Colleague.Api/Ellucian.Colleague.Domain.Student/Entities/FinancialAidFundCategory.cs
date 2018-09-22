// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FinancialAidFundCategory : GuidCodeItem
    {

        public bool restrictedFlag { get; private set; }

        /// <summary>
        /// The type indicates the very top level kind of awards this award category describes. An award category's type
        /// is recommended, but not required.
        /// </summary>
        public AwardCategoryType? AwardCategoryType { get; private set; }

        /// <summary>
        /// The name indicates the very top level kind of awards this award category describes. An award category's name
        /// is recommended, but not required.
        /// </summary>
        public FinancialAidFundAidCategoryType? AwardCategoryName { get; private set; }
        /// <summary>
        /// Constructor for FinancialAidAwardPeriod
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public FinancialAidFundCategory(string guid, string code, string description, AwardCategoryType? awardCategoryType = null, FinancialAidFundAidCategoryType? awardCategoryName = null, bool restricted = false)
            : base(guid, code, description)
        {
            AwardCategoryType = awardCategoryType;
            AwardCategoryName = awardCategoryName;
            restrictedFlag = restricted;
        }
    }
}
