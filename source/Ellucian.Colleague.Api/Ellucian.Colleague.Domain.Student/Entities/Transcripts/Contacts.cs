// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Contacts
    {
        public Address Address { get; set; }
        public Phone Phone { get; set; }
        public Phone FaxPhone { get; set; }
        public Email Email { get; set; }
    }
}
