// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Provides a class that describes an academic credit credit.
    /// This is light weight academic credit class used by program evaluation.
    /// </summary>
    public class AcademicCredit
    {
        /// <summary>
        /// Id of the academic credit
        /// /// </summary>
        public string AcademicCreditId { get; set; }

        /// <summary>
        /// Adjusted Credit value, accounting for repeats
        /// </summary>
        public decimal? AdjustedCredit { get; set; }

        /// <summary>
        /// Intended credits
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Status indicates whether credit is replaced or possibly replaced
        /// </summary>
        public ReplacedStatus ReplacedStatus { get; set; }

        /// <summary>
        /// Status indicates whether credit is a replacement or a possible replacement of another credit
        /// </summary>
        public ReplacementStatus ReplacementStatus { get; set; }

    }
}
