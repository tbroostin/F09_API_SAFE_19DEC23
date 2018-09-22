// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Request
    {
        public RequestedStudent RequestedStudent { get; set; }
        public Recipient Recipient { get; set; }
    }
}
