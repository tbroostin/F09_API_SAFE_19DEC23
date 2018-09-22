/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CommunicationRepository : BaseColleagueRepository, ICommunicationRepository
    {
        public CommunicationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        public Communication SubmitCommunication(Communication communication)
        {
            if (communication == null)
            {
                throw new ArgumentNullException("communication");
            }

            var existingCommunications = GetCommunications(communication.PersonId);

            if (existingCommunications.ContainsDuplicate(communication))
            {
                try
                {
                    return UpdateCommunication(communication);
                }
                catch (Exception e)
                {
                    var message = string.Format("Error updating communication {0} for student {1}", communication.Code, communication.PersonId);
                    logger.Warn(e, message);
                    throw;
                }
            }
            else
            {
                try
                {
                    return CreateCommunication(communication);
                }
                catch (Exception e)
                {
                    var message = string.Format("Error creating communication {0} for student {1}", communication.Code, communication.PersonId);
                    logger.Warn(e, message);
                    throw;
                }
            }
        }

        public Communication CreateCommunication(Communication communication, IEnumerable<Communication> existingCommunications = null)
        {
            if (communication == null)
            {
                throw new ArgumentNullException("communication");
            }

            var personId = communication.PersonId;
            if (existingCommunications == null)
            {
                existingCommunications = GetCommunications(personId);
            }

            if (existingCommunications.ContainsDuplicate(communication))
            {
                var message = string.Format("Communication {0} already exists for person {1}", communication, personId);
                logger.Error(message);
                throw new ExistingResourceException(message, communication.ToString());
            }

            var newCommunication = communication.ReviseDatesForCreateOrUpdate(existingCommunications);

            var truncatedInstance = (!string.IsNullOrEmpty(newCommunication.InstanceDescription) && newCommunication.InstanceDescription.Length > 57) ?
                newCommunication.InstanceDescription.Substring(0, 57) : newCommunication.InstanceDescription;

            var request = new CreateCommunicationRequest()
            {
                PersonId = newCommunication.PersonId,
                CommunicationCode = newCommunication.Code,
                Instance = newCommunication.InstanceDescription,
                AssignedDate = newCommunication.AssignedDate,
                StatusCode = newCommunication.StatusCode,
                StatusDate = newCommunication.StatusDate,
                ActionDate = newCommunication.ActionDate
            };

            var response = transactionInvoker.Execute<CreateCommunicationRequest, CreateCommunicationResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Count() > 0)
            {
                var errorMessagesString = string.Join(Environment.NewLine, response.ErrorMessages);
                logger.Error(errorMessagesString);
                throw new ApplicationException(errorMessagesString);
            }

            //verify record was created
            var createdCommunication = GetCommunications(personId).GetDuplicate(newCommunication);
            if (createdCommunication == null)
            {
                var message = "Communication object not created in database";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            return createdCommunication;

        }

        public Communication UpdateCommunication(Communication communication, IEnumerable<Communication> existingCommunications = null)
        {
            if (communication == null)
            {
                throw new ArgumentNullException("updatedCommunication");
            }

            var personId = communication.PersonId;
            if (existingCommunications == null)
            {
                existingCommunications = GetCommunications(personId);
            }

             var existingCommunication = existingCommunications.GetDuplicate(communication);

            if (existingCommunication == null)
            {
                var message = string.Format("Cannot update a communication {0} that does not exist for person {1}", communication, personId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var updatedCommunication = communication.ReviseDatesForCreateOrUpdate(existingCommunications);

            //var truncatedInstance = (!string.IsNullOrEmpty(updatedCommunication.InstanceDescription) && updatedCommunication.InstanceDescription.Length > 57) ?
            //    updatedCommunication.InstanceDescription.Substring(0, 57) : updatedCommunication.InstanceDescription;

            var request = new UpdateCommunicationRequest()
            {
                PersonId = updatedCommunication.PersonId,
                CommunicationCode = updatedCommunication.Code,
                Instance = updatedCommunication.InstanceDescription,
                AssignedDate = updatedCommunication.AssignedDate,
                StatusCode = updatedCommunication.StatusCode,
                StatusDate = updatedCommunication.StatusDate,
                ActionDate = updatedCommunication.ActionDate,
                CommentId = !string.IsNullOrEmpty(updatedCommunication.CommentId) ? updatedCommunication.CommentId : existingCommunication.CommentId
            };

            var response = transactionInvoker.Execute<UpdateCommunicationRequest, UpdateCommunicationResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Count() > 0)
            {
                var errorMessagesString = string.Join(Environment.NewLine, response.ErrorMessages);
                logger.Error(errorMessagesString);
                throw new ApplicationException(errorMessagesString);
            }

            return GetCommunications(personId).GetDuplicate(updatedCommunication);
        }


        public IEnumerable<Communication> GetCommunications(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            var communications = new List<Communication>();

            var mailingData = DataReader.ReadRecord<Mailing>(personId);
            if (mailingData == null)
            {
                var message = "Person has no mailing record";
                logger.Warn(message);
                return communications;
            }

            //Get communications from the Mailing.Correspondance association
            if (mailingData.ChCorrEntityAssociation != null && mailingData.ChCorrEntityAssociation.Count() > 0)
            {
                foreach (var correspondance in mailingData.ChCorrEntityAssociation)
                {
                    try
                    {
                        var communication = new Communication(personId, correspondance.MailingCorrReceivedAssocMember)
                        {
                            InstanceDescription = correspondance.MailingCorrRecvdInstanceAssocMember,
                            AssignedDate = correspondance.MailingCorrRecvdAsgnDtAssocMember,
                            StatusCode = correspondance.MailingCorrRecvdStatusAssocMember,
                            StatusDate = correspondance.MailingCorrReceivedDateAssocMember,
                            ActionDate = correspondance.MailingCorrRecvdActDtAssocMember,
                            CommentId = correspondance.MailingCorrRecvdCommentAssocMember
                        };
                        if (communications.FirstOrDefault(comm => comm.Similar(communication)) == null)
                        {
                            communications.Add(communication);
                        }
                        else
                        {
                            LogDataError("MAILING", personId, correspondance, null, "Duplicate Correspondance Received in CH.CORR association of MAILING record");
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("MAILING", personId, correspondance, e, "Unable to create Communication from MAILING");
                    }
                }
            }

            //Get communcations from Current Tracks
            if (mailingData.MailingCurrentCrcCode != null && mailingData.MailingCurrentCrcCode.Count() > 0)
            {
                var coreqIds = mailingData.MailingCurrentCrcCode.Where(code => !string.IsNullOrEmpty(code)).Select(code => string.Format("{0}*{1}", personId, code));
                var coreqRecords = DataReader.BulkReadRecord<Coreq>(coreqIds.ToArray());

                if (coreqRecords != null && coreqRecords.Count() > 0)
                {
                    foreach (var coreq in coreqRecords)
                    {
                        foreach (var correspondance in coreq.CoreqRequestsEntityAssociation)
                        {
                            try
                            {
                                var communication = new Communication(personId, correspondance.CoreqCcCodeAssocMember)
                                {
                                    InstanceDescription = correspondance.CoreqCcInstanceAssocMember,
                                    AssignedDate = correspondance.CoreqCcAssignDtAssocMember,
                                    StatusCode = correspondance.CoreqCcStatusAssocMember,
                                    StatusDate = correspondance.CoreqCcDateAssocMember,
                                    ActionDate = correspondance.CoreqCcExpActDtAssocMember,
                                    CommentId = correspondance.CoreqCcCommentAssocMember
                                };

                                if (communications.FirstOrDefault(comm => comm.Similar(communication)) == null)
                                {
                                    communications.Add(communication);
                                }
                                //Unlike above, don't log a data error because it's valid that a correspondance is part of a Track AND
                                //received (received codes from tracks get copied into the CH.CORR association)
                            }
                            catch (Exception e)
                            {
                                LogDataError("COREQ", coreq.Recordkey, coreq, e, "Unable to create Communication from COREQ");
                            }
                        }
                    }
                }
            }

            return communications;
        }
    }
}
