// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class HousingAssignment
    {
        public string Guid { get; private set; }

        public string RecordKey { get; private set; }

        public string StudentId { get; private set; }

        public string RoomId { get; private set; }

        public string Term { get; set; }

        public DateTimeOffset? StartOn { get; private set; }

        public string Status { get; set; }

        public DateTimeOffset? StatusDate { get; set; }

        public DateTimeOffset? EndDate { get; private set; }

        public string RoomRate { get; set; }

        public string RatePeriod { get; set; }

        public decimal? RateOverride { get; set; }

        public string ResidentStaffIndicator { get; set; }

        public IEnumerable<HousingAssignmentStatus> Statuses { get; set; }

        public string RateOverrideReason { get; set; }

        public IEnumerable<ArAdditionalAmount> ArAdditionalAmounts { get; set; }

        public string ContractNumber { get; set; }

        public string Comments { get; set; }

        public string HousingRequest { get; set; }

        public string RoomRateTable { get; set; }

        public HousingAssignment(string guid, string id, string personId, string roomId, DateTimeOffset? startOn, DateTimeOffset? endOn)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Format("Guid is required for housing assignment. Id: {0}", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Id is required for housing assignment. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException(string.Format("Student id is required for housing assignment. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new ArgumentNullException(string.Format("Room id is required for housing assignment. Guid: {0}", guid));
            }
            if (!startOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("Start on date is required for housing assignment. Guid: {0}", guid));
            }
            if (!endOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("End on date is required for housing assignment. Guid: {0}", guid));
            }
            this.Guid = guid;
            this.RecordKey = id;
            this.StudentId = personId;
            this.RoomId = roomId;
            this.StartOn = startOn;
            this.EndDate = endOn;
        }

        public HousingAssignment(string guid, string personId, string roomId, DateTimeOffset? startOn, DateTimeOffset? endOn)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required for housing assignment. Id: {0}");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException(string.Format("Student id is required for housing assignment. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(roomId))
            {
                throw new ArgumentNullException(string.Format("Room id is required for housing assignment. Guid: {0}", guid));
            }
            if (!startOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("Start on date is required for housing assignment. Guid: {0}", guid));
            }
            if (!endOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("End on date is required for housing assignment. Guid: {0}", guid));
            }
            this.Guid = guid;
            this.StudentId = personId;
            this.RoomId = roomId;
            this.StartOn = startOn;
            this.EndDate = endOn;
        }

        public string Building { get; set; }
    }
}
