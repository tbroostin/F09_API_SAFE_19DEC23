// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Behavior for assigning academic program to students in instant enrollment workflows
    /// </summary>
    [Serializable]

    public enum AddNewStudentProgramBehavior
    {
        /// <summary>
        /// Only new and inactive students are assigned an academic program
        /// </summary>
        New,
        /// <summary>
        /// Any student is assigned an academic program
        /// </summary>
        Any
    }
}
