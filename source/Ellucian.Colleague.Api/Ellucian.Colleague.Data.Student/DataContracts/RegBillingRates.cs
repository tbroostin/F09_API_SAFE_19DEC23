//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:14:13 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: REG.BILLING.RATES
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "RegBillingRates")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:14:13 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "REG.BILLING.RATES", EntityType = "PHYS")]
	public class RegBillingRates : IColleagueEntity
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
		/// CDD Name: RGBR.AR.CODE
		/// </summary>
		[DataMember(Order = 0, Name = "RGBR.AR.CODE")]
		public string RgbrArCode { get; set; }
		
		/// <summary>
		/// CDD Name: RGBR.RULE
		/// </summary>
		[DataMember(Order = 4, Name = "RGBR.RULE")]
		public string RgbrRule { get; set; }
		
		/// <summary>
		/// CDD Name: RGBR.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 5, Name = "RGBR.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? RgbrChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: RGBR.AMT.CALC.TYPE
		/// </summary>
		[DataMember(Order = 6, Name = "RGBR.AMT.CALC.TYPE")]
		public string RgbrAmtCalcType { get; set; }
		
		/// <summary>
		/// CDD Name: RGBR.CR.AMT
		/// </summary>
		[DataMember(Order = 23, Name = "RGBR.CR.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? RgbrCrAmt { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}