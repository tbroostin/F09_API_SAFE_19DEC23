// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A room or meal charge
    /// </summary>
    public class ActivityRoomAndBoardItem : ActivityDateTermItem
    {
        /// <summary>
        /// Room identifier
        /// </summary>
        public string Room { get; set; }
    }
}
