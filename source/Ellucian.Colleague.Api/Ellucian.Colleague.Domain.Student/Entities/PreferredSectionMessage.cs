// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PreferredSectionMessage
    {
        public string SectionId { get; set; }
        public string Message { get; set; }

        public PreferredSectionMessage(string sectionId, string message)
        {
            SectionId = sectionId;
            Message = message;
        }
    }
}
