// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Based on current user, processes work tasks for current user.
    /// </summary>
    [RegisterType]
    public class MessageService : BaseCoordinationService, IMessageService
    {
        private IMessageRepository _messageRepository;
       
        /// <summary>
        /// Constructor of the Work Task Service
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="workTaskRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="proxyRepository"></param>
        /// <param name="logger"></param>
        public MessageService(IAdapterRegistry adapterRegistry, IMessageRepository messageRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _messageRepository = messageRepository;

        }

        /// <summary>
        /// Returns person's updated message. 
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="personId"></param>
        /// <param name="newState"></param>
        /// <returns>Updated message</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.WorkTask> UpdateMessageWorklistAsync(string messageId, string personId, Dtos.Base.ExecutionState newState)
        {
            var messageDto = new Ellucian.Colleague.Dtos.Base.WorkTask();
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentNullException(messageId);
            }

            if (!CurrentUser.IsPerson(personId))
            {
                throw new PermissionsException(String.Format("Authenticated user (person ID {0}) does not match passed person ID {1} and has not been granted proxy access.", CurrentUser.PersonId, personId));
            }

            var execStateDtoToEntityAdapter =
                _adapterRegistry.GetAdapter<Dtos.Base.ExecutionState, Domain.Base.Entities.ExecutionState>();
            Domain.Base.Entities.ExecutionState execStateEntity;
            try
            {
                execStateEntity = execStateDtoToEntityAdapter.MapToType(newState);
            }
            catch (Exception ex)
            {
                // The adapter will throw an exception if anything about the data is not acceptable to domain rules
                logger.Error("Error occurred converting profile dto to entity: " + ex.Message);
                throw;
            }
            // Get the workTasks pertinent to the specified users and roles
            var message = await _messageRepository.UpdateMessageWorklistAsync(messageId, personId, execStateEntity);
            var messageAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>();
            messageDto = messageAdapter.MapToType(message);

            return messageDto;
        }

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="personId">The person for whom the message is created in database.</param>
        /// <param name="workflowDefId">ID of the workflow.</param>
        /// <param name="processCode">Process code of workflow</param>
        /// <param name="subjectLine">Subject line of worktask</param>
        /// <returns>The newly created message</see></returns>
        public async Task<Ellucian.Colleague.Dtos.Base.WorkTask> CreateMessageWorklistAsync(string personId, string workflowDefId, string processCode, string subjectLine)
        {
            var messageDto = new Ellucian.Colleague.Dtos.Base.WorkTask();
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException(personId);
            }
            if (string.IsNullOrEmpty(workflowDefId))
            {
                throw new ArgumentNullException(workflowDefId);
            }
            if (string.IsNullOrEmpty(processCode))
            {
                throw new ArgumentNullException(processCode);
            }
            if (string.IsNullOrEmpty(subjectLine))
            {
                throw new ArgumentNullException(subjectLine);
            }

            var message = await _messageRepository.CreateMessageWorklistAsync(personId, workflowDefId, processCode, subjectLine);
            var messageAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>();
            messageDto = messageAdapter.MapToType(message);

            return messageDto;
        }
    }
}
