// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ExternalTranscriptStatus : CodeItem
    {
        public ExternalTranscriptStatus(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}