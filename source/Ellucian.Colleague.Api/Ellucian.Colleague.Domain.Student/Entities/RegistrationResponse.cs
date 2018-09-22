// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationResponse
    {
        public List<RegistrationMessage> Messages { get; set; }
        public string PaymentControlId { get; set; }

        public RegistrationResponse(IEnumerable<RegistrationMessage> messages, string rpcId)
        {
            Messages = new List<RegistrationMessage>(messages);
            PaymentControlId = rpcId;
        }
    }
}
