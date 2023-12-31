//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/5/2017 2:35:14 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: CREATE.BOOK
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.BOOK", GeneratedDateTime = "10/5/2017 2:35:14 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CreateBookRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.TITLE", InBoundData = true)]        
		public string InBookTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.AUTHOR", InBoundData = true)]        
		public string InBookAuthor { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.PUBLISHER", InBoundData = true)]        
		public string InBookPublisher { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.COPYRIGHT.DATE", InBoundData = true)]        
		public string InBookCopyrightDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "IN.BOOK.PRICE", InBoundData = true)]        
		public Nullable<Decimal> InBookPrice { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.COMMENT", InBoundData = true)]        
		public string InBookComment { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.ISBN", InBoundData = true)]        
		public string InBookIsbn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.EDITION", InBoundData = true)]        
		public string InBookEdition { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "IN.BOOK.USED.PRICE", InBoundData = true)]        
		public Nullable<Decimal> InBookUsedPrice { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.BOOK.EXTERNAL.COMMENTS", InBoundData = true)]        
		public List<string> InBookExternalComments { get; set; }

		public CreateBookRequest()
		{	
			InBookExternalComments = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.BOOK", GeneratedDateTime = "10/5/2017 2:35:14 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CreateBookResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.BOOK.ID", OutBoundData = true)]        
		public string OutBookId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool OutError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR.MSGS", OutBoundData = true)]        
		public List<string> OutErrorMsgs { get; set; }

		public CreateBookResponse()
		{	
			OutErrorMsgs = new List<string>();
		}
	}
}
