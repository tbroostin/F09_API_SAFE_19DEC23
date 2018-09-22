/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of possible 
    /// </summary>
    [Serializable]
    public enum LeaveTypeCategory
    {

        /// <summary>
        /// Default - this value must be the first one set here
        /// </summary>
        None,

        /// <summary>
        /// Compensatory
        /// </summary>
        Compensatory,

        /// <summary>
        /// Sick
        /// </summary>
        Sick,

        /// <summary>
        /// Vacation
        /// </summary>
        Vacation
    }
}
