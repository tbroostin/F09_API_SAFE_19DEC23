// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// All information related to Recruiter test connection
    /// </summary>
    [Serializable]
    public class ConnectionStatus
    {
        public string ResponseServiceURL { get; set; }
        public string Message { get; set; }
        public string Duration { get; set; }
        public string Success { get; set; }
        public string RecruiterOrganizationName { get; set; }
        public string RecruiterOrganizationId { get; set; }
    }
}
