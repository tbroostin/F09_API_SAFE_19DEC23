//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of Categories for the AwardStatuses.
    /// Colleague uses AwardStatusCategories to understand the basic
    /// state of an award.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AwardStatusCategory
    {
        /// <summary>
        /// The Accepted category implies that a student will receive the money awarded for this particular award.
        /// </summary>
        Accepted,

        /// <summary>
        /// The Pending category implies that an action needs to be taken on this award. The student or financial aid officer
        /// must decide whether to accept or reject an award with a Pending category. Estimated and Pending are generally the same thing.
        /// </summary>
        Pending,

        /// <summary>
        /// The Estimated category implies that an action needs to be taken on this award. The student or financial aid officer
        /// must decide whether to accept or reject an award with an Estimated category. Estimated and Pending are generally the same thing
        /// </summary>
        Estimated,

        /// <summary>
        /// The Rejected category implies that the student will not receive the money awarded for this particular award.
        /// Rejected and Denied are generally the same thing
        /// </summary>
        Rejected,

        /// <summary>
        /// The Denied category implies that the student will not receive the money awarded for this particular award. 
        /// Rejected and Denied are generally the same thing
        /// </summary>
        Denied
    }
}
