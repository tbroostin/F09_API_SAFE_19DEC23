// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AdmissionPopulation : GuidCodeItem
    {
        /// <summary>
        /// Admission Population (Colleague admit status)
        /// </summary>
        public AdmissionPopulation(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}