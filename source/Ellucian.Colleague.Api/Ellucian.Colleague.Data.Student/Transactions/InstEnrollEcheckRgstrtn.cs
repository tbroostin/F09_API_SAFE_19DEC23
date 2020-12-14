//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/11/2020 1:02:14 PM by user jmansfield
//
//     Type: CTX
//     Transaction ID: INST.ENROLL.ECHECK.RGSTRTN
//     Application: ST
//     Environment: dvcoll_wstst01
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
	public class EcheckProposedSectionInformation
	{
		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "AL.PROPOSED.SECTIONS", InBoundData = true)]
		public string ProposedSection { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "AL.PROPOSED.SECTION.CREDITS", InBoundData = true)]
		public Nullable<Decimal> ProposedSectionCredit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROPOSED.SECTION.REG.REASON", InBoundData = true)]
		public string ProposedSectionRegReason { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROPOSED.SECTION.MKTG.SRC", InBoundData = true)]
		public string ProposedSectionMktgSrc { get; set; }
	}

	[DataContract]
	public class EcheckStudentPhones
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.NUMBERS", InBoundData = true)]
		public string StudentPhoneNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.EXTENSIONS", InBoundData = true)]
		public string StudentPhoneExtension { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.TYPES", InBoundData = true)]
		public string StudentPhoneType { get; set; }
	}

	[DataContract]
	public class EcheckRegisteredSectionInformation
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REGISTERED.SECTIONS", OutBoundData = true)]
		public string RegisteredSection { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AL.REGISTERED.SECTION.COSTS", OutBoundData = true)]
		public Nullable<Decimal> RegisteredSectionCost { get; set; }
	}

	[DataContract]
	public class EcheckRegistrationMessages
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.MESSAGES", OutBoundData = true)]
		public string Message { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.MESSAGE.SECTIONS", OutBoundData = true)]
		public string MessageSection { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.ECHECK.RGSTRTN", GeneratedDateTime = "6/11/2020 1:02:14 PM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollEcheckRgstrtnRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.STUDENT.EMAIL.ADDRESS", InBoundData = true)]        
		public string StudentEmailAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.PREFIX", InBoundData = true)]        
		public string StudentPrefix { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.STUDENT.GIVEN.NAME", InBoundData = true)]        
		public string StudentGivenName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.MIDDLE.NAME", InBoundData = true)]        
		public string StudentMiddleName { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.STUDENT.FAMILY.NAME", InBoundData = true)]        
		public string StudentFamilyName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.SUFFIX", InBoundData = true)]        
		public string StudentSuffix { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.TAX.ID", InBoundData = true)]        
		public string StudentTaxId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.ADDRESS", InBoundData = true)]        
		public List<string> StudentAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.CITY", InBoundData = true)]        
		public string StudentCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.STATE", InBoundData = true)]        
		public string StudentState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.POSTAL.CODE", InBoundData = true)]        
		public string StudentPostalCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.COUNTY", InBoundData = true)]        
		public string StudentCounty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.COUNTRY", InBoundData = true)]        
		public string StudentCountry { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.STUDENT.BIRTH.DATE", InBoundData = true)]        
		public Nullable<DateTime> StudentBirthDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.ETHNICS", InBoundData = true)]        
		public List<string> StudentEthnics { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.RACIAL.GROUPS", InBoundData = true)]        
		public List<string> StudentRacialGroups { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.GENDER", InBoundData = true)]        
		public string StudentGender { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.CITIZENSHIP", InBoundData = true)]        
		public string StudentCitizenship { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACAD.PROGRAM", InBoundData = true)]        
		public string AcadProgram { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CATALOG", InBoundData = true)]        
		public string Catalog { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.EDUCATIONAL.GOAL", InBoundData = true)]        
		public string EducationalGoal { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "A.PAYMENT.AMT", InBoundData = true)]        
		public Nullable<Decimal> PaymentAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYMENT.METHOD", InBoundData = true)]        
		public string PaymentMethod { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.NAME.ON.BANK.ACCOUNT", InBoundData = true)]        
		public string NameOnBankAccount { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.ACCOUNT.ROUTING.NUMBER", InBoundData = true)]        
		public string RoutingNumber { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.ACCOUNT.NUMBER", InBoundData = true)]        
		public string AccountNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CHECK.NUMBER", InBoundData = true)]        
		public string CheckNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACCOUNT.TYPE", InBoundData = true)]        
		public string AccountType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DRIVERS.LICENSE", InBoundData = true)]        
		public string DriversLicense { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.LICENSE.STATE", InBoundData = true)]        
		public string LicenseState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYER.EMAIL.ADDRESS", InBoundData = true)]        
		public string PayerEmailAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PROVIDER.ACCOUNT", InBoundData = true)]        
		public string ProviderAccount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CONVENIENCE.FEE.DESCR", InBoundData = true)]        
		public string ConvenienceFeeDescr { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "A.CONVENIENCE.FEE.AMT", InBoundData = true)]        
		public Nullable<Decimal> ConvenienceFeeAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CONVENIENCE.FEE.GL.NO", InBoundData = true)]        
		public string ConvenienceFeeGlNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYER.ADDRESS", InBoundData = true)]        
		public string PayerAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYER.CITY", InBoundData = true)]        
		public string PayerCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYER.STATE", InBoundData = true)]        
		public string PayerState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PAYER.POSTAL.CODE", InBoundData = true)]        
		public string PayerPostalCode { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:AL.PROPOSED.SECTIONS", InBoundData = true)]
		public List<EcheckProposedSectionInformation> EcheckProposedSectionInformation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.STUDENT.PHONE.NUMBERS", InBoundData = true)]
		public List<EcheckStudentPhones> EcheckStudentPhones { get; set; }

		public InstEnrollEcheckRgstrtnRequest()
		{	
			StudentAddress = new List<string>();
			StudentEthnics = new List<string>();
			StudentRacialGroups = new List<string>();
			EcheckProposedSectionInformation = new List<EcheckProposedSectionInformation>();
			EcheckStudentPhones = new List<EcheckStudentPhones>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.ECHECK.RGSTRTN", GeneratedDateTime = "6/11/2020 1:02:14 PM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollEcheckRgstrtnResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.ID", OutBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CASH.RECEIPT.ID", OutBoundData = true)]        
		public string CashReceiptId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.USER.NAME", OutBoundData = true)]        
		public string AUserName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.REGISTERED.SECTIONS", OutBoundData = true)]
		public List<EcheckRegisteredSectionInformation> EcheckRegisteredSectionInformation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.MESSAGES", OutBoundData = true)]
		public List<EcheckRegistrationMessages> EcheckRegistrationMessages { get; set; }

		public InstEnrollEcheckRgstrtnResponse()
		{	
			EcheckRegisteredSectionInformation = new List<EcheckRegisteredSectionInformation>();
			EcheckRegistrationMessages = new List<EcheckRegistrationMessages>();
		}
	}
}
