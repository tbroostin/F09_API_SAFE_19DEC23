// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Define the Institution object
    /// </summary>
    public class Institution
    {
        /// <summary>
        /// Institution Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Institution Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public InstType InstitutionType { get; set; }
        /// <summary>
        /// Name of the Institution
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// City where Institution is located
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State where Institution is located
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Flag that indicates if it is the host institution
        /// </summary>
        public bool IsHostInstitution { get; set; }
        /// <summary>
        /// This name appears on Financial Aid Transcripts and at the top 
        /// of the Financial Aid Shopping Sheet
        /// </summary>
        public string FinancialAidInstitutionName { get; set; }
    }
}
