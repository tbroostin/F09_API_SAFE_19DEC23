// Copyright 2014 Ellucian Company L.P. and its acffiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The type of instruction offered and the amount of contact with it.
    /// </summary>
    [Serializable]
    public class InstructionalContact
    {
        private readonly string _instructionalMethodCode;
        /// <summary>
        /// A code indicating the method of instruction used
        /// </summary>
        public string InstructionalMethodCode { get { return _instructionalMethodCode; } }
        /// <summary>
        /// A number representing how much this instruction method counts toward an
        /// instructor's full load.
        /// </summary>
        public decimal? Load { get; set; }
        /// <summary>
        /// The number of clock hours spent in this type of instruction
        /// </summary>
        public decimal? ClockHours { get; set; }
        /// <summary>
        /// The number of hours the instructor meets with a student for the units
        /// specified in the ContactMeasure.  A 3 credit class might have 3 contact
        /// hours per week, but 45 hours per term.
        /// </summary>
        public decimal? ContactHours { get; set; }
        /// <summary>
        /// A code representing the type of measure used for contact hours
        /// </summary>
        public string ContactMeasure { get; set; }

        /// <summary>
        /// Instructional method constructor
        /// </summary>
        /// <param name="instructionalMethodCode">Code indicating the instructional method</param>
        public InstructionalContact(string instructionalMethodCode)
        {
            if (string.IsNullOrEmpty(instructionalMethodCode))
            {
                throw new ArgumentNullException("instructionalMethodCode");
            }

            _instructionalMethodCode = instructionalMethodCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            InstructionalContact other = obj as InstructionalContact;
            if (other == null)
            {
                return false;
            }

            return InstructionalMethodCode.Equals(other.InstructionalMethodCode);
        }

        public override int GetHashCode()
        {
            return InstructionalMethodCode.GetHashCode();
        }
    }
}
