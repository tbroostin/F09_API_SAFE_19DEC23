// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// Transfer Equivalency Work
    /// </summary>
    public class TransferEquivalencies
    {
        /// <summary>
        /// Id of the Institution the Transfer Work is from
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// List of equivalencies
        /// </summary>
        public List<Equivalency> Equivalencies { get; set; }

        /// <summary>
        /// Total number of credits transfered for these equivalencies
        /// </summary>
        public decimal TotalTransferCredits { get; set; }

        /// <summary>
        /// Total number of institutional credits for these equivalencies
        /// </summary>
        public decimal TotalEquivalentCredits { get; set; }
    }
}
