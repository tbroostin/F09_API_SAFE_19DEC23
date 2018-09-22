/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The ShoppingSheetOfficeType is used to configure the shopping sheet's scorecard graphics, particularly the graduation rate.
    /// </summary>
    [Serializable]
    public enum ShoppingSheetOfficeType
    {
        /// <summary>
        /// A financial aid office assigned this type is primarily a bachelor's degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 6 years.
        /// Low: 0-37.3
        /// Med: 37.4-66.7
        /// High: 66.8-100
        /// </summary>
        BachelorDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily an associate's degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// Low: 0-17.7
        /// Med: 17.8-32.2
        /// High: 32.3-100
        /// </summary>
        AssociateDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a certificate granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// Low: 0-24.1
        /// Med: 24.2-50.1
        /// High: 50.2-100
        /// </summary>
        CertificateGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a graduate degree granting office.
        /// Shopping sheet will display the percentage of full-time undergraduate students who graduate within 150% of the expected time for completion.
        /// Since there are no undergraduates at a graduate institution, the graduation rates are blank.
        /// </summary>
        GraduateDegreeGranting,

        /// <summary>
        /// A financial aid office assigned this type is primarily a non-degree granting office.
        /// Shopping sheet will display the percentage of full-time students who graduate within 150% of the expected time for completion.
        /// Low: 0-62.5
        /// Med: 62.6-77.6
        /// High: 77.7-100
        /// </summary>
        NonDegreeGranting
    }
}
