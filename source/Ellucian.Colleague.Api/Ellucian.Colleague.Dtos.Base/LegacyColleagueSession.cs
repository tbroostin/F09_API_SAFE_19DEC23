
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents the session identifiers of a Colleague session.
    /// </summary>
    public class LegacyColleagueSession
    {
        /// <summary>
        /// Colleague security token.
        /// </summary>
        public string SecurityToken { get; set; }
        /// <summary>
        /// Colleague control id.
        /// </summary>
        public string ControlId { get; set; }
    }
}
