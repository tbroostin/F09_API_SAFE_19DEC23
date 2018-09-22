using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestCommunicationRepository : ICommunicationRepository
    {
        public class MailingRecord
        {
            public List<string> CurrentCorrespondanceRequestCodes;
            public List<CorrespondanceRecord> ChangeCorespondanceData;
        }

        public class CorrespondanceRecord
        {
            public string Code;
            public string StatusCode;
            public DateTime? StatusDate;
            public string Instance;
            public DateTime? AssignedDate;
            public DateTime? ActionDate;
            public string CommentId;
        }

        public class CorrespondanceTrackRecord
        {
            public string Code;
            public List<CorrespondanceRequestRecord> CorrespondanceRequests;
        }

        public class CorrespondanceRequestRecord
        {
            public string Code;
            public string StatusCode;
            public DateTime? StatusDate;
            public string Instance;
            public DateTime? AssignedDate;
            public DateTime? ActionDate;
            public string CommentId;
        }

        public MailingRecord mailingData = new MailingRecord()
        {
            CurrentCorrespondanceRequestCodes = new List<string>() { "TRACK1", "TRACK2" },
            ChangeCorespondanceData = new List<CorrespondanceRecord>()
            {
                new CorrespondanceRecord()
                {
                    Code = "COMM1",
                    StatusCode = "I",
                    StatusDate = new DateTime(2015,1,1),
                    Instance = string.Empty,
                    AssignedDate = new DateTime(2015,1,1),
                    ActionDate = null,
                    CommentId = string.Empty
                },
                new CorrespondanceRecord()
                {
                    Code = "COMM2",
                    StatusCode = "R",
                    StatusDate = new DateTime(2015,2,1),
                    Instance = "SPECIAL",
                    AssignedDate = new DateTime(2015,2,1),
                    ActionDate = new DateTime(2015,2,2),
                    CommentId = "217"
                }
            }
        };
        public List<CorrespondanceTrackRecord> correspondanceTrackData = new List<CorrespondanceTrackRecord>()
        {
            new CorrespondanceTrackRecord()
            {
                Code = "TRACK1",
                CorrespondanceRequests = new List<CorrespondanceRequestRecord>()
                {
                    new CorrespondanceRequestRecord()
                    {
                        Code = "COMM2",
                        StatusCode = "R",
                        StatusDate = new DateTime(2015,2,1),
                        Instance = "SPECIAL",
                        AssignedDate = new DateTime(2015,2,1),
                        ActionDate = new DateTime(2015,2,2),
                        CommentId = "217"
                    },
                    new CorrespondanceRequestRecord()
                    {
                        Code = "COMM3",
                        StatusCode = "",
                        StatusDate = null,
                        Instance = "",
                        AssignedDate = null,
                        ActionDate = null,
                        CommentId = ""
                    }
                }
            },
            new CorrespondanceTrackRecord()
            {
                Code = "TRACK2",
                CorrespondanceRequests = new List<CorrespondanceRequestRecord>()
                {
                    new CorrespondanceRequestRecord()
                    {
                        Code = "COMM4",
                        StatusCode = "W",
                        StatusDate = new DateTime(2015,3,1),
                        Instance = "MISTAKE",
                        AssignedDate = new DateTime(2015,2,1),
                        ActionDate = null,
                        CommentId = "218"
                    },
                    new CorrespondanceRequestRecord()
                    {
                        Code = "COMM4",
                        StatusCode = "I",
                        StatusDate = new DateTime(2015,3,1),
                        Instance = "",
                        AssignedDate = new DateTime(2015,2,1),
                        ActionDate = new DateTime(2015,4,1),
                        CommentId = ""
                    },
                }
            }
        };

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
                throw new ExistingResourceException("duplicate", communication.ToString());
            }

            var newCommunication = communication.ReviseDatesForCreateOrUpdate(existingCommunications);

            var truncatedInstance = (!string.IsNullOrEmpty(newCommunication.InstanceDescription) && newCommunication.InstanceDescription.Length > 57) ?
                newCommunication.InstanceDescription.Substring(0, 57) : newCommunication.InstanceDescription;

            mailingData.ChangeCorespondanceData.Add(new CorrespondanceRecord()
                {
                    Code = newCommunication.Code,
                    Instance = truncatedInstance,
                    AssignedDate = newCommunication.AssignedDate,
                    StatusCode = newCommunication.StatusCode,
                    StatusDate = newCommunication.StatusDate,
                    ActionDate = newCommunication.ActionDate,
                    CommentId = newCommunication.CommentId
                });

            return GetCommunications(personId).GetDuplicate(newCommunication);
        }

        public Communication UpdateCommunication(Communication communication, IEnumerable<Communication> existingCommunications = null)
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

            var existingCommunication = existingCommunications.GetDuplicate(communication);
            if (existingCommunication == null)
            {
                throw new ApplicationException("not a duplicate");
            }

            var updatedCommunication = communication.ReviseDatesForCreateOrUpdate(existingCommunications);

            var truncatedInstance = (!string.IsNullOrEmpty(updatedCommunication.InstanceDescription) && updatedCommunication.InstanceDescription.Length > 57) ?
                updatedCommunication.InstanceDescription.Substring(0, 57) : updatedCommunication.InstanceDescription;

            //loosely recreates logic of ctx
            var existingMailingRecord = mailingData.ChangeCorespondanceData.FirstOrDefault(c => c.Code == updatedCommunication.Code && c.Instance == truncatedInstance && c.AssignedDate == updatedCommunication.AssignedDate);
            var existingTrackCorrespondanceRecord = correspondanceTrackData.SelectMany(t => t.CorrespondanceRequests).FirstOrDefault(c => c.Code == updatedCommunication.Code && c.Instance == truncatedInstance && c.AssignedDate == updatedCommunication.AssignedDate);
            if (existingMailingRecord != null)
            {
                existingMailingRecord.ActionDate = updatedCommunication.ActionDate;
                existingMailingRecord.CommentId = !string.IsNullOrEmpty(updatedCommunication.CommentId) ? updatedCommunication.CommentId : existingMailingRecord.CommentId;
                existingMailingRecord.StatusCode = updatedCommunication.StatusCode;
                existingMailingRecord.StatusDate = updatedCommunication.StatusDate;
            }
            if (existingTrackCorrespondanceRecord != null)
            {
                existingTrackCorrespondanceRecord.ActionDate = updatedCommunication.ActionDate;
                existingTrackCorrespondanceRecord.CommentId = !string.IsNullOrEmpty(updatedCommunication.CommentId) ? updatedCommunication.CommentId : existingMailingRecord.CommentId;
                existingTrackCorrespondanceRecord.StatusCode = updatedCommunication.StatusCode;
                existingTrackCorrespondanceRecord.StatusDate = updatedCommunication.StatusDate;
            }

            return GetCommunications(personId).GetDuplicate(updatedCommunication);

        }

        public IEnumerable<Communication> GetCommunications(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            var communicationList = new List<Communication>();

            foreach (var correspondance in mailingData.ChangeCorespondanceData)
            {
                var communication = new Communication(personId, correspondance.Code)
                {
                    InstanceDescription = correspondance.Instance,
                    AssignedDate = correspondance.AssignedDate,
                    StatusCode = correspondance.StatusCode,
                    StatusDate = correspondance.StatusDate,
                    ActionDate = correspondance.ActionDate,
                    CommentId = correspondance.CommentId
                };
                if (communicationList.FirstOrDefault(c => c.Similar(communication)) == null)
                {
                    communicationList.Add(communication);
                }
            }

            var currentCorrespondanceTracks = correspondanceTrackData.Where(t => mailingData.CurrentCorrespondanceRequestCodes.Contains(t.Code));

            foreach (var correspondance in currentCorrespondanceTracks.SelectMany(t => t.CorrespondanceRequests))
            {
                var communication = new Communication(personId, correspondance.Code)
                {
                    InstanceDescription = correspondance.Instance,
                    AssignedDate = correspondance.AssignedDate,
                    StatusCode = correspondance.StatusCode,
                    StatusDate = correspondance.StatusDate,
                    ActionDate = correspondance.ActionDate,
                    CommentId = correspondance.CommentId
                };
                if (communicationList.FirstOrDefault(c => c.Similar(communication)) == null)
                {
                    communicationList.Add(communication);
                }
            }

            return communicationList;
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
                return UpdateCommunication(communication);
            }
            else
            {
                return CreateCommunication(communication);
            }

        }
    }
}
