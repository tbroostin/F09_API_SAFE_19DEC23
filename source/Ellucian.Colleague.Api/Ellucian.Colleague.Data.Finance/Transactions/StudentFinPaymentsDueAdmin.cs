//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:43:47 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: SFX004AD
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

namespace Ellucian.Colleague.Data.Finance.Transactions
{
	[DataContract]
	public class PaymentsDue
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "PERIODS", OutBoundData = true)]
		public string Periods { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERIOD.DESCS", OutBoundData = true)]
		public string PeriodDescs { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PERIOD.BEGIN.DATES", OutBoundData = true)]
		public Nullable<DateTime> PeriodBeginDates { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PERIOD.END.DATES", OutBoundData = true)]
		public Nullable<DateTime> PeriodEndDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RELATED.TERMS", OutBoundData = true)]
		public string RelatedTerms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RELATED.TERM.DESCS", OutBoundData = true)]
		public string RelatedTermDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.TYPES", OutBoundData = true)]
		public string ArTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.TYPE.DESCS", OutBoundData = true)]
		public string ArTypeDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.TYPE.DIST", OutBoundData = true)]
		public string ArTypeDist { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AR.TYPE.DUE.DATES", OutBoundData = true)]
		public Nullable<DateTime> ArTypeDueDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.TYPE.OVERDUE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool ArTypeOverdue { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AR.TYPE.BALS", OutBoundData = true)]
		public Nullable<Decimal> ArTypeBals { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICES", OutBoundData = true)]
		public string Invoices { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICE.DESCS", OutBoundData = true)]
		public string InvoiceDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICE.AR.TYPES", OutBoundData = true)]
		public string InvoiceArTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICE.DIST", OutBoundData = true)]
		public string InvoiceDist { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "INVOICES.DUE.DATE", OutBoundData = true)]
		public Nullable<DateTime> InvoicesDueDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICES.OVERDUE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool InvoicesOverdue { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "INVOICE.BALS", OutBoundData = true)]
		public Nullable<Decimal> InvoiceBals { get; set; }
	}

	[DataContract]
	public class PaymentPlansDue
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "PP.PERIODS", OutBoundData = true)]
		public string PaymentPlanPeriods { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.PERIOD.DESCS", OutBoundData = true)]
		public string PaymentPlanPeriodDescs { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PP.PERIOD.BEGIN.DATES", OutBoundData = true)]
		public Nullable<DateTime> PaymentPlanPeriodBeginDates { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PP.PERIOD.END.DATES", OutBoundData = true)]
		public Nullable<DateTime> PaymentPlanPeriodEndDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.TERMS", OutBoundData = true)]
		public string PaymentPlanTerms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.TERM.DESCS", OutBoundData = true)]
		public string PaymentPlanTermDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.AR.TYPES", OutBoundData = true)]
		public string PaymentPlanArTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.AR.TYPE.DESCS", OutBoundData = true)]
		public string PaymentPlanArTypeDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.IDS", OutBoundData = true)]
		public string PaymentPlanIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.DESCS", OutBoundData = true)]
		public string PaymentPlanDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.DIST", OutBoundData = true)]
		public string PaymentPlanDist { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PP.DUE.DATES", OutBoundData = true)]
		public Nullable<DateTime> PaymentPlanDueDates { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "PP.UNPAID.AMTS", OutBoundData = true)]
		public Nullable<Decimal> PaymentPlanUnpaidAmts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.OVERDUE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool PaymentPlanOverdue { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PP.CURRENT", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool PaymentPlanCurrent { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "SFX004AD", GeneratedDateTime = "10/4/2017 1:43:47 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class StudentFinPaymentsDueAdminRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		public StudentFinPaymentsDueAdminRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "SFX004AD", GeneratedDateTime = "10/4/2017 1:43:47 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class StudentFinPaymentsDueAdminResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERSON.NAME", OutBoundData = true)]        
		public string PersonName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INTVL.TYPE", OutBoundData = true)]        
		public string IntvlType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.LIST", OutBoundData = true)]        
		public List<string> TermList { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:PERIODS", OutBoundData = true)]
		public List<PaymentsDue> PaymentsDue { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:PP.PERIODS", OutBoundData = true)]
		public List<PaymentPlansDue> PaymentPlansDue { get; set; }

		public StudentFinPaymentsDueAdminResponse()
		{	
			TermList = new List<string>();
			PaymentsDue = new List<PaymentsDue>();
			PaymentPlansDue = new List<PaymentPlansDue>();
		}
	}
}
