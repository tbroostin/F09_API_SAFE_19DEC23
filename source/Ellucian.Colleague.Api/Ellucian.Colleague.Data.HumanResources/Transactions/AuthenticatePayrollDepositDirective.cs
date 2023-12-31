//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:34:41 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: AUTHENTICATE.PAYROLL.DEPOSIT.DIRECTIVE
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "AUTHENTICATE.PAYROLL.DEPOSIT.DIRECTIVE", GeneratedDateTime = "10/4/2017 2:34:41 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class AuthenticatePayrollDepositDirectiveRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EMPLOYEE.ID", InBoundData = true)]        
		public string EmployeeId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYROLL.DEPOSIT.DIRECTIVE.ID", InBoundData = true)]        
		public string PayrollDepositDirectiveId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BANK.ACCOUNT.ID", InBoundData = true)]        
		public string BankAccountId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOKEN", InBoundData = true)]        
		public string Token { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "EXPIRATION.DATE", InBoundData = true)]        
		public Nullable<DateTime> ExpirationDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "EXPIRATION.TIME", InBoundData = true)]        
		public Nullable<DateTime> ExpirationTime { get; set; }

		public AuthenticatePayrollDepositDirectiveRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "AUTHENTICATE.PAYROLL.DEPOSIT.DIRECTIVE", GeneratedDateTime = "10/4/2017 2:34:41 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class AuthenticatePayrollDepositDirectiveResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOKEN", OutBoundData = true)]        
		public string Token { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "EXPIRATION.DATE", OutBoundData = true)]        
		public Nullable<DateTime> ExpirationDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "EXPIRATION.TIME", OutBoundData = true)]        
		public Nullable<DateTime> ExpirationTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public AuthenticatePayrollDepositDirectiveResponse()
		{	
		}
	}
}
