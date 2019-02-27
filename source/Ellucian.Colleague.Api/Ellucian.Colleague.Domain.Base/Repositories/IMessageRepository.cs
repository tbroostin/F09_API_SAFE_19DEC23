// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Define required Message repository methods
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Get the worktasks for the specified person and roles
        /// </summary>
        /// <param name="messageId">ID of the message being updated</param>
        /// <param name="personId">ID of the person whose message is being changed</param>
        /// <param name="newState">New state of the message</param>
        /// <returns></returns>
        Task<WorkTask> UpdateMessageWorklistAsync(string messageId, string personId, ExecutionState newState);
        Task<WorkTask> CreateMessageWorklistAsync(string personId, string workflowId, string processCode, string subjectLine);
    }
}
