// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Status of a test
    /// </summary>
    [Serializable]
    public class NoncourseStatus : CodeItem
    {
        private NoncourseStatusType _StatusType;
        public NoncourseStatusType StatusType { get { return _StatusType; } }

        public NoncourseStatus(string code, string description, NoncourseStatusType statusType)
            : base(code, description)
        {
            _StatusType = statusType;
        }
    }
}
