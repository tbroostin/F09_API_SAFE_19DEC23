// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class HousingRequest
    {
        public string Guid { get; private set; }

        public string RecordKey { get; private set; }

        public DateTimeOffset? StartDate { get; private set; }

        public string Status { get; private set; }

        public DateTimeOffset? EndDate { get; set; }

        public DateTimeOffset? StatusDate { get; set; }

        public long? LotteryNo { get; set; }

        public string PersonId { get; set; }

        public string Term { get; set; }

        public string FloorCharacteristic { get; set; }

        public string FloorCharacteristicReqd { get; set; }

        public List<RoomPreference> RoomPreferences { get; set; }

        public List<RoommatePreference> RoommatePreferences { get; set; }

        public List<RoommateCharacteristicPreference> RoommateCharacteristicPreferences { get; set; }

        public IEnumerable<RoomCharacteristicPreference> RoomCharacerstics { get; set; }
        
        public HousingRequest(string guid, string id, DateTimeOffset? startOn, string status)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Format("Guid is required for housing request. Id: {0}", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Id is required for housing request. Guid: {0}", guid));
            }
            if (!startOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("Start on date is required for housing request. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(string.Format("Status is required for housing request. Guid: {0}", guid));
            }

            this.Guid = guid;
            this.RecordKey = id;
            this.StartDate = startOn;
            this.Status = status;
        }

        public HousingRequest(string guid, DateTimeOffset? startOn, string status)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required for housing request.");
            }
            if (!startOn.HasValue)
            {
                throw new ArgumentNullException(string.Format("Start on date is required for housing request. Guid: {0}", guid));
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException(string.Format("Status is required for housing request. Guid: {0}", guid));
            }
            this.Guid = guid;
            this.RecordKey = string.Empty;
            this.StartDate = startOn;
            this.Status = status;
        }
    }
}
