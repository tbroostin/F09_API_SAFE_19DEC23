/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The ShoppingSheetOfficeType is used to configure the shopping sheet's scorecard graphics, particularly the graduation rate.
    /// </summary>
    public enum ShoppingSheetOfficeType
    {
        /// <summary>
        /// A financial aid office assigned this type is primarily a bachelor's degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 6 years.
        /// </summary>
        BachelorDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily an associate's degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// </summary>
        AssociateDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a certificate granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// </summary>
        CertificateGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a graduate degree granting office.
        /// Shopping sheet will display the percentage of full-time undergraduate students who graduate within 150% of the expected time for completion.
        /// </summary>
        GraduateDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a non-degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// </summary>
        NonDegreeGranting
    }
}
