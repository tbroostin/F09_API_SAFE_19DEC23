// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Institution
    {
        /// <summary>
        /// Institution id
        /// </summary>
        private readonly string _Id;
        public string Id { get { return _Id; } }
        /// <summary>
        /// Institution type
        /// </summary>
        private readonly InstType _InstitutionType;
        public InstType InstitutionType { get { return _InstitutionType; } }
        /// <summary>
        /// Institution name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Institution CEEB Code
        /// </summary>
        public string Ceeb { get; set; }
        /// <summary>
        /// City of institution location
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State of institution location
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Flag that indicates if an institution is the host/self
        /// </summary>
        public bool IsHostInstitution { get; set; }
        /// <summary>
        /// This name appears on Financial Aid Transcripts and at the top 
        /// of the Financial Aid Shopping Sheet
        /// </summary>
        public string FinancialAidInstitutionName { get; set; }
        /// <summary>
        /// Email addresses required by educational-institutions
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }
        /// <summary>
        /// Phones required by educational-institutions
        /// </summary>
        public List<Phone> Phones { get; set; }
        /// <summary>
        /// List of addresses as required by educational-institutions
        /// </summary>
        public List<Domain.Base.Entities.Address> Addresses { get; set; }
        /// <summary>
        /// List of social media as required by educational-institutions
        /// </summary>
        public List<Domain.Base.Entities.SocialMedia> SocialMedia { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">institution id</param>
        /// <param name="institutionType">institution type</param>
        public Institution(string id, InstType institutionType)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Institution Id must be specified");
            }
            _Id = id;
            _InstitutionType = institutionType;
        }
        public Institution()
        {

        }
    }
}
