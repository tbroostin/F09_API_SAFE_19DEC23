/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates*/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The Financial Aid Office class
    /// </summary>
    [Serializable]
    public class FinancialAidOffice
    {
        private readonly string id;

        /// <summary>
        /// The unique id of this Office.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// The Id of the Institution's Location that this Office works with.
        /// </summary>
        // public string LocationId { get; set; }

        /// <summary>
        /// The Ids of the Institution's Locations that this office works with. An office can be assigned
        /// to multiple locations.
        /// </summary>
        public List<string> LocationIds { get; set; }

        /// <summary>
        /// Flag indicating whether or not this is the default office. If a student's current
        /// office cannot be determined, or assignment of a student to an office fails, we use
        /// the default office.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The name of the office
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The AddressLabel is the address (street address, city, state, zip) used in an address label.
        /// Each element in this address list corresponds to a new line in the address label.
        /// </summary>
        public List<string> AddressLabel { get; set; }

        /// <summary>
        /// The city of the financial aid office location
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The state of the financial aid office location
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The postal code of the financial aid office location
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// The name of the Financial Aid Director in charge of this office.
        /// </summary>
        public string DirectorName { get; set; }

        /// <summary>
        /// The phone number used to contact the office
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The fax number used to contact the office
        /// </summary>
        public string FaxNumber { get; set; }

        /// <summary>
        /// The email address used to contact the office
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Office Title IV Code
        /// </summary>
        public string TitleIVCode { get; set; }

        /// <summary>
        /// A list of configuration objects that contain data about how this Financial Aid office
        /// wants to expose data and control certain actions.
        /// </summary>
        public ReadOnlyCollection<FinancialAidConfiguration> Configurations { get; private set; }
        private readonly List<FinancialAidConfiguration> _configurations;
        /// <summary>
        /// Office default display year code
        /// </summary>
        public string DefaultDisplayYearCode { get; set; }

        /// <summary>
        /// Add a range of configuration objects to this office. The configurationsToAdd are first filtered to ensure
        /// only unique configurations and configurations that apply to this office are added.
        /// </summary>
        /// <param name="configurationsToAdd">A list of configurations to potentially add to this office</param>
        public void AddConfigurationRange(IEnumerable<FinancialAidConfiguration> configurationsToAdd)
        {
            _configurations.AddRange(
                configurationsToAdd.Where(c => c != null && c.OfficeId == this.Id && !Configurations.Contains(c)).Distinct()
            );

        }

        /// <summary>
        /// Add a configuration object to this office. If this office already contains a configuration with the same
        /// award year as the configurationToAdd, the configurationToAdd is not added and null is returned. Otherwise,
        /// we add the configuration to this office
        /// </summary>
        /// <param name="configurationToAdd">The Configuration object to add</param>
        /// <returns>The added Configuration object, or null if the object was not added.</returns>
        public bool AddConfiguration(FinancialAidConfiguration configurationToAdd)
        {
            if (configurationToAdd == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (configurationToAdd.OfficeId != this.Id ||
                Configurations.Any(c => c.Equals(configurationToAdd)))
            {
                return false;
            }
            _configurations.Add(configurationToAdd);

            return true;
        }

        /// <summary>
        /// Helper method to get a configuration object with the given award year.
        /// </summary>
        /// <param name="awardYear">A configuration object will be returned with this awardYear</param>
        /// <returns>A configuration object with the given award year, or null if no configuration object exists with the award year</returns>
        public FinancialAidConfiguration GetConfiguration(string awardYear)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            return _configurations.FirstOrDefault(c => c.AwardYear == awardYear);
        }

        /// <summary>
        /// US Department of Education's Office of Postsecondary Education Identifier
        /// </summary>
        public string OpeId { get; set; }

        /// <summary>
        /// Describes how to display AcademicProgressEvaluations to students
        /// </summary>
        public AcademicProgressConfiguration AcademicProgressConfiguration { get; set; }

        /// <summary>
        /// Office Constructor creates Office object with given officeId.
        /// </summary>        
        /// <param name="officeId">Id of Office to create</param>
        /// <exception cref="ArgumentNullException">Thrown if officeId is null or empty.</exception>
        public FinancialAidOffice(string officeId)
        {
            if (string.IsNullOrEmpty(officeId))
            {
                throw new ArgumentNullException("officeId");
            }

            id = officeId;
            AddressLabel = new List<string>();

            LocationIds = new List<string>();
            _configurations = new List<FinancialAidConfiguration>();
            this.Configurations = _configurations.AsReadOnly();
        }

        /// <summary>
        /// Compares this Office object with the given object.
        /// </summary>
        /// <param name="obj">The object to compare to this Office object</param>
        /// <returns>True if this Office's Id equals the given Office object's Id. False, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var office = obj as FinancialAidOffice;

            if (office.Id == this.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Computes the HashCode of this Office object, based on the Office Id.
        /// </summary>
        /// <returns>A HashCode representation of this object.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
