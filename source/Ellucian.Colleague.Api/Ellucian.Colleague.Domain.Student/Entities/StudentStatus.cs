using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentStatus : GuidCodeItem
    {
        
        private string _SpecialProcessing1;
        /// <summary>
        /// The usage(s) associated with the student academic period status.
        /// </summary>
        public string SpecialProcessing1 { get { return _SpecialProcessing1; } }


        public StudentStatus(string guid, string code, string desc, string specialProcessing1 = "")
            : base(guid, code, desc)
        {
            _SpecialProcessing1 = specialProcessing1;
        }
    }
}