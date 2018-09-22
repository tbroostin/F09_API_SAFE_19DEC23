// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The Dto that holds the Drop Reason
    /// </summary>
    public class DropReason
    {
        /// <summary>
        /// Unique code for the Drop Reason
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Drop Reason description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To indicate if a drop reason code is displayed in SelfService.
        /// </summary>
        public bool DisplayInSelfService { get; set; }

    }
}
