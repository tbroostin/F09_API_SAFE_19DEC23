// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentOverloadPetition
    {
        /// <summary>
        /// Petition Id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string _id;

        /// <summary>
        /// The Student this petition belongs to
        /// </summary>
        public string StudentId { get { return _studentId; } }
        private string _studentId;

        /// <summary>
        /// Status Code - Accepted, Denied, Pending
        /// </summary>
        public string StatusCode
        {
            get { return _statusCode; }
        }
        private string _statusCode;

        /// <summary>
        /// Term code associated to the petition
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Date/time this petition was last changed
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }

        /// <summary>
        /// Constructor to create a new student overload petition. 
        /// </summary>
        /// <param name="id">Id of the student petition</param>
        /// <param name="studentId">Student associated to the petition. Required.</param>
        /// <param name="statusCode">Status code of the petition. Required.</param>
        public StudentOverloadPetition(string id, string studentId, string statusCode)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Petition ID is required");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required");
            }
            if (string.IsNullOrEmpty(statusCode))
            {
                throw new ArgumentNullException("statusCode", "Status Code is required");
            }
            
            this._id = id;
            this._studentId = studentId;
            this._statusCode = statusCode;
        }
    }
}
