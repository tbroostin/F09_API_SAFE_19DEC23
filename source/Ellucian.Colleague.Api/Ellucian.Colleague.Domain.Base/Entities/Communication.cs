/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A Communication represents both a communication (paper, email, etc.) that was sent to a person, 
    /// and a communication that will be sent to a person.
    /// </summary>
    [Serializable]
    public class Communication : IComparable<Communication>
    {
        /// <summary>
        /// The Colleague PERSON id of the person to whom this communication was/will be sent to
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// The CommunicationCode code
        /// </summary>
        public string Code { get { return code; } }
        private readonly string code;

        /// <summary>
        /// A short description of this instance of the communication, which helps distinguish it from 
        /// other communications with the same code.
        /// </summary>
        public string InstanceDescription { get; set; }

        /// <summary>
        /// The date this communication was assigned to the person
        /// </summary>
        public DateTime? AssignedDate { get; set; }

        /// <summary>
        /// The CommunicationStatus Code of this communication
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// The date the status was assigned or changed
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// The date that the person is expected to take action on this communication.
        /// </summary>
        public DateTime? ActionDate { get; set; }

        /// <summary>
        /// The Id of the Comment. This is currently unsupported
        /// </summary>
        public string CommentId { get; set; }

        /// <summary>
        /// Create a Communication object
        /// </summary>
        /// <param name="personId">Required: the Colleague PERSON id of the person to whom this communication was/will be sent to</param>
        /// <param name="code">Required: The CommunicationCode code</param>
        /// <exception cref="ArgumentNullException">Thrown if any required argument is null or empty</exception>
        public Communication(string personId, string code)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("persondId");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            this.personId = personId;
            this.code = code;
            this.InstanceDescription = string.Empty;
            this.StatusCode = string.Empty;
            this.CommentId = string.Empty;
        }

        /// <summary>
        /// The string representation of this communication object using the Code, the Assigned Date, and the Instance Description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var formatDate = (this.AssignedDate.HasValue) ? AssignedDate.Value.ToShortDateString() : AssignedDate.ToString();
            return string.Format("{0}:{1}:{2}", Code, formatDate, InstanceDescription);
        }

        /// <summary>
        /// Compares this Communication object with another. If two communication objects are similar, they are also equal.
        /// If two communication objects are equal, they are not necessarily similar.
        /// </summary>
        /// <param name="communication">The communication object to compare to this</param>
        /// <returns></returns>
        public bool Similar(Communication communication)
        {
            if (communication == null)
            {
                return false;
            }

            if (communication.PersonId == this.PersonId &&
                communication.Code == this.Code &&
                communication.InstanceDescription == this.InstanceDescription &&
                communication.AssignedDate == this.AssignedDate)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Compares this object with another Communication object. Two Communication objects are equal when 
        /// 1. they are similar, or
        /// 2. if the statusCode is empty, their PersonIds, Codes and InstanceDescriptions are equal, or
        /// 3. if their PersonIds, Codes and InstanceDescriptions are equal and either Communication has an Assigned Date of Today
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var communication = obj as Communication;

            if (Similar(communication))
            {
                return true;
            }

            if (communication.PersonId == this.PersonId &&
                communication.Code == this.Code &&
                communication.InstanceDescription == this.InstanceDescription)
            {
                if (string.IsNullOrEmpty(this.StatusCode) || string.IsNullOrEmpty(communication.StatusCode))
                {
                    return true;
                }
                else if (communication.AssignedDate.HasValue && communication.AssignedDate.Value.Date.Equals(DateTime.Today.Date))
                {
                    return true;
                }
                else if (this.AssignedDate.HasValue && this.AssignedDate.Value.Date.Equals(DateTime.Today.Date))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode of this object based on the PersonId, Code, InstanceDescription and AssignedDate
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return PersonId.GetHashCode() ^ Code.GetHashCode() ^ InstanceDescription.GetHashCode() ^ AssignedDate.GetHashCode();
        }

        /// <summary>
        /// Compares this object with another Communication object.
        /// If two communications equal, then
        ///     StatusDates are compared, then
        ///     Codes are compared, then
        ///     StatusCodes are compared
        /// </summary>
        /// <param name="other">A Communication object to compare with this object.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: 
        /// Less than zero
        ///      This object is less than the other parameter.
        /// Zero 
        ///      This object is equal to other in relative terms, but the two objects may not be Equal.
        /// Greater than zero 
        ///      This object is greater than other.</returns>
        public int CompareTo(Communication other)
        {
            if (this.Equals(other))
            {
                return 0;
            }
            var statusDateCompare = this.StatusDate.GetValueOrDefault().CompareTo(other.StatusDate.GetValueOrDefault());
            if (statusDateCompare == 0)
            {
                var codeCompare = this.Code.CompareTo(other.Code);
                if (codeCompare == 0)
                {
                    return this.StatusCode.CompareTo(other.StatusCode);
                }
                else
                {
                    return codeCompare;
                }
            }
            else
            {
                return statusDateCompare;
            }
        }
    }
}
