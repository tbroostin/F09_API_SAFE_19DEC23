/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Describes how to display a particular property of an academic progress evaluation
    /// </summary>
    [Serializable]
    public class AcademicProgressPropertyConfiguration
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public AcademicProgressPropertyType Type { get { return type; } }
        private readonly AcademicProgressPropertyType type;

        /// <summary>
        /// The Label used to identify the property for display purposes
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// A description of what this property is and means for a student
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether or not to show this property to the student. True means
        /// do not show this property to the student. The default value is true.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Create an AcademicProgressPropertyConfiguration identified by the Type
        /// </summary>
        /// <param name="type">The AcademicProgressPropertyType of the AcademicProgress property this configuration describes</param>
        public AcademicProgressPropertyConfiguration(AcademicProgressPropertyType type)
        {
            this.type = type;
            IsHidden = true;
        }

        /// <summary>
        /// Two AcademicProgressPropertyConfiguration objects are equal when their Types are equal
        /// </summary>
        /// <param name="obj">AcademicProgressPropertyConfiguration object to compare to this</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var config = obj as AcademicProgressPropertyConfiguration;

            return config.Type == this.Type;
        }


        /// <summary>
        /// Computes the HashCode of this object based on the Type
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        /// <summary>
        /// Gets a string representation of this object based on the type
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type.ToString();
        }

    }
}
