//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:50:51 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.PERSON.MATCH.RESULTS
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
	[DataContract]
	public class MatchResults
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "MATCHED.PERSON.IDS", OutBoundData = true)]
		public string MatchPersonIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MATCHED.SCORES", OutBoundData = true)]
		public Nullable<int> MatchScores { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MATCHED.CATEGORIES", OutBoundData = true)]
		public string MatchCategories { get; set; }
	}

	[DataContract]
	public class MatchPersonNames
	{
		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "LAST.NAMES", InBoundData = true)]
		public string LastNames { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "FIRST.NAMES", InBoundData = true)]
		public string FirstNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MIDDLE.NAMES", InBoundData = true)]
		public string MiddleNames { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.MATCH.RESULTS", GeneratedDateTime = "10/4/2017 12:50:51 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonMatchResultsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MATCH.CRITERIA.RECORD.ID", InBoundData = true)]        
		public string MatchCriteriaRecordId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PREFIX", InBoundData = true)]        
		public string Prefix { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SUFFIX", InBoundData = true)]        
		public string Suffix { get; set; }

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

		[DataMember]
		[SctrqDataMember(AppServerName = "PHONE", InBoundData = true)]        
		public string Phone { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PHONE.EXTENSION", InBoundData = true)]        
		public string PhoneExtension { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "Grp:FIRST.NAMES", InBoundData = true)]
		public List<MatchPersonNames> MatchPersonNames { get; set; }

		public GetPersonMatchResultsRequest()
		{	
			MatchPersonNames = new List<MatchPersonNames>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.MATCH.RESULTS", GeneratedDateTime = "10/4/2017 12:50:51 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonMatchResultsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:MATCHED.PERSON.IDS", OutBoundData = true)]
		public List<MatchResults> MatchResults { get; set; }

		public GetPersonMatchResultsResponse()
		{	
			ErrorMessages = new List<string>();
			MatchResults = new List<MatchResults>();
		}
	}
}
