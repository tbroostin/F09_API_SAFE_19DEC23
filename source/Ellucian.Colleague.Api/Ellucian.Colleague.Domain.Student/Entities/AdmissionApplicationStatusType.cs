// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionApplicationStatusType : GuidCodeItem
    {
        /// <summary>
        /// Admission Application Type (INTG.APPLICATION.TYPES)
        /// </summary>
        public AdmissionApplicationStatusType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        public AdmissionApplicationStatusTypesCategory AdmissionApplicationStatusTypesCategory { get; set; }

        public string SpecialProcessingCode { get; set; }
    }
}