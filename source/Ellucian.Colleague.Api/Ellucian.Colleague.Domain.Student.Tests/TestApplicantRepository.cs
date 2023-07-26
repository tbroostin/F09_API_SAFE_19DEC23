// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestApplicantRepository : IApplicantRepository
    {

        public class Person
        {
            public string Id;
            public string LastName;
            public string PrivacyStatusCode;
        }

        public List<Person> personData = new List<Person>();

        //Financial Aid dataset added by Matt DeDiana.
        public class FaStudent
        {
            public string studentId;
            public List<FaCounselor> faCounselors;

            public class FaCounselor
            {
                public string counselorId;
                public DateTime? startDate;
                public DateTime? endDate;
            }
        }

        //studentIds come from BasePersonSetup
        public List<FaStudent> faStudentData = new List<FaStudent>()
        {
            new FaStudent() 
            {
                studentId = "0000001",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {
                    new FaStudent.FaCounselor() {counselorId = "0001111", startDate = new DateTime(2014, 5, 1), endDate = new DateTime(2014, 6, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent() 
            {
                studentId = "0000002",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent() 
            {
                studentId = "0000003",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)},
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent() 
            {
                studentId = "0000004",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {
                    new FaStudent.FaCounselor() {counselorId = "0004444", startDate = null, endDate = null}
                }
            },
            new FaStudent() 
            {
                studentId = "0000005",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {                                  
                }
            },
            new FaStudent() 
            {
                studentId = "0000006",
                faCounselors = new List<FaStudent.FaCounselor>() 
                {
                    new FaStudent.FaCounselor() {counselorId = "0002222", startDate = new DateTime(2014, 6, 2), endDate = null},
                    new FaStudent.FaCounselor() {counselorId = "0003333", startDate = null, endDate = new DateTime(2014, 7, 1)}
                }
            }
        };

        public Task<Applicant> GetApplicantAsync(string applicantId)
        {
            var person = personData.FirstOrDefault(p => p.Id == applicantId);
            var faStudent = faStudentData.FirstOrDefault(f => f.studentId == applicantId);
            if (person != null)
            {
                var applicant = new Applicant(applicantId, person.LastName, person.PrivacyStatusCode);

                if (faStudent != null && faStudent.faCounselors != null && faStudent.faCounselors.Count() > 0)
                {
                    applicant.FinancialAidCounselorId = faStudent.faCounselors.First().counselorId;
                }

                return Task.FromResult(applicant);
            }
            throw new KeyNotFoundException(string.Format("Applicant {0} not found in person", applicantId));
        }

        public Applicant GetApplicant(string applicantId)
        {
            try
            { 
                return GetApplicantAsync(applicantId).Result; 
            }
            catch
            { 
                throw ;
            }

        }

        public async Task<string> GetStwebDefaultsHierarchyAsync()
        {
            return "true";
        }
    }
}
