namespace Ellucian.Colleague.Api.Client.Exceptions
{
    /// <summary>
    /// Represents specific exception codes for handling and reporting errors associated with Advising objects.
    /// </summary>
    public enum AdvisingExceptionCodes
    {
        /// <summary>
        /// Occurs when the degree plan cannot be read or does not exist.
        /// </summary>
        PlanNotFound,
        /// <summary>
        /// Occurs when a search for advisees fails for any reason.
        /// </summary>
        SearchFailed,
        /// <summary>
        /// Occurs when the active advisor is not fully authorized to perform some requested action.
        /// </summary>
        UnauthorizedAdvisor,
        /// <summary>
        /// Occurs when the active advisor does not have permission to modify an advisee's plan.
        /// </summary>
        UnauthorizedAdvisorModifyPlan,
        /// <summary>
        /// Occurs when the active advisor does not have permission to view an advisee's plan.
        /// </summary>
        UnauthorizedAdvisorViewPlan
    }
}