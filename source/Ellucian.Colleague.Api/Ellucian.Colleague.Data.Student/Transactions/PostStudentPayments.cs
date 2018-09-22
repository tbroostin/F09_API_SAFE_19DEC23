//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/22/2018 2:03:34 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: CREATE.AR.PAYMENT
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
	public class StudentPaymentErrors
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
	[ColleagueDataContract(ColleagueId = "CREATE.AR.PAYMENT", GeneratedDateTime = "3/22/2018 2:03:34 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PostStudentPaymentsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.PAY.ITEMS.INTG.ID", InBoundData = true)]        
		public string ArPayItemsIntgId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "ARP.INTG.AMT", InBoundData = true)]        
		public Nullable<Decimal> ArpIntgAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.AMT.CURRENCY", InBoundData = true)]        
		public string ArpIntgAmtCurrency { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.AR.CODE", InBoundData = true)]        
		public string ArpIntgArCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.AR.TYPE", InBoundData = true)]        
		public string ArpIntgArType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.PAYMENT.TYPE", InBoundData = true)]        
		public string ArpIntgPaymentType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.COMMENTS", InBoundData = true)]        
		public string ArpIntgComments { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ARP.INTG.PAYMENT.DATE", InBoundData = true)]        
		public Nullable<DateTime> ArpIntgPaymentDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.PERSON.ID", InBoundData = true)]        
		public string ArpIntgPersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.TERM", InBoundData = true)]        
		public string ArpIntgTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.DISTR.MTHD", InBoundData = true)]        
		public string ArpIntgDistrMthd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.GUID", InBoundData = true)]        
		public string ArpIntgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ELEVATE.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ElevateFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public PostStudentPaymentsRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.AR.PAYMENT", GeneratedDateTime = "3/22/2018 2:03:34 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PostStudentPaymentsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AR.PAY.ITEMS.INTG.ID", OutBoundData = true)]        
		public string ArPayItemsIntgId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ARP.INTG.GUID", OutBoundData = true)]        
		public string ArpIntgGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<StudentPaymentErrors> StudentPaymentErrors { get; set; }

		public PostStudentPaymentsResponse()
		{	
			StudentPaymentErrors = new List<StudentPaymentErrors>();
		}
	}
}
