/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The types of checklist items
    /// </summary>
    public enum ChecklistItemType
    {
        /// <summary>
        /// FAFSA Application on file checklist item
        /// </summary>
        FAFSA,

        /// <summary>
        /// PROFILE Application on file checklist item
        /// </summary>
        PROFILE,

        /// <summary>
        /// Completed Documents checklist item
        /// </summary>
        CompletedDocuments,

        /// <summary>
        /// Application under Review checklist item
        /// </summary>
        ApplicationReview,

        /// <summary>
        /// Review Award Package checklist item
        /// </summary>
        ReviewAwardPackage,

        /// <summary>
        /// Review Award Letter checklist item
        /// </summary>
        ReviewAwardLetter
    }
}
