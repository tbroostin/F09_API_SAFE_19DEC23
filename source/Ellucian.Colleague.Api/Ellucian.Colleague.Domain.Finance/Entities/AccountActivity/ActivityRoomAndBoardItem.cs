// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityRoomAndBoardItem : ActivityDateTermItem
    {
        public string Room { get; set; }
    }
}
