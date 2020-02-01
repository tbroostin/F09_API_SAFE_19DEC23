namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Represents a request to load a sample degree plan.
    /// </summary>
    public class LoadDegreePlanRequest
    {
        /// <summary>
        /// The ID of the student requesting a sample degree plan.
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// The code of the program for which the sample plan is being requested.
        /// </summary>
        public string ProgramCode { get; set; }
    }
}