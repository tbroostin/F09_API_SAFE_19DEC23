/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Exceptions
{
    /// <summary>
    /// Throw a Record Lock Exception when there's a lock on a database record you're attempting to update
    /// </summary>
    public class RecordLockException : Exception
    {
        /// <summary>
        /// The name of the DB Table containing the locked record
        /// </summary>
        public string LockedTableName { get; set; }

        /// <summary>
        /// The id of the record that is locked
        /// </summary>
        public string LockedRecordId { get; set; }

        /// <summary>
        /// The User Id holding the lock
        /// </summary>
        public string UserIdWithLock { get; set; }

        /// <summary>
        /// Message coming in from the constructor. Used to generate Message property
        /// </summary>
        private string inputMessage;

        /// <summary>
        /// Create a generic RecordLockException
        /// </summary>
        public RecordLockException() : base() { }
        
        /// <summary>
        /// Create a RecordLockException with a message and an inner exception 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RecordLockException(string message, Exception innerException) 
            : base(message, innerException) 
        {
            inputMessage = message;
        }

        /// <summary>
        /// Create a RecordLockException with a message, and optionally, the lockedTableName, the lockedRecordId and the userIdWithLock
        /// </summary>
        /// <param name="message"></param>
        /// <param name="lockedTableName"></param>
        /// <param name="lockedRecordId"></param>
        /// <param name="userIdWithLock"></param>
        public RecordLockException(string message, string lockedTableName="", string lockedRecordId="", string userIdWithLock="") 
            : base(message) 
        {
            inputMessage = message;
            LockedTableName = lockedTableName;
            LockedRecordId = lockedRecordId;
            UserIdWithLock = userIdWithLock;
        }

        /// <summary>
        /// Returns the Exception Message
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("{0}\nUser {1} has lock on {2} record {3}", inputMessage, UserIdWithLock, LockedTableName, LockedRecordId);
            }
        }
    }
}
