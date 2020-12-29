// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Used to control how advisees are to be retrieved when getting advisor information
    /// </summary>
    [Serializable]
    public enum AdviseeInclusionType
    {
        /// <summary>
        /// Return all advisees for advisior, past, present and future.
        /// </summary>
        AllAdvisees,
        /// <summary>
        /// Do not return any advisees
        /// </summary>
        NoAdvisees,
        /// <summary>
        /// Return all advisees for the advisor except those with an advisement that has ended
        /// </summary>
        ExcludeFormerAdvisees,
        /// <summary>
        /// Return only current advisees - excluding those with an advisement that has ended or that has not yet started.  
        /// </summary>
        CurrentAdviseesOnly
    }
}
