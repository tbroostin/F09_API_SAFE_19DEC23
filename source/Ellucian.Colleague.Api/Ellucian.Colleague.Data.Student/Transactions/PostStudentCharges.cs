//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/17/2020 5:11:05 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: CREATE.AR.INVOICE
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[DataContract]
	public class StudentChargeErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.AR.INVOICE", GeneratedDateTime = "1/17/2020 5:11:05 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PostStudentChargesRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.INV.ITEMS.INTG.ID", InBoundData = true)]        
		public string ArInvItemsIntgId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.AMT", InBoundData = true)]        
		public Nullable<Decimal> InviIntgAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.AMT.CURRENCY", InBoundData = true)]        
		public string InviIntgAmtCurrency { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.AR.CODE", InBoundData = true)]        
		public string InviIntgArCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.AR.INV.ITEM", InBoundData = true)]        
		public string InviIntgArInvItem { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.AR.TYPE", InBoundData = true)]        
		public string InviIntgArType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.CHARGE.TYPE", InBoundData = true)]        
		public string InviIntgChargeType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.COMMENTS", InBoundData = true)]        
		public string InviIntgComments { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.DUE.DATE", InBoundData = true)]        
		public Nullable<DateTime> InviIntgDueDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.PERSON.ID", InBoundData = true)]        
		public string InviIntgPersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.TERM", InBoundData = true)]        
		public string InviIntgTerm { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.UNIT.COST", InBoundData = true)]        
		public Nullable<Decimal> InviIntgUnitCost { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.UNIT.CURRENCY", InBoundData = true)]        
		public string InviIntgUnitCurrency { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.UNIT.QTY", InBoundData = true)]        
		public Nullable<long> InviIntgUnitQty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.USAGE", InBoundData = true)]        
		public string InviIntgUsage { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.ORIGINATED.ON", InBoundData = true)]        
		public Nullable<DateTime> InviIntgOriginatedOn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.OVERRIDE.DESC", InBoundData = true)]        
		public string InviIntgOverrideDesc { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.BILLING.START.DATE", InBoundData = true)]        
		public Nullable<DateTime> InviIntgBillingStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "INVI.INTG.BILLING.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> InviIntgBillingEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.GUID", InBoundData = true)]        
		public string InviIntgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ELEVATE.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ElevateFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public PostStudentChargesRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.AR.INVOICE", GeneratedDateTime = "1/17/2020 5:11:05 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PostStudentChargesResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.INV.ITEMS.INTG.ID", OutBoundData = true)]        
		public string ArInvItemsIntgId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVI.INTG.GUID", OutBoundData = true)]        
		public string InviIntgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<StudentChargeErrors> StudentChargeErrors { get; set; }

		public PostStudentChargesResponse()
		{	
			StudentChargeErrors = new List<StudentChargeErrors>();
		}
	}
}
