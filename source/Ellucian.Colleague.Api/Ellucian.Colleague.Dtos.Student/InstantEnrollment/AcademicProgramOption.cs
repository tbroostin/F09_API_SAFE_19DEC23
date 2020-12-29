// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Academic program for a catalog year that users may select when registering and paying for classes through instant enrollment
    /// </summary>
    public class AcademicProgramOption
    {
        /// <summary>
        /// Unique code for the academic program
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Catalog code associated with the academic program
        /// </summary>
        public string CatalogCode { get; set; }
    }
}
