//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// An FA StudentAwardPeriod object
    /// </summary>
    public class StudentAwardPeriod
    {
        /// <summary>
        /// The Student ID number
        /// Required in PUT StudentAward and PUT StudentAwardPackage
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The year this record belongs to
        /// Required in PUT StudentAward and PUT StudentAwardPackage
        /// </summary>
        public string AwardYearId { get; set; }

        /// <summary>
        /// The Award code
        /// Required in PUT StudentAward and PUT StudentAwardPackage
        /// </summary>
        public string AwardId { get; set; }

        /// <summary>
        /// The Award Period
        /// Required in PUT StudentAward and PUT StudentAwardPackage
        /// </summary>
        public string AwardPeriodId { get; set; }

        /// <summary>
        /// Award amount for the year and award period
        /// Cannot be set to null in PUT StudentAward and PUT StudentAwardPackage.
        /// </summary>
        public decimal? AwardAmount { get; set; }

        /// <summary>
        /// The Award Action Status
        /// Cannot be set to null in PUT StudentAward and PUT StudentAwardPackage
        /// </summary>
        public string AwardStatusId { get; set; }

        /// <summary>
        /// The action taken by the API consumer. No longer used. Use AwardStatusId to request a status update instead.
        /// </summary>
        public AwardStatusCategory? UpdateActionTaken { get; set; }

        /// <summary>
        /// ReadOnly: Indicates whether the amount of this StudentAwardPeriod is frozen, i.e. can't be modified.
        /// If true, PUT StudentAward and PUT StudentAwardPackage will throw an exception
        /// </summary>
        public bool IsFrozen { get; set; }

        /// <summary>
        /// ReadOnly: Indicates whether all or a portion of the amount of the StudentAwardPeriod has been transmitted to the
        /// student's account.
        /// If true, PUT StudentAward and PUT StudentAwardPackage will throw an exception.
        /// </summary>
        public bool IsTransmitted { get; set; }

        /// <summary>
        /// Indicates whether or not this award period amount can be modified. 
        /// Generally, only loans can be modified.
        /// If false, PUT StudentAward and PUT StudentAwardPackage will throw an exception if you attempt to update the award amount
        /// </summary>
        public bool IsAmountModifiable { get; set; }

        /// <summary>
        /// Indicates whether or not this award period status can be modified
        /// If false, PUT StudentAward and PUT StudentAwardPackage will throw an exception if you attempt to update the award status id
        /// </summary>
        public bool IsStatusModifiable { get; set; }

        /// <summary>
        /// Indicates whether to ignore this award on the award letter when deciding on whether to allow
        /// the award letter to be signed.
        /// </summary>
        public bool IsIgnoredOnAwardLetter { get; set; }

        /// <summary>
        /// Indicates whether this award period is viewable on award letter and shopping sheet pages
        /// </summary>
        public bool IsViewableOnAwardLetterAndShoppingSheet { get; set; }

        /// <summary>
        /// Determine whether this period should be taken into account when calculating completeness of the 
        /// award package checklist item
        /// </summary>
        public bool IsIgnoredOnChecklist { get; set; }

    }
}
