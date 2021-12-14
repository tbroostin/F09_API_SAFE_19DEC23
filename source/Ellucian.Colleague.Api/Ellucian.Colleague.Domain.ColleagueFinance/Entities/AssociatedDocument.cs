// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// An associated document.
    /// </summary>
    [Serializable]
    public class AssociatedDocument
    {
        /// <summary>
        /// The type of document.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The ID of the associated document.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The associated document number.
        /// </summary>
        public string Number { get; set; }
    }
}
