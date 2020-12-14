//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/11/2020 12:34:31 PM by user jmansfield
//
//     Type: CTX
//     Transaction ID: INST.ENROLL.PROPOSED.RGSTRTN
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
	public class StudentPhones
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.NUMBERS", InBoundData = true)]
		public string StudentPhoneNumbers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.EXTENSIONS", InBoundData = true)]
		public string StudentPhoneExtensions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.PHONE.TYPES", InBoundData = true)]
		public string StudentPhoneTypes { get; set; }
	}

	[DataContract]
	public class RegisteredSectionInformation
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
	public class RegistrationMessages
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.MESSAGES", OutBoundData = true)]
		public string Messages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.MESSAGE.SECTIONS", OutBoundData = true)]
		public string MessageSections { get; set; }
	}

	[DataContract]
	public class ProposedSectionInformation
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

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.PROPOSED.RGSTRTN", GeneratedDateTime = "6/11/2020 12:34:31 PM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollProposedRgstrtnRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACAD.PROGRAM", InBoundData = true)]        
		public string AcadProgram { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CATALOG", InBoundData = true)]        
		public string Catalog { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.STUDENT.EMAIL.ADDRESS", InBoundData = true)]        
		public string StudentEmailAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.EMAIL.TYPE", InBoundData = true)]        
		public string StudentEmailType { get; set; }

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
		[SctrqDataMember(AppServerName = "A.EDUCATIONAL.GOAL", InBoundData = true)]        
		public string EducationalGoal { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.STUDENT.PHONE.NUMBERS", InBoundData = true)]
		public List<StudentPhones> StudentPhones { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:AL.PROPOSED.SECTIONS", InBoundData = true)]
		public List<ProposedSectionInformation> ProposedSectionInformation { get; set; }

		public InstEnrollProposedRgstrtnRequest()
		{	
			StudentAddress = new List<string>();
			StudentEthnics = new List<string>();
			StudentRacialGroups = new List<string>();
			StudentPhones = new List<StudentPhones>();
			ProposedSectionInformation = new List<ProposedSectionInformation>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.PROPOSED.RGSTRTN", GeneratedDateTime = "6/11/2020 12:34:31 PM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollProposedRgstrtnResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.REGISTERED.SECTIONS", OutBoundData = true)]
		public List<RegisteredSectionInformation> RegisteredSectionInformation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.MESSAGES", OutBoundData = true)]
		public List<RegistrationMessages> RegistrationMessages { get; set; }

		public InstEnrollProposedRgstrtnResponse()
		{	
			RegisteredSectionInformation = new List<RegisteredSectionInformation>();
			RegistrationMessages = new List<RegistrationMessages>();
		}
	}
}
