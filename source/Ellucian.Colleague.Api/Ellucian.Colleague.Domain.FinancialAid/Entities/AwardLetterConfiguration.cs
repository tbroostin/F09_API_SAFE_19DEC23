//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardLetter configuration class provides award letter parameters data 
    /// that is not student specific
    /// </summary>
    [Serializable]
    public class AwardLetterConfiguration
    {
        /// <summary>
        /// Configuration id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Flag to indicate whether contact block should be included on the award letter
        /// </summary>
        public bool IsContactBlockActive { get; set; }

        /// <summary>
        /// Flag to indicate whether to display the housing code on the award letter
        /// </summary>
        public bool IsHousingBlockActive { get; set; }

        /// <summary>
        /// Flag to indicate whether EFC, Need, and Budget should be included on the
        /// award letter
        /// </summary>
        public bool IsNeedBlockActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsEfcActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsBudgetActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsDirectCostActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsIndirectCostActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsPreAwardTextActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsPostAwardTextActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsPostClosingTextActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsEnrollmentActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsRenewalActive { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public bool IsPellEntitlementActive { get; set; }

        /// <summary>
        /// Paragraph spacing for the opening and closing paragraphs of an award letter:
        /// single or double; single by default
        /// </summary>
        public string ParagraphSpacing { get; set; }

        /// <summary>
        /// Award letter awards table title;
        /// defaults to "Awards"
        /// </summary>
        public string AwardTableTitle { get; set; }

        /// <summary>
        /// Award total title for the award letter awards table;
        /// defaults to "Total"
        /// </summary>
        public string AwardTotalTitle { get; set; }

        /// <summary>
        /// Collection of award categories row groups
        /// </summary>
        public ReadOnlyCollection<AwardLetterGroup2> AwardCategoriesGroups { get; private set; }
        private readonly List<AwardLetterGroup2> _awardCategoriesGroups;

        /// <summary>
        /// Collection of award period column groups
        /// </summary>
        public ReadOnlyCollection<AwardLetterGroup2> AwardPeriodsGroups { get; private set; }
        private readonly List<AwardLetterGroup2> _awardPeriodsGroups;

        /// <summary>
        /// Constructor that accepts award letter configuration id and throws an exception
        /// if the id is null or empty
        /// </summary>
        /// <param name="id">configuration id</param>
        public AwardLetterConfiguration(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            Id = id;
            IsContactBlockActive = false;
            IsHousingBlockActive = false;
            IsNeedBlockActive = false;
            IsEfcActive = false;
            IsBudgetActive = false;
            IsDirectCostActive = false;
            IsIndirectCostActive = false;
            IsPellEntitlementActive = false;
            IsPostAwardTextActive = false;
            IsPreAwardTextActive = false;
            IsPostClosingTextActive = false;
            IsRenewalActive = false;

            _awardCategoriesGroups = new List<AwardLetterGroup2>();
            this.AwardCategoriesGroups = _awardCategoriesGroups.AsReadOnly();

            _awardPeriodsGroups = new List<AwardLetterGroup2>();
            this.AwardPeriodsGroups = _awardPeriodsGroups.AsReadOnly();
        }

        /// <summary>
        /// Method to add an award category group to a read only collection of
        /// award category groups
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <param name="groupNumber">group number</param>
        /// <param name="groupType">group type</param>
        /// <returns>true/false to indicate success/failure of adding a group</returns>
        public bool AddAwardCategoryGroup(string groupName, int groupNumber, GroupType groupType)
        {
            if (groupNumber < 0)
            {
                throw new ArgumentException("groupNumber must be greater than or equal to 0", "groupNumber");
            }

            if (groupType != GroupType.AwardCategories)
            {
                throw new ArgumentException("group type must be of type 'AwardCategories'", "groupType");
            }

            if (_awardCategoriesGroups.FirstOrDefault(alg => alg.GroupNumber == groupNumber) == null)
            {
                _awardCategoriesGroups.Add(new AwardLetterGroup2(groupName, groupNumber, groupType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to add an award period column group to a read only collection of
        /// award period column groups
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <param name="groupNumber">group number</param>
        /// <param name="groupType">group type</param>
        /// <returns>true/false to indicate success/failure of adding a group</returns>
        public bool AddAwardPeriodColumnGroup(string groupName, int groupNumber, GroupType groupType)
        {
            if (groupNumber < 0)
            {
                throw new ArgumentException("groupNumber must be greater than or equal to 0", "groupNumber");
            }

            if (groupType != GroupType.AwardPeriodColumn)
            {
                throw new ArgumentException("group type must be of type 'AwardPeriodColumn'", "groupType");
            }

            if (_awardPeriodsGroups.FirstOrDefault(alg => alg.GroupNumber == groupNumber) == null)
            {
                _awardPeriodsGroups.Add(new AwardLetterGroup2(groupName, groupNumber, groupType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method to remove an award category group from a read only collection
        /// of award category groups
        /// </summary>
        /// <param name="groupNumber">Group Number of the group to remove</param>/
        /// <returns>true/false to indicate success/failure of the group removal</returns>
        public bool RemoveAwardCategoryGroup(int groupNumber)
        {

            var groupToRemove = _awardCategoriesGroups.FirstOrDefault(g => g.GroupNumber == groupNumber);
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
        /// <param name="groupNumber">Group Number of the group to remove</param>
        /// <returns>true/false to indicate success/failure of the group removal</returns>
        public bool RemoveAwardPeriodColumnGroup(int groupNumber)
        {
            var groupToRemove = _awardPeriodsGroups.FirstOrDefault(g => g.GroupNumber == groupNumber);
            if (groupToRemove != null)
            {
                return _awardPeriodsGroups.Remove(groupToRemove);
            }
            return false;
        }
    }
}
