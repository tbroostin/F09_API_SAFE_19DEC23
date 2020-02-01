// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationResponse
    {
        /// <summary>
        /// List of <see cref="RegistrationMessage">messages</see>
        /// </summary>
        public List<RegistrationMessage> Messages { get; set; }

        /// <summary>
        /// Immediate Payment Control registration record created or updated by the student's registration
        /// </summary>
        /// <remarks>This value will be null if no Immediate Payment Control activity occurred as part of the registration</remarks>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// List of identifiers for course sections for which the student was successfully registered
        /// </summary>
        public List<string> RegisteredSectionIds { get; private set; }

        /// <summary>
        /// Creates a new <see cref="RegistrationResponse"/>
        /// </summary>
        /// <param name="messages">List of <see cref="RegistrationMessage">messages</see></param>
        /// <param name="rpcId">Immediate Payment Control registration record created or updated by the student's registration</param>
        /// <param name="registeredSectionIds">List of identifiers for course sections for which the student was successfully registered</param>
        public RegistrationResponse(IEnumerable<RegistrationMessage> messages, string rpcId, IEnumerable<string> registeredSectionIds)
        {
            Messages = new List<RegistrationMessage>(messages != null ? messages.Where(m => m != null) : new List<RegistrationMessage>());
            PaymentControlId = rpcId;
            RegisteredSectionIds = registeredSectionIds == null ? new List<string>() : registeredSectionIds.Where(rsi => !string.IsNullOrEmpty(rsi)).Distinct().ToList();
        }
    }
}
