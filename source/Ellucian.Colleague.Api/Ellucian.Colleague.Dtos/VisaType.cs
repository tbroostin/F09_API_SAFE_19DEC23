// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// VisaType object
    /// </summary>
    [DataContract]
    public class VisaType : CodeItem2
    {
        /// <summary>
        /// The <see cref="VisaTypeCategory">entity type</see> for the visa type categories
        /// </summary>
        [DataMember(Name = "category")]
        public VisaTypeCategory VisaTypeCategory { get; set; }
    }
}
