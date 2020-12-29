// Copyright 2017 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api.Client.Exceptions
{
    /// <summary>
    /// Represents specific exception codes for handling and reporting errors associated with DegreePlan objects.
    /// </summary>
    public enum DegreePlanExceptionCodes
    {
        /// <summary>
        /// Occurs when attempting to perform an update on a DegreePlan model which has been updated elsewhere.
        /// </summary>
        StalePlan,

        /// <summary>
        /// Occurs when the degree plan cannot be read/found
        /// </summary>
        NotFound,

        /// <summary>
        /// Occurs when the user tries to load a sample plan that does not exist.
        /// </summary>
        SamplePlanNotFound,

        /// <summary>
        /// Occurs when unable to add a new plan because of a record lock or existing plan
        /// </summary>
        AddPlanConflict
    }
}