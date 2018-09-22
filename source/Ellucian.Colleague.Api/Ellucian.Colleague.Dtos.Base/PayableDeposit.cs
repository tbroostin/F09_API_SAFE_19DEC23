/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This object is obsolete as of API 1.16 for security reasons
    /// </summary>
    [Obsolete("Obsolete as of API 1.16")]
    public class PayableDeposit
    {
        /// <summary>
        /// The unique system-generated id of this PayableDeposit
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Address Id this deposit is associated to
        /// </summary>
        public string AddressId { get; set; }

        /// <summary>
        /// The date this deposit should become active
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date this deposit should become inactive
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this deposit is electronic
        /// </summary>
        public bool IsElectronicPaymentRequested { get; set; }

        /// <summary>
        /// The datetime this deposit account is added
        /// </summary>
        public DateTimeOffset AddDateTime { get; set; }

        /// <summary>
        /// The user who added the account
        /// </summary>
        public string AddOperator { get; set; }

        /// <summary>
        /// The datetime this account was last changed
        /// </summary>
        public DateTimeOffset ChangeDateTime { get; set; }

        /// <summary>
        /// The user who changed this account
        /// </summary>
        public string ChangeOperator { get; set; }

    }
}
