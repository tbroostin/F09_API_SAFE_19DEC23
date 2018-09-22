// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of rooms
    /// </summary>
    [Serializable]
    public class Room
    {
        /// <summary>
        /// Id of the room item. In Colleague this is something like BAK*102.
        /// </summary>
        private readonly string _id;
        /// <summary>
        /// Gets the room item's identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get { return _id; } }

        /// <summary>
        /// GUID of the room item
        /// </summary>
        private readonly string _guid;
        /// <summary>
        /// Gets the room's GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public string Guid { get { return _guid; } }

        /// <summary>
        /// Description of the room
        /// </summary>
        private readonly string _description;
        /// <summary>
        /// Gets the room's description.
        /// </summary>
        /// <value>
        /// The Description.
        /// </value>
        public string Description { get { return _description; } }

        /// <summary>
        /// Gets the building code for this room.
        /// </summary>
        public string BuildingCode { get { return _id.Split('*')[0]; } }

        /// <summary>
        /// Gets the number for this room.
        /// </summary>
        public string Number { get { return _id.Split('*')[1]; } }

        /// <summary>
        /// Gets the code for this room.
        /// </summary>
        public string Code { get { return _id.Split('*')[1]; } }

        /// <summary>
        /// Floor
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Room Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Room Capacity
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Room Type
        /// </summary>
        public string RoomType { get; set; }

        /// <summary>
        /// Room Wing
        /// </summary>
         public string Wing { get; set; }

        /// <summary>
         /// Room Characteristics
        /// </summary>
         public List<string> Characteristics { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// </summary>
        /// <param name="guid">GUID for the room</param>
        /// <param name="id">ID for the room.</param>
        /// <param name="description">The description.</param>
        public Room(string guid, string id, string description)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (id.IndexOf('*') < 1 || id.Split('*').Length != 2 || string.IsNullOrEmpty(id.Split('*')[0]) || string.IsNullOrEmpty(id.Split('*')[1]))
            {
                throw new ArgumentException("Room ID must contain a building code, an asterisk, and a room number.", "id");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            _guid = guid;
            _id = id;
            _description = description;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" }, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Room other = obj as Room;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(_id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int roomHash = _id.GetHashCode();
            return roomHash;
        }
    }
}
