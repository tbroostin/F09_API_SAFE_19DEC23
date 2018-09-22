// Copyright 2016-2017 Ellucian Company L.P. and it's affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student term type and type date.
    /// </summary>
    [Serializable]
    public class StudentTypeInfo
    {
        private readonly string _type;
        private readonly DateTime? _typeDate;

        /// <summary>
        /// Student Type
        /// Ex: East
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Student Type Date
        /// </summary>
        public DateTime? TypeDate
        {
            get { return _typeDate; }
        }

        /// <summary>
        /// Initialize the Student Type
        /// </summary>
        /// <param name="type">Student type</param>
        /// <param name="typeDate">Student type date</param>

        public StudentTypeInfo(string type, DateTime? typeDate)
        {
            _type = type;
            _typeDate = typeDate;
        }
    }
}