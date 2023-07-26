/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public enum ChecklistItemType
    {
        /// <summary>
        /// FAFSA
        /// </summary>
        FAFSA,

        /// <summary>
        /// PROFILE
        /// </summary>
        PROFILE,

        /// <summary>
        /// Completed Documents
        /// </summary>
        CompletedDocuments,

        /// <summary>
        /// Application Review
        /// </summary>
        ApplicationReview,

        /// <summary>
        /// Review Award Package
        /// </summary>
        ReviewAwardPackage,

        /// <summary>
        /// Review Award Letter
        /// </summary>
        ReviewAwardLetter,

        /// <summary>
        /// Housing option selection
        /// </summary>
        HousingOption
    }
}
