// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonHoldRequest
    {
        public PersonHoldRequest(string id, string personId, string restrictionType, DateTimeOffset? startOn, DateTimeOffset? endOn, string notificationIndicator = "Y")
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId is required");
            }
            if (string.IsNullOrEmpty(restrictionType))
            {
                throw new ArgumentNullException("restrictionType is required");
            }
            if (startOn == null)
            {
                throw new ArgumentNullException("startOn is required");
            }
            _id = id;
            _personId = personId;
            _restrictionType = restrictionType;
            _startOn = startOn;
            _endOn = endOn;
            _notificationIndicator = notificationIndicator;
        }

        private string _id;
        public string Id 
        { 
            get { return _id; } 
            set { _id = value; } 
        }

        private string _personId;
        public string PersonId 
        { 
            get { return _personId; } 
            set { _personId = value; }
        }

        private string _restrictionType;
        public string RestrictionType 
        { 
            get { return _restrictionType; } 
            set { _restrictionType = value; } 
        }

        private DateTimeOffset? _startOn;
        public DateTimeOffset? StartOn
        {
            get { return _startOn; }
            set { _startOn = value; }
        }

        private DateTimeOffset? _endOn;
        public DateTimeOffset? EndOn
        {
            get { return _endOn; }
            set { _endOn = value; }
        }

        private string _notificationIndicator;
        public string NotificationIndicator
        {
            get { return _notificationIndicator; }
            set { _notificationIndicator = value; }
        }
        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        private string _personHoldGuid;
        public string PersonHoldGuid
        {
            get { return _personHoldGuid; }
            set { _personHoldGuid = value; }
        }
        
    }
}
