//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/11/2019 5:21:14 PM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.F09.SS.DIRT
//     Application: ST
//     Environment: FGU3
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

namespace Ellucian.Colleague.Data.F09.Transactions
{
	[DataContract]
	public class ReportOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REPORT.CODE", OutBoundData = true)]
		public string ReportCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REPORT.DESC", OutBoundData = true)]
		public string ReportDesc { get; set; }
	}

	[DataContract]
	public class FormatOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.FORMAT.CODE", OutBoundData = true)]
		public string FormatCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.FORMAT.DESC", OutBoundData = true)]
		public string FormatDesc { get; set; }
	}

	[DataContract]
	public class SortOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SORT.CODE", OutBoundData = true)]
		public string SortCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SORT.DESC", OutBoundData = true)]
		public string SortDesc { get; set; }
	}

	[DataContract]
	public class DeptOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DEPT.CODE", OutBoundData = true)]
		public string DeptCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DEPT.DESC", OutBoundData = true)]
		public string DeptDesc { get; set; }
	}

	[DataContract]
	public class ProgOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.CODE", OutBoundData = true)]
		public string ProgCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.DESC", OutBoundData = true)]
		public string ProgDesc { get; set; }
	}

	[DataContract]
	public class FacOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.FAC.CODE", OutBoundData = true)]
		public string FacCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.FAC.DESC", OutBoundData = true)]
		public string FacDesc { get; set; }
	}

	[DataContract]
	public class StateOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STATE.CODE", OutBoundData = true)]
		public string StateCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STATE.DESC", OutBoundData = true)]
		public string StateDesc { get; set; }
	}

	[DataContract]
	public class CountryOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.COUNTRY.CODE", OutBoundData = true)]
		public string CountryCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.COUNTRY.DESC", OutBoundData = true)]
		public string CountryDesc { get; set; }
	}

	[DataContract]
	public class ConcentraOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CONCENTRA.CODE", OutBoundData = true)]
		public string ConcentraCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CONCENTRA.DESC", OutBoundData = true)]
		public string ConcentraDesc { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.DIRT", GeneratedDateTime = "5/11/2019 5:21:14 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09DirectoriesRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.ID", InBoundData = true)]        
		public string Id { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.REQUEST.TYPE", InBoundData = true)]        
		public string RequestType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REPORT.SELECTED", InBoundData = true)]        
		public string ReportSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.FORMAT.SELECTED", InBoundData = true)]        
		public string FormatSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SORT.SELECTED", InBoundData = true)]        
		public string SortSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DEPT.SELECTED", InBoundData = true)]        
		public List<string> DeptSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.SELECTED", InBoundData = true)]        
		public List<string> ProgSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.FAC.SELECTED", InBoundData = true)]        
		public string FacSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STATE.SELECTED", InBoundData = true)]        
		public string StateSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.COUNTRY.SELECTED", InBoundData = true)]        
		public string CountrySelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CONCENTRA.SELECTED", InBoundData = true)]        
		public string ConcentraSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DISS.SEARCH.STRING", InBoundData = true)]        
		public string DissSearchString { get; set; }

		public ctxF09DirectoriesRequest()
		{	
			DeptSelected = new List<string>();
			ProgSelected = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.DIRT", GeneratedDateTime = "5/11/2019 5:21:14 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09DirectoriesResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.RESPOND.TYPE", OutBoundData = true)]        
		public string RespondType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.HTML.REPORT", OutBoundData = true)]        
		public string HtmlReport { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MSG", OutBoundData = true)]        
		public string ErrorMsg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.REPORT.CODE", OutBoundData = true)]
		public List<ReportOptions> ReportOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.FORMAT.CODE", OutBoundData = true)]
		public List<FormatOptions> FormatOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.SORT.CODE", OutBoundData = true)]
		public List<SortOptions> SortOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.DEPT.CODE", OutBoundData = true)]
		public List<DeptOptions> DeptOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.PROG.CODE", OutBoundData = true)]
		public List<ProgOptions> ProgOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.FAC.CODE", OutBoundData = true)]
		public List<FacOptions> FacOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.STATE.CODE", OutBoundData = true)]
		public List<StateOptions> StateOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.COUNTRY.CODE", OutBoundData = true)]
		public List<CountryOptions> CountryOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CONCENTRA.CODE", OutBoundData = true)]
		public List<ConcentraOptions> ConcentraOptions { get; set; }

		public ctxF09DirectoriesResponse()
		{	
			ReportOptions = new List<ReportOptions>();
			FormatOptions = new List<FormatOptions>();
			SortOptions = new List<SortOptions>();
			DeptOptions = new List<DeptOptions>();
			ProgOptions = new List<ProgOptions>();
			FacOptions = new List<FacOptions>();
			StateOptions = new List<StateOptions>();
			CountryOptions = new List<CountryOptions>();
			ConcentraOptions = new List<ConcentraOptions>();
		}
	}
}
