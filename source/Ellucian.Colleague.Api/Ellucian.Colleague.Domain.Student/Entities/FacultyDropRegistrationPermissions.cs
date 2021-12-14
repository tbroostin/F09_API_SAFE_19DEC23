// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FacultyDropRegistrationPermissions
    {
        /// <summary>
        /// Faculty is elibigle to drop a student from their sections
        /// </summary>
        public bool IsEligibleToDrop { get; private set; }

        /// <summary>
        /// Faculty has registration overrides in the sections
        /// </summary>
        public bool HasEligibilityOverrides { get; private set; }

        public FacultyDropRegistrationPermissions(bool isEligibleToDrop, bool hasEligibilityOverrides)
        {
            IsEligibleToDrop = isEligibleToDrop;
            HasEligibilityOverrides = hasEligibilityOverrides;
        }
    }
}
