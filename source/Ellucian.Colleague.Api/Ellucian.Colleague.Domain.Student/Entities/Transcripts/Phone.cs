// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Phone
    {
        public string AreaCityCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
