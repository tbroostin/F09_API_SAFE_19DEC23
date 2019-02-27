//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestCorrespondenceRequestsRepository : ICorrespondenceRequestsRepository
    {
        public string personId = "0003914";

        public class MailingRecord
        {
            public List<string> CurrentCorrespondanceRequestCodes;
            public List<ChangeCorrespondanceRecord> ChangeCorespondanceData;
        }

        public class ChangeCorrespondanceRecord
        {
            public string Code;
            public string StatusCode;
            public string Instance;
            public DateTime? DueDate;
            public DateTime? StatusDate;
        }

        public MailingRecord MailingData = new MailingRecord()
        {
            ChangeCorespondanceData = new List<ChangeCorrespondanceRecord>()
            {
                new ChangeCorrespondanceRecord()
                {
                    Code = "FAC01STX",
                    StatusCode = "",
                    Instance = "Instance1",
                    DueDate = new DateTime(2014, 08, 01),
                    StatusDate = null
                },

                new ChangeCorrespondanceRecord()
                {
                    Code = "FAC01CPK",
                    StatusCode = "F",
                    Instance = "Instance2",
                    DueDate = new DateTime(2014, 09, 28),
                    StatusDate = DateTime.Today
                },

                new ChangeCorrespondanceRecord()
                {
                    Code = "DEPDUE",
                    StatusCode = "Z",
                    Instance = "Instance3",
                    DueDate = new DateTime(2014, 12, 25),
                    StatusDate = new DateTime(2014, 12, 25)
                },

                new ChangeCorrespondanceRecord()
                {
                    Code = "FAJ01REJ",
                    StatusCode = "Y",
                    Instance = "Instance4",
                    DueDate = null,
                    StatusDate = DateTime.Today
                }

            },

            CurrentCorrespondanceRequestCodes = new List<string>() { "FA14TRACK1" }

        };



        public class CorrespondanceTrackRecord
        {
            public string Code;
            public List<CorrespondanceRequestRecord> CorrespondanceRequests;
        }

        public class CorrespondanceRequestRecord
        {
            public string Code;
            public DateTime? DueDate;
            public DateTime? StatusDate;
            public string Instance;
            public string StatusCode;
        }

        public List<CorrespondanceTrackRecord> CorrespondanceTrackData = new List<CorrespondanceTrackRecord>()
        {
            new CorrespondanceTrackRecord() {
                Code = "FA14TRACK1",
                CorrespondanceRequests = new List<CorrespondanceRequestRecord>()
                {
                    new CorrespondanceRequestRecord()
                    {
                        Code = "FA14ISIR",
                        DueDate = new DateTime(2014, 1, 1)

                    },
                    new CorrespondanceRequestRecord()
                    {
                        Code = "FA14SAP",
                        Instance = "SAP Document",
                        StatusDate = new DateTime(2014, 3, 13),
                        StatusCode = "Y"
                    },
                    new CorrespondanceRequestRecord()
                    {
                        Code = "FA14IART",
                        DueDate = DateTime.Today.AddDays(5),
                        StatusCode = "F",
                        StatusDate = DateTime.Today
                    },
                    new CorrespondanceRequestRecord()
                    {
                        Code = "FA14MPN",
                        StatusCode = "Z",
                        StatusDate = DateTime.Today,
                        Instance = "MPN Document"
                    }
                }
            }
        };



        public class DocumentStatusValcodeValue
        {
            public string InternalCode;
            public string Action1Code;
            public string Desc;
        }

        public List<DocumentStatusValcodeValue> statusValcodeValues = new List<DocumentStatusValcodeValue>()
        {
            new DocumentStatusValcodeValue()
            {
                InternalCode = "F",
                Action1Code = "1",
                Desc = "Failed"
            },
            new DocumentStatusValcodeValue()
            {
                InternalCode = "Z",
                Action1Code = "0",
                Desc = "Zulu"
            },
            new DocumentStatusValcodeValue()
            {
                InternalCode = "Y",
                Action1Code = "",
                Desc = "Yellow"
            }
        };

        public Task<IEnumerable<CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personId)
        {
            var correspondenceRequestsList = new List<CorrespondenceRequest>();

            correspondenceRequestsList.AddRange(
                MailingData.ChangeCorespondanceData
                    .Select(cor =>
                        CreateCorrespondenceRequest(personId, cor.Code, cor.DueDate, cor.StatusDate, cor.Instance, cor.StatusCode)
                    )
                );

            var currentCorrespondanceTracks = CorrespondanceTrackData.Where(t => MailingData.CurrentCorrespondanceRequestCodes.Contains(t.Code));

            correspondenceRequestsList.AddRange(
                currentCorrespondanceTracks
                    .SelectMany(track => track.CorrespondanceRequests)
                    .Select(req =>
                        CreateCorrespondenceRequest(personId, req.Code, req.DueDate, req.StatusDate, req.Instance, req.StatusCode)
                    )
                );

            return Task.FromResult(correspondenceRequestsList.AsEnumerable());
        }

        private CorrespondenceRequest CreateCorrespondenceRequest(string personId, string code, DateTime? dueDate, DateTime? statusDate, string instance, string statusCode)
        {
            var correspondenceRequest = new CorrespondenceRequest(personId, code)
            {
                DueDate = dueDate,
                StatusDate = statusDate,
                Instance = instance
            };

            if (statusCode == null) statusCode = string.Empty;
            var statusCodeObj = statusValcodeValues.FirstOrDefault(v => v.InternalCode.ToUpper() == statusCode.ToUpper());
            if (statusCodeObj == null)
            {
                correspondenceRequest.Status = CorrespondenceRequestStatus.Incomplete;
                correspondenceRequest.StatusDescription = "";
            }
            else
            {
                switch (statusCodeObj.Action1Code)
                {
                    case "0":
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Waived;
                        break;
                    case "1":
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Received;
                        break;
                    default:
                        correspondenceRequest.Status = CorrespondenceRequestStatus.Incomplete;
                        break;
                }
                correspondenceRequest.StatusDescription = statusCodeObj.Desc;
            }

            return correspondenceRequest;
        }
    }
}

