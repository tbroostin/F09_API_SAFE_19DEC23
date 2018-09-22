// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of an event
    /// </summary>
    [Serializable]
    public class Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The type.</param>
        /// <param name="location">The location.</param>
        /// <param name="pointer">The pointer.</param>
        /// <param name="date">The date.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="roomIds">The rooms.</param>
        /// <exception cref="System.ArgumentNullException">
        /// type;Calendar Schedule Type is required
        /// or
        /// pointer;Calendar Schedule Associated Record Pointer is required
        /// or
        /// date;Calendar Schedule Date is required
        /// or
        /// startTime;Calendar Schedule Start Time is required
        /// or
        /// endTime;Calendar Schedule EndTime is required
        /// </exception>
        /// <exception cref="System.ArgumentException">Start time cannot be later than end time</exception>
        public Event(string id, string description, string type, string location, string pointer, 
            DateTimeOffset startTime, DateTimeOffset endTime)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Calendar Schedule Type is required");
            }
            if (string.IsNullOrEmpty(pointer))
            {
                throw new ArgumentNullException("pointer", "Calendar Schedule Associated Record Pointer is required");
            }
            if (startTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("startTime", "Start time may not be the default date/time");
            }
            if (startTime.Date == DateTime.MinValue)
            {
                throw new ArgumentException("startTime", "Start time must include a valid date");
            }
            if (endTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("endTime", "End time may not be the default date/time");
            }
            if (endTime.Date == DateTime.MinValue)
            {
                throw new ArgumentException("endTime", "End time must include a valid date");
            }
            if (startTime.TimeOfDay > endTime.TimeOfDay)
            {
                throw new ArgumentException("Start time cannot be later than end time");
            }

            _Id = id;
            _Description = description.Trim();
            _Type = type;
            _LocationCode = location;
            _Pointer = pointer;
            _StartTime = startTime;
            _EndTime = endTime;

            PersonIds = _PersonIds.AsReadOnly();
            RoomIds = _RoomIds.AsReadOnly();
        }

        private string _Id;
        private readonly string _Description;
        private readonly string _Type;
        private readonly string _LocationCode;
        private readonly string _Pointer;
        private readonly DateTimeOffset _StartTime;
        private readonly DateTimeOffset _EndTime;

        private List<string> _PersonIds = new List<string>();
        private List<string> _RoomIds = new List<string>();

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Calendar Schedule Id cannot be changed</exception>
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Event Id cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get { return _Description; } }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get { return _Type; } }

        /// <summary>
        /// Gets the location code.
        /// </summary>
        /// <value>
        /// The location code.
        /// </value>
        public string LocationCode { get { return _LocationCode; } }

        /// <summary>
        /// List of person IDs assigned to this event
        /// </summary>
        public ReadOnlyCollection<string> PersonIds { get; private set; }

        /// <summary>
        /// List of room IDs assigned to this event
        /// </summary>
        public ReadOnlyCollection<string> RoomIds { get; private set; }

        /// <summary>
        /// Gets the buildings.
        /// </summary>
        /// <value>
        /// The buildings.
        /// </value>
        public ReadOnlyCollection<string> Buildings { get { return new ReadOnlyCollection<string>(RoomIds.Select(r => r.Split('*')[0]).ToList()); } }

        /// <summary>
        /// Gets the rooms.
        /// </summary>
        /// <value>
        /// The rooms.
        /// </value>
        public ReadOnlyCollection<string> Rooms { get { return new ReadOnlyCollection<string>(RoomIds.Select(r => r.Split('*')[1]).ToList()); } }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(_LocationCode))
                {
                    builder.Append("Location: ");
                    builder.Append(_LocationCode);
                }
                // buildings and rooms may not be present
                if (RoomIds.Count > 0)
                {
                    foreach (var room in RoomIds)
                    {
                        if (room.IndexOf('*') > 0)
                        {
                            if (builder.Length > 0)
                            {
                                builder.Append(", ");
                            }
                            var roomSplit = room.Split('*');
                            if (roomSplit.Length > 1)
                            {
                                builder.Append(string.Format("Building:{0}, Room:{1}", roomSplit[0], roomSplit[1]));
                            }
                        }
                    }
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the pointer.
        /// </summary>
        /// <value>
        /// The pointer.
        /// </value>
        public string Pointer { get { return _Pointer; } }

        /// <summary>
        /// Gets the start date/time in local time.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime Start
        {
            get
            {
                return _StartTime.ToLocalTime().DateTime;
            }
        }

        /// <summary>
        /// Gets the UTC start time
        /// </summary>
        public DateTimeOffset StartTime
        {
            get
            {
                return _StartTime;
            }
        }
            
        /// <summary>
        /// Gets the end date/time in local time.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime End
        {
            get
            {
                return _EndTime.ToLocalTime().DateTime;
            }
        }

        /// <summary>
        /// Gets the UTC end time
        /// </summary>
        public DateTimeOffset EndTime
        {
            get
            {
                return _EndTime;
            }
        }

        /// <summary>
        /// Add a person to the event
        /// </summary>
        /// <param name="personId">Person ID</param>
        public void AddPerson(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            if (!_PersonIds.Contains(personId))
            {
                _PersonIds.Add(personId);
            }
        }

        /// <summary>
        /// Add a room to the event
        /// </summary>
        /// <param name="roomId">Room ID (building*room)</param>
        public void AddRoom(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                throw new ArgumentNullException("roomId", "Room ID must be specified.");
            }
            if (roomId.IndexOf('*') < 1 || roomId.Split('*').Length != 2 || string.IsNullOrEmpty(roomId.Split('*')[0]) || string.IsNullOrEmpty(roomId.Split('*')[1]))
            {
                throw new ArgumentException("Room ID must contain a building code, an asterisk, and a room number.", "roomId");
            }

            if (!_RoomIds.Contains(roomId))
            {
                _RoomIds.Add(roomId);
            }
        }
    }
}
