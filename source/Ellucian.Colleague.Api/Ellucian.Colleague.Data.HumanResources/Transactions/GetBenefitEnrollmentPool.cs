//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/7/2020 9:21:33 AM by user namrathak
//
//     Type: CTX
//     Transaction ID: GET.BENEFIT.ENROLLMENT.POOL
//     Application: HR
//     Environment: Dvcoll
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
    public class BenefitEnrollmentPool
    {
        [DataMember]
        [SctrqDataMember(AppServerName = "BEN.ENR.POOL.ID", OutBoundData = true)]
        public string BenEnrPoolId { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.PERSON.ID", OutBoundData = true)]
        public string BeplPersonId { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.TRUST.FLAG", OutBoundData = true)]
        public string BeplTrustFlag { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.PREFIX.CODE", OutBoundData = true)]
        public string BeplPrefixCode { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.PREFIX.DESC", OutBoundData = true)]
        public string BeplPrefixDesc { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.FIRST.NAME", OutBoundData = true)]
        public string BeplFirstName { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.MIDDLE.NAME", OutBoundData = true)]
        public string BeplMiddleName { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.LAST.NAME", OutBoundData = true)]
        public string BeplLastName { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.SUFFIX.CODE", OutBoundData = true)]
        public string BeplSuffixCode { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.SUFFIX.DESC", OutBoundData = true)]
        public string BeplSuffixDesc { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.LINE1", OutBoundData = true)]
        public string BeplAddrLine1 { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.LINE2", OutBoundData = true)]
        public string BeplAddrLine2 { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.CITY", OutBoundData = true)]
        public string BeplAddrCity { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.STATE", OutBoundData = true)]
        public string BeplAddrState { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.ZIP", OutBoundData = true)]
        public string BeplAddrZip { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ADDR.COUNTRY", OutBoundData = true)]
        public string BeplAddrCountry { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.RELATIONSHIP", OutBoundData = true)]
        public string BeplRelationship { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.GENDER", OutBoundData = true)]
        public string BeplGender { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.MARITAL.STATUS", OutBoundData = true)]
        public string BeplMaritalStatus { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.FULL.TIME.FLAG", OutBoundData = true)]
        public string BeplFullTimeFlag { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ORG.ID", OutBoundData = true)]
        public string BeplOrgId { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.ORG.NAME", OutBoundData = true)]
        public string BeplOrgName { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.SSN.ON.FILE.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
        public bool BeplSsnOnFileFlag { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "BEPL.BIRTHDATE.ON.FILE.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
        public bool BeplBirthdateOnFileFlag { get; set; }
    }

    [GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
    [DataContract]
    [ColleagueDataContract(ColleagueId = "GET.BENEFIT.ENROLLMENT.POOL", GeneratedDateTime = "1/7/2020 9:21:33 AM", User = "namrathak")]
    [SctrqDataContract(Application = "HR", DataContractVersion = 1)]
    public class GetBenefitEnrollmentPoolRequest
    {
        /// <summary>
        /// Version
        /// </summary>
        [DataMember]
        public int _AppServerVersion { get; set; }

        [DataMember(IsRequired = true)]
        [SctrqDataMember(AppServerName = "EMPLOYEE.ID", InBoundData = true)]
        public string EmployeeId { get; set; }

        public GetBenefitEnrollmentPoolRequest()
        {
        }
    }

    [GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
    [DataContract]
    [ColleagueDataContract(ColleagueId = "GET.BENEFIT.ENROLLMENT.POOL", GeneratedDateTime = "1/7/2020 9:21:33 AM", User = "namrathak")]
    [SctrqDataContract(Application = "HR", DataContractVersion = 1)]
    public class GetBenefitEnrollmentPoolResponse
    {
        /// <summary>
        /// Version
        /// </summary>
        [DataMember]
        public int _AppServerVersion { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]
        public string ErrorMessage { get; set; }

        [DataMember]
        [SctrqDataMember(AppServerName = "Grp:BEN.ENR.POOL.ID", OutBoundData = true)]
        public List<BenefitEnrollmentPool> BenefitEnrollmentPool { get; set; }

        public GetBenefitEnrollmentPoolResponse()
        {
            BenefitEnrollmentPool = new List<BenefitEnrollmentPool>();
        }
    }
}