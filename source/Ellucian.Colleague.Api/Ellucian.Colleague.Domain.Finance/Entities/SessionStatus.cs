// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public enum SessionStatus
    {
        /// <summary>
        /// Session is Open
        /// </summary>
        Open,
        /// <summary>
        /// Session is Closed
        /// </summary>
        Closed,
        /// <summary>
        /// Session has been Reconciled
        /// </summary>
        Reconciled,
        /// <summary>
        /// Session has been Voided
        /// </summary>
        Voided
    }
}
