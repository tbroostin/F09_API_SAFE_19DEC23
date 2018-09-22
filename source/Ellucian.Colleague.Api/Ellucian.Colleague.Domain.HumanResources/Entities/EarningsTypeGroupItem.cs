using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class EarningsTypeGroupItem
    {
        /// <summary>
        /// The id of the earnings type of this group entry
        /// </summary>
        public string EarningsTypeId { get; private set; }

        /// <summary>
        /// A custom description of this entry in the group
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The id of the EarningsTypeGroup to which this entry belongs.
        /// </summary>
        public string EarningsTypeGroupId { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="earningsTypeId"></param>
        /// <param name="description"></param>
        /// <param name="earningsTypeGroupId"></param>
        public EarningsTypeGroupItem(string earningsTypeId, string description, string earningsTypeGroupId)
        {
            if (string.IsNullOrWhiteSpace(earningsTypeId))
            {
                throw new ArgumentNullException("earningsTypeId");
            }
            if (string.IsNullOrWhiteSpace(earningsTypeGroupId))
            {
                throw new ArgumentNullException("earningsTypeGroupId");
            }

            EarningsTypeId = earningsTypeId;
            Description = description;
            EarningsTypeGroupId = earningsTypeGroupId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var groupItem = obj as EarningsTypeGroupItem;

            if (EarningsTypeGroupId == groupItem.EarningsTypeGroupId &&
                EarningsTypeId == groupItem.EarningsTypeId)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return EarningsTypeGroupId.GetHashCode() ^ EarningsTypeId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}: {2}", EarningsTypeGroupId, EarningsTypeId, Description);
        }
    }
}
