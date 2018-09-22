/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Timestamp data object
    /// </summary>
    [Serializable]
    public class Timestamp
    {
        /// <summary>
        /// Identity of record adder
        /// </summary>
        public string AddOperator { get { return addOperator; } }
        private readonly string addOperator;
        /// <summary>
        /// Identity of record changer
        /// </summary>
        public string ChangeOperator { get; private set; }
        /// <summary>
        /// Date and time and record add
        /// </summary>
        public DateTimeOffset AddDateTime { get { return addDateTime; } }
        private readonly DateTimeOffset addDateTime;
        /// <summary>
        /// Date and time of record change
        /// </summary>
        public DateTimeOffset ChangeDateTime { get; private set; }

        public Timestamp(string addOperator, DateTimeOffset addDateTime, string changeOperator, DateTimeOffset changeDateTime)
        {
            if (string.IsNullOrWhiteSpace(addOperator))
            {
                throw new ArgumentNullException("addOperator");
            }
            

            this.addOperator = addOperator;
            this.addDateTime = addDateTime;

            SetChangeStamp(changeOperator, changeDateTime);
        }

        public void SetChangeStamp(string changeOperator, DateTimeOffset changeDateTime)
        {
            if (string.IsNullOrWhiteSpace(changeOperator))
            {
                throw new ArgumentNullException("changeOperator");
            }
            if (changeDateTime < AddDateTime)
            {
                throw new ArgumentOutOfRangeException("change time cannot be before add time", "changeDateTime");
            }

            ChangeOperator = changeOperator;
            ChangeDateTime = changeDateTime;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var timestamp = obj as Timestamp;

            return AddOperator == timestamp.AddOperator &&
                AddDateTime == timestamp.AddDateTime &&
                ChangeOperator == timestamp.ChangeOperator &&
                ChangeDateTime == timestamp.ChangeDateTime;
        }

        public override int GetHashCode()
        {
            return AddOperator.GetHashCode() ^
                AddDateTime.GetHashCode() ^
                ChangeOperator.GetHashCode() ^
                ChangeDateTime.GetHashCode();
        }
    }
}
