// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Source of the regisration date information.
    /// </summary>
    [Serializable]
    public enum RegistrationDateSource
    {
        /// <summary>
        /// No source for registration dates 
        /// </summary>
        None,

        /// <summary>
        /// Registration dates determined by the term (RYAT > ACTM)
        /// </summary>
        Term,

        /// <summary>
        /// Registration dates are determined by the section override (SECT > SRGD)
        /// </summary>
        Section,

        /// <summary>
        /// Registration dates determined by the term/location override (RYAT > ACTM > TLOC) (TERM+LOCATION)
        /// </summary>
        TermLocation,

        /// <summary>
        /// Registration dates determined by the registartion user term override (RGUS > RGUD > RGUT) (REGUSER+TERM)
        /// </summary>
        RegistrationUserTerm,

        /// <summary>
        /// Registration dates determined by the registration user section override (RGUS > RGUD > RGUC) (REGUSER+SECT)
        /// </summary>
        RegistrationUserSection,

        /// <summary>
        /// Registration dates determined by the registartion user term/location override (RGUS > RGUD > RGUL) (REGUSER+TERM+LOCATION)
        /// </summary>
        RegistrationUserTermLocation,
    }
}
