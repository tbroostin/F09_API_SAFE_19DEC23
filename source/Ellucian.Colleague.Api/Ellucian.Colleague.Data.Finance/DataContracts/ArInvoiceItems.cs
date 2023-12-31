//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:34:02 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: AR.INVOICE.ITEMS
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

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ArInvoiceItems")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:34:02 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "AR.INVOICE.ITEMS", EntityType = "PHYS")]
	public class ArInvoiceItems : IColleagueEntity
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
		/// CDD Name: INVI.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "INVI.DESC")]
		public string InviDesc { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INVOICE
		/// </summary>
		[DataMember(Order = 1, Name = "INVI.INVOICE")]
		public string InviInvoice { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.AR.CODE
		/// </summary>
		[DataMember(Order = 2, Name = "INVI.AR.CODE")]
		public string InviArCode { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.EXT.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 5, Name = "INVI.EXT.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InviExtChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.EXT.CR.AMT
		/// </summary>
		[DataMember(Order = 7, Name = "INVI.EXT.CR.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InviExtCrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.AR.CODE.TAX.DISTRS
		/// </summary>
		[DataMember(Order = 17, Name = "INVI.AR.CODE.TAX.DISTRS")]
		public List<string> InviArCodeTaxDistrs { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.AR.PAY.PLANS
		/// </summary>
		[DataMember(Order = 21, Name = "INVI.AR.PAY.PLANS")]
		public List<string> InviArPayPlans { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}