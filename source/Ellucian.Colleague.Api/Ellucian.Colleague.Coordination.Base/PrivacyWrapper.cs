// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base
{
    /// <summary>
    /// Utility class to allow coordination services to return privacy restriction info to calling methods.
    /// </summary>
    public class PrivacyWrapper<T>
    {
        /// <summary>
        /// The DTO to return
        /// </summary>
        public T Dto;

        /// <summary>
        /// If true, the Dto has been limited by the existence of privacy restrictions
        /// </summary>
        public bool HasPrivacyRestrictions;

        /// <summary>
        /// Default constructor needed for generic class
        /// </summary>
        public PrivacyWrapper()
        {

        }

        /// <summary>
        /// Privacy wrapper constructor
        /// </summary>
        /// <param name="dto">Generic typed dto object that is to be wrapped</param>
        /// <param name="hasPrivacyRestrictions">Indicates if the Dto has been limited by privacy restrictions</param>
        public PrivacyWrapper(T dto, bool hasPrivacyRestrictions)
        {
            Dto = dto;
            HasPrivacyRestrictions = hasPrivacyRestrictions;
        }

    }
}
