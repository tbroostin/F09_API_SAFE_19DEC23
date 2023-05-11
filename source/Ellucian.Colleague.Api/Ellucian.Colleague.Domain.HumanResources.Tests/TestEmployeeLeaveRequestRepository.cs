// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEmployeeLeaveRequestRepository : IEmployeeLeaveRequestRepository
    {
        #region Data_Setup
        public const string colleagueTimeZone = "Eastern Standard Time";
        public static string randomLeaveRequestId = RandomLeaveRequestIdGenerator();

        public class LeaveRequestRecord
        {
            public string Id { get; set; }
            public string PerLeaveId { get; set; }
            public string EmployeeId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string ApproverId { get; set; }
            public string ApproverName { get; set; }
            public string EmployeeName { get; set; }
            public LeaveStatusAction Status { get; set; }
            public List<LeaveRequestDetailRecord> LeaveRequestDetailRecords { get; set; }
            public List<LeaveRequestStatusRecord> LeaveRequestStatusRecords { get; set; }
            // To Do: LeaveRequestCommentRecords

            public bool EnableDeleteForSupervisor { get; set; }
        }

        public class LeaveRequestDetailRecord
        {
            public string Id { get; set; }
            public string LeaveRequestId { get; set; }
            public DateTime LeaveDate { get; set; }
            public decimal? LeaveHours { get; set; }
            public bool ProcessedInPayPeriod { get; set; }
            public string LeaveRequestDetailChgopr { get; set; }
        }

        public class LeaveRequestStatusRecord
        {
            public string Id { get; set; }
            public string LeaveRequestId { get; set; }
            public LeaveStatusAction ActionType { get; set; }
            public string ActionerId { get; set; }
            public string AddOpr { get; set; }
            public DateTime? AddDate { get; set; }
            public DateTime? AddTime { get; set; }
            public string ChangeOpr { get; set; }
            public DateTime? ChangeDate { get; set; }
            public DateTime? ChangeTime { get; set; }
        }

        #region LeaveRequestStatusRecords
        public List<LeaveRequestStatusRecord> leaveRequestStatusRecords = new List<LeaveRequestStatusRecord>()
        {
            new LeaveRequestStatusRecord()
            {
                Id = "1",
                LeaveRequestId = "1",
                ActionType = LeaveStatusAction.Draft,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = new DateTime(2019,11,05),
                ChangeDate = new DateTime(2019,11,05),
                AddTime = new DateTime(2019,11,05, 08, 30, 00),
                ChangeTime = new DateTime(2019,11,05, 08, 30, 00)
            },
            new LeaveRequestStatusRecord()
            {
                Id = "2",
                LeaveRequestId = "2",
                ActionType = LeaveStatusAction.Draft,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = new DateTime(2019,04,04),
                ChangeDate =  new DateTime(2019,04,04),
                AddTime = new DateTime(2019,04,04, 08, 30, 00),
                ChangeTime = new DateTime(2019,04,04, 08, 30, 00)
            },
            new LeaveRequestStatusRecord()
        {
                Id = "3",
                LeaveRequestId = "2",
                ActionType = LeaveStatusAction.Submitted,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = new DateTime(2019, 04, 04),
                ChangeDate = new DateTime(2019, 04, 04),
                AddTime = new DateTime(2019, 04, 04, 08, 30, 05),
                ChangeTime = new DateTime(2019, 04, 04, 08, 30, 05)
            },
            new LeaveRequestStatusRecord()
            {
                Id = "4",
                LeaveRequestId = "3",
                ActionType = LeaveStatusAction.Draft,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = new DateTime(2019,10,01),
                ChangeDate =  new DateTime(2019,10,01),
                AddTime = new DateTime(2019,10,01, 08, 30, 00),
                ChangeTime = new DateTime(2019,10,01, 08, 30, 00)
            },
            new LeaveRequestStatusRecord()
            {
                Id = "5",
                LeaveRequestId = "3",
                ActionType = LeaveStatusAction.Submitted,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = new DateTime(2019,10,01),
                ChangeDate =  new DateTime(2019,10,01),
                AddTime = new DateTime(2019,10,01, 08, 30, 05),
                ChangeTime = new DateTime(2019,10,01, 08, 30, 05)
            },
              new LeaveRequestStatusRecord()
            {
                Id = "6",
                LeaveRequestId = randomLeaveRequestId,
                ActionType = LeaveStatusAction.Draft,
                ActionerId = "0011560",
                AddOpr = "0011560",
                ChangeOpr = "0011560",
                AddDate = DateTime.Today,
                ChangeDate =  DateTime.Today,
                AddTime = DateTime.Today,
                ChangeTime = DateTime.Today
            },
                    new LeaveRequestStatusRecord()
            {
                Id = "7",
                LeaveRequestId = "14",
                ActionType = LeaveStatusAction.Approved,
                ActionerId = "0010351",
                AddOpr = "0011560",
                ChangeOpr = "0010351",
                AddDate = DateTime.Today,
                ChangeDate =  DateTime.Today,
                AddTime = DateTime.Today,
                ChangeTime = DateTime.Today
            },
                          new LeaveRequestStatusRecord()
            {
                Id = "8",
                LeaveRequestId = "15",
                ActionType = LeaveStatusAction.Approved,
                ActionerId = "0010351",
                AddOpr = "0011560",
                ChangeOpr = "0010351",
                AddDate = DateTime.Today,
                ChangeDate =  DateTime.Today,
                AddTime = DateTime.Today,
                ChangeTime = DateTime.Today
            }
        };
        #endregion

        #region LeaveRequestDetailRecords
        public static List<LeaveRequestDetailRecord> leaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
        {
                    new LeaveRequestDetailRecord()
                    {
                        Id = "1",
                        LeaveRequestId = "1",
                        LeaveDate = new DateTime(2019,11,05),
                        LeaveHours = 8.00m,
                        ProcessedInPayPeriod = false
                    },
                      new LeaveRequestDetailRecord()
                    {
                        Id = "2",
                        LeaveRequestId = "1",
                        LeaveDate = new DateTime(2019,11,06),
                        LeaveHours = 8.00m,
                        ProcessedInPayPeriod = false
                    },
                      new LeaveRequestDetailRecord()
                    {
                        Id = "961",
                        LeaveRequestId = "2",
                        LeaveDate = new DateTime(2019,04,04),
                        LeaveHours = 4.00m,
                        ProcessedInPayPeriod = false
                    },
                      new LeaveRequestDetailRecord()
                    {
                        Id = "963",
                        LeaveRequestId = "3",
                        LeaveDate = new DateTime(2019,10,01),
                        LeaveHours = 4.00m,
                        ProcessedInPayPeriod = false
                    },
                    new LeaveRequestDetailRecord()
                    {
                        Id = "123456",
                        LeaveRequestId = randomLeaveRequestId,
                        LeaveDate = DateTime.Today,
                        LeaveHours = 8.00m,
                        ProcessedInPayPeriod = false
                    },
                          new LeaveRequestDetailRecord()
                    {
                        Id = "39", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "14"
                    },
                                new LeaveRequestDetailRecord()
                    {
                     Id = "40", LeaveDate = DateTime.Today.AddDays(1), LeaveHours = 8.00m, LeaveRequestId = "14"
                    },
                                new LeaveRequestDetailRecord()  {
                    Id = "41", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "15"
                    }
        };
        #endregion

        #region LeaveRequestRecords
        public List<LeaveRequestRecord> leaveRequestRecords = new List<LeaveRequestRecord>()
        {
            new LeaveRequestRecord()
            {
                Id = "1",
                PerLeaveId = "805",
                EmployeeId = "0011560",
                StartDate = new DateTime(2019,11,05),
                EndDate = new DateTime(2019,11,06),
                ApproverId="0010351",
                ApproverName="CH_Brown, CH_Jennifer",
                Status = LeaveStatusAction.Draft,
                LeaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
                {
                    leaveRequestDetailRecords[0],
                    leaveRequestDetailRecords[1]
                }
            },
            new LeaveRequestRecord()
            {
                Id = "2",
                PerLeaveId = "697",
                EmployeeId = "0011560",
                StartDate = new DateTime(2019,04,04),
                EndDate = new DateTime(2019,04,04),
                ApproverId="0010351",
                ApproverName="Jennifer Aniston",
                Status = LeaveStatusAction.Submitted,
                LeaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
                {
                    leaveRequestDetailRecords[2]
                }
            },
            new LeaveRequestRecord()
            {
                Id = "3",
                PerLeaveId = "761",
                EmployeeId = "0011560",
                StartDate = new DateTime(2019,10,01),
                EndDate = new DateTime(2019,10,01),
                ApproverId="0010351",
                //ApproverName="Hadrian O. Racz",
                Status = LeaveStatusAction.Submitted,
                LeaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
                {
                    leaveRequestDetailRecords[3]
                }
            }
        };

        public List<LeaveRequestRecord> leaveRequestsForTimeEntry = new List<LeaveRequestRecord>()
        {
                        new LeaveRequestRecord() {
                        Id = "14",
                        PerLeaveId ="698",
                        EmployeeId ="0011560",
                        ApproverId ="0010351",
                        ApproverName ="Hadrian O. Racz",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(1),
                        Status = LeaveStatusAction.Approved,
                        LeaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
                        {
                           leaveRequestDetailRecords[5],
                            leaveRequestDetailRecords[6]
                        }
                    },
                           new LeaveRequestRecord() {
                        Id = "15",
                        PerLeaveId ="784",
                        EmployeeId ="0011560",
                        ApproverId ="0010351",
                        ApproverName ="Hadrian O. Racz",
                        StartDate = DateTime.Today,
                        EndDate =  DateTime.Today,
                        Status = LeaveStatusAction.Approved,
                        LeaveRequestDetailRecords = new List<LeaveRequestDetailRecord>()
                        {
                             leaveRequestDetailRecords[7]

                        }
                    }
                };
        #endregion
        #endregion

        #region Helper_Methods

        /// <summary>
        /// Randomly generates a string
        /// </summary>
        /// <returns></returns>
        public static string RandomLeaveRequestIdGenerator()
        {
            // Generate a random Id for the leave request record
            var idGenerator = new Random();
            return idGenerator.Next().ToString();
        }

        /// <summary>
        /// Builds a leave request entity using LeaveRequestRecord
        /// </summary>
        /// <param name="leaveRequestRecord"></param>
        /// <returns>LeaveRequest entity</returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.LeaveRequest BuildLeaveRequestEntity(LeaveRequestRecord leaveRequestRecord)
        {
            if (leaveRequestRecord == null)
            {
                throw new ArgumentNullException("leaveRequestRecord");
            }

            // Extract the LeaveRequestDetailRecords
            List<Ellucian.Colleague.Domain.HumanResources.Entities.LeaveRequestDetail> leaveRquestDetails = null;
            if (leaveRequestRecord.LeaveRequestDetailRecords != null && leaveRequestRecord.LeaveRequestDetailRecords.Any())
            {
                leaveRquestDetails = new List<HumanResources.Entities.LeaveRequestDetail>();
                leaveRequestRecord.LeaveRequestDetailRecords.ForEach(lrd => leaveRquestDetails.Add(new HumanResources.Entities.LeaveRequestDetail(lrd.Id, lrd.LeaveRequestId, lrd.LeaveDate, lrd.LeaveHours, lrd.ProcessedInPayPeriod, lrd.LeaveRequestDetailChgopr)));
            }

            // Get the latest Leave Request Status record of the current Leave Request Id
            var latestLeaveRequestStatus = leaveRequestStatusRecords.Where(lrs => lrs.Id != null && lrs.LeaveRequestId == leaveRequestRecord.Id).OrderByDescending(lrs => int.Parse(lrs.Id)).FirstOrDefault();
            // To Do: Extract the LeaveRequestCommentRecords

            // Build the LeaveRequest Entity
            return new HumanResources.Entities.LeaveRequest(leaveRequestRecord.Id,
                leaveRequestRecord.PerLeaveId,
                leaveRequestRecord.EmployeeId,
                leaveRequestRecord.StartDate,
                leaveRequestRecord.EndDate,
                latestLeaveRequestStatus.ActionerId,
                leaveRequestRecord.ApproverName,
                leaveRequestRecord.EmployeeName,
                leaveRequestRecord.Status,
                leaveRquestDetails,
                new List<LeaveRequestComment>(),
                leaveRequestRecord.EnableDeleteForSupervisor);
        }

        /// <summary>
        /// Builds a leave request status entity using LeaveRequestStatusRecord
        /// </summary>
        /// <param name="leaveRequestStatusRecord"></param>
        /// <returns>LeaveRequestStatus entity</returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.LeaveRequestStatus BuildLeaveRequestStatusEntity(LeaveRequestStatusRecord leaveRequestStatusRecord)
        {
            if (leaveRequestStatusRecord == null)
            {
                throw new ArgumentNullException("leaveRequestStatusRecord");
            }

            // Build the LeaveRequestStatus Entity
            return new HumanResources.Entities.LeaveRequestStatus(leaveRequestStatusRecord.Id,
                leaveRequestStatusRecord.LeaveRequestId,
                leaveRequestStatusRecord.ActionType,
                leaveRequestStatusRecord.ActionerId)
            {
                Timestamp = new Timestamp(leaveRequestStatusRecord.AddOpr,
            leaveRequestStatusRecord.AddTime.ToPointInTimeDateTimeOffset(leaveRequestStatusRecord.AddDate, colleagueTimeZone).Value,
            leaveRequestStatusRecord.ChangeOpr,
            leaveRequestStatusRecord.ChangeTime.ToPointInTimeDateTimeOffset(leaveRequestStatusRecord.ChangeDate, colleagueTimeZone).Value)
            };
        }

        /// <summary>
        /// Helper method to retrieve the id of the newly created leave request
        /// </summary>
        /// <param name="perLeaveId"></param>
        /// <param name="employeeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approverId"></param>
        /// <param name="approverName"></param>
        /// <param name="status"></param>
        /// <param name="leaveRequestDetails"></param>
        /// <returns></returns>
        public string CreateLeaveRequestHelper(string perLeaveId,
            string employeeId,
            DateTime? startDate,
            DateTime? endDate,
            string approverId,
            string approverName,            
            LeaveStatusAction status,
            List<HumanResources.Entities.LeaveRequestDetail> leaveRequestDetails)
        {
            //To DO: Comments
            var leaveRequestRecordToBeCreated = new LeaveRequestRecord()
            {
                Id = randomLeaveRequestId,
                PerLeaveId = perLeaveId,
                EmployeeId = employeeId,
                StartDate = startDate,
                EndDate = endDate,
                ApproverId = approverId,
                ApproverName = approverName,                
                Status = status,
                LeaveRequestDetailRecords = leaveRequestDetailRecords.Where(lrd => lrd.LeaveRequestId == randomLeaveRequestId).ToList(),
                LeaveRequestStatusRecords = leaveRequestStatusRecords.Where(lrs => lrs.LeaveRequestId == randomLeaveRequestId).ToList()
            };

            leaveRequestRecords.Add(leaveRequestRecordToBeCreated);

            return leaveRequestRecordToBeCreated.Id;
        }

        /// <summary>
        /// Helper method to retrieve the id of the newly created leave request status
        /// </summary>
        /// <param name="leaveRequestId"></param>
        /// <param name="actionType"></param>
        /// <param name="actionerid"></param>
        /// <returns></returns>
        public string CreateLeaveRequestStatusHelper(string leaveRequestId,
            LeaveStatusAction actionType,
            string actionerid)
        {
            // Generate a random Id for the leave request status record
            var idGenerator = new Random();
            var randomleaveRequestStatusId = idGenerator.Next().ToString();

            var leaveRequestStatusRecordToBeCreated = new LeaveRequestStatusRecord()
            {
                Id = randomleaveRequestStatusId,
                LeaveRequestId = leaveRequestId,
                ActionType = actionType,
                ActionerId = actionerid,
                AddOpr = actionerid,
                AddDate = new DateTime(2019, 1, 1),
                AddTime = new DateTime(2019, 1, 1, 12, 00, 0),
                ChangeOpr = actionerid,
                ChangeDate = new DateTime(2019, 1, 1),
                ChangeTime = new DateTime(2019, 1, 1, 12, 00, 0)
            };

            leaveRequestStatusRecords.Add(leaveRequestStatusRecordToBeCreated);

            return leaveRequestStatusRecordToBeCreated.Id;
        }
        #endregion

        #region Repository_Methods
        public async Task<IEnumerable<HumanResources.Entities.LeaveRequest>> GetLeaveRequestsAsync(IEnumerable<string> effectivePersonIds)
        {
            var entities = leaveRequestRecords.Where(lr => effectivePersonIds.Contains(lr.EmployeeId))
              .Select(lr => BuildLeaveRequestEntity(lr));

            return await Task.FromResult(entities);
        }

        public async Task<HumanResources.Entities.LeaveRequest> GetLeaveRequestInfoByLeaveRequestIdAsync(string leaveRequestId, string currentUserId = null)
        {
            var record = leaveRequestRecords.FirstOrDefault(lr => lr.Id == leaveRequestId);

            return await Task.FromResult(BuildLeaveRequestEntity(record));
        }

        public async Task<HumanResources.Entities.LeaveRequest> CreateLeaveRequestAsync(HumanResources.Entities.LeaveRequest leaveRequest)
        {
            var leaveRequestId = CreateLeaveRequestHelper(leaveRequest.PerLeaveId,
                leaveRequest.EmployeeId,
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                leaveRequest.ApproverId,
                leaveRequest.ApproverName,                
                leaveRequest.Status,
                leaveRequest.LeaveRequestDetails);

            return await GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestId);
        }

        public async Task<HumanResources.Entities.LeaveRequestStatus> CreateLeaveRequestStatusAsync(HumanResources.Entities.LeaveRequestStatus leaveRequestStatus)
        {
            var leaveRequestStatusId = CreateLeaveRequestStatusHelper(leaveRequestStatus.LeaveRequestId,
                leaveRequestStatus.ActionType,
                leaveRequestStatus.ActionerId);

            var newlyCreatedLeaveRequestStatusEntity = BuildLeaveRequestStatusEntity(leaveRequestStatusRecords.FirstOrDefault(lrs => lrs.Id == leaveRequestStatusId));

            return await Task.FromResult(newlyCreatedLeaveRequestStatusEntity);
        }

        public async Task<HumanResources.Entities.LeaveRequest> UpdateLeaveRequestAsync(LeaveRequestHelper leaveRequestHelper)
        {
            if (leaveRequestHelper == null)
            {
                throw new ArgumentNullException("leaveRequestHelper");
            }
            if (leaveRequestHelper.LeaveRequest == null)
            {
                throw new ArgumentException("LeaveRequest object of leaveRequestHelper is null");
            }
            // Retrieve the existing leave request record
            var existingLeaveRequestRecord = leaveRequestRecords.FirstOrDefault(lr => lr.Id == leaveRequestHelper.LeaveRequest.Id);

            // Update the existing leave request record with the new values
            existingLeaveRequestRecord.PerLeaveId = leaveRequestHelper.LeaveRequest.PerLeaveId;
            existingLeaveRequestRecord.StartDate = leaveRequestHelper.LeaveRequest.StartDate;
            existingLeaveRequestRecord.EndDate = leaveRequestHelper.LeaveRequest.EndDate;
            existingLeaveRequestRecord.ApproverId = leaveRequestHelper.LeaveRequest.ApproverId;
            existingLeaveRequestRecord.ApproverName = leaveRequestHelper.LeaveRequest.ApproverName;
            existingLeaveRequestRecord.Status = leaveRequestHelper.LeaveRequest.Status;

            // Add/Update/Delete the leave request detail records 
            var idGen = new Random();
            if (leaveRequestHelper.LeaveRequestDetailsToCreate != null && leaveRequestHelper.LeaveRequestDetailsToCreate.Any())
            {
                leaveRequestHelper.LeaveRequestDetailsToCreate.ForEach(lrd =>
                    existingLeaveRequestRecord.LeaveRequestDetailRecords.Add(new LeaveRequestDetailRecord()
                    {
                        Id = idGen.Next().ToString(),
                        LeaveRequestId = existingLeaveRequestRecord.Id,
                        LeaveDate = lrd.LeaveDate,
                        LeaveHours = lrd.LeaveHours
                    }));
            }

            if (leaveRequestHelper.LeaveRequestDetailsToUpdate != null && leaveRequestHelper.LeaveRequestDetailsToUpdate.Any())
            {
                leaveRequestHelper.LeaveRequestDetailsToUpdate.ForEach(lrd =>
                {
                    var entry = existingLeaveRequestRecord.LeaveRequestDetailRecords.FirstOrDefault(rec => rec.Id == lrd.Id);
                    if (entry != null)
                    {
                        entry.LeaveDate = lrd.LeaveDate;
                        entry.LeaveHours = lrd.LeaveHours;
                    }
                });
            }

            if (leaveRequestHelper.LeaveRequestDetailsToDelete != null && leaveRequestHelper.LeaveRequestDetailsToDelete.Any())
            {
                leaveRequestHelper.LeaveRequestDetailsToDelete.ForEach(lrd =>
                {
                    var entry = existingLeaveRequestRecord.LeaveRequestDetailRecords.FirstOrDefault(rec => rec.Id == lrd.Id);
                    if (entry != null)
                    {
                        existingLeaveRequestRecord.LeaveRequestDetailRecords.Remove(entry);
                    }
                }
                );
            }

            return await GetLeaveRequestInfoByLeaveRequestIdAsync(leaveRequestHelper.LeaveRequest.Id);
        }

        public Task<LeaveRequestComment> CreateLeaveRequestCommentsAsync(LeaveRequestComment leaveRequestComment)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<HumanResources.Entities.LeaveRequest>> GetLeaveRequestsForSupervisorAsync(string supervisorId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<HumanResources.Entities.LeaveRequest>> GetLeaveRequestsForTimeEntryAsync(DateTime startDate, DateTime endDate, IEnumerable<string> effectivePersonIds)
        {
            List<HumanResources.Entities.LeaveRequest> LeaveRequestForTimeEntry = new List<HumanResources.Entities.LeaveRequest>();
            leaveRequestsForTimeEntry.ForEach(x => LeaveRequestForTimeEntry.Add(BuildLeaveRequestEntity(x)));

            return await Task.FromResult(LeaveRequestForTimeEntry);
        }

        public Task<PersonHierarchyName> GetPersonNameFromNameHierarchy(PersonBase personBase)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
