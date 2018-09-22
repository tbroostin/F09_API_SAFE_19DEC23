//Copyright 2013-2016 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Phone number
    /// </summary>
    public class Phone
    {
        /// <summary>
        /// Entire phone number, excluding extension
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Phone extension
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// Phone Type such as Home, Fax or Business
        /// </summary>
        public string TypeCode { get; set; }
    }
}
