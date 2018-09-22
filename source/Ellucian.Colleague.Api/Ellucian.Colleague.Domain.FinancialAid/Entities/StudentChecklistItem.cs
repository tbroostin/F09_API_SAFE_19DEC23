/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// A StudentChecklistItem is one item in a student's financial aid checklist
    /// </summary>
    [Serializable]
    public class StudentChecklistItem
    {
        /// <summary>
        /// The Code of the Checklist Item assigned to the student
        /// <see cref="ChecklistItem">ChecklistItem</see>
        /// </summary>
        public string Code { get { return code; } }
        private readonly string code;

        /// <summary>
        /// The control status indicates whether this item is required to be completed in order, required later, or not required at all.
        /// </summary>
        public ChecklistItemControlStatus ControlStatus { get; set; }

        /// <summary>
        /// Control status code is a display action code that gets later translated into <see cref>ChceklistItemControlStatus</see> enum type
        /// </summary>
        public string ControlStatusCode { get { return statusCode; } }
        private readonly string statusCode;

        /// <summary>
        /// A StudentChecklistItem is identified by its Code and Control Status. This constructor
        /// sets the values of those attributes.
        /// </summary>
        /// <param name="code">Checklist Item Code</param>
        /// <param name="controlStatus">Control Status of the item</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are empty or null</exception>
        public StudentChecklistItem(string code, ChecklistItemControlStatus controlStatus)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            this.code = code;
            ControlStatus = controlStatus;
        }

        /// <summary>
        /// A StudentChecklistItem is identified by its code and control status (code). This constructor
        /// sets the values of those attributes.
        /// </summary>
        /// <param name="code">Checklist Item Code</param>
        /// <param name="controlStatusCode">Control Status of the item</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are empty or null</exception>
        public StudentChecklistItem(string code, string controlStatusCode)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (string.IsNullOrEmpty(controlStatusCode))
            {
                throw new ArgumentNullException("controlStatusCode");
            }

            this.code = code;
            this.statusCode = controlStatusCode;
        }

        /// <summary>
        /// Two StudentChecklistItem objects are equal when the Codes are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentChecklistItem = obj as StudentChecklistItem;

            if (studentChecklistItem.Code == this.Code)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode based on the Code of the StudentChecklistItem
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation based on the Code of the StudentChecklistItem
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code;
        }
    }
}
