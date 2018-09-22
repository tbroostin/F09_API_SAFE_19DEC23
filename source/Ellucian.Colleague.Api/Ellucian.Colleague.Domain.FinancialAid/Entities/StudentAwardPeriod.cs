//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Definition of a StudentAwardPeriod
    /// </summary>
    [Serializable]
    public class StudentAwardPeriod
    {
        private readonly StudentAward _StudentAward;
        private readonly string _AwardPeriodId;
        private readonly bool _IsTransmitted;
        private readonly bool _IsFrozen;
        private decimal? _AwardAmount;

        /// <summary>
        /// The StudentAward object this StudentAwardPeriod is attached to.
        /// </summary>
        public StudentAward StudentAward { get { return _StudentAward; } }

        /// <summary>
        /// Student ID
        /// </summary>
        public string StudentId { get { return StudentAward.StudentId; } }

        /// <summary>
        /// Financial Aid Award Year
        /// </summary>
        public StudentAwardYear StudentAwardYear { get { return StudentAward.StudentAwardYear; } }

        /// <summary>
        /// Award Code
        /// </summary>
        public Award Award { get { return StudentAward.Award; } }

        /// <summary>
        /// Award Period code
        /// </summary>
        public string AwardPeriodId { get { return _AwardPeriodId; } }

        /// <summary>
        /// Award amount for the award period can be null, but must be greater than or equal to 0.
        /// An ArgumentOutOfRangeException is thrown if this attribute is set to a negative value.
        /// </summary>
        public decimal? AwardAmount
        {
            get { return _AwardAmount; }
            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("AwardAmount cannot be less than 0");
                }
                else
                {
                    _AwardAmount = value;
                }
            }
        }

        /// <summary>
        /// Award Status object indicates the state of this award period.
        /// </summary>
        public AwardStatus AwardStatus { get; set; }

        /// <summary>
        /// This attribute indicates whether or not the financial aid office has
        /// frozen the award amount of this award period.
        /// </summary>
        public bool IsFrozen { get { return _IsFrozen; } }

        /// <summary>
        /// This attribute indicates whether or not this award period was ever transmitted
        /// to the student's accounts.
        /// </summary>
        public bool IsTransmitted { get { return _IsTransmitted; } }

        /// <summary>
        /// This attribute indicates whether or not a loan disbursement record is assigned to this award period.
        /// Note that this is a temporary attribute and should be replaced when we add full support for disbursement objects
        /// </summary>
        public bool HasLoanDisbursement { get; set; }

        /// <summary>
        /// This indicates whether or not the award period's amount can be modified.
        /// Generally, only pending and estimated loans can be modified. If the loan is accepted
        /// and the configuration permits it, the amount could be changed as well.
        /// </summary>
        public bool IsAmountModifiable
        {
            get
            {
                return (Award.LoanType.HasValue) && (Award.LoanType.Value != LoanType.OtherLoan) && (Award.IsFederalDirectLoan) &&
                    (StudentAwardYear.CurrentConfiguration != null) && (StudentAwardYear.CurrentConfiguration.AllowLoanChanges) && (StudentAwardYear.CurrentConfiguration.AreAwardChangesAllowed) &&
                    (HasLoanDisbursement) &&
                    ((IsStatusModifiable) || (AwardStatus.Category == AwardStatusCategory.Accepted 
                    && (StudentAwardYear.CurrentConfiguration.AllowLoanChangeIfAccepted || StudentAwardYear.CurrentConfiguration.AllowDeclineZeroOfAcceptedLoans)))
                    && (!IsTransmitted) 
                    && (!IsFrozen);
            }
        }

        /// <summary>
        /// This indicates whether or not the award period's status can be modified
        /// </summary>
        public bool IsStatusModifiable
        {
            get
            {
                //if awarding is not active for the year, return false
                if (StudentAwardYear.CurrentConfiguration == null || !StudentAwardYear.IsAwardingActive || !StudentAwardYear.CurrentConfiguration.AreAwardChangesAllowed) return false;

                //if the award category is contained in the list of award categories to exclude from change, return false
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromChange != null &&
                    Award.AwardCategory != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromChange.Contains(Award.AwardCategory.Code))
                {
                    return false;
                }

                //if the award code is contained in the list of award codes to exclude from change, return false
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardsFromChange != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardsFromChange.Contains(Award.Code))
                {
                    return false;
                }

                //If I have a loan that has been transmitted set the status to false
                if((Award.LoanType.HasValue) && (IsTransmitted))
                {
                    return false;
                }


                //If I have a Direct Loan that is Accepted, Check flag to see if it can be modified
                if ((Award.LoanType.HasValue) && (Award.LoanType.Value != LoanType.OtherLoan) && (Award.IsFederalDirectLoan) &&
                    (AwardStatus.Category == AwardStatusCategory.Accepted) &&
                    (StudentAwardYear.CurrentConfiguration.AllowDeclineZeroOfAcceptedLoans))
                {
                    return true;
                }

                // If an Award Status is not Pending or Estimated then set to not modifiable
                if (AwardStatus.Category == AwardStatusCategory.Accepted ||
                    AwardStatus.Category == AwardStatusCategory.Denied ||
                    AwardStatus.Category == AwardStatusCategory.Rejected)
                {
                    return false;
                }

                // If the Award Status is contained in the list of award statuses to exclude from change, return false
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusesFromChange != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusesFromChange.Contains(AwardStatus.Code))
                {
                    return false;
                }

                //If StudentAward is in the holding bin then set to not modifiable
                if (!string.IsNullOrEmpty(StudentAward.PendingChangeRequestId))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Determine whether this should be shown on the award letter.
        /// </summary>
        public bool IsIgnoredOnAwardLetter
        {
            get
            {
                //if award letter is not active for the year, don't let student sign letter, return true to ignore award
                if (StudentAwardYear.CurrentConfiguration == null || !StudentAwardYear.CurrentConfiguration.IsAwardLetterActive) return true;

                //if the award category is contained in the list of award categories to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesFromEval != null &&
                    Award.AwardCategory!= null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesFromEval.Contains(Award.AwardCategory.Code))
                {
                    return true;
                }

                //if the award is contained in the list of awards to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardsFromEval != null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardsFromEval.Contains(Award.Code))
                {
                    return true;
                }


                //if the action status is contained in the list of action statuses to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardStatusesFromEval != null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardStatusesFromEval.Contains(AwardStatus.Code))
                {
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// True if the StudentAward Is Viewable and 
        /// if this award period id is not contained in the list of award periods to exclude and
        /// if this award period's award status category is not contained in the list of award status categories to exclude
        /// in the current configuration
        /// </summary>
        public bool IsViewable
        {
            get
            {
                //if the current configuration is null or awarding is not active, return false
                if (StudentAwardYear.CurrentConfiguration == null || !StudentAwardYear.CurrentConfiguration.IsAwardingActive) return false;

                //if the award category is contained in the list of award categories to exclude, return false.
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesView != null &&
                    Award.AwardCategory != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesView.Contains(Award.AwardCategory.Code))
                {
                    return false;
                }

                //if the award code is contained in the list of award codes to exclude, return false.
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardsView != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardsView.Contains(Award.Code))
                {
                    return false;
                }

                //if the awardStatus category is in the list of award status categories to exclude, return false;
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesView != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesView.Contains(this.AwardStatus.Category))
                {
                    return false;
                }

                //if the award period is in the list of award periods to exclude, return false
                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriods != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriods.Contains(this.AwardPeriodId))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// This is true if the AwardStatusCategory, Award code, Award Period id, and Award Category code is not excluded from the AwardLetter/Shopping 
        /// sheet by the configuration.
        /// </summary>
        public bool IsViewableOnAwardLetterAndShoppingSheet
        {
            get
            {
                if (StudentAwardYear.CurrentConfiguration == null || 
                    (!StudentAwardYear.CurrentConfiguration.IsAwardLetterActive && !StudentAwardYear.CurrentConfiguration.IsShoppingSheetActive)) return false;

                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet.Contains(this.AwardStatus.Category))
                {
                    return false;
                }

                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet.Contains(this.Award.Code))
                {
                    return false;
                }

                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet.Contains(this.AwardPeriodId))
                {
                    return false;
                }

                if (StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet != null &&
                    this.Award.AwardCategory != null &&
                    StudentAwardYear.CurrentConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet.Contains(this.Award.AwardCategory.Code))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Determine whether this period should be taken into account when calculating completeness of the 
        /// award package checklist item
        /// </summary>
        public bool IsIgnoredOnChecklist
        {
            get
            {
                //if there is no configuration for the year, return true to ignore award
                if (StudentAwardYear.CurrentConfiguration == null) return true;

                //if the award category is contained in the list of award categories to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist != null &&
                    Award.AwardCategory != null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardCategoriesOnChecklist.Contains(Award.AwardCategory.Code))
                {
                    return true;
                }

                //if the award is contained in the list of awards to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist != null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardsOnChecklist.Contains(Award.Code))
                {
                    return true;
                }


                //if the action status is contained in the list of action statuses to ignore, return true
                if (StudentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist != null &&
                    StudentAwardYear.CurrentConfiguration.IgnoreAwardStatusesOnChecklist.Contains(AwardStatus.Code))
                {
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// The constructor for StudentAwardPeriod assigns this period to a StudentAward object, identifies the period with 
        /// an awardPeriodId, and sets attributes that indicate the award period's current state (frozen, transmitted, modifiable, etc.)
        /// </summary>
        /// <param name="studentAward">The StudentAward to which this object belongs</param>
        /// <param name="awardPeriodId">The award period identifier that distinguishes this StudentAwardPeriod from others</param>
        /// <param name="awardStatus">The AwardStatus of this object</param>
        /// <param name="isFrozen">Flag indicating whether or not the award amount is frozen</param>
        /// <param name="isTransmitted">Flag indicating whether or not the award period has ever been transmitted.</param>       
        public StudentAwardPeriod(StudentAward studentAward, string awardPeriodId, AwardStatus awardStatus, bool isFrozen, bool isTransmitted)
        {
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }
            if (string.IsNullOrEmpty(awardPeriodId))
            {
                throw new ArgumentNullException("awardPeriodId");
            }

            _StudentAward = studentAward;
            _AwardPeriodId = awardPeriodId;
            AwardStatus = awardStatus;
            _IsFrozen = isFrozen;
            _IsTransmitted = isTransmitted;

            StudentAward.StudentAwardPeriods.Add(this);
        }

        /// <summary>
        /// The equals method override compares two StudentAwardPeriod objects. Two StudentAwardPeriod objects
        /// are equal if they are assigned to the same StudentAward and their awardPeriodIds are equal.
        /// </summary>
        /// <param name="obj">A StudentAwardPeriod object to compare</param>
        /// <returns>True if this StudentAwardPeriod and object are equal. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var studentAwardPeriod = obj as StudentAwardPeriod;

            if (studentAwardPeriod.StudentAward.Equals(this.StudentAward) &&
                studentAwardPeriod.AwardPeriodId == this.AwardPeriodId)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a unique HashCode for this object
        /// </summary>
        /// <returns>A HashCode based on the unique properties of this object</returns>
        public override int GetHashCode()
        {
            return StudentAward.GetHashCode() + AwardPeriodId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}*{1}*{2}", this.StudentId, this.Award.Code, this.AwardPeriodId);
        }
    }
}
