// Copyright 2022 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Section registration action request
    /// </summary>
    public class SectionRegistrationGuid
    {
        /// <summary>
        /// GUID of section for registration request.
        /// </summary>
        public string SectionGuid{ get; set; }

        /// <summary>
        /// <see cref="RegistrationAction">RegistrationAction</see> to take (e.g., Add, Drop, Audit, etc)
        /// </summary>
        public RegistrationAction Action { get; set; }
        /// <summary>
        /// Decimal credits to register, only for variable credit sections
        /// </summary>
        public decimal? Credits { get; set; }

        /// <summary>
        /// A drop reason code which must be a value from the STUDENT.ACAD.CRED.STATUS.REASONS valcode table.
        /// A drop reason may only be specified when the Action is Drop. A drop reason is optional when the
        /// Action is drop.
        /// </summary>
        public string DropReasonCode { get; set; }

        /// <summary>
        /// The system ID of a record in the the INT.TO.WDRWL.CODE file.
        /// An intent to withdraw ID may only be specified when the Action is Drop. It is optional when the
        /// action is drop.
        /// </summary>
        public string IntentToWithdrawId { get; set; }
    }

}
