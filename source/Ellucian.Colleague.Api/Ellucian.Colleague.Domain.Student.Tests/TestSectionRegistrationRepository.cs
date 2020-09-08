// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestSectionRegistrationRepository: ISectionRegistrationRepository
    {

        private readonly List<SectionRegistrationResponse> responses = new List<SectionRegistrationResponse>();

        public async Task<SectionRegistrationResponse> GetAsync(string guid)
        {
            if (!responses.Any()) 
            {
                Populate(); 
            }
            var response = responses.Where(g => g.Guid == guid).FirstOrDefault();
            return await Task.FromResult<SectionRegistrationResponse>(response);
        }

        private void Populate()
        {
            string[,] responseData = {
                                          // Id, StudentId, SectionId, StatusCode, GradeScheme, PassAudit
                                         { "1", "0000001", "489",       "A",            "UG",       ""  },
                                         { "2", "0000002", "490",       "N",            "UG",       "P" },
                                         { "3", "0000002", "491",       "D",            "GR",       "A" },
                                         { "4", "0000003", "",          "X",            "",         ""  },
                                         { "5", "0000004", null,        "C",            "",         ""  },
                                         { "6", "0000005", null,      null,             "",         ""  }
                                     };
            int priorityCnt = responseData.Length / 6;
            for (int x = 0; x < priorityCnt; x++)
            {
                var id = responseData[x, 0];
                var studentId = responseData[x, 1];
                var sectionId = responseData[x, 2];
                var statusCode = responseData[x, 3];
                var gradeScheme = responseData[x, 4];
                var passAudit = responseData[x, 5];
                
                SectionRegistrationResponse response = new SectionRegistrationResponse(id, studentId, sectionId, statusCode, gradeScheme, passAudit, new List<RegistrationMessage>());
                
                responses.Add(response);
            }
        }

        public SectionRegistrationResponse GetSectionRegistrationResponse()
        {
            SectionRegistrationResponse response = new SectionRegistrationResponse("bf374372-3a1e-4b47-b2e6-69b6f9204ad3", "0012297", "19442", "N", "UG", "", new List<RegistrationMessage>());
            //Get all grades, midterm, final & verified
            var midTermGrades = new List<MidTermGrade>() 
            {
                new MidTermGrade(1, "14", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(2, "15", DateTime.Parse("11/10/2015"), "SBHOLE"),
                new MidTermGrade(3, "16", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(4, "14", DateTime.Parse("11/11/2015"), "SBHOLE"),
                new MidTermGrade(5, "14", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(6, "16", DateTime.Parse("11/12/2015"), "SBHOLE"),
            };
            response.StudentAcadCredKey = "123";
            response.StudentCourseSecKey = "456";
            response.MidTermGrades = new List<MidTermGrade>();
            response.MidTermGrades.AddRange(midTermGrades);

            response.VerifiedTermGrade = new VerifiedTermGrade("14", DateTime.Parse("11/10/2015"), "SBHOLE");
            response.FinalTermGrade = new TermGrade("15", DateTime.Parse("12/10/2015"), "SBHOLE");
            response.InvolvementStartOn = DateTime.Parse("01/06/2016");
            response.InvolvementEndOn = DateTime.Parse("04/06/2016");
            response.ReportingStatus = "N";
            response.ReportingLastDayOdAttendance = null;
            response.GradeExtentionExpDate = null;
            response.TranscriptVerifiedGradeDate = DateTime.Parse("11/10/2015");
            response.TranscriptVerifiedBy = "SBHOLE";

            //V7 changes
            response.AcademicLevel = "UG";
            //V16.0.0
            response.StatusDateTuple = new List<Tuple<string, DateTime?>>()
            {
                new Tuple<string, DateTime?>("1", DateTime.Today.Date),
                new Tuple<string, DateTime?>("2", DateTime.Today.Date.AddDays(-1)),
                new Tuple<string, DateTime?>("3", DateTime.Today.Date.AddDays(-3))
            };
            response.StatusCode = "Registered";
            response.OverrideAcadPeriod = "2018/FA";
            response.OverrideSite = "Site1";            

            return response;
        }

        public List<Domain.Student.Entities.SectionRegistrationStatusItem> GetSectionRegistrationStatusItems()
        {
            var statusItems = new List<Domain.Student.Entities.SectionRegistrationStatusItem>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("12d65fb1-1df7-405c-b0ef-47edd2371392", "Registered", "Registered" ) { Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.Registered, SectionRegistrationStatusReason = RegistrationStatusReason.Registered } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("45052d34-1e35-4f38-80f7-472dcb283c5c", "Registered", "Registered" ) { Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.Registered, SectionRegistrationStatusReason = RegistrationStatusReason.Registered } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("4102eac9-8646-4d64-bbe2-2164564b77d4", "NotRegistered", "Dropped" ) { Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Dropped } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("5ce32cba-16e2-4c5e-9796-3d1b59610ec4", "NotRegistered", "Withdrawn" ) { Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Withdrawn } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("358c8b40-a59d-48a9-ada9-215b3ee1aa83", "NotRegistered", "Withdrawn" ){ Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Withdrawn } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("7cb6da1e-9e79-47b8-b226-8e3d161ea8ad", "Registered", "Registered" ){ Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.Registered, SectionRegistrationStatusReason = RegistrationStatusReason.Registered } },
                    new Ellucian.Colleague.Domain.Student.Entities.SectionRegistrationStatusItem("88516505-d27f-45da-9529-8301a3a8aee7", "NotRegistered", "Dropped" ){ Status = new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Dropped } }
                };
            return statusItems;
        }

        public string GetsectionRegistration2Json()
        {
            return "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '3cf900894jck'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";
        }

        public string GetsectionRegistration3Json()
        {
            return "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'academicLevel':{'id':'5b65853c-3d6c-4949-8de1-74861dfe6bb1'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '3cf900894jck'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";
        }

        public string GetsectionRegistration4Json()
        {
            return "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'academicLevel':{'id':'5b65853c-3d6c-4949-8de1-74861dfe6bb1'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'credit': {'measure': 'credit','registrationCredit': 3},'status': { 'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '3cf900894jck'}},'gradingOption':{'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode':'standard'},'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'metadata': {'dataOrigin': 'Colleague'}}";
        }

        public string GetsectionRegistration2JsonWithFinalGrade()
        {
            return "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'academicLevel':{'id':'5b65853c-3d6c-4949-8de1-74861dfe6bb1'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '12d65fb1-1df7-405c-b0ef-47edd2371392'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': '27178aab-a6e8-4d1e-ae27-eca1f7b33363'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";
        }

        public string GetsectionRegistration3JsonWithFinalGrade()
        {
            return "{'id': '0ccc21ba-daeb-4c20-81e1-7a864b91a881', 'registrant': { 'id': 'bfc549d4-c1fa-4dc5-b186-f2aabd8386c0'},'section': {'id': 'f14ed8ef-4f5a-4594-a1b2-268d219c06e7'},'approvals': [{'approvalType': 'all','approvalEntity': 'system'}],'status': {'registrationStatus': 'registered','sectionRegistrationStatusReason': 'registered','detail': {'id': '12d65fb1-1df7-405c-b0ef-47edd2371392'}},'awardGradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'transcript': {'gradeScheme': {'id': '9a1914f6-ee9c-449c-92bc-8928267dfe4d'},'mode': 'standard'},'grades': [{'type': {'id': '27178aab-a6e8-4d1e-ae27-eca1f7b33363'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': 'bb66b971-3ee0-4477-9bb7-539721f93434'},'grade': {'id': 'd874e05d-9d97-4fa3-8862-5044ef2384d0'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}},{'type': {'id': '5aeebc5c-c973-4f83-be4b-f64c95002124'},'grade': {'id': '62b7fa62-5950-46eb-9145-a67e0733af12'},'submission': {'submittedBy': {'id': '02dc2629-e8a7-410e-b4df-572d02822f8b'},'submittedOn': '2015-12-03T12:00:00z','method': 'manual','reason': {'id': 'bf775687-6dfe-42ef-b7c0-aee3d9e681cf'}}}],'involvement': {'startOn': '2016-01-21T12:00:00z','endOn': '2016-05-11T12:00:00z'},'reporting': {'countryCode': 'USA','lastDayOfAttendance': {'status': 'attended','lastAttendedOn': null}},'metadata': {'dataOrigin': 'Colleague'}}";
        }

        public SectionRegistrationRequest GetSectionRegistrationRequest()
        {
            SectionRegistrationRequest request = new SectionRegistrationRequest("bfc549d4-c1fa-4dc5-b186-f2aabd8386c0", "0ccc21ba-daeb-4c20-81e1-7a864b91a881", GetSectionRegistration());
            //Get all grades, midterm, final & verified
            var midTermGrades = new List<MidTermGrade>() 
            {
                new MidTermGrade(1, "14", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(2, "15", DateTime.Parse("11/10/2015"), "SBHOLE"),
                new MidTermGrade(3, "16", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(4, "14", DateTime.Parse("11/11/2015"), "SBHOLE"),
                new MidTermGrade(5, "14", DateTime.Parse("11/9/2015"), "SBHOLE"),
                new MidTermGrade(6, "16", DateTime.Parse("11/12/2015"), "SBHOLE"),
            };
            request.MidTermGrades = new List<MidTermGrade>();
            request.MidTermGrades.AddRange(midTermGrades);

            request.VerifiedTermGrade = new VerifiedTermGrade("14", DateTime.Parse("11/10/2015"), "SBHOLE");
            request.FinalTermGrade = new TermGrade("15", DateTime.Parse("12/10/2015"), "SBHOLE");
            request.InvolvementStartOn = DateTime.Parse("01/06/2016");
            request.InvolvementEndOn = DateTime.Parse("04/06/2016");
            request.ReportingStatus = "N";
            request.ReportingLastDayOfAttendance = null;
            request.GradeExtentionExpDate = null;
            request.TranscriptVerifiedGradeDate = DateTime.Parse("11/10/2015");
            request.TranscriptVerifiedBy = "SBHOLE";
            return request;
        }

        private static SectionRegistration GetSectionRegistration()
        {
            return new SectionRegistration() { Action = RegistrationAction.Add, Credits = 3.00m, RegistrationDate = DateTime.Now, SectionId = "f14ed8ef-4f5a-4594-a1b2-268d219c06e7" };
        }

        #region NotImplementedException region
        public Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrationsAsync(int offset, int limit, string sectionId, string personId)
        {
            throw new NotImplementedException();
        }
        
        
        public Task<RegistrationResponse> RegisterAsync(RegistrationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSectionRegistrationIdFromGuidAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRegistrationResponse> DeleteAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRegistrationResponse> UpdateAsync(SectionRegistrationRequest request, string guid, string personId, string sectionId, string statusId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetGradeGuidFromIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRegistrationResponse> UpdateGradesAsync(SectionRegistrationResponse response, SectionRegistrationRequest request)
        {
            throw new NotImplementedException();
        }
        
        public Task<List<SectionRegistrationResponse>> GetSectionRegistrationsAsync(string sectionId, string personId)
        {
            throw new NotImplementedException();
        }
        #endregion


        public Task<SectionRegistrationResponse> Update2Async(SectionRegistrationRequest request, string guid, string personId, string sectionId, string statusCode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckStuAcadCredRecord(string id)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrations2Async(int offset, int limit, SectionRegistrationResponse sectReg, string acadPeriod, string sectionInstructor)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRegistrationResponse> GetSectionRegistrationByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<SectionRegistrationResponse> GetSectionRegistrationById2Async(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<StudentAcadCredCourseSecInfo>, int>> GetSectionRegistrationGradeOptionsAsync(int offset, int limit, StudentAcadCredCourseSecInfo criteria)
        {
            throw new NotImplementedException();
        }

        public Task<StudentAcadCredCourseSecInfo> GetSectionRegistrationGradeOptionsByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<SectionRegistrationResponse>, int>> GetSectionRegistrations3Async(int offset, int limit, SectionRegistrationResponse sectReg, string acadPeriod, string sectionInstructor)
        {
            throw new NotImplementedException();
        }
    }
}