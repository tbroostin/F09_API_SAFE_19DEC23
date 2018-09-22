//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The Financial Aid Award Letter class provides all the data, except Student Award data, to write on a student's award letter.
    /// </summary>
    [Serializable]
    public class AwardLetter
    {
        /// <summary>
        /// The StudentId of the student that this award letter belongs to
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The AwardYear object that this award letter describes
        /// </summary>
        public StudentAwardYear AwardYear { get { return _AwardYear; } }
        private readonly StudentAwardYear _AwardYear;

        /// <summary>
        /// This is the date the award letter was signed and accepted by the student. 
        /// If the award letter is not accepted, this date is null.
        /// </summary>
        public DateTime? AcceptedDate { get; set; }

        /// <summary>
        /// The opening paragraph to be displayed on the award letter before the student awards
        /// </summary>
        public string OpeningParagraph { get; set; }

        /// <summary>
        /// The closing paragraph to be displayed on the award letter after the student awards
        /// </summary>
        public string ClosingParagraph { get; set; }

        /// <summary>
        /// Flag indicates whether the contact information should be displayed on the award letter
        /// </summary>
        public bool IsContactBlockActive { get; set; }

        /// <summary>
        /// The Contact name is the name used in an address label
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// The contact address is the address (street address, city, state, zip) used in an address label.
        /// Each element in this address corresponds to a new line in the address label.
        /// </summary>
        public List<string> ContactAddress { get; set; }

        /// <summary>
        /// The contact phone number is the last line used in an address label
        /// </summary>
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        /// Flag indicates whether the student's Financial Need information should be displayed on the award letter.
        /// </summary>
        public bool IsNeedBlockActive { get; set; }

        /// <summary>
        /// Flag to indicate whether the student's housing information should be displayed
        /// on the award letter
        /// </summary>
        public bool IsHousingCodeActive { get; set; }

        /// <summary>
        /// Housing code
        /// </summary> 
        public HousingCode? HousingCode { get; set; }

        /// <summary>
        /// Title of the awards info section
        /// </summary>
        public string AwardNameTitle { get; set; }

        /// <summary>
        /// Title of the total awards column/section
        /// </summary>
        public string AwardTotalTitle { get; set; }


        /// <summary>
        /// Collection of award categories groups as received from award letter parameter form
        /// </summary>
        public ReadOnlyCollection<AwardLetterGroup> AwardCategoriesGroups { get; private set; }
        private readonly List<AwardLetterGroup> _awardCategoriesGroups;

        /// <summary>
        /// Collection of award period column groups as received from award letter parameter form
        /// </summary>
        public ReadOnlyCollection<AwardLetterGroup> AwardPeriodColumnGroups { get; private set; }
        private readonly List<AwardLetterGroup> _awardPeriodColumnGroups;

        /// <summary>
        /// The group for all the awards that were not put in other categories groups
        /// </summary>
        public AwardLetterGroup NonAssignedAwardsGroup { get; set; }

        /// <summary>
        /// The student's Financial Aid Budget (Cost of school as known to FA)
        /// </summary>
        private int budgetAmount;

        /// <summary>
        /// Get the student's Financial Aid Budget amount (Cost of school as known to FA)
        /// </summary>
        /// <returns>The student's FA budget amount </returns>
        public int BudgetAmount { get { return budgetAmount; } }

        /// <summary>
        /// Set the student's Financial Aid budget amount. This method adds the student's total expenses and budget adjustment
        /// </summary>
        /// <param name="totalExpenses">The student's total expenses for the year.</param>
        /// <param name="budgetAdjustment">The student's budget adjustment for the year.</param>
        public void SetBudgetAmount(int totalExpenses, int budgetAdjustment)
        {
            budgetAmount = (totalExpenses + budgetAdjustment);
        }

        /// <summary>
        /// The student's Estimated Family Contribution (how much the student can pay)
        /// </summary>
        private int estimatedFamilyContributionAmount;

        /// <summary>
        /// Get the student's Estimated Family Contribution (how much the student can pay)
        /// </summary>
        /// <returns>The student's Estimated Family Contribution </returns>
        public int EstimatedFamilyContributionAmount { get { return estimatedFamilyContributionAmount; } }

        /// <summary>
        /// Set the student's estimated family contribution. This setter adds the student's estimated family contribution and the institutional adjustment.
        /// </summary>
        /// <param name="estimatedFamilyContribution">The student's estimated family contribution</param>
        /// <param name="institutionalAdjustment">Any adjustment to the estimated family contribution the institution has applied to the student.</param>
        public void SetEstimatedFamilyContributionAmount(int estimatedFamilyContribution, int institutionalAdjustment)
        {
            estimatedFamilyContributionAmount = (estimatedFamilyContribution + institutionalAdjustment);
        }

        /// <summary>
        /// The student's Financial Need (how much financial assistance the student needs)
        /// </summary>
        public int NeedAmount { get; set; }

        /// <summary>
        /// Constructor builds a basic award letter.
        /// </summary>
        /// <param name="studentId">The Colleague Person Id of the student this award letter belongs to</param>
        /// <param name="studentAwardYear">The award year that this award letter describes</param>
        public AwardLetter(string studentId, StudentAwardYear studentAwardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            if (studentId != studentAwardYear.StudentId)
            {
                throw new ArgumentException("studentAwardYearEntity must apply to the studentId: " + studentId, "studentId");
            }

            _StudentId = studentId;
            _AwardYear = studentAwardYear;

            OpeningParagraph = string.Empty;
            ClosingParagraph = string.Empty;

            IsContactBlockActive = false;
            ContactName = string.Empty;
            ContactAddress = new List<string>();
            ContactPhoneNumber = string.Empty;
            AwardNameTitle = string.Empty;
            AwardTotalTitle = string.Empty;


            IsNeedBlockActive = false;
            budgetAmount = 0;
            estimatedFamilyContributionAmount = 0;
            NeedAmount = 0;

            IsHousingCodeActive = false;
            HousingCode = null;

            _awardCategoriesGroups = new List<AwardLetterGroup>();
            this.AwardCategoriesGroups = _awardCategoriesGroups.AsReadOnly();

            _awardPeriodColumnGroups = new List<AwardLetterGroup>();
            this.AwardPeriodColumnGroups = _awardPeriodColumnGroups.AsReadOnly();

        }

        /// <summary>
        /// The equals method compares two AwardLetter objects. Two award letter objects are equal when they have the same
        /// StudentId and AwardYear
        /// </summary>
        /// <param name="obj">The AwardLetter object to compare to this</param>
        /// <returns>True if the two objects are equal. False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var awardLetter = obj as AwardLetter;

            if (awardLetter.StudentId != this.StudentId ||
                awardLetter.AwardYear != this.AwardYear)
            //||
            //awardLetter.OpeningParagraph != this.OpeningParagraph ||
            //awardLetter.ClosingParagraph != this.ClosingParagraph ||
            //awardLetter.ContactName != this.ContactName ||
            //awardLetter.BudgetAmount != this.BudgetAmount ||
            //awardLetter.EstimatedFamilyContributionAmount != this.EstimatedFamilyContributionAmount ||
            //awardLetter.NeedAmount != this.NeedAmount)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method returns a HashCode for this AwardLetter object. It uses the same attributes to generate a HashCode as the equals method uses
        /// to determine equality: StudentId and AwardYear
        /// </summary>
        /// <returns>A HashCode integer identifying this object</returns>
        public override int GetHashCode()
        {
            var studentIdHash = StudentId.GetHashCode();
            var awardYearHash = AwardYear.GetHashCode();

            return studentIdHash ^ awardYearHash;
        }

        /// <summary>
        /// Method to add an award category group to a read only collection of
        /// award category groups
        /// </summary>
        /// <param name="groupTitle">group title</param>
        /// <param name="sequenceNumber">sequence number</param>
        /// <param name="groupType">group type</param>
        /// <returns>true/false to indicate success/failure of adding a group</returns>
        public bool AddAwardCategoryGroup(string groupTitle, int sequenceNumber, GroupType groupType)
        {
            if (sequenceNumber < 0)
            {
                throw new ArgumentException("sequenceNumber must be greater than or equal to 0", "sequenceNumber");
            }

            if (groupType != GroupType.AwardCategories)
            {
                throw new ArgumentException("group type must be of type 'AwardCategories'", "groupType");
            }

            if (_awardCategoriesGroups.FirstOrDefault(alg => alg.SequenceNumber == sequenceNumber) == null)
            {
                _awardCategoriesGroups.Add(new AwardLetterGroup(groupTitle, sequenceNumber, groupType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to add an award period column group to a read only collection of
        /// award period column groups
        /// </summary>
        /// <param name="groupTitle">group title</param>
        /// <param name="sequenceNumber">sequence number</param>
        /// <param name="groupType">group type</param>
        /// <returns>true/false to indicate success/failure of adding a group</returns>
        public bool AddAwardPeriodColumnGroup(string groupTitle, int sequenceNumber, GroupType groupType)
        {
            if (sequenceNumber < 0)
            {
                throw new ArgumentException("sequenceNumber must be greater than or equal to 0", "sequenceNumber");
            }

            if (groupType != GroupType.AwardPeriodColumn)
            {
                throw new ArgumentException("group type must be of type 'AwardPeriodColumn'", "groupType");
            }

            if (_awardPeriodColumnGroups.FirstOrDefault(alg => alg.SequenceNumber == sequenceNumber) == null)
            {
                _awardPeriodColumnGroups.Add(new AwardLetterGroup(groupTitle, sequenceNumber, groupType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to remove an award category group from a read only collection
        /// of award category groups
        /// </summary>
        /// <param name="sequenceNumber">Sequence Number of the group to remove</param>/
        /// <returns>true/false to indicate success/failure of the group removal</returns>
        public bool RemoveAwardCategoryGroup(int sequenceNumber)
        {

            var groupToRemove = _awardCategoriesGroups.FirstOrDefault(g => g.SequenceNumber == sequenceNumber);
            if (groupToRemove != null)
            {
                return _awardCategoriesGroups.Remove(groupToRemove);
            }
            return false;
        }

        /// <summary>
        /// Method to remove an award period column group from a read only collection
        /// of award period column groups
        /// </summary>
        /// <param name="sequenceNumber">Sequence Number of the group to remove</param>
        /// <returns>true/false to indicate success/failure of the group removal</returns>
        public bool RemoveAwardPeriodColumnGroup(int sequenceNumber)
        {
            var groupToRemove = _awardPeriodColumnGroups.FirstOrDefault(g => g.SequenceNumber == sequenceNumber);
            if (groupToRemove != null)
            {
                return _awardPeriodColumnGroups.Remove(groupToRemove);
            }
            return false;
        }
    }
}
