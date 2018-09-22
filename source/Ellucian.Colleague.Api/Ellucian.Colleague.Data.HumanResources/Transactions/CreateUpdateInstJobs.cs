//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/20/2018 10:54:18 AM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: CREATE.UPDATE.PERPOS
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.Transactions
{
	[DataContract]
	public class hoursPerPeriod
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "HOURS.PER.PERIOD", InBoundData = true)]
		public string HoursPerPeriodPeriod { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "HOURS.PER.PERIOD.HOURS", InBoundData = true)]
		public Nullable<Decimal> HoursPerPeriodHours { get; set; }
	}

	[DataContract]
	public class supervisors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "SUPERVISORS", InBoundData = true)]
		public string Supervisors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SUPERVISORS.TYPE", InBoundData = true)]
		public string SupervisorsType { get; set; }
	}

	[DataContract]
	public class AccountingAllocation
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ACCOUNT.STRINGS", InBoundData = true)]
		public string AccountStrings { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "ACCOUNT.STRING.PCT", InBoundData = true)]
		public Nullable<Decimal> AccountStringPct { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ACCOUNT.STRING.START.DATE", InBoundData = true)]
		public Nullable<DateTime> AccountStringStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ACCOUNT.STRING.END.DATE", InBoundData = true)]
		public Nullable<DateTime> AccountStringEndDate { get; set; }
	}

	[DataContract]
	public class Salaries
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "SALARY.GRADE", InBoundData = true)]
		public string SalaryGrade { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SALARY.STEP", InBoundData = true)]
		public string SalaryStep { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SALARY.AMT.PERIOD", InBoundData = true)]
		public string SalaryAmtPeriod { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SALARY.AMT.VALUE", InBoundData = true)]
		public string SalaryAmtValue { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SALARY.START.ON", InBoundData = true)]
		public Nullable<DateTime> SalaryStartOn { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SALARY.END.ON", InBoundData = true)]
		public Nullable<DateTime> SalaryEndOn { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.UPDATE.PERPOS", GeneratedDateTime = "3/20/2018 10:54:18 AM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class CreateUpdateInstJobsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INST.JOB.ID", InBoundData = true)]        
		public string InstJobId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GUID", InBoundData = true)]        
		public string Guid { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EMPLOYER.ID", InBoundData = true)]        
		public string EmployerId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "POS.ID", InBoundData = true)]        
		public string PositionId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DEPARTMENT", InBoundData = true)]        
		public string Department { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "START.ON", InBoundData = true)]        
		public Nullable<DateTime> StartOn { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "END.ON", InBoundData = true)]        
		public Nullable<DateTime> EndOn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "JOB.CHANGE.REASON.ID", InBoundData = true)]        
		public string JobChangeReasonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STATUS", InBoundData = true)]        
		public string Status { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[SctrqDataMember(AppServerName = "FULL.TIME.EQUIV", InBoundData = true)]        
		public Nullable<Decimal> FullTimeEquiv { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SALARY.AMT.CURRENCY", InBoundData = true)]        
		public string SalaryAmtCurrency { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CLASSIFICATION.ID", InBoundData = true)]        
		public string ClassificationId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INST.JOB.PAY.CLASS.ID", InBoundData = true)]        
		public string InstJobPayClassId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INST.JOB.PAY.CYCLE", InBoundData = true)]        
		public string InstJobPayCycle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PREFERENCE", InBoundData = true)]        
		public string Preference { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:HOURS.PER.PERIOD", InBoundData = true)]
		public List<hoursPerPeriod> hoursPerPeriod { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:SUPERVISORS", InBoundData = true)]
		public List<supervisors> supervisors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ACCOUNT.STRINGS", InBoundData = true)]
		public List<AccountingAllocation> AccountingAllocation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:SALARY.AMT.PERIOD", InBoundData = true)]
		public List<Salaries> Salaries { get; set; }

		public CreateUpdateInstJobsRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
			hoursPerPeriod = new List<hoursPerPeriod>();
			supervisors = new List<supervisors>();
			AccountingAllocation = new List<AccountingAllocation>();
			Salaries = new List<Salaries>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.UPDATE.PERPOS", GeneratedDateTime = "3/20/2018 10:54:18 AM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class CreateUpdateInstJobsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INST.JOB.ID", OutBoundData = true)]        
		public string InstJobId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GUID", OutBoundData = true)]        
		public string Guid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]        
		public List<string> ErrorCodes { get; set; }

		public CreateUpdateInstJobsResponse()
		{	
			ErrorMessages = new List<string>();
			ErrorCodes = new List<string>();
		}
	}
}
