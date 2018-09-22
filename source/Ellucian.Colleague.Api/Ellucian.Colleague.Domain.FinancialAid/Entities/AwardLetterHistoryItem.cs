/*Copyright 2015 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardLetterHistoryItem class
    /// </summary>
    [Serializable]
    public class AwardLetterHistoryItem
    {
        /// <summary>
        /// AwardLetterHistoryItem id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// AwardLetterHistoryItem created date
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// AwardLetterHistoryItem Constructor
        /// </summary>
        /// <param name="id">id of the award letter history record</param>
        /// <param name="createdDate">created date of the award letter history record</param>
        public AwardLetterHistoryItem(string id, DateTime? createdDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (createdDate == null)
            {
                throw new ArgumentNullException("createdDate");
            }

            Id = id;
            CreatedDate = createdDate;
        }

        /// <summary>
        /// Two award letter history items are equal when their ids match
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var historyItem = obj as AwardLetterHistoryItem;

            if (historyItem.Id != this.Id)
            {
                return false;
            }

            return true;
        }
    }
}
