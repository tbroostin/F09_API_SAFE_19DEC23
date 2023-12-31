//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:36:15 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PAY.PLAN.TEMPLATES
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
	[DataContract(Name = "PayPlanTemplates")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:36:15 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PAY.PLAN.TEMPLATES", EntityType = "PHYS")]
	public class PayPlanTemplates : IColleagueEntity
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
		/// CDD Name: PPT.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "PPT.DESCRIPTION")]
		public string PptDescription { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.ACTIVE.FLAG
		/// </summary>
		[DataMember(Order = 2, Name = "PPT.ACTIVE.FLAG")]
		public string PptActiveFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.FREQUENCY
		/// </summary>
		[DataMember(Order = 3, Name = "PPT.FREQUENCY")]
		public string PptFrequency { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.PAY.FREQUENCY.SUBR
		/// </summary>
		[DataMember(Order = 4, Name = "PPT.PAY.FREQUENCY.SUBR")]
		public string PptPayFrequencySubr { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.NO.PAYMENTS
		/// </summary>
		[DataMember(Order = 5, Name = "PPT.NO.PAYMENTS")]
		public int? PptNoPayments { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 6, Name = "PPT.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.CHARGE.PCT
		/// </summary>
		[DataMember(Order = 7, Name = "PPT.CHARGE.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptChargePct { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.CHARGE.AR.CODE
		/// </summary>
		[DataMember(Order = 8, Name = "PPT.CHARGE.AR.CODE")]
		public string PptChargeArCode { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.DOWN.PAY.PCT
		/// </summary>
		[DataMember(Order = 9, Name = "PPT.DOWN.PAY.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptDownPayPct { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.DOWN.PAY.NO.DAYS
		/// </summary>
		[DataMember(Order = 10, Name = "PPT.DOWN.PAY.NO.DAYS")]
		public int? PptDownPayNoDays { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.PREPAY.SETUP.FLAG
		/// </summary>
		[DataMember(Order = 11, Name = "PPT.PREPAY.SETUP.FLAG")]
		public string PptPrepaySetupFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.GRACE.NO.DAYS
		/// </summary>
		[DataMember(Order = 12, Name = "PPT.GRACE.NO.DAYS")]
		public int? PptGraceNoDays { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.CALC.ON.BAL.FLAG
		/// </summary>
		[DataMember(Order = 13, Name = "PPT.CALC.ON.BAL.FLAG")]
		public string PptCalcOnBalFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.LATE.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 14, Name = "PPT.LATE.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptLateChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.LATE.CHARGE.AR.CODE
		/// </summary>
		[DataMember(Order = 15, Name = "PPT.LATE.CHARGE.AR.CODE")]
		public string PptLateChargeArCode { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.LATE.CHRG.PCT
		/// </summary>
		[DataMember(Order = 16, Name = "PPT.LATE.CHRG.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptLateChrgPct { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.LATE.PCT.AR.CODE
		/// </summary>
		[DataMember(Order = 17, Name = "PPT.LATE.PCT.AR.CODE")]
		public string PptLatePctArCode { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.MIN.PLAN.AMT
		/// </summary>
		[DataMember(Order = 18, Name = "PPT.MIN.PLAN.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptMinPlanAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.CALC.AMT.FLAG
		/// </summary>
		[DataMember(Order = 21, Name = "PPT.CALC.AMT.FLAG")]
		public string PptCalcAmtFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.REBILL.MODIFY.FLAG
		/// </summary>
		[DataMember(Order = 22, Name = "PPT.REBILL.MODIFY.FLAG")]
		public string PptRebillModifyFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.MODIFY.METHOD
		/// </summary>
		[DataMember(Order = 23, Name = "PPT.MODIFY.METHOD")]
		public string PptModifyMethod { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.ALLOWED.AR.TYPES
		/// </summary>
		[DataMember(Order = 28, Name = "PPT.ALLOWED.AR.TYPES")]
		public List<string> PptAllowedArTypes { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.INVOICE.EXCL.RULES
		/// </summary>
		[DataMember(Order = 29, Name = "PPT.INVOICE.EXCL.RULES")]
		public List<string> PptInvoiceExclRules { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.INCLUDE.AR.CODES
		/// </summary>
		[DataMember(Order = 30, Name = "PPT.INCLUDE.AR.CODES")]
		public List<string> PptIncludeArCodes { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.EXCLUDE.AR.CODES
		/// </summary>
		[DataMember(Order = 31, Name = "PPT.EXCLUDE.AR.CODES")]
		public List<string> PptExcludeArCodes { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.MAX.PLAN.AMT
		/// </summary>
		[DataMember(Order = 34, Name = "PPT.MAX.PLAN.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PptMaxPlanAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.SUBTRACT.ANTICIPATED.FA
		/// </summary>
		[DataMember(Order = 35, Name = "PPT.SUBTRACT.ANTICIPATED.FA")]
		public string PptSubtractAnticipatedFa { get; set; }
		
		/// <summary>
		/// CDD Name: PPT.TERMS.AND.CONDITIONS.DOC
		/// </summary>
		[DataMember(Order = 36, Name = "PPT.TERMS.AND.CONDITIONS.DOC")]
		public string PptTermsAndConditionsDoc { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}