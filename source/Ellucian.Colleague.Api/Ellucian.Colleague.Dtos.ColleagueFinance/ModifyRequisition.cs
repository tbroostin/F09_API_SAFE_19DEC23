// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The modify requisition information exposed from the domain entity
    /// </summary>
    public class ModifyRequisition
    {
        /// <summary>
        /// Requisition object that needs to be modified.
        /// </summary>
        public Requisition Requisition { get; set; }

        /// <summary>
        /// Default line item additional details object.
        /// </summary>
        public NewLineItemDefaultAdditionalInformation DefaultLineItemAdditionalDetails { get; set; }

    }
}
