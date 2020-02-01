//Copyright 2019 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Contains the descriptions for housing and enrollment to display in a table in ConfigurableOfferLetter.rdlc
    /// </summary>
    public class OfferLetterHousingEnrollmentItem
    {
        /// <summary>
        /// Description associated with a housing assignment
        /// </summary>
        public string AlhHousingDesc { get; set; }

        /// <summary>
        /// Description associated with an enrollment assignment
        /// </summary>
        public string AlhEnrollmentDesc { get; set; }
    }
}
