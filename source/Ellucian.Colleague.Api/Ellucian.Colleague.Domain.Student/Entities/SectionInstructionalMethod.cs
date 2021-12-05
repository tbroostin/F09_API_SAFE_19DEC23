// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section's Instruction method
    ///  These are loaded from section meeting info and if section meeting is missing then
    /// are loaded from SEC.INSTR.METHODS
    /// </summary>
    [Serializable]
    public class SectionInstructionalMethod
    {    
        /// <summary>
        /// Instructional method code
        /// </summary>
        public string Code{ get; private set; }


        /// <summary>
        /// Instructional method description
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Is instructional method online
        /// </summary>
        public bool IsOnline { get; private set; }

        public SectionInstructionalMethod(string code, string description, bool isOnline)
        {
            Code = code;
            Description = description;
            IsOnline = isOnline;
        }

    }
}

