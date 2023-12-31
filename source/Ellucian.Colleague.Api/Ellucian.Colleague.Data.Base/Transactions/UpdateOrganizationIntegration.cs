//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/22/2018 10:51:08 AM by user mbscs
//
//     Type: CTX
//     Transaction ID: UPDATE.ORG.INTEGRATION
//     Application: CORE
//     Environment: dvcoll_wstst01
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class OrgIntgEmailAddresses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.EMAIL.ADDRESSES", InBoundData = true)]
		public string OrgEmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.EMAIL.ADDRESS.TYPES", InBoundData = true)]
		public string OrgEmailAddressTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.EMAIL.PREF", InBoundData = true)]
		public string OrgEmailPref { get; set; }
	}

	[DataContract]
	public class OrgIntgPhones
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.PHONE.TYPES", InBoundData = true)]
		public string OrgPhoneTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.PHONE.NUMBERS", InBoundData = true)]
		public string OrgPhoneNumbers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.PHONE.EXTENSIONS", InBoundData = true)]
		public string OrgPhoneExtensions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.PHONE.PREF", InBoundData = true)]
		public string OrgPhonePref { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.PHONE.CALLING.CODE", InBoundData = true)]
		public string OrgPhoneCallingCode { get; set; }
	}

	[DataContract]
	public class OrgIntgAddresses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.IDS", InBoundData = true)]
		public string OrgAddrIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.TYPES", InBoundData = true)]
		public string OrgAddrTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.PREFERENCE", InBoundData = true)]
		public string OrgAddrPreference { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ORG.ADDR.START.DATE", InBoundData = true)]
		public Nullable<DateTime> OrgAddrStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ORG.ADDR.END.DATE", InBoundData = true)]
		public Nullable<DateTime> OrgAddrEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.LINES", InBoundData = true)]
		public string OrgAddrLines { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.CITIES", InBoundData = true)]
		public string OrgAddrCities { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.REGIONS", InBoundData = true)]
		public string OrgAddrRegions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.SUBREGIONS", InBoundData = true)]
		public string OrgAddrSubregions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.COUNTRIES", InBoundData = true)]
		public string OrgAddrCountries { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.CODES", InBoundData = true)]
		public string OrgAddrCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.DELIVERY.POINT", InBoundData = true)]
		public string OrgAddrDeliveryPoint { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.CARRIER.ROUTE", InBoundData = true)]
		public string OrgAddrCarrierRoute { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.CORRECTION.DIGIT", InBoundData = true)]
		public string OrgAddrCorrectionDigit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.LATITUDE", InBoundData = true)]
		public string OrgAddrLatitude { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.LONGITUDE", InBoundData = true)]
		public string OrgAddrLongitude { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ADDR.CHAPTERS", InBoundData = true)]
		public string OrgAddrChapters { get; set; }
	}

	[DataContract]
	public class OrgIntgAlternate
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ALTERNATE.ID.TYPES", InBoundData = true)]
		public string OrgAlternateIdTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ALTERNATE.IDS", InBoundData = true)]
		public string OrgAlternateIds { get; set; }
	}

	[DataContract]
	public class OrgIntgErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[DataContract]
	public class OrgIntgRoles
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ROLES", InBoundData = true)]
		public string OrgRoles { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ORG.ROLE.START.DATE", InBoundData = true)]
		public Nullable<DateTime> OrgRoleStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ORG.ROLE.END.DATE", InBoundData = true)]
		public Nullable<DateTime> OrgRoleEndDate { get; set; }
	}

	[DataContract]
	public class OrgIntgSocialMedia
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.SOCIAL.MEDIA.TYPES", InBoundData = true)]
		public string OrgSocialMediaTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.SOCIAL.MEDIA.HANDLES", InBoundData = true)]
		public string OrgSocialMediaHandles { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.SOCIAL.MEDIA.PREF", InBoundData = true)]
		public string OrgSocialMediaPref { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.ORG.INTEGRATION", GeneratedDateTime = "3/22/2018 10:51:08 AM", User = "mbscs")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class UpdateOrganizationIntegrationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ID", InBoundData = true)]        
		public string OrgId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "ORG.GUID", InBoundData = true)]        
		public string OrgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.TITLE", InBoundData = true)]        
		public string OrgTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.EMAIL.ADDRESS.TYPES", InBoundData = true)]
		public List<OrgIntgEmailAddresses> OrgIntgEmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.PHONE.NUMBERS", InBoundData = true)]
		public List<OrgIntgPhones> OrgIntgPhones { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.ADDR.CITIES", InBoundData = true)]
		public List<OrgIntgAddresses> OrgIntgAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.ALTERNATE.ID.TYPES", InBoundData = true)]
		public List<OrgIntgAlternate> OrgIntgAlternate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.ROLES", InBoundData = true)]
		public List<OrgIntgRoles> OrgIntgRoles { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ORG.SOCIAL.MEDIA.HANDLES", InBoundData = true)]
		public List<OrgIntgSocialMedia> OrgIntgSocialMedia { get; set; }

		public UpdateOrganizationIntegrationRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
			OrgIntgEmailAddresses = new List<OrgIntgEmailAddresses>();
			OrgIntgPhones = new List<OrgIntgPhones>();
			OrgIntgAddresses = new List<OrgIntgAddresses>();
			OrgIntgAlternate = new List<OrgIntgAlternate>();
			OrgIntgRoles = new List<OrgIntgRoles>();
			OrgIntgSocialMedia = new List<OrgIntgSocialMedia>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.ORG.INTEGRATION", GeneratedDateTime = "3/22/2018 10:51:08 AM", User = "mbscs")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class UpdateOrganizationIntegrationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ID", OutBoundData = true)]        
		public string OrgId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "ORG.GUID", OutBoundData = true)]        
		public string OrgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<OrgIntgErrors> OrgIntgErrors { get; set; }

		public UpdateOrganizationIntegrationResponse()
		{	
			OrgIntgErrors = new List<OrgIntgErrors>();
		}
	}
}
