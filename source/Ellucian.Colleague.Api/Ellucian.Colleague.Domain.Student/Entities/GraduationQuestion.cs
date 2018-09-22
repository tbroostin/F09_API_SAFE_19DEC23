// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This object defines optional graduation questions that are available for the graduation application and how they should be handled on the application.
    /// </summary>
    [Serializable]
    public class GraduationQuestion
    {
        /// <summary>
        /// Indicates whether an answer for this question is required in order to submit a graduation application
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// Indicates the type of question so it can be properly placed on the graduation application
        /// </summary>
        public GraduationQuestionType Type { get; set; }

        public GraduationQuestion(GraduationQuestionType type, bool isRequired = false)
        {
            Type = type;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Equality for a graduation question is simply the type. Two items of the same type are considered a match.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is GraduationQuestion))
            {
                return false;
            }
            return (obj as GraduationQuestion).Type == this.Type;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }
    }
}
