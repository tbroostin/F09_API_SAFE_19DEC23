/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Contains parameters that control how Pay Statements are displayed to users in Colleague Self Service.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Controls whether or not to display an employee's benefit or deduction that has a zero amount paid year to date.
        /// </summary>
        public bool DisplayZeroAmountBenefitDeductions { get; set; }

        /// <summary>
        /// Controls the display of an employee's social security number
        /// </summary>
        public SSNDisplay SocialSecurityNumberDisplay { get; set; }

        /// <summary>
        /// Control the display of the filing status
        /// </summary>
        public bool DisplayWithholdingStatusFlag { get; set; }

        /// <summary>
        /// Message to display to employee
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The name of the institution
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// The institution's mailing label
        /// </summary>
        public List<PayStatementAddress> InstitutionMailingLabel { get; set; }

        /// <summary>
        /// Default constructor defaults all configuration. override specific values.
        /// </summary>
        public PayStatementConfiguration()
        {
            OffsetDaysCount = 0;
            PreviousYearsCount = null;
            DisplayZeroAmountBenefitDeductions = false;
            SocialSecurityNumberDisplay = SSNDisplay.LastFour;
            DisplayWithholdingStatusFlag = true;
            Message = string.Empty;
            InstitutionName = string.Empty;
            InstitutionMailingLabel = new List<PayStatementAddress>();
        }
    }
}
