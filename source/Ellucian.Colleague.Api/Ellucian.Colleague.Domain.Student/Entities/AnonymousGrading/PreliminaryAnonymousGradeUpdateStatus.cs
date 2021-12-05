// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading
{
    [Serializable]
    /// <summary>
    /// Status of a preliminary anonymous grade update
    /// </summary>
    public enum PreliminaryAnonymousGradeUpdateStatus
    {
        /// <summary>
        /// Preliminary anonymous grade update was successful
        /// </summary>
        Success = 0,

        /// <summary>
        /// Preliminary anonymous grade update failed
        /// </summary>
        Failure = 1
    }
}
