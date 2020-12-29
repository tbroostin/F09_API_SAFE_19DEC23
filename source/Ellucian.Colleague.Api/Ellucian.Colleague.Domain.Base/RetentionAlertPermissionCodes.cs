// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base
{
    /// <summary>
    /// Retention alert permission codes
    /// </summary>
    [Serializable]
    public class RetentionAlertPermissionCodes
    {
        // Access to work on assigned retention alert cases
        public const string WorkCases = "WORK.CASES";

        // Access to work on any retention alert cases
        public const string WorkAnyCase = "WORK.ANY.CASE";

        // Access to contribute to retention alert cases
        public const string ContributeToCases = "CONTRIBUTE.TO.CASES";
    }
}
