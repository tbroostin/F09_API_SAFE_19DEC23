// Copyright 2018-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Web.Cache;
using slf4net;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Ellucian.Web.Dependency;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class MessageRepository : BaseColleagueRepository, IMessageRepository
    {
        private ApplValcodes categories;
        private Data.Base.DataContracts.IntlParams internationalParameters;
        //private List<String> roles;
        private WorkTaskRepository wrkRep;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public MessageRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            wrkRep = new WorkTaskRepository(cacheProvider, transactionFactory, logger, apiSettings);
        }

        /// <summary>
        /// Updates a message to new state
        /// </summary>
        /// <param name="messageId">The ID of the message to be updated</param>
        /// <param name="personId">The ID of the person</param>
        /// <param name="newState">TThe new state</param>
        /// <returns>Updated message(worktask).</returns>
        public async Task<WorkTask> UpdateMessageWorklistAsync(string messageId, string personId, ExecutionState newState)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException("Must specify a message");
            }
            List<WorkTask> taskList = new List<WorkTask>();

            // Build the query string based on the provided arguments
            string[] worklistAddrIds = null;
            string personCriteria = string.Empty;
            string roleCriteria = string.Empty;

            if (!string.IsNullOrEmpty(personId))
            {
                personCriteria = "WITH WKLAD.ORG.ENTITY EQ '" + personId + "'";
            }

            worklistAddrIds = await DataReader.SelectAsync("WORKLIST.ADDR", personCriteria);
            WorkTask result = null;
            var messages = await wrkRep.GetAsync(personId, null);
            foreach (var message in messages)
            {
                 if (message.Id.Equals(messageId))
                {
                    //var m = message;
                    //result = new WorkTask(message.Id, message.Category, message.Description, message.ProcessCode, newState);
                    result = message;
                    //result = new WorkTask();
                    //return message;
                }
            }

            // Bulkread worklist data
            if (worklistAddrIds != null && worklistAddrIds.Count() > 0)
            {
                worklistAddrIds = worklistAddrIds.Distinct().ToArray();

                Collection<WorklistAddr> worklistAddrRecords = new Collection<WorklistAddr>();
                Collection<Worklist> worklistRecords = new Collection<Worklist>();

                List<string> worklistIds = new List<string>();
                worklistAddrRecords = await DataReader.BulkReadRecordAsync<WorklistAddr>(worklistAddrIds.ToArray());

                if (worklistAddrRecords != null && worklistAddrRecords.Count() > 0)
                {
                    worklistIds = worklistAddrRecords.Where(wa => !string.IsNullOrEmpty(wa.WkladWorklist)).Select(wa => wa.WkladWorklist).Distinct().ToList();
                }
                
                if (worklistIds != null && worklistIds.Count() > 0)
                {
                    worklistRecords = await DataReader.BulkReadRecordAsync<Worklist>(worklistIds.ToArray());
                }

                if (worklistRecords != null && worklistRecords.Count() > 0)
                {
                    foreach (var item in worklistRecords)
                    {
                        if (item.Recordkey.Equals(messageId))
                        {
                            //result = new WorkTask(item.id);
                            UpdateMsgWorklistRequest messageWorklistUpdateRequest = new UpdateMsgWorklistRequest()
                            {
                                AWorklistId = messageId,
                                ANewState = newState.ToString()

                            };

                            UpdateMsgWorklistResponse updateResponse = null;
                            try
                            {
                                updateResponse = await transactionInvoker.ExecuteAsync<UpdateMsgWorklistRequest, UpdateMsgWorklistResponse>(messageWorklistUpdateRequest);
                               
                            }
                            catch (ColleagueTransactionException ce)
                            {
                                logger.Error(ce.Message);
                                if (ce.Message.Contains("SECURITY") || ce.Message.Contains("TOKEN"))
                                {
                                    logger.Error(ce.Message);
                                    throw new PermissionsException("Login expired");
                                }
                                else
                                {
                                    logger.Error(ce.Message);
                                    throw ce;
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error(e.Message);
                                throw;
                            }

                            if (updateResponse.AErrorOccurred == "3")
                            {
                                logger.Info("No changes detected, no update made to message  " + messageId);
                            }
                            else if (!string.IsNullOrEmpty(updateResponse.AErrorOccurred) && updateResponse.AErrorOccurred != "0")
                            {
                                var errorMessage = "Error(s) occurred updating message '" + messageId + "': ";
                                errorMessage += updateResponse.AMsg;
                                logger.Error(errorMessage.ToString());
                                throw new InvalidOperationException("Error occurred updating message");
                            }




                        }
                    }

                }

            }

            return result;
        }

        /// <summary>
        /// Create a message in the database.
        /// </summary>
        /// <param name="personId">The person for whom the message is created in database.</param>
        /// <param name="workflowDefId">ID of the workflow.</param>
        /// <param name="processCode"> Process code of workflow</param>
        ///<param name="subjectLine"> Subject Line of worktask </param>
        /// <returns>The newly created <see cref="WorkTask"></see> entity</returns>
        public async Task<WorkTask> CreateMessageWorklistAsync(string personId, string workflowDefId, string processCode, string subjectLine)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("Must provide a valid person ID to create message.");

            if (string.IsNullOrEmpty(workflowDefId))
            {
                throw new ArgumentException("Must provide a valid workflow definition ID to create message.");
            }

            if (string.IsNullOrEmpty(processCode))
            {
                throw new ArgumentException("Must provide a valid process code to create message.");
            }

            if (string.IsNullOrEmpty(subjectLine))
            {
                throw new ArgumentException("Must provide a valid subject line to create message.");
            }

            List<WorkTask> taskList = new List<WorkTask>();

            CreateMsgWorklistRequest messageWorklistCreateRequest = new CreateMsgWorklistRequest()
            {
                APersonId = personId,
                AWorkflowDefId = workflowDefId,
                ASubjectLine = subjectLine,
                ASpecProcCode = processCode
            };

            CreateMsgWorklistResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(messageWorklistCreateRequest);
            }
            catch (ColleagueTransactionException ce)
            {
                logger.Error(ce.Message);
                if (ce.Message.Contains("SECURITY") || ce.Message.Contains("TOKEN"))
                {
                    logger.Error(ce.Message);
                    throw new PermissionsException("Login expired");
                }
                else
                {
                    logger.Error(ce.Message);
                    throw ce;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;
            }

            if (createResponse.AErrorOccurred == "3")
            {
                logger.Info("No changes detected, no message created  ");
            }
            else if (!string.IsNullOrEmpty(createResponse.AErrorOccurred) && createResponse.AErrorOccurred != "0")
            {
                var errorMessage = "Error(s) occurred creatings message '" + "': ";
                errorMessage += createResponse.AMsg;
                logger.Error(errorMessage.ToString());
                throw new InvalidOperationException("Error occurred creating message");
            }

            var messages = await wrkRep.GetAsync(personId, null);
            foreach (var message in messages)
            {
                if (message.Id.Equals(createResponse.AlWorklistIds[0])) //double
                {
                    return message;
                }
            }
            return null;
        }
    }
}
