// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionApplicationType : GuidCodeItem
    {
        /// <summary>
        /// Admission Application Type (INTG.APPLICATION.TYPES)
        /// </summary>
        public AdmissionApplicationType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}