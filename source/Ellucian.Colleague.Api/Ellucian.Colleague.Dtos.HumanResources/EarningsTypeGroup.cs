using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Dto contains information about an earnings type group including the list of earningsTypes
    /// included in the group. An EarningsType group can be applied to a PersonPositionWage to indicate
    /// which earnings types that person can work on for that particular wage.
    /// </summary>
    public class EarningsTypeGroup
    {
        /// <summary>
        /// The ID of the EarningsTypeGroup
        /// </summary>
        public string EarningsTypeGroupId { get; set; }

        /// <summary>
        /// The earnings type group description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The list of EarningsTypes contained in teh earningsTypeGroup
        /// </summary>
        public IEnumerable<EarningsTypeGroupItem> EarningsTypeGroupItems { get; set; }

        /// <summary>
        /// Whether or not this earnings type group is enabled for use by Self Service Time Entry and Approval
        /// </summary>
        public bool IsEnabledForTimeManagement { get; set; }

        /// <summary>
        /// The Id of the Campus Calendar <see cref="CampusCalendar"/> on which Holidays are specified
        /// </summary>
        public string HolidayCalendarId { get; set; }
    }
}
