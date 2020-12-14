// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Academic program for a catalog year that users may select when registering and paying for classes through instant enrollment
    /// </summary>
    [Serializable]
    public class AcademicProgramOption
    {
        /// <summary>
        /// Unique code for the academic program
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Catalog code associated with the academic program
        /// </summary>
        public string CatalogCode { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AcademicProgramOption"/>
        /// </summary>
        /// <param name="code">Unique code for the academic program</param>
        /// <param name="catalogCode">Catalog code associated with the academic program</param>
        public AcademicProgramOption(string code, string catalogCode)
        {
            if(string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "An academic program code is required when building an instant enrollment academic program option.");
            }
            if (string.IsNullOrEmpty(catalogCode))
            {
                throw new ArgumentNullException("catalogCode", "An academic program catalog code is required when building an instant enrollment academic program option.");
            }
            Code = code;
            CatalogCode = catalogCode;
        }
    }
}
