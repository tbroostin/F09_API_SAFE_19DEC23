// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public partial class ChargesCategory
    {
        public ChargesCategory()
        {
            FeeGroups = new List<FeeType>();
            TuitionBySectionGroups = new List<TuitionBySectionType>();
            OtherGroups = new List<OtherType>();
            Miscellaneous = new OtherType();
            RoomAndBoardGroups = new List<RoomAndBoardType>();
            TuitionByTotalGroups = new List<TuitionByTotalType>();
        }

        public List<FeeType> FeeGroups { get; set; }

        public OtherType Miscellaneous { get; set; }

        public List<OtherType> OtherGroups { get; set; }

        public List<RoomAndBoardType> RoomAndBoardGroups { get; set; }

        public List<TuitionBySectionType> TuitionBySectionGroups { get; set; }

        public List<TuitionByTotalType> TuitionByTotalGroups { get; set; }
    }
}