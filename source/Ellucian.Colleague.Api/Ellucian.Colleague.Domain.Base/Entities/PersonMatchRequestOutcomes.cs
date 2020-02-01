//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonMatchRequestOutcomes
    {
        public PersonMatchRequestOutcomes(PersonMatchRequestType type, PersonMatchRequestStatus status, DateTimeOffset date)
        {
            Type = type;
            Status = status;
            Date = date;
        }

        public PersonMatchRequestType Type { get; set; }

        public PersonMatchRequestStatus Status { get; set; }

        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Compares two PersonMatchRequestOutcomes objects for equality
        /// </summary>
        /// <param name="obj">The other object to test</param>
        /// <returns>A boolean indicating the equality</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PersonMatchRequestOutcomes other = obj as PersonMatchRequestOutcomes;
            if (other == null)
            {
                return false;
            }
            return Type.Equals(other.Type) &&
                 Status.Equals(other.Status) &&
                 Date.Equals(other.Date);
        }

        /// <summary>
        /// Return a hashcode for the PersonMatchRequestOutcomes
        /// </summary>
        /// <returns>The generated hashcode</returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Status.GetHashCode() ^ Date.GetHashCode();
        }
    }
}