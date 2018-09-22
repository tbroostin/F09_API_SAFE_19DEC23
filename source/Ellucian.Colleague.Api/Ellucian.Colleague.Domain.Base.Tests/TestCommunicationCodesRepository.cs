/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    public class TestCommunicationCodesRepository
    {
        public class CommunicationCodeRecord
        {
            public string Guid { get; set; }
            public string RecordKey {get; set;}
            public string CcDescription { get; set; }
            public string CcExplanation { get; set; }
            public List<CommunicationCodeUrlRecord> CcUrls { get; set; }
            public string CcFaYear { get; set; }
            public string CcOffice { get; set; }
        }

        public class CommunicationCodeUrlRecord
        {
            public string url;
            public string title;
        }

        public List<CommunicationCodeRecord> communicationCodeData = new List<CommunicationCodeRecord>()
        {
            new CommunicationCodeRecord(){
                Guid = "440cd328-7404-408f-80af-7b56c6731271",
                RecordKey = "CcCode1",
                CcDescription ="Description1",
                CcExplanation = "Explanation1",
                CcUrls = new List<CommunicationCodeUrlRecord>()
                {
                    new CommunicationCodeUrlRecord() {url = "url1.com", title ="title1"},
                    new CommunicationCodeUrlRecord() {url = "url2.com", title ="title2"},
                },
                CcFaYear = "2014",
                CcOffice = "officeId1"
            },

            new CommunicationCodeRecord(){
                Guid = "abdd63fc-263e-4c91-be4f-75e410b1de52",
                RecordKey = "CcCode2",
                CcDescription ="Description2",
                CcExplanation = "Explanation2",
                CcUrls = new List<CommunicationCodeUrlRecord>()
                {
                    new CommunicationCodeUrlRecord() {url = "url3.com", title ="title3"},
                    new CommunicationCodeUrlRecord() {url = "url4.com", title ="title4"},
                },
                CcFaYear = "2014",
                CcOffice = "officeId2"
            },

            new CommunicationCodeRecord(){
                Guid = "5e36e6fb-41df-400b-ba05-61a776323371",
                RecordKey = "CcCode3",
                CcDescription ="Description3",
                CcExplanation = "Explanation3",
                CcUrls = null,
                CcFaYear = "2014",
                CcOffice = "officeId3"
            },

            new CommunicationCodeRecord(){
                Guid = "5bce3016-b7de-4e7e-ac7f-b53714a15f1c",
                RecordKey = "CcCode4",
                CcDescription ="Description4",
                CcExplanation = "Explanation4",
                CcUrls = new List<CommunicationCodeUrlRecord>(),
                CcFaYear = "2014",
                CcOffice = "officeId4"
            },
        };

        public class CorewebDefaultsRecord
        {
            public List<string> communicationCodesAsRequiredDocuments;
        }

        public CorewebDefaultsRecord corewebDefaultsData = new CorewebDefaultsRecord()
        {
            communicationCodesAsRequiredDocuments = new List<string>()
            {
                "CcCode1",
                "CCCODE2",
                "CCCode3",
                "CCCode7"
            }
        };

        /// <summary>
        /// Get a list of communication code entities
        /// </summary>
        /// <param name="communicationCodes">list of custom communication code objects</param>
        /// <returns>List of communication code entities</returns>
        public IEnumerable<Base.Entities.CommunicationCode> GetCommunicationCodeEntities()
        {
            return communicationCodeData.Select(record =>
                new CommunicationCode(record.Guid, record.RecordKey, record.CcDescription)
                {                    
                    AwardYear = record.CcFaYear,
                    Explanation = record.CcExplanation,
                    OfficeCodeId = record.CcOffice,
                    IsStudentViewable = (corewebDefaultsData == null || corewebDefaultsData.communicationCodesAsRequiredDocuments == null) ? false :
                        corewebDefaultsData.communicationCodesAsRequiredDocuments.Contains(record.RecordKey),
                    Hyperlinks = record.CcUrls == null ? new List<CommunicationCodeHyperlink>() :
                        record.CcUrls.Select(url => new CommunicationCodeHyperlink(url.url, url.title)).ToList()
                });
        }
    }
}
