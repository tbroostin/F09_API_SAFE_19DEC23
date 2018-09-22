/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// The paystatement configuration contains the settings necessary to display the number of pay statements (by year) and 
    /// the days before or after the pay statements are viewable to the user.
    /// </summary>
    public class PayStatementConfiguration
    {
        /// <summary>
        /// Days a pay statement becomes viewable to the users (relative to the pay statement pay date). If positive, the
        /// pay statement is viewable OffsetDaysCount after the pay date. If negative, the pay statement is viewable
        /// OffsetDaysCount before the pay date.
        /// </summary>
        public int? OffsetDaysCount { get; set; }

        /// <summary>
        /// Number of historical years before the current year to display available pay statements.
        /// </summary>
        public int? PreviousYearsCount { get; set; }
    }
}
