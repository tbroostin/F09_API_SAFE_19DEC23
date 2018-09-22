// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionDecisionType : GuidCodeItem
    {
        /// <summary>
        /// Admission Decision Type (INTG.APPLICATION.TYPES)
        /// </summary>
        public AdmissionDecisionType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        public AdmissionApplicationStatusTypesCategory AdmissionApplicationStatusTypesCategory { get; set; }

        public string SpecialProcessingCode { get; set; }
    }
}