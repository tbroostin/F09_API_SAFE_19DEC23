//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/8/2020 12:51:25 PM by user vinay_an
//
//     Type: ENTITY
//     Entity: VENDOR.COMMODITIES
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
	[DataContract(Name = "VendorCommodities")]
	[ColleagueDataContract(GeneratedDateTime = "4/8/2020 12:51:25 PM", User = "vinay_an")]
	[EntityDataContract(EntityName = "VENDOR.COMMODITIES", EntityType = "PHYS")]
	public class VendorCommodities : IColleagueEntity
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
		/// CDD Name: VCM.STANDARD.PRICE
		/// </summary>
		[DataMember(Order = 0, Name = "VCM.STANDARD.PRICE")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? VcmStandardPrice { get; set; }
		
		/// <summary>
		/// CDD Name: VCM.STANDARD.PRICE.DATE
		/// </summary>
		[DataMember(Order = 7, Name = "VCM.STANDARD.PRICE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? VcmStandardPriceDate { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}