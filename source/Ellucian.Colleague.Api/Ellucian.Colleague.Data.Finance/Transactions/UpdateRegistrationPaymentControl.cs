//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:50:48 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: UPDATE.REGISTRATION.PAYMENT.CONTROL
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.REGISTRATION.PAYMENT.CONTROL", GeneratedDateTime = "10/4/2017 1:50:48 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateRegistrationPaymentControlRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PAYMENT.CONTROL.ID", InBoundData = true)]        
		public string PaymentControlId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYMENT.STATUS", InBoundData = true)]        
		public string PaymentStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYMENT.IDS", InBoundData = true)]        
		public List<string> PaymentIds { get; set; }

		public UpdateRegistrationPaymentControlRequest()
		{	
			PaymentIds = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.REGISTRATION.PAYMENT.CONTROL", GeneratedDateTime = "10/4/2017 1:50:48 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateRegistrationPaymentControlResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public UpdateRegistrationPaymentControlResponse()
		{	
		}
	}
}
