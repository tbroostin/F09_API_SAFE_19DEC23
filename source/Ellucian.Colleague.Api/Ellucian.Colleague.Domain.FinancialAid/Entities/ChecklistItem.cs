/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Defines a Checklist item and it's sorting order
    /// </summary>
    [Serializable]
    public class ChecklistItem
    {
        /// <summary>
        /// The item code
        /// </summary>
        public string ChecklistItemCode { get { return checklistItemCode; } }
        private readonly string checklistItemCode;

        /// <summary>
        /// The item type
        /// </summary>
        public ChecklistItemType ChecklistItemType;

        /// <summary>
        /// The sorting order
        /// </summary>
        public int ChecklistSortNumber { get { return checklistSortNumber; } }
        private readonly int checklistSortNumber;

        /// <summary>
        /// Description of the Checklist Item
        /// </summary>
        public string Description { get { return description; } }
        private readonly string description;

        /// <summary>
        /// Create a new ChecklistItem
        /// </summary>
        /// <param name="code">Required: Checklist Item Code cannot be null or empty</param>
        /// <param name="sortNumber">Required: Sort Number</param>
        /// <param name="description">Optional: Description can be null or empty</param>
        public ChecklistItem(string code, int sortNumber, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            checklistItemCode = code;
            checklistSortNumber = sortNumber;
            this.description = description;
        }

        /// <summary>
        /// Two ChecklistItem objects are equal when their codes are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var checklistItem = obj as ChecklistItem;

            if (checklistItem.ChecklistItemCode == this.ChecklistItemCode)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode based on the checklist item's code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ChecklistItemCode.GetHashCode();
        }


        /// <summary>
        /// Returns a string representation of this object based on the Code
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ChecklistItemCode;
        }
    }
}
