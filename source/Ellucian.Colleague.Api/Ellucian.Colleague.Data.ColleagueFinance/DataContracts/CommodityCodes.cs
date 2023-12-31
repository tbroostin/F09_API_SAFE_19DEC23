//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/5/2019 1:51:09 PM by user vaidyasubrahmanyadv
//
//     Type: ENTITY
//     Entity: COMMODITY.CODES
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CommodityCodes")]
	[ColleagueDataContract(GeneratedDateTime = "12/5/2019 1:51:09 PM", User = "vaidyasubrahmanyadv")]
	[EntityDataContract(EntityName = "COMMODITY.CODES", EntityType = "PHYS")]
	public class CommodityCodes : IColleagueGuidEntity
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
		/// CDD Name: CMDTY.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "CMDTY.DESC")]
		public string CmdtyDesc { get; set; }
		
		/// <summary>
		/// CDD Name: CMDTY.MSDS.FLAG
		/// </summary>
		[DataMember(Order = 1, Name = "CMDTY.MSDS.FLAG")]
		public string CmdtyMsdsFlag { get; set; }
		
		/// <summary>
		/// CDD Name: CMDTY.FIXED.ASSETS.FLAG
		/// </summary>
		[DataMember(Order = 3, Name = "CMDTY.FIXED.ASSETS.FLAG")]
		public string CmdtyFixedAssetsFlag { get; set; }
		
		/// <summary>
		/// CDD Name: CMDTY.TAX.CODES
		/// </summary>
		[DataMember(Order = 6, Name = "CMDTY.TAX.CODES")]
		public List<string> CmdtyTaxCodes { get; set; }
		
		/// <summary>
		/// CDD Name: CMDTY.DEFAULT.DESC.FLAG
		/// </summary>
		[DataMember(Order = 14, Name = "CMDTY.DEFAULT.DESC.FLAG")]
		public string CmdtyDefaultDescFlag { get; set; }
		
		/// <summary>
		/// CDD Name: CMDTY.PRICE
		/// </summary>
		[DataMember(Order = 20, Name = "CMDTY.PRICE")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? CmdtyPrice { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}