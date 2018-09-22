using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class EarningsTypeGroup
    {
        /// <summary>
        /// The earntype group ID
        /// </summary>
        public string EarningsTypeGroupId { get; private set; }

        /// <summary>
        /// The earnings type group description
        /// </summary>
        public string Description { get; private set; }


        /// <summary>
        /// Whether or not this earnings type group is enabled for use by Self Service Time Entry and Approval
        /// </summary>
        public bool IsEnabledForTimeManagement { get; private set; }

        /// <summary>
        /// The Id of the Campus Calendar <see cref="CampusCalendar"/> on which Holidays are specified
        /// </summary>
        public string HolidayCalendarId { get; set; }



        public ReadOnlyCollection<EarningsTypeGroupItem> EarningsTypeGroupItems
        {
            get
            {
                return earningsTypeGroupItems.AsReadOnly();
            }
        }
        private List<EarningsTypeGroupItem> earningsTypeGroupItems;

        /// <summary>
        /// Try to add an earningsTypeGroupItem. This method ensures all items in the list are unique.
        /// Method will return false if item already exists in the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryAdd(EarningsTypeGroupItem item)
        {
            if (earningsTypeGroupItems.Contains(item))
            {
                return false;
            }

            earningsTypeGroupItems.Add(item);
            return true;
        }

        /// <summary>
        /// Add a range of earningsTypeGroupItems. This method ensures all items in the list are unique. 
        /// It will not add duplicates from itemsToAdd.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddRange(IEnumerable<EarningsTypeGroupItem> itemsToAdd)
        {
            if (!earningsTypeGroupItems.Any())
            {
                earningsTypeGroupItems.AddRange(itemsToAdd.Distinct());
                return;
            }

            earningsTypeGroupItems.AddRange(itemsToAdd);
            earningsTypeGroupItems = earningsTypeGroupItems.Distinct().ToList();
            return;
            
        }

        public EarningsTypeGroup(string earningsTypeGroupId, string description, bool isEnabledForTimeManagement)
        {
            if (string.IsNullOrEmpty(earningsTypeGroupId))
            {
                throw new ArgumentNullException("earningsTypeGroupId");
            }
       
            EarningsTypeGroupId = earningsTypeGroupId;
            Description = description;
            IsEnabledForTimeManagement = isEnabledForTimeManagement;

            earningsTypeGroupItems = new List<EarningsTypeGroupItem>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var group = obj as EarningsTypeGroup;

            if (group.EarningsTypeGroupId == EarningsTypeGroupId)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return EarningsTypeGroupId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", EarningsTypeGroupId, Description);
        }
    }
    
}
