//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:50:46 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.PERSON.MATCH
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.MATCH", GeneratedDateTime = "10/4/2017 12:50:46 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonMatchRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "LAST.NAME", InBoundData = true)]        
		public string LastName { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "FIRST.NAME", InBoundData = true)]        
		public string FirstName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MIDDLE.NAME", InBoundData = true)]        
		public string MiddleName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BIRTH.NAME.LAST", InBoundData = true)]        
		public string BirthNameLast { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BIRTH.NAME.FIRST", InBoundData = true)]        
		public string BirthNameFirst { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BIRTH.NAME.MIDDLE", InBoundData = true)]        
		public string BirthNameMiddle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SSN", InBoundData = true)]        
		public string Ssn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GENDER", InBoundData = true)]        
		public string Gender { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "BIRTH.DATE", InBoundData = true)]        
		public Nullable<DateTime> BirthDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EMAIL.ADDRESS", InBoundData = true)]        
		public string EmailAddress { get; set; }

		public GetPersonMatchRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.MATCH", GeneratedDateTime = "10/4/2017 12:50:46 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonMatchResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERSON.MATCH.GUIDS", OutBoundData = true)]        
		public List<string> PersonMatchingGuids { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		public GetPersonMatchResponse()
		{	
			PersonMatchingGuids = new List<string>();
			ErrorMessages = new List<string>();
		}
	}
}
