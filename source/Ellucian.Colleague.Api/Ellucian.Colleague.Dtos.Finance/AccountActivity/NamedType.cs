// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A charge group
    /// </summary>
    public partial class NamedType
    {
        /// <summary>
        /// Order in which the charge group is displayed
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Description of the charge group
        /// </summary>
        public string Name { get; set; }
    }
}