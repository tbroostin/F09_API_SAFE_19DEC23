// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a person name type
    /// </summary>
    [DataContract]
    public class PersonNameTypeItem : CodeItem2
    {
        /// <summary>
        /// <see cref="PersonNameType2">PersonNameType</see>
        /// </summary>
        [DataMember(Name = "category")]
        public PersonNameType2 Type { get; set; }
    }
}