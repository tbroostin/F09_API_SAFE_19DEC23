// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Charges on an account
    /// </summary>
    public partial class ChargesCategory
    {
        /// <summary>
        /// ChargesCategory constructor
        /// </summary>
        public ChargesCategory()
        {
            FeeGroups = new List<FeeType>();
            TuitionBySectionGroups = new List<TuitionBySectionType>();
            OtherGroups = new List<OtherType>();
            Miscellaneous = new OtherType();
            RoomAndBoardGroups = new List<RoomAndBoardType>();
            TuitionByTotalGroups = new List<TuitionByTotalType>();
        }

        /// <summary>
        /// List of <see cref="FeeType">Fee</see> charges
        /// </summary>
        public List<FeeType> FeeGroups { get; set; }

        /// <summary>
        /// Miscellaneous <see cref="OtherType">Other</see> charges
        /// </summary>
        public OtherType Miscellaneous { get; set; }

        /// <summary>
        /// List of <see cref="OtherType">Other</see> charges
        /// </summary>
        public List<OtherType> OtherGroups { get; set; }

        /// <summary>
        /// List of <see cref="RoomAndBoardType">Room and board</see> charges
        /// </summary>
        public List<RoomAndBoardType> RoomAndBoardGroups { get; set; }

        /// <summary>
        /// List of <see cref="TuitionBySectionType">Tuition by Section</see> charges
        /// </summary>
        public List<TuitionBySectionType> TuitionBySectionGroups { get; set; }

        /// <summary>
        /// List of <see cref="TuitionByTotalType">Tuition by Total</see> charges
        /// </summary>
        public List<TuitionByTotalType> TuitionByTotalGroups { get; set; }
    }
}