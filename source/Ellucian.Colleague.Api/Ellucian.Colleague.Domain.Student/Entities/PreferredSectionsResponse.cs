// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PreferredSectionsResponse
    {
        private readonly List<PreferredSection> _preferredSections = new List<PreferredSection>();
        public ReadOnlyCollection<PreferredSection> PreferredSections { get; private set; }

        private readonly List<PreferredSectionMessage> _messages = new List<PreferredSectionMessage>();
        public ReadOnlyCollection<PreferredSectionMessage> Messages { get; private set; }

        public PreferredSectionsResponse(IEnumerable<PreferredSection> sections, IEnumerable<PreferredSectionMessage> messages)
        {
            _preferredSections = new List<PreferredSection>(sections);
            _messages = new List<PreferredSectionMessage>(messages);
            PreferredSections = _preferredSections.AsReadOnly();
            Messages = _messages.AsReadOnly();
        }
    }
}
