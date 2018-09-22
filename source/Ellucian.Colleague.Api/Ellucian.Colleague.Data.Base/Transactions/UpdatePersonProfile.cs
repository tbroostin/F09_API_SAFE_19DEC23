//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/26/2018 9:00:05 AM by user jennifermoran
//
//     Type: CTX
//     Transaction ID: UPDATE.PERSON.PROFILE
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
	public class ProfileEmailAddresses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.ADDRESSES", InBoundData = true)]
		public string AlEmailAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.TYPES", InBoundData = true)]
		public string AlEmailType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.PREFERRED", InBoundData = true)]
		public string AlEmailPreferred { get; set; }
	}

	[DataContract]
	public class ProfilePersonalPhones
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PERSONAL.PHONE.NUMBERS", InBoundData = true)]
		public string AlPersonalPhoneNumbers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PERSONAL.PHONE.EXTENSIONS", InBoundData = true)]
		public string AlPersonalPhoneExtensions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PERSONAL.PHONE.TYPES", InBoundData = true)]
		public string AlPersonalPhoneTypes { get; set; }
	}

	[DataContract]
	public class ProfileAddresses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.ID", InBoundData = true)]
		public string AlAddressId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.TYPES", InBoundData = true)]
		public string AlAddressTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.LINES", InBoundData = true)]
		public string AlAddressLines { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.CITY", InBoundData = true)]
		public string AlAddressCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.STATE", InBoundData = true)]
		public string AlAddressState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.POSTAL.CODE", InBoundData = true)]
		public string AlAddressPostalCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.COUNTY", InBoundData = true)]
		public string AlAddressCounty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.COUNTRY", InBoundData = true)]
		public string AlAddressCountry { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.EFFECTIVE.START", InBoundData = true)]
		public Nullable<DateTime> AlAddressEffectiveStart { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.EFFECTIVE.END", InBoundData = true)]
		public Nullable<DateTime> AlAddressEffectiveEnd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ADDRESS.SOURCE", InBoundData = true)]
		public string AlAddressSource { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.PERSON.PROFILE", GeneratedDateTime = "4/26/2018 9:00:05 AM", User = "jennifermoran")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class UpdatePersonProfileRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.LAST.CHANGE.DATE", InBoundData = true)]        
		public Nullable<DateTime> ALastChangeDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "A.LAST.CHANGE.TIME", InBoundData = true)]        
		public Nullable<DateTime> ALastChangeTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.NICKNAME", InBoundData = true)]        
		public string ANickname { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CHOSEN.FIRST.NAME", InBoundData = true)]        
		public string AChosenFirstName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CHOSEN.MIDDLE.NAME", InBoundData = true)]        
		public string AChosenMiddleName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CHOSEN.LAST.NAME", InBoundData = true)]        
		public string AChosenLastName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.GENDER.IDENTITY", InBoundData = true)]        
		public string AGenderIdentity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSONAL.PRONOUN", InBoundData = true)]        
		public string APersonalPronoun { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EMAIL.ADDRESSES", InBoundData = true)]
		public List<ProfileEmailAddresses> ProfileEmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.PERSONAL.PHONE.NUMBERS", InBoundData = true)]
		public List<ProfilePersonalPhones> ProfilePersonalPhones { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.ADDRESS.CITY", InBoundData = true)]
		public List<ProfileAddresses> ProfileAddresses { get; set; }

		public UpdatePersonProfileRequest()
		{	
			ProfileEmailAddresses = new List<ProfileEmailAddresses>();
			ProfilePersonalPhones = new List<ProfilePersonalPhones>();
			ProfileAddresses = new List<ProfileAddresses>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.PERSON.PROFILE", GeneratedDateTime = "4/26/2018 9:00:05 AM", User = "jennifermoran")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class UpdatePersonProfileResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", OutBoundData = true)]        
		public string AErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.MSG", OutBoundData = true)]        
		public string AMsg { get; set; }

		public UpdatePersonProfileResponse()
		{	
		}
	}
}
