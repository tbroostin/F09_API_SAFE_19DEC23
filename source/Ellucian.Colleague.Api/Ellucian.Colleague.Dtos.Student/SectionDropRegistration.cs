
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Drop Section registration action request
    /// </summary>
    public class SectionDropRegistration
    {
        /// <summary>
        /// Id of section for drop registration request
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// A drop reason code which must be a value from the STUDENT.ACAD.CRED.STATUS.REASONS valcode table.
        /// </summary>
        public string DropReasonCode { get; set; }
    }
}
