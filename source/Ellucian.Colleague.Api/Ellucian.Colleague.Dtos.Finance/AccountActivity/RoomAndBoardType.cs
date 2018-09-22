// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges with the "Room/Board" type
    /// </summary>
    public partial class RoomAndBoardType : NamedType
    {
        /// <summary>
        /// A list of <see cref="ActivityRoomAndBoardItem">room and meal charges</see>
        /// </summary>
        public List<ActivityRoomAndBoardItem> RoomAndBoardCharges { get; set; }
    }
}