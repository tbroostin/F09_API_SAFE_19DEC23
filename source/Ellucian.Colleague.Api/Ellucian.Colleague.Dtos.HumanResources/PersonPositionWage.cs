/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// A PersonPositionWage object describes the parameters used to determine how a 
    /// person is paid for their particular position.
    /// </summary>
    public class PersonPositionWage
    {
        /// <summary>
        /// The DB Id of the object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Colleague PersonId of the object
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The PositionId of the object
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// The Id of the PersonPosition this wage object is attached to
        /// </summary>
        public string PersonPositionId { get; set; }

        /// <summary>
        /// The Id of the PositionPayDefault object that contains the default parameters used to build
        /// this object
        /// </summary>
        public string PositionPayDefaultId { get; set; }

        /// <summary>
        /// The start date of when these wage parameters take effect
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of when these wage parameters stop taking effect
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The Id of the PayClass which describes when and how the employee is paid for the work done in the position.
        /// PayCycles contain a list of PayClasses.
        /// </summary>
        public string PayClassId { get; set; }

        /// <summary>
        /// The Id of the Paycycle, which is the effective PayCycle based on the PayClassId, of this object
        /// </summary>
        public string PayCycleId { get; set; }

        /// <summary>
        /// The Id of the EarningsType the person tracks time against for regular work for this position
        /// </summary>
        public string RegularWorkEarningsTypeId { get; set; }

        /// <summary>
        /// Indicates whether this person's pay is suspended for this position.
        /// </summary>
        public bool IsPaySuspended { get; set; }

        /// <summary>
        /// List of PositionFundingSources
        /// </summary>
        public List<PositionFundingSource> FundingSources { get; set; }

        /// <summary>
        /// The Id of the earnings type group associated to the position pay record for this employee's wage
        /// </summary>
        public string EarningsTypeGroupId { get; set; }


    }
}
