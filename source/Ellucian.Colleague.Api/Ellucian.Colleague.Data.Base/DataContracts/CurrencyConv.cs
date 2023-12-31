//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/29/2019 10:00:07 AM by user bsf1
//
//     Type: ENTITY
//     Entity: CURRENCY.CONV
//     Application: CF
//     Environment: dvcoll
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
	[DataContract(Name = "CurrencyConv")]
	[ColleagueDataContract(GeneratedDateTime = "3/29/2019 10:00:07 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "CURRENCY.CONV", EntityType = "PHYS")]
	public class CurrencyConv : IColleagueGuidEntity
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
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: CURRENCY.CONV.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "CURRENCY.CONV.DESC")]
		public string CurrencyConvDesc { get; set; }
		
		/// <summary>
		/// CDD Name: CURRENCY.CONV.ISO.CODE
		/// </summary>
		[DataMember(Order = 7, Name = "CURRENCY.CONV.ISO.CODE")]
		public string CurrencyConvIsoCode { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}