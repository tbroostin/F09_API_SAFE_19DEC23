// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IMessageService
    {
        /// <summary>
        /// Updates a given message to a new state.
        /// </summary>
        /// <param name="messageId">Required. ID of message that is to be updated.</param>
        /// <param name="personId">Required. ID of person whose message is to be updated</param>
        /// <param name="newState">Required. The state that the message will be changed to.</param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Base.WorkTask> UpdateMessageWorklistAsync(string messageId, string personId, Dtos.Base.ExecutionState newState);
        Task<Ellucian.Colleague.Dtos.Base.WorkTask> CreateMessageWorklistAsync(string personId, string workflowId, string processCode, string subjectLine);
    }
}
