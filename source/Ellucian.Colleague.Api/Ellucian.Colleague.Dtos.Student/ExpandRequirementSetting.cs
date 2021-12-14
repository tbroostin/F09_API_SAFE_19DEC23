//Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// ExpandRequirementSetting enumeration defines various settings for the requirements in my progress page
    /// </summary>
    public enum ExpandRequirementSetting
    {
        /// <summary>
        /// This is to enforce the default expansion behavior in my progress page
        /// </summary>
        None,
        /// <summary>
        /// This is to expand all requirements in my progress page
        /// </summary>
        Expand,
        /// <summary>
        /// This is to collapse all requirements in my progress page
        /// </summary>
        Collapse
    }
}
