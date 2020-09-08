//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ProspectOpportunity
    {
        public ProspectOpportunity()
        {
        }

        public ProspectOpportunity(string guid, string recordKey)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Prospect opportunity guid is required.");
            }
            if (string.IsNullOrEmpty(recordKey))
            {
                throw new ArgumentNullException("Prospect opportunity record key is required.");
            }

            _guid = guid;
            _recordKey = recordKey;
        }

        private string _guid;
        public string Guid { get { return _guid; } }

        private string _recordKey;
        public string RecordKey { get { return _recordKey; } }
        public string ProspectId { get; set; }
        public string EntryAcademicPeriod { get; set; }
        public string AdmissionPopulation { get; set; }
        public string Site { get; set; }
        public string StudentAcadProgId { get; set; }
        public string EducationalGoal { get; set; }
        public List<string> CareerGoals { get; set; }
    }
}
