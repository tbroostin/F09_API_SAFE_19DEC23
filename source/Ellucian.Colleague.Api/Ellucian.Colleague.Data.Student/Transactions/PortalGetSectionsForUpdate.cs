//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/11/2020 12:22:48 PM by user rebecca.rowland
//
//     Type: CTX
//     Transaction ID: PORTAL.GET.SECTIONS.FOR.UPDT
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
	public class PortalUpdatedSections
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SECTIONS.ID", OutBoundData = true)]
		public string SectionsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.SHORT.TITLE", OutBoundData = true)]
		public string SecShortTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.DESC", OutBoundData = true)]
		public string CrsDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.LOCATION", OutBoundData = true)]
		public string SecLocation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.TERM", OutBoundData = true)]
		public string SecTerm { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.SEC.START.DATE", OutBoundData = true)]
		public Nullable<DateTime> SecStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.SEC.END.DATE", OutBoundData = true)]
		public Nullable<DateTime> SecEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.CAPACITY", OutBoundData = true)]
		public Nullable<int> SecCapacity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.SUBJECT", OutBoundData = true)]
		public string SecSubject { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.COURSE.NO ", OutBoundData = true)]
		public string SecCourseNo  { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.NO", OutBoundData = true)]
		public string SecNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.ACAD.LEVEL", OutBoundData = true)]
		public string SecAcadLevel { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.SYNONYM ", OutBoundData = true)]
		public string SecSynonym  { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "AL.SEC.MIN.CRED", OutBoundData = true)]
		public Nullable<Decimal> SecMinCred { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "AL.SEC.MAX.CRED", OutBoundData = true)]
		public Nullable<Decimal> SecMaxCred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.NAME", OutBoundData = true)]
		public string SecName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.COURSE", OutBoundData = true)]
		public string SecCourse { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.PREREQS", OutBoundData = true)]
		public string CrsPrereqs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.BLDG", OutBoundData = true)]
		public string CsmBldg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.ROOM", OutBoundData = true)]
		public string CsmRoom { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.INSTR.METHOD", OutBoundData = true)]
		public string CsmInstrMethod { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.DAYS", OutBoundData = true)]
		public string CsmDays { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.START.TIME", OutBoundData = true)]
		public string CsmStartTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CSM.END.TIME", OutBoundData = true)]
		public string CsmEndTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.DEPTS", OutBoundData = true)]
		public string SecDepts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.FACULTY", OutBoundData = true)]
		public string SecFaculty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.TYPE", OutBoundData = true)]
		public string CrsType { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AL.SEC.CEUS", OutBoundData = true)]
		public Nullable<Decimal> SecCeus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SEC.PRINTED.COMMENTS", OutBoundData = true)]
		public string SecPrintedComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.BOOK.DATA", OutBoundData = true)]
		public string BookData { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.BOOK.COST", OutBoundData = true)]
		public string BookCost { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AL.BOOK.TOTAL", OutBoundData = true)]
		public Nullable<Decimal> BookTotal { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "PORTAL.GET.SECTIONS.FOR.UPDT", GeneratedDateTime = "9/11/2020 12:22:48 PM", User = "rebecca.rowland")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PortalGetSectionsForUpdateRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }


		public PortalGetSectionsForUpdateRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "PORTAL.GET.SECTIONS.FOR.UPDT", GeneratedDateTime = "9/11/2020 12:22:48 PM", User = "rebecca.rowland")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class PortalGetSectionsForUpdateResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.HOST.SHORT.DATE.FORMAT", OutBoundData = true)]        
		public string HostShortDateFormat { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.TOTAL.SECTIONS", OutBoundData = true)]        
		public Nullable<int> TotalSections { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.SECTIONS.ID", OutBoundData = true)]
		public List<PortalUpdatedSections> PortalUpdatedSections { get; set; }

		public PortalGetSectionsForUpdateResponse()
		{	
			PortalUpdatedSections = new List<PortalUpdatedSections>();
		}
	}
}