// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Instructional methods used for specific section meetings. Includes such things as instruction, discussion, lab.
    /// </summary>
    [Serializable]
    public class InstructionalMethod : GuidCodeItem
    {

        private readonly bool _isOnline;
        public bool IsOnline { get { return _isOnline; } }
        
        public InstructionalMethod(string guid, string code, string description, bool isOnline)
            : base(guid, code, description)
        {
            _isOnline = isOnline;
        }
    }
}
