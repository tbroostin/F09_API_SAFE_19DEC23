﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of retention alert case resassignment detail
    /// </summary>
    public class RetentionAlertCaseReassignmentDetail
    {
        /// <summary>
        /// string value that will be used for reassignment
        /// </summary>
        public string AssignmentCode { get; set; }

        /// <summary>
        /// Title for reassignment
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To indicate if its the entity or role for reassignment 
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Boolean value to indicate if selected or not
        /// </summary>
        public bool IsSelected { get; set; }
    }
}