//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:43:08 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PERSON.CONFIRMATIONS
//     Application: CORE
//     Environment: dvColl
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "PersonConfirmations")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:43:08 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PERSON.CONFIRMATIONS", EntityType = "PHYS")]
	public class PersonConfirmations : IColleagueEntity
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		/// <summary>
		/// Record Key
		/// </summary>
		[DataMember]
		public string Recordkey { get; set; }
		
		public void setKey(string key)
		{
			Recordkey = key;
		}
		
		/// <summary>
		/// CDD Name: CONF.PHONES.CONFIRM.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "CONF.PHONES.CONFIRM.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfPhonesConfirmDate { get; set; }
		
		/// <summary>
		/// CDD Name: CONF.ADDRESSES.CONFIRM.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "CONF.ADDRESSES.CONFIRM.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfAddressesConfirmDate { get; set; }
		
		/// <summary>
		/// CDD Name: CONF.EMAILS.CONFIRM.DATE
		/// </summary>
		[DataMember(Order = 4, Name = "CONF.EMAILS.CONFIRM.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfEmailsConfirmDate { get; set; }
		
		/// <summary>
		/// CDD Name: CONF.ADDRESSES.CONFIRM.TIME
		/// </summary>
		[DataMember(Order = 9, Name = "CONF.ADDRESSES.CONFIRM.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfAddressesConfirmTime { get; set; }
		
		/// <summary>
		/// CDD Name: CONF.EMAILS.CONFIRM.TIME
		/// </summary>
		[DataMember(Order = 10, Name = "CONF.EMAILS.CONFIRM.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfEmailsConfirmTime { get; set; }
		
		/// <summary>
		/// CDD Name: CONF.PHONES.CONFIRM.TIME
		/// </summary>
		[DataMember(Order = 11, Name = "CONF.PHONES.CONFIRM.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ConfPhonesConfirmTime { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}