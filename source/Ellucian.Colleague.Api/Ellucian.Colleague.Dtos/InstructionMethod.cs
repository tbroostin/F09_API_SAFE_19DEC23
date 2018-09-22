// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    [DataContract]
    public class InstructionMethod
    {
        [DataMember(Name = "guid")]
        public Guid Guid { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}