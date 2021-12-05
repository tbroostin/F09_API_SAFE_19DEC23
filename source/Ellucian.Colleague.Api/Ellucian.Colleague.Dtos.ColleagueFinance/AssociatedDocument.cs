// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Associated Document DTO
    /// </summary>
    public class AssociatedDocument
    {
        /// <summary>
        /// This is the document type of the associated document.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// This is the ID of the associated document.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is the document number of the associated document.
        /// </summary>
        public string Number { get; set; }
    } 
}
