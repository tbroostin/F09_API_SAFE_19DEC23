// Copyright 2022 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Release Access Code,Description and Comments
    /// </summary>
    public class StudentReleaseAccess
    {
        /// <summary>
        /// Unique code for the Student Release Access Record
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Student Release Access Code Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Student Release Access Comments
        /// </summary>
        public string Comments { get; set; }
    }
}
