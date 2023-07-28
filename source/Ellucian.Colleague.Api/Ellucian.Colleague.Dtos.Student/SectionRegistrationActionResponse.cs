// Copyright 2022 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Section registration action request
    /// </summary>
    public class SectionRegistrationActionResponse
    {
        /// <summary>
        /// GUID of section. 
        /// </summary>
        public string SectionGuid{ get; set; }

        /// <summary>
        /// <see cref="RegistrationAction">RegistrationAction</see> to take (e.g., Add, Drop, Audit, etc)
        /// </summary>
        public RegistrationAction Action { get; set; }

        /// <summary>
        /// True if the action was or would be successful, false if not
        /// </summary>
        public bool SectionActionSuccess { get; set; }
    }

}
